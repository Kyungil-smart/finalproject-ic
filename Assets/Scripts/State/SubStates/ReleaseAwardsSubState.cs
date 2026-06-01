using System;
using UnityEngine;
using System.Collections;


public class ReleaseAwardsSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;




    public void Enter()
    {
        Debug.Log($"[ReleaseAwardsSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
        Execute();
    }

    public void Execute()
    {
        StartCoroutine(Wait1SecondsRoutine(0.5f));
        // Exit();  // 코루틴을 위해 임시로 주석 처리
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
    }



    // 메모리 누수 확인 용 지연 코루틴 (한 번에 모든 순환이 작동하면 판단하기 힘들어 추가)
    IEnumerator Wait1SecondsRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Debug.Log($"{seconds}초 경과");

        Exit();
    }



    // 아래는 서브 스테이트에서 현재 사용 안하나 확장 가능성 + 인터페이스 고려해 놔둠
    // Enter 시 자신의 상태를 변경
    public void ChangeMyState(ProcessStateSO newStateSO)
    {
        // CurrentStateDataSO = newStateSO;
    }

    // 외부에서 호출 시 대상에 상태 전달
    public ProcessStateSO ChangeMachineState()
    {
        return null;
    }
}