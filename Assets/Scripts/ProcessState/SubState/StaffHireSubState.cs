using System;
using UnityEngine;

public class StaffHireSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO StateData { get; private set; }   // 현재 상태

    [Header("상태 종료 여부")]
    [SerializeField] private bool _isFinished;    // 상태가 끝났는지 여부


    public event Action<IProcessState> OnStateFinished;

    public void Enter()
    {
        _isFinished = false;
        Debug.Log("[StaffManagingState] : 1-1 상태 진입");
        Execute();
    }

    public void Execute()
    {
        Debug.Log("[StaffManagingState] : 보유 직원 리스트 및 채용 진행");

        Exit();
    }

    public void Exit()
    {
        _isFinished = true;
        OnStateFinished?.Invoke(this);
    }
}
