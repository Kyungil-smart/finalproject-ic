using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// 시장 조사
/// </summary>
public class T02MarketResearchRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    public async UniTask Execute()
    {
        ResearchTrend();
        CheckTrend();
    }

    private void ResearchTrend()
    {
        Debug.Log($"[MarketSearchSubState] : 트렌드 조사");
        //TODO : 트렌드 탐색
    }

    private void CheckTrend()
    {
        Debug.Log($"[MarketSearchSubState] : 트렌드 조사");
        //TODO : 트렌드 장르와 테마 확인
    }
}