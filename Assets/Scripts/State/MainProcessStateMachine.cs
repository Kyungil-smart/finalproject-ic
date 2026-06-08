using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using DataDispatcher;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Channel = DataDispatcher.Channel;
using Utils;

[Serializable]
public struct StateData
{
    public GameDevProcName name;
    public ProcessStateSO stateSO;
    public GameObject taskRunner;
}

[Serializable]
public struct SimpleStateData
{
    public int id;
    public int textId;
}

[Serializable]
public struct StateViewData
{
    public SimpleStateData prev;
    public SimpleStateData current;
    public SimpleStateData next;
}


public class MainProcessStateMachine : Manager, IMainStateMachine
{
    [Header("테스트 플래그")] 
    [SerializeField] private bool isTest = true;
    
    [Header("현재 실행 중인 메인 상태")]
    [SerializeField] private StateData _curStateData;
    [SerializeField] private List<StateData> _mainStates;

    [Header("메인 프로세스 상태 정보 (IStateInformation)")]
    [SerializeField] private StateViewData stateViewData;
    public StateViewData StateViewData => stateViewData;
    private IPostManager _postManager;
    
    public void OnEnable() => Register();
    public void OnDisable() => Unregister();

    private void Start()
    {
        _postManager = ServiceLocater.Get<IPostManager>();
        _postManager.Subscribe<bool, StateViewData>(Channel.ProcessUIUpdate, UpdateStateInformation);
    }
    
    public UniTask SetCurrentMainState(GameDevProcName stepName)
    {
        foreach(var s in _mainStates)
        {
            if (s.name == stepName)
            {
                _curStateData = s;
                UpdateStateInformation(true);
                return UniTask.CompletedTask;
            }          
        }
        return UniTask.CompletedTask;
    }

    private GameDevProcName GetStateEnum(ProcessStateSO stateSO)
    {
        foreach(var s in _mainStates)
        {
            if (s.stateSO == stateSO)
            {
                return s.name;
            }          
        }

        return GameDevProcName.Initialization;
    }

    private void ChangeState(StateData nxSo)
    {
        _curStateData = nxSo;
        ServiceLocater.Get<IGameManager>().ChangeState(_curStateData.name);
    }
    
    public void Run()
    {
        // 씬 넘어가고, 넘어간 후에 제대로 진행하도록 해야하네
        UniTask.Void(async () =>
        {
            await RunTask();
        });
    }

    private async UniTask GoToNextState()
    {
        // 현재 메인 상태 스크립트에게 다음 순환할 메인 상태 SO 데이터를 요구
        ProcessStateSO nextState = _curStateData.stateSO.nextState;
        if (nextState == null) SetCurrentMainState(GameDevProcName.HumanResources);
        else
        {
            foreach (var s in _mainStates)
            {
                if (s.stateSO == nextState)
                {
                    ChangeState(s);
                    break;
                }
            }
        }
        
        if (!isTest)
        {
            ServiceLocater.Get<ISceneChanger>().ChangeScene("MainScene");
            await UniTask.WaitUntil(() => ServiceLocater.Get<ISceneChanger>().GetCurrentSceneName() == "MainScene");
            await UniTask.WaitUntil(() => ServiceLocater.Get<IMainUIReadyable>() != null);
            await UniTask.WaitUntil(() => ServiceLocater.Get<IMainUIReadyable>().IsReady);
        }
        Debug.Log($"[MainProcessStateMachine] : 다음 메인 상태로 전환 - {_curStateData.stateSO.gameDevProcName}");
    }

    private async UniTask RunTask()
    {
        var prePostProc = _curStateData.taskRunner.GetComponent<IProcessTaskRunnerEnterExit>();
        var executeProc = _curStateData.taskRunner.GetComponent<IProcessTaskRunnerExecute>();
        
        await prePostProc.Enter(_curStateData.stateSO);
        await prePostProc.EventPreExecute();
        await executeProc.Execute();
        await prePostProc.EventPostExecute();
        await prePostProc.Exit();
        await GoToNextState();
    }

    public StateViewData UpdateStateInformation(bool dummy)
    {
        if (_curStateData.stateSO == null)
        {
            Debug.LogWarning("[MainProcessStateMachine] Step UI 용 데이터 획득 불가");
            return new StateViewData();
        }
        var curSO = _curStateData.stateSO;
        
        var previous = curSO.prevState;
        stateViewData.prev.id = previous != null ? previous.StateID : -1; 
        stateViewData.prev.textId = previous != null ? previous.stateNameId : 0;
        
        var current = curSO; 
        stateViewData.current.id = current != null ? current.StateID : -1;
        stateViewData.current.textId = current != null ? current.stateNameId : 0;

        var next = curSO.nextState;
        stateViewData.next.id = next != null ? next.StateID : -1;
        stateViewData.next.textId = next != null ? next.stateNameId : 0;
        return stateViewData;
    }

    protected override void Register()
    {
        ServiceLocater.Register<IMainStateMachine>(this);
    }

    protected override void Unregister()
    {
        ServiceLocater.Unregister<IMainStateMachine>(this);
        _postManager.Unsubscribe<bool, StateViewData>(Channel.ProcessUIUpdate, UpdateStateInformation);
    }
    
    [ContextMenu("테스트용 메인 상태 머신 실행")]
    private void TestStateMachine()
    {
        SetCurrentMainState(GameDevProcName.HumanResources);
        Run();
    }
}