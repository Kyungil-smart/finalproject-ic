using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class T04To09ProductionRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    T0409ProductionStaffListRenderData _t0409ProductionStaffListRenderData;
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
            _t0409ProductionStaffListRenderData = new T0409ProductionStaffListRenderData();
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
        await CheckSelectedLeaders();
    }

    private async void SelectedStaffCallback(List<int> staffs)
    {
        _waiting = false;
        _selectedStaffIdxs = staffs;
    }


    public async UniTask CheckSelectedLeaders()
    {
        _waiting = true;
        T0409ProductionLeaderResultRenderData selectedStaffs = new T0409ProductionLeaderResultRenderData() { leaderList = new List<StaffViewData>() };
        foreach (var idx in _selectedStaffIdxs)
        {
            selectedStaffs.leaderList.Add(_t0409ProductionStaffListRenderData.staffList[idx].viewData);
            _t0409ProductionStaffListRenderData.staffList[idx].selected = true;
        }

        selectedStaffs.onGoBackCallback = GoCheckSelectLeadersToCreateStaffList;
        selectedStaffs.onGoNextCallback = GoCheckSelectLeadersToWaitingAnimation;

        await UniTask.Yield();
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.StaffCandidateUI, selectedStaffs);
        await WaitProcess();
        if (_conditionGoback) await CreateStaffList();
        else await SelectLeaderProcessing();
    }

    private async void GoCheckSelectLeadersToCreateStaffList()
    {
        _waiting = false;
        _conditionGoback = true;
    }

    private async void GoCheckSelectLeadersToWaitingAnimation()
    {
        _waiting = false;
        _conditionGoback = false;

    }

    private async UniTask SelectLeaderProcessing()
    {
        _waiting = true;
        // ToDO. Animation 이 들어올 경우 대비 해야함.
        var data = new SimpleUIRenderData(9900020, 9900007, GoProcess);
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcessSimpleUI, data);

        // 리더 되는 프로세스



        await UniTask.Yield();
        await WaitProcess();
        await EmitResultEvent();
    }

    private void GoProcess()
    {
        _waiting = false;
    }

    // 직원간 이벤트 발생


    // T06 (프리 프로덕션의 마지막 단계) 에서만 발생하는 이벤트
    private async UniTask EmitResultEvent()
    {

    }




    private async UniTask CheckResult()
    {

    }




    // 마지막에 다음 프로세스 상태로 가기 위한 기능
    private async UniTask GoToNextProcess()
    {
        _waiting = false;
        _endProcess = true;
    }
}