using System;
using System.Linq;
using UnityEngine;

public class DesignFullProductionState : MonoBehaviour, IProcessState




{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO StateData { get; private set; }   // 현재 상태

    [Header("상태 종료 여부")]
    [field: SerializeField] public bool IsFinished { get; private set; }    // 상태가 끝났는지 여부

    public event Action<IProcessState> OnStateFinished;

    // 서브 상태 관련 변수들
    private ProcessStateSO _targetSubState;
    private IProcessState _currentSubState; // 현재 실행중인 하위 상태
    private int _currentSubStateIndex;  // 현재 하위 상태 인덱스

    private bool _isSubState;

    // private DevStateMachine _machine;   // 상태 전이를 요청할 상태 머신 참조 변수


    private void Awake()
    {
        Init();
    }



    public void Enter()
    {
        // 들어갈 시 다시 초기화
        IsFinished = false;
        _currentSubStateIndex = 0;

        Debug.Log("[StaffManagingState] : 7 상태 진입");

        // Execute();

        // 하위 단계가 있다면 첫 번째 서브 상태 가동, 없으면 종료
        if (StateData.subStates != null && StateData.subStates.Length > 0)
        {
            _targetSubState = StateData.subStates[_currentSubStateIndex];
            FindTargetObject();
        }
        else
        {
            Exit();
        }
    }

    public void Execute()
    {
        /*
        Debug.Log(_currentSubState);
        CheckNextSubState();
        */
    }

    public void Exit()
    {
        if (_currentSubState != null)
        {
            _currentSubState.OnStateFinished -= HandleSubStateFinished; // 하위 상태 이벤트 해제
            _currentSubState.Exit();
        }

        IsFinished = true;


        OnStateFinished?.Invoke(this);

    }

    private void Init()
    {

        if (StateData.IsSubState == true) _isSubState = true;
        else _isSubState = false;
    }


    // IProcessState 인터페이스 오브젝트 중 해당 서브 상태인 타겟 오브젝트 찾기
    private void FindTargetObject()
    {
        IProcessState[] allStates = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                            .OfType<IProcessState>()
                            .ToArray();

        foreach (IProcessState state in allStates)
        {
            if (state.StateData == _targetSubState)
            {
                _currentSubState = state;

                // 찾아온 서브 상태가 끝났을 때의 신호를 구독
                _currentSubState.OnStateFinished += HandleSubStateFinished;
                _currentSubState.Enter();
                break;
            }
        }
    }



    private void HandleSubStateFinished(IProcessState finishedSubState)
    {
        // 일을 마친 서브 상태의 이벤트 해제
        finishedSubState.OnStateFinished -= HandleSubStateFinished;

        _currentSubStateIndex++;

        // 서브 단계가 남아있다면 다음 단계 순차 진행 (1-1 -> 1-2)
        if (_currentSubStateIndex < StateData.subStates.Length)
        {
            _targetSubState = StateData.subStates[_currentSubStateIndex];
            FindTargetObject();
        }
        // 모든 서브 단계가 끝나면 메인 상태를 최종 종료
        else
        {
            Exit();
        }
    }
}
