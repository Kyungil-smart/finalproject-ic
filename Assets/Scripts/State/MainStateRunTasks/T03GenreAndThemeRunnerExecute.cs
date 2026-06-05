using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class T03GenreAndThemeRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    public async UniTask Execute()
    {
        SelectTheme();
    }

    private void SelectTheme()
    {
        Debug.Log($"[ConceptConfirmSubState] : 테마 및 장르 결정 시작");
        //TODO : 제작할 게임의 테마 및 장르 결정
    }
}