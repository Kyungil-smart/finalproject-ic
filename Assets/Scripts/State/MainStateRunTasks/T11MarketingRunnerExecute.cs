using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class T11MarketingRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    public async UniTask Execute()
    {
        SelectMarketing();
        WorkOnMarketing();
        FinishMarketing();
    }

    private void SelectMarketing()
    {
        Debug.Log($"[MarketingSelectionSubState] : 마케팅 선택");
        // TODO : 마케팅 방식 선택
    }

    private void WorkOnMarketing()
    {
        // TODO : 작업 진행 페이즈
    }

    private void FinishMarketing()
    {
        Debug.Log($"[MarketingSelectionSubState] : 마케팅 완료 및 산출");
        // TODO : 마케팅 완료
    }

    private void CalculateMarketingResult()
    {
        Debug.Log($"[MarketingSelectionSubState] : 마케팅 완료 및 산출");
        // TODO : 마케팅 효과 산출 및 저장
    }
}