using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class QAMiniGameSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;


    public void Enter()
    {
        Debug.Log($"[QAMiniGameSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
        // TODO : 10. QA 시작
    }

    public async UniTask Execute()
    {
        StartQAMiniGame();
        CalculateResult();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
        // TODO : 10. QA 종료
    }

    public void StartQAMiniGame()
    {
        Debug.Log($"[QAMiniGameSubState] : QA 미니게임 시작");
        // TODO : 작업 진행 (미니게임 또는 랜덤)
    }

    public void CalculateResult()
    {
        Debug.Log($"[QAMiniGameSubState] : 보정 결과 계산");
        // TODO : 퀄리티 보정 계수 산출
    }
}