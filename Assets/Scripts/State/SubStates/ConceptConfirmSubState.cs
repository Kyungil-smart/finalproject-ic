using System;
using UnityEngine;

public class ConceptConfirmSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;




    public void Enter()
    {
        Debug.Log($"[ConceptConfirmSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
    }

    public void Execute()
    {

    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
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