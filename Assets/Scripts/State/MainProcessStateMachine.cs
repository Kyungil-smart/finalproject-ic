using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;



public class MainProcessStateMachine : MonoBehaviour, IStateInformation
{
    [Header("현재 실행 중인 메인 상태")]
    [field: SerializeField] public ProcessStateSO CurrentMainState { get; private set; }

    [Header("초기 메인 상태")]
    [field: SerializeField] public ProcessStateSO FirstMainState { get; private set; }

    [Header("메인 상태 컴포넌트 연결")]
    [field: SerializeField] public GameObject ProcessStateObject { get; private set; }
    [SerializeField] private IProcessState _mainStateObject;

    [Header("현재 서브 상태 목록")]
    [field: SerializeField] public List<ProcessStateSO> SubStates { get; private set; } = new List<ProcessStateSO>();

    [Header("서브 상태 머신")]
    [SerializeField] private GameObject _subStateObject; // 서브 상태 머신 연결
    private SubProcessStateMachine _subStateMachine; // 서브 상태 머신 스크립트 직접 참조

    private ProcessStateSO nextState; // 다음 메인 상태 정보 저장용 변수

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
        _mainStateObject = ProcessStateObject.GetComponent<IProcessState>();

        // _mainStateObject.OnStateFinished += HandleStateFinished;

        // 서브 상태 머신 구독
        if (_subStateObject != null)
        {
            _subStateMachine = _subStateObject.GetComponent<SubProcessStateMachine>();
            _subStateMachine.OnAllSubStatesFinished += HandleAllSubStatesFinished;
        }

        Init(FirstMainState);
    }

    private void OnDestroy()
    {
        /*
        if (_mainStateObject != null)
        {
            _mainStateObject.OnStateFinished -= HandleStateFinished;
        }
        */

        if (_subStateMachine != null)
        {
            _subStateMachine.OnAllSubStatesFinished -= HandleAllSubStatesFinished;
        }

    }


    // 세이브 로드 있을까봐 public 처리
    public void Init(ProcessStateSO startState)
    {
        ChangeMainState(FirstMainState);
        
    }


    private void HandleAllSubStatesFinished()
    {
        // 현재 메인 상태 스크립트에게 다음 순환할 메인 상태 SO 데이터를 요구
        nextState = _mainStateObject.ChangeMachineState();

        if (nextState != null)
        {
            // Debug.Log($"[ProcessStateMachineNew] -> 서브 머신 완료 확인. 다음 메인 상태 [{nextState.name}]로 재발동");

            // 이전 메인 상태 종료시키고 다음 메인 상태를 가지고 1번부터 다시 반복
            // ChangeMainState(nextState);
            // Debug.Log($"[MainProcessStateMachine] : 모든 SubProcess 완료");
        }
        else
        {
            Debug.LogError($"[MainProcessStateMachine] : 다음 메인 상태가 없음");
        }
    }


    // 한 메인 상태 끝나면 메인 상태 SO 변경하기
    // 원래는 ChangeMainState에서 다양한 기능을 처리했으나 기능을 분리 -> TempGodFuntion() 보면 원래 기능 넣음
    public void ChangeMainState(ProcessStateSO newState)
    {
        /*
        if (newState == null)
        {
            Debug.LogError($"[MainProcessStateMachine] : ChangeMainState의 newState가 null");
            return;
        }
        */
        
        // 메인 상태 관리
        // 현재 상태 있다면 종료 처리
        if (CurrentMainState != null)
        {           
            _mainStateObject.Exit();    // -> 코드 드러내고 별도 함수로 트리거
        }
        

        // 메인 상태 다음 것으로 초기화
        CurrentMainState = newState;
        UpdateStateInformation();   // 상태 머신의 메인 프로세스 상태 정보 업데이트

        // 메인 상태 컴포넌트에 새로운 상태 정보 전달
        _mainStateObject.ChangeMyState(newState);
        // _mainStateObject.Enter();   // 자동으로 시작 -> 기다리도록 체인지는 체인지만, 코드 드러내고 별도 함수로 트리거


        // 서브 상태 초기화
        // SubStates.Clear();
        // SubStates = newState.subStates?.ToList() ?? new List<ProcessStateSO>();

        // 서브 상태 머신 발동
        // ChangeSubState(SubStates);
    }

    public void ChanggSubStateList(ProcessStateSO newState)
    {
        // 서브 상태 초기화
        SubStates.Clear();
        SubStates = newState.subStates?.ToList() ?? new List<ProcessStateSO>();
    }


    // 서브 상태 머신에 발동할 서브 상태 전달
    public void ChangeSubState(List<ProcessStateSO> subStates)
    {
        if (_subStateMachine != null)
        {
            _subStateMachine.ChangeSubStateList(subStates);
        }
    }





    // IStateInformation 인터페이스 구현
    public void UpdateStateInformation()
    {
        // 현재 메인 프로세스 상태를 이전 메인 프로세스 상태로 저장
        // 메인 프로세스 상태 SO의 PrevState가 아닌, 실제 데이터를 기반으로 업데이트하기 전에 이전 상태 정보를 저장
        // 처음에는 -1과 "None"으로 초기화
        PreviousStateID = CurrentStateID != 0 ? CurrentStateID : -1;
        PreviousStateName = CurrentStateName != null ? CurrentStateName : "None";

        // 상태 정보 업데이트 로직 구현
        CurrentStateID = CurrentMainState != null ? CurrentMainState.StateID : -1;
        CurrentStateName = CurrentMainState != null ? CurrentMainState.StateName : "None";

        // 다음 상태 정보는 메인 프로세스 상태 SO에서 가져옴
        NextStateID = CurrentMainState != null && CurrentMainState.nextState != null ? CurrentMainState.nextState.StateID : -1;
        NextStateName = CurrentMainState != null && CurrentMainState.nextState != null ? CurrentMainState.nextState.StateName : "None";

        /*  // 테스트용 로그
        Debug.Log($"[MainProcessStateMachine] : 상태 정보 업데이트 - 이전: {PreviousStateName} ({PreviousStateID})," +
            $"현재: {CurrentStateName} ({CurrentStateID}), 다음: {NextStateName} ({NextStateID})");
        */
    }




    // 외부에서 메인 상태 머신에게 메인 프로세스 상태를 Enter 하라고 요청하는 함수
    public void EnterCurrentMainState()
    {
        if (_mainStateObject != null)
        {
            _mainStateObject.Enter();
        }
    }


    // 외부에서 메인 상태 머신에게 메인 프로세스 상태를 Excute 하라고 요청하는 함수
    public void ExecuteCurrentMainState()
    {
        if (_mainStateObject != null)
        {
            _mainStateObject.Execute();
        }
    }


    // 외부에서 메인 상태 머신에게 메인 프로세스 상태를 Exit 하라고 요청하는 함수
    public void ExitCurrentMainState()
    {
        if (_mainStateObject != null)
        {
            _mainStateObject.Exit();
        }
    }


    // 기능이 잘 작동하는지 보기 위해 임시로 모든 진행을 알아서 해주는 함수 -> 추후 제거 필요
    public void TempGodFuntion(ProcessStateSO newState)
    {
        /*
        if (nextState == null)
        {
            Debug.LogError($"[MainProcessStateMachine] : TempGodFuntion 실행 실패");
            return;
        }*/

        Init(FirstMainState);
        ExitCurrentMainState();
        ChangeMainState(nextState);
        EnterCurrentMainState();
        ChangeSubState(SubStates);
    }

}