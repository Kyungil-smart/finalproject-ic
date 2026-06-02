using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct StateData
{
    public GameDevProcName name;
    public ProcessStateSO stateSO;
}

[Serializable]
public struct SimpleStateData
{
    public int id;
    public string name;
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
    [SerializeField] private ProcessStateSO _currentMainState;
    [SerializeField] private List<StateData> _mainStates;

    [Header("서브 상태 머신")]
    [SerializeField] private SubProcessStateMachine _subStateMachine; // 서브 상태 머신 스크립트 직접 참조

    [Header("메인 프로세스 상태 정보 (IStateInformation)")]
    [SerializeField] private StateViewData stateViewData;
    public StateViewData StateViewData => stateViewData;
    
    public void OnEnable() => Register();
    public void OnDisable() => Unregister();
    
    private void Start()
    {
        // 서브 상태 머신 구독
        if (_subStateMachine != null)
        {
            _subStateMachine.OnAllSubStatesFinished += HandleAllSubStatesFinished;
        }
    }

    private void OnDestroy()
    {
        if (_subStateMachine != null)
        {
            _subStateMachine.OnAllSubStatesFinished -= HandleAllSubStatesFinished;
        }
    }


    public void SetCurrentMainState(GameDevProcName stepName)
    {
        foreach(var s in _mainStates)
        {
            if (s.name == stepName)
            {
               _currentMainState = s.stateSO;
                return;
            }          
        }
    }

 
    public void Run()
    {
        if(_currentMainState != null) RunSubMachine();
        else Debug.LogError("[MainProcessStateMachine] Must set the current main state before running this machine.");
    }

    private void ChangeState(ProcessStateSO nextState)
    {
        _currentMainState = nextState;
        UpdateStateInformation();
    }


    private void HandleAllSubStatesFinished()
    {
        // 현재 메인 상태 스크립트에게 다음 순환할 메인 상태 SO 데이터를 요구
        ProcessStateSO nextState = _currentMainState.nextState;

        if (nextState != null)
        {
            ChangeState(nextState);
            if (!isTest) SceneManager.LoadScene("MainScene");
            Debug.Log($"[MainProcessStateMachine] : 다음 메인 상태로 전환 - {_currentMainState.StateName}");
        }
        else
        {
            Debug.LogError($"[MainProcessStateMachine] : 다음 메인 상태가 없음");
        }
    }


    private void RunSubMachine()
    {
        _subStateMachine.ChangeSubStateList(_currentMainState.subStates);
        _subStateMachine.RunSubState();
    }


    public void UpdateStateInformation()
    {
        if (_currentMainState == null)
        {
            Debug.LogWarning("[MainProcessStateMachine] Step UI 용 데이터 획득 불가");
            return;
        }
        
        var previous = _currentMainState.prevState;
        stateViewData.prev.id = previous != null ? previous.StateID : -1; 
        stateViewData.prev.name = previous != null ? previous.StateName : "None";
        

        var current = _currentMainState; 
        stateViewData.current.id = current != null ? current.StateID : -1;
        stateViewData.current.name = current != null ? current.StateName : "None";

        var next = _currentMainState.nextState;
        stateViewData.next.id = next != null ? next.StateID : -1;
        stateViewData.next.name = next != null ? next.StateName : "None";
    }

    protected override void Register() => ServiceLocater.Register<IMainStateMachine>(this);
    protected override void Unregister() =>  ServiceLocater.Unregister<IMainStateMachine>(this);
    
    [ContextMenu("테스트용 메인 상태 머신 실행")]
    private void TestStateMachine()
    {
        SetCurrentMainState(GameDevProcName.HumanResources);
        Run();
    }
}