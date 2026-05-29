using System;
using UnityEngine;

public class QAMiniGameSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO StateData { get; private set; }   // 현재 상태

    [Header("상태 종료 여부")]
    [field: SerializeField] public bool IsFinished { get; private set; }    // 상태가 끝났는지 여부를 나타내는 프로퍼티

    public event Action<IProcessState> OnStateFinished;

    // 아래는 이 서브 상태에서만 사용하는 함수
    private bool doMinigame = true;

    public void Enter()
    {
        IsFinished = false;

        Debug.Log("[QAMiniGameSubState] : 10-1 상태 진입");

        Execute();
    }


    public void Execute()
    {
        
        if(doMinigame)
        {
            Debug.Log("[QAMiniGameSubState] : 10-1 미니게임 실행");
        }
        else
        {
            // 미니 게임 실행 안함
        }
        

        Debug.Log("[QAMiniGameSubState] : 10-1 미니게임 결과 포함 결과 산출");

        Exit();
    }

    public void Exit()
    {
        
        IsFinished = true;
        OnStateFinished?.Invoke(this);
    }
}

