using System.Linq;
using UnityEngine;

public class ProcessStateMachine : MonoBehaviour
{
    [Header("첫 상태")]
    [SerializeField] private ProcessStateSO startState;

    [field: SerializeField] public ProcessStateSO CurrentMainState { get; private set; }    // 현재 실행중인 메인 상태

    private IProcessState currentState;    // 받을 상태의 행동

    private void Start()
    {
        Init();
    }

    /*
    private void Update()
    {
        // 현재 메인 상태의 Execute 실행
        currentState?.Execute();
    }
    */

    private void Init()
    {
        if (startState != null)
        {
            ChangeState(startState);
        }
        else
        {
            Debug.LogError("[DevStateMachine] : startState 없음!");
        }
    }

    /*
    // 조건 판단 후 다음 상태로 넘어가기
    private void GoAheadNextState()
    {
        
        if (CurrentMainState.nextState != null)
        {
            ChangeState(CurrentMainState.nextState);
        }
        else
        {
            // TODO : 게임 엔딩 시 종료 처리 추가 필요
        }
    }
    */


    // 실제 상태 변경 기능
    public void ChangeState(ProcessStateSO nextStateSO)
    {
        // Debug.Log(currentState);
        if (nextStateSO != null)
        {
            if (currentState != null)
            {
                // 기존 연결 해제
                currentState.OnStateFinished -= HandleMainStateFinished;
                currentState.Exit();
            }

            CurrentMainState = nextStateSO; // 상태 정보 업데이트

            currentState = null;

            // 하이라키에서 찾기 -> 인터페이스로 바꿔야 함
            IProcessState[] allStates = GetComponentsInChildren<MonoBehaviour>()
                            .OfType<IProcessState>()
                            .ToArray();


            foreach (IProcessState state in allStates)
            {
                

                if (state.StateData == nextStateSO)
                {
                    currentState = state;
                    break;
                }
            }

            /*
            Debug.Log(currentState);
            currentState?.Enter();  // 다음 현재 상태 입장 기능
            */

            if (currentState != null)
            {
                // 함수 등록
                currentState.OnStateFinished += HandleMainStateFinished;
                currentState.Enter();
            }
        }
    }


    private void HandleMainStateFinished(IProcessState finishedState)
    {
        Debug.Log($"[DevStateMachine] : {finishedState.StateData.StateName} 메인 단계 완료 신호 수신");

        // 기획서에 명시된 다음 메인 단계(SO)가 존재한다면 상태를 전환합니다.
        if (finishedState.StateData.nextState != null)
        {
            ChangeState(finishedState.StateData.nextState);
        }
    }

    // TODO : 모든 서브 행동이 완료 되었는지 확인



    // 업데이트(Excute)는 필요할까?? 
}
