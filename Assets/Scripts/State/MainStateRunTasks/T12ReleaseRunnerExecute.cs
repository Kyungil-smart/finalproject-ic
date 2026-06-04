using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class T12ReleaseRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    public async UniTask Execute()
    {
        ReviewGamers();
        ReviewCritics();
        WorkOnProcess();
        CalculateRevenue();
        NominatedAwards();
    }

    private void ReviewGamers()
    {
        Debug.Log($"[ReleaseReviewGamersSubState] : 게이머 리뷰 진행");
        // TODO : 유저 반응 확인
    }

    private void ReviewCritics()
    {
        Debug.Log($"[ReleaseReviewGamersSubState] : 평론가 리뷰 진행");
        // TODO : 평론가 반응 확인
    }

    private void WorkOnProcess()
    {
        // TODO : 작업 진행 페이즈
    }

    private void CalculateRevenue()
    {
        // TODO : 매출 발생
    }

    private void NominatedAwards()
    {
        // TODO : 어워즈 선정
    }
}