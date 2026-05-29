using System;
using UnityEngine;
using System.Collections;


public class ReleaseAwardsSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO StateData { get; private set; }   // 현재 상태

    [Header("상태 종료 여부")]
    [field: SerializeField] public bool IsFinished { get; private set; }    // 상태가 끝났는지 여부를 나타내는 프로퍼티

    public event Action<IProcessState> OnStateFinished;

    public void Enter()
    {
        IsFinished = false;

        Debug.Log("[StaffManagingState] : 12-4 상태 진입");

        Execute();
    }

    public void Execute()
    {
        Debug.Log("[ReleaseReviewGamersSubState] : 12-4 어워즈 판정 확인 진행");

        StartCoroutine(Wait1SecondRoutine());

        // Exit();
    }

    public void Exit()
    {
        IsFinished = true;
        OnStateFinished?.Invoke(this);
    }


    // 메모리 누수 확인 용 지연 코루틴 (한 번에 모든 순환이 작동하면 판단하기 힘들어 추가)
    IEnumerator Wait1SecondRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("0.5초 경과");

        Exit();
    }
}
