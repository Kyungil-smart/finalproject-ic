using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class T04To09ProductionRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    T0409ProductionStaffListRenderData _t0409ProductionStaffListRenderData = new T0409ProductionStaffListRenderData();
    private List<int> _selectedStaffIdxs = new();  // UI에 콜백을 보내기 위한 List<int> 타입의 인스턴스 변수

    private bool _endProcess;
    private bool _conditionGoback;

    // 직원 리스트 만들어 보내기 -> (유저의 선택 대기) -> 프로덕션 2: 스태프에서 리더 2명 선택 -> 프로덕션 4: 선택한 리더 2명 확인하기
    //   -> 직원 간 상호작용 이벤트 실행 -> 프로덕션 5: 진행 애니메이션 대기 -> 지표 결과 이벤트 발생(프리 프로덕션 마지막에만 1번) -> 프로덕션 6: 지표 결과 출력하기
    public async UniTask Execute()
    {
        _endProcess = false;
        await CreateStaffList();
        await UniTask.WaitUntil(() => _endProcess);
    }

    private async UniTask CreateStaffList()
    {
        _waiting = true;
        await UniTask.Yield();
        if (_t0409ProductionStaffListRenderData == null)
        {
            List<StaffViewData> curStaffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList(); // 매니저에 저장된 직원 리스트        
            _t0409ProductionStaffListRenderData.staffList = new List<StaffSummaryData>(); // 랜더용 직원 리스트

            foreach (var item in curStaffList)
            {
                StaffSummaryData staffSummaryData = new StaffSummaryData();
                staffSummaryData.selected = false;
                staffSummaryData.hired = true;
                staffSummaryData.viewData = item;

                _t0409ProductionStaffListRenderData.staffList.Add(staffSummaryData);
            }
            await UniTask.Yield();
            _t0409ProductionStaffListRenderData.onSelectCallback = SelectedStaffCallback;
        }
        else
        {
            foreach (int idx in _selectedStaffIdxs)
                _t0409ProductionStaffListRenderData.staffList[idx].selected = true;
        }
        await UniTask.Yield();
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.StaffCandidateUI, _t0409ProductionStaffListRenderData);
        await WaitProcess();
        await CheckSelectedStaff();
    }

    private async void SelectedStaffCallback(List<int> staffs)
    {
        _waiting = false;
        _selectedStaffIdxs = staffs;
    }


    public async UniTask CheckSelectedStaff()
    {
        _waiting = true;
        T0409ProductionLeaderResultRenderData selectedStaffs = new T0409ProductionLeaderResultRenderData() { leaderList = new List<StaffViewData>() };
        foreach (var idx in _selectedStaffIdxs)
        {
            selectedStaffs.leaderList.Add(_t0409ProductionStaffListRenderData.staffList[idx]);
            _t0409ProductionStaffListRenderData.staffList[idx].selected = true;
        }
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





    // 마지막에 다음 프로세스 상태로 가기 위한 기능
    private async UniTask GoToNextProcess()
    {
        _waiting = false;
        _endProcess = true;
    }
}