using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;


public class ReleaseAwardsSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;


    public void Enter()
    {
        Debug.Log($"[ReleaseAwardsSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
    }

    public async UniTask Execute()
    {
        CalculateAwards();
        CheckAwards();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
    }

    public void CalculateAwards()
    {
        Debug.Log($"[ReleaseAwardsSubState] : 수상 계산");
        // TODO : 수상 계산 로직
    }

    public void CheckAwards()
    {
        Debug.Log($"[ReleaseAwardsSubState] : 수상 내역 확인");
        // TODO : 수상 내역 확인 로직
    }
}