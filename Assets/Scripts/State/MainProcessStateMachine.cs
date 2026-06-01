using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class MainProcessStateMachine : MonoBehaviour
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

    /*  // 메인 상태가 아닌 서브 상태가 끝나야 발동해서 주석 처리
    private void HandleStateFinished(IProcessState finishedState)
    {
        // 다음 상태 가져오기
        ProcessStateSO nextState = _mainStateObject.ChangeMachineState();

        if (nextState != null)
        {
            
            ChangeMainState(nextState);
        }
        else
        {
            
            Debug.Log($"[ProcessStateMachineNew] : 다음 상태 확인 불가");
        }

    }
    */

    private void HandleAllSubStatesFinished()
    {
        // 현재 메인 상태 스크립트에게 다음 순환할 메인 상태 SO 데이터를 요구
        ProcessStateSO nextState = _mainStateObject.ChangeMachineState();

        if (nextState != null)
        {
            // Debug.Log($"[ProcessStateMachineNew] -> 서브 머신 완료 확인. 다음 메인 상태 [{nextState.name}]로 재발동");

            // 이전 메인 상태 종료시키고 다음 메인 상태를 가지고 1번부터 다시 반복
            ChangeMainState(nextState);
        }
        else
        {
            Debug.Log($"[ProcessStateMachineNew] -> 다음 메인 상태가 없음 상태 머신 종료");
        }
    }



    // 한 메인 상태 끝나면 메인 상태 SO 변경하기
    public void ChangeMainState(ProcessStateSO newState)
    {
        // 메인 상태 관리
        // 현재 상태 있다면 종료 처리
        if (CurrentMainState != null)
        {           
            _mainStateObject.Exit();
        }

        // 메인 상태 다음 것으로 초기화
        CurrentMainState = newState;

        // 메인 상태 컴포넌트에 새로운 상태 정보 전달
        _mainStateObject.ChangeMyState(newState);
        _mainStateObject.Enter();   // 자동으로 시작


        // 서브 상태 초기화
        SubStates = new List<ProcessStateSO>();   
        SubStates = newState.subStates?.ToList() ?? new List<ProcessStateSO>();

        // 서브 상태 머신 발동
        ChangeSubState(SubStates);


    }


    // 서브 상태 머신에 발동할 서브 상태 전달
    public void ChangeSubState(List<ProcessStateSO> subStates)
    {
        if (_subStateMachine != null)
        {
            _subStateMachine.ChangeSubStateList(subStates);
        }
    }
}