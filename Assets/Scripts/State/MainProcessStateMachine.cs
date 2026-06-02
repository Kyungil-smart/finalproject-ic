using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


[Serializable]
public struct StateData
{
    public GameDevProcName name;
    public ProcessStateSO stateSO;
}


public class MainProcessStateMachine : MonoBehaviour, IStateInformation
{
    [Header("현재 실행 중인 메인 상태")]
    [SerializeField] private ProcessStateSO _currentMainState;
    [SerializeField] private List<StateData> _mainStates;

    [Header("서브 상태 머신")]
    [SerializeField] private SubProcessStateMachine _subStateMachine; // 서브 상태 머신 스크립트 직접 참조

    // IStateInformation 인터페이스 구현
    [field: Header("메인 프로세스 상태 정보 (IStateInformation)")]
    [field: SerializeField] public int PreviousStateID { get; private set; }
    [field: SerializeField] public int CurrentStateID { get; private set; }
    [field: SerializeField] public int NextStateID { get; private set; }
    [field: SerializeField] public string PreviousStateName { get; private set; }
    [field: SerializeField] public string CurrentStateName { get; private set; }
    [field: SerializeField] public string NextStateName { get; private set; }



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


    public void SetCurrentMainState(GameDevProcName name)
    {
        foreach(var s in _mainStates)
        {
            if (s.name == name)
            {
               _currentMainState = s.stateSO;
                return;
            }          
        }
    }

 
    public void Run()
    {
        if(_currentMainState != null)
        {
            RunSubMachine();
        }
        
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
            // SceneManager.LoadScene("MainScene");
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


    // IStateInformation 인터페이스 구현
    public void UpdateStateInformation()
    {
        PreviousStateID = CurrentStateID != 0 ? CurrentStateID : -1;
        PreviousStateName = CurrentStateName != null ? CurrentStateName : "None";

        // 상태 정보 업데이트 로직 구현
        CurrentStateID = _currentMainState != null ? _currentMainState.StateID : -1;
        CurrentStateName = _currentMainState != null ? _currentMainState.StateName : "None";

        // 다음 상태 정보는 메인 프로세스 상태 SO에서 가져옴
        NextStateID = _currentMainState != null && _currentMainState.nextState != null ? _currentMainState.nextState.StateID : -1;
        NextStateName = _currentMainState != null && _currentMainState.nextState != null ? _currentMainState.nextState.StateName : "None";
    }


    [ContextMenu("테스트용 메인 상태 머신 실행")]
    private void TestStateMachine()
    {
        SetCurrentMainState(GameDevProcName.HumanResources);
        Run();
    }
}