using System;
using UnityEngine;
using System.Collections;



public class StaffHireSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;


    // 이벤트 데이터 자체가 아닌 어떤 이벤트 발생해야할지만 가지고 있어야함 -> 실제 발동은 이벤트 매니저에서

    public void Enter()
    {
        Debug.Log($"[StaffHireSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
    }

    public void Execute()
    {

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