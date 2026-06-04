using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class MarketingSelectionSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;


    public void Enter()
    {
        Debug.Log($"[MarketingSelectionSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
        // TODO : 11. 마케팅 시작
    }

    public async UniTask Execute()
    {
        SelectMarketing();
        WorkOnMarketing();
        FinishMarketing();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
        // TODO : 11. 마케팅 종료
    }

    public void SelectMarketing()
    {
        Debug.Log($"[MarketingSelectionSubState] : 마케팅 선택");
        // TODO : 마케팅 방식 선택
    }

    public void WorkOnMarketing()
    {
        // TODO : 작업 진행 페이즈
    }

    public void FinishMarketing()
    {
        Debug.Log($"[MarketingSelectionSubState] : 마케팅 완료 및 산출");
        // TODO : 마케팅 완료
    }

    public void CalculateMarketingResult()
    {
        Debug.Log($"[MarketingSelectionSubState] : 마케팅 완료 및 산출");
        // TODO : 마케팅 효과 산출 및 저장
    }
}