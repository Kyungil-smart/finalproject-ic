using System;
using UnityEngine;

public class MarketTrandSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO StateData { get; private set; }   // 현재 상태

    [Header("상태 종료 여부")]
    [SerializeField] private bool _isFinished;    // 상태가 끝났는지 여부


    public event Action<IProcessState> OnStateFinished;

    public void Enter()
    {
        _isFinished = false;

        Debug.Log("[MarketTrandSubState] : 2-2 상태 진입");

        Execute();
    }

    public void Execute()
    {
        Debug.Log("[MarketTrandSubState] : 2-2 트랜드 장르 및 테마 확인 진행");

        Exit();
    }

    public void Exit()
    {
        _isFinished = true;
        OnStateFinished?.Invoke(this);
    }
}
