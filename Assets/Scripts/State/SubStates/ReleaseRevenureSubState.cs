using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ReleaseRevenureSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;


    public void Enter()
    {
        Debug.Log($"[ReleaseRevenureSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
        // TODO : 12. 출시 - 시작
    }

    public async UniTask Execute()
    {
        CalculateRevenue();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
        // TODO : 12. 출시 -  종료
    }

    public void CalculateRevenue()
    {
        Debug.Log($"[ReleaseRevenureSubState] : 매출 계산");
        // TODO : 매출 계산 로직
    }
}