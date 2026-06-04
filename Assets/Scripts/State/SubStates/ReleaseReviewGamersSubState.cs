using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ReleaseReviewGamersSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;


    public void Enter()
    {
        Debug.Log($"[ReleaseReviewGamersSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
        // TODO : 12. 출시 시작
    }

    public async UniTask Execute()
    {
        ReviewGamers();
        ReviewCritics();
        WorkOnProcess();
        CalculateRevenue();
        NominatedAwards();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
        // TODO : 12. 출시 종료
    }

    public void ReviewGamers()
    {
        Debug.Log($"[ReleaseReviewGamersSubState] : 게이머 리뷰 진행");
        // TODO : 유저 반응 확인
    }

    public void ReviewCritics()
    {
        Debug.Log($"[ReleaseReviewGamersSubState] : 평론가 리뷰 진행");
        // TODO : 평론가 반응 확인
    }

    public void WorkOnProcess()
    {
        // TODO : 작업 진행 페이즈
    }

    public void CalculateRevenue()
    {
        // TODO : 매출 발생
    }

    public void NominatedAwards()
    {
        // TODO : 어워즈 선정
    }
}