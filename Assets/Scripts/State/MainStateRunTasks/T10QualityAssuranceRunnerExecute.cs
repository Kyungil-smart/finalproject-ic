using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class T10QualityAssuranceRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    public async UniTask Execute()
    {
        await StartQAMiniGame();
        CalculateResult();
    }

    private async UniTask StartQAMiniGame()
    {
        Debug.Log($"[QAMiniGameSubState] : QA 미니게임 시작");
        await SceneManager.LoadSceneAsync("MinigameScene").ToUniTask();
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name == "ProcessScene");
    }

    private void CalculateResult()
    {
        Debug.Log($"[QAMiniGameSubState] : 보정 결과 계산");
        var qm = ServiceLocater.Get<IQualityManager>();
        qm.Calculator.ApplyTotalQualityWithQAResult();
    }
}