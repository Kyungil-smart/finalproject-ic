using System;
using UnityEngine;

public class MarketingResultSubState : MonoBehaviour, IProcessState




{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO StateData { get; private set; }   // 현재 상태

    [Header("상태 종료 여부")]
    [field: SerializeField] public bool IsFinished { get; private set; }    // 상태가 끝났는지 여부를 나타내는 프로퍼티

    public event Action<IProcessState> OnStateFinished;

    // 아래는 이 서브 상태에서만 사용하는 함수
    // 부모의 상태 구분, 추후 메인 상태에서 주입해줘야
    // 프리 프로덕션 ~ 플 프로덕션의 이벤트 형식이 모두 동일해서 필요
    [field: SerializeField] public EMainState ParentState { get; private set; }

    public void Enter()
    {
        IsFinished = false;


        Debug.Log("[MarketingSelectionSubState] : 11-2 상태 진입");

        Execute();
    }

    public void Execute()
    {
        Debug.Log("[MarketingSelectionSubState] : 11-2 마케팅 결과 진행");

        Exit();
    }

    public void Exit()
    {
        IsFinished = true;
        OnStateFinished?.Invoke(this);
    }
}
