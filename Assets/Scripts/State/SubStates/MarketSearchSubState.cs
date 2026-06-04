using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class MarketSearchSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;




    public void Enter()
    {
        Debug.Log($"[MarketSearchSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
        // TODO : 2. 시장 조사 시작
    }

    public async UniTask Execute()
    {
        ResearchTrend();
        CheckTrend();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
        // TODO : 2. 시장 조사 종료
    }

    public void ResearchTrend()
    {
        Debug.Log($"[MarketSearchSubState] : 트렌드 조사");
        //TODO : 트렌드 탐색
    }

    public void CheckTrend()
    {
        Debug.Log($"[MarketSearchSubState] : 트렌드 조사");
        //TODO : 트렌드 장르와 테마 확인
    }
}