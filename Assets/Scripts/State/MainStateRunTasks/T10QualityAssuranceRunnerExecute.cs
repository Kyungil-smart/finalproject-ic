using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class T10QualityAssuranceRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    public async UniTask Execute()
    {
        StartQAMiniGame();
        CalculateResult();
    }

    private void StartQAMiniGame()
    {
        Debug.Log($"[QAMiniGameSubState] : QA 미니게임 시작");
        // TODO : 작업 진행 (미니게임 또는 랜덤)
    }

    private void CalculateResult()
    {
        Debug.Log($"[QAMiniGameSubState] : 보정 결과 계산");
        // TODO : 퀄리티 보정 계수 산출
    }
}