using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class StaffAssignmentSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    private GameDevProcName _currentProcName;


    public event Action<IProcessState> OnStateFinished;


    public void Enter()
    {
        Debug.Log($"[StaffAssignmentSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
        // TODO : 4 ~ 9. 프로덕션 단계 시작
    }

    public async UniTask Execute()
    {
        SelectStaff();
        InputStaff();
        OccurStaffEvent();
        WorkOnDevelopment();
        OccurRewardEvent();
        ViewResult();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
        // TODO : 4 ~ 9. 프로덕션 단계 종료
    }

    public void SelectStaff()
    {
        Debug.Log($"[StaffAssignmentSubState] : 직원 선택");
        //TODO : 작업에 투입할 직원 선택
    }

    public void InputStaff()
    {
        Debug.Log($"[StaffAssignmentSubState] : 직원 투입");
        //TODO : 선택한 직원 투입
    }

    public void OccurStaffEvent()
    {      
        Debug.Log($"[StaffAssignmentSubState] : 직원 간 상호작용 이벤트 발생");
        //TODO : 직원 간 상호작용 이벤트 발생
    }

    public void WorkOnDevelopment()
    {
        //TODO : 작업 진행
    }

    public void OccurRewardEvent()
    {       
        Debug.Log($"[StaffAssignmentSubState] : 지표 달성 이벤트 시작");
        //TODO : 지표값 산출
    }

    public void ViewResult()
    {
        Debug.Log($"[StaffAssignmentSubState] : 지표 달성 이벤트 시작");
        //TODO : 결과 확인
    }
}