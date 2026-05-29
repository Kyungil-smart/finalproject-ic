using System;
using UnityEngine;

public class MarketingSelectionSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO StateData { get; private set; }   // 현재 상태

    [Header("상태 종료 여부")]
    [field: SerializeField] public bool IsFinished { get; private set; }    // 상태가 끝났는지 여부를 나타내는 프로퍼티

    public event Action<IProcessState> OnStateFinished;


    public void Enter()
    {
        IsFinished = false;


        Debug.Log("[MarketingSelectionSubState] : 11-1 상태 진입");

        Execute();
    }

    public void Execute()
    {
        Debug.Log("[MarketingSelectionSubState] : 11-1 마케팅 방법 진행");

        Exit();
    }

    public void Exit()
    {
        IsFinished = true;
        OnStateFinished?.Invoke(this);
    }
}
