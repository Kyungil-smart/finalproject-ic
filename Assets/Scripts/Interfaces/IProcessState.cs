using System;
using System.Runtime.InteropServices;
using UnityEngine;

public interface IProcessState
{
    [Header("이 상태의 SO")]
    public ProcessStateSO CurrentStateDataSO { get; }   // 현재 상태

    // [Header("상태 종료 여부")]
    // private bool _isFinished;    // 상태가 끝났는지 여부를 나타내는 프로퍼티

    public event Action<IProcessState> OnStateFinished;


    public void Enter();
    public void Execute();
    public void Exit();


    public void ChangeMyState(ProcessStateSO newState); // 자신의 상태를 변경하는 기능, 상태 머신에서 호출함

    public ProcessStateSO ChangeMachineState(); // 상태 머신에게 다음 상태를 알려주는 기능
}
