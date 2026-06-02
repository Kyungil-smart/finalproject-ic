using System;
using UnityEngine;
using System.Collections;



public class MainState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;




    public void Enter()
    {
        Debug.Log($"[MainStateNew] : {CurrentStateDataSO.StateName} 상태 진입");
        Execute();
    }

    public void Execute()
    {

        // Exit();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
    }

    // Enter 시 자신의 상태를 변경
    public void ChangeMyState(ProcessStateSO newStateSO)
    {
        CurrentStateDataSO = newStateSO;
    }

    // 외부에서 호출 시 대상에 상태 전달
    public ProcessStateSO ChangeMachineState()
    {
        return CurrentStateDataSO.nextState;
    }



}
