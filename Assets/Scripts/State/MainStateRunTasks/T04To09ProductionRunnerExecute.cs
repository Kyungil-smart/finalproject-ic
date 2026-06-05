using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class T04To09ProductionRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    
    public async UniTask Execute()
    {
        SelectStaff();
        InputStaff();
        OccurStaffEvent();
        WorkOnDevelopment();
        OccurRewardEvent();
        ViewResult();
    }

    private void SelectStaff()
    {
        Debug.Log($"[StaffAssignmentSubState] : 직원 선택");
        //TODO : 작업에 투입할 직원 선택
    }

    private void InputStaff()
    {
        Debug.Log($"[StaffAssignmentSubState] : 직원 투입");
        //TODO : 선택한 직원 투입
    }

    private void OccurStaffEvent()
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