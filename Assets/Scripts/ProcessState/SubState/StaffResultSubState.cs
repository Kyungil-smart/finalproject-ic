using System;
using UnityEngine;

public class StaffResultSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO StateData { get; private set; }   // 현재 상태

    [Header("상태 종료 여부")]
    [field: SerializeField] public bool IsFinished { get; private set; }    // 상태가 끝났는지 여부를 나타내는 프로퍼티

    // 부모의 상태 구분, 추후 메인 상태에서 주입해줘야
    // 프리 프로덕션 ~ 플 프로덕션의 이벤트 형식이 모두 동일해서 필요
    [field: SerializeField] public EMainState ParentState { get; private set; }

    public event Action<IProcessState> OnStateFinished;

    public void Enter()
    {
        IsFinished = false;


        Debug.Log("[StaffAssignmentSubState] : 4-2 ~ 9~2 상태 진입");

        Debug.Log("[StaffAssignmentSubState] : 4-2 ~ 9~2 지표값 산출 ~ 결과 학인");

        Execute();
    }

    public void Execute()
    {
        

        Exit();
    }

    public void Exit()
    {
        IsFinished = true;
        OnStateFinished?.Invoke(this);
    }
}
