using Cysharp.Threading.Tasks;
using DataDispatcher;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 직원 관리
/// </summary>
public class T01HumanResourceRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    private StaffSummaryRenderData _totalCandidateStaffs; // 선택 가능한 모든 스태프
    private StaffSummaryRenderData _selectedStaffs;  // 선택된 스태프
    private List<int> _selectedStaffIdxs = new();  // UI에 콜백을 보내기 위한 List<int> 타입의 인스턴스 변수
    private bool _endProcess;
    private bool _conditionGoback;

    // 후보 리스트 생성 및 변환 -> 직원 리스트 확인 -> 기존 직원과 리스트 통합 -> 계약할 캐릭터 선택 -> 계약 및 채용 확정 -> 채용 진행 애니메이션 -> 채용 확인
    public async UniTask Execute()
    {
        _endProcess = false;
        await MergeStaffList();
        UniTask.WaitUntil(() => _endProcess);
    }

    // TODO: 임시로 이곳에 넣음, 추후 Enter로 이동 필요
    private async UniTask<List<StaffViewData>> CreateCandidateList()
    {
        var gameManager = ServiceLocater.Get<IGameManager>();
        var staffManager = ServiceLocater.Get<IStaffRecruit>();
        await staffManager.GenerateRecruitCandidatesAsync(playerLevel: gameManager.PlayerLevel.CurrentValue, cardCount: 8);
        return staffManager.GetAvailableStaffList();
    }

    private List<StaffViewData> CheckStaffList()
    {
        return ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
    }

    private async UniTask MergeStaffList()
    {
        _waiting = true;
        await UniTask.Yield();
        if (_totalCandidateStaffs == null)
        {
            List<StaffViewData> candidateList = await CreateCandidateList();
            List<StaffViewData> staffList = CheckStaffList();
            _totalCandidateStaffs = new StaffSummaryRenderData();
            _totalCandidateStaffs.staffSummaryData = new List<StaffSummaryData>();
            foreach (var item in staffList)
            {
                StaffSummaryData staffSummaryData = new StaffSummaryData();
                staffSummaryData.selected = false;
                staffSummaryData.hired = true;
                staffSummaryData.viewData = item;

                _totalCandidateStaffs.staffSummaryData.Add(staffSummaryData);
            }

            foreach (var item in candidateList)
            {
                StaffSummaryData staffSummaryData = new StaffSummaryData();
                staffSummaryData.selected = false;
                staffSummaryData.hired = false;
                staffSummaryData.viewData = item;

                _totalCandidateStaffs.staffSummaryData.Add(staffSummaryData);
            }
            await UniTask.Yield();
            var tail = new StaffSummaryTailData() { num = 1, confirmCallback = SelectedStaffCallback };
            _totalCandidateStaffs.tailType = tail;
            _totalCandidateStaffs.selectable = true;
        }
        else
        {
            foreach (int idx in _selectedStaffIdxs)
                _totalCandidateStaffs.staffSummaryData[idx].selected = true;
        }
        await UniTask.Yield();
        Debug.Log($"[T01] _totalCandidateStaffs = {_totalCandidateStaffs.staffSummaryData.Count}");
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.StaffCandidateUI, _totalCandidateStaffs);
        await WaitProcess();
        await CheckHireStaffs();
    }

    private async UniTaskVoid SelectedStaffCallback(List<int> staffs)
    {
        _waiting = false;
        _selectedStaffIdxs = staffs;
    }

    public async UniTask CheckHireStaffs()
    {
        _waiting = true;
        _selectedStaffs = new StaffSummaryRenderData();
        _selectedStaffs.staffSummaryData = new List<StaffSummaryData>();
        foreach (var idx in _selectedStaffIdxs)
            _selectedStaffs.staffSummaryData.Add(_totalCandidateStaffs.staffSummaryData[idx]);
        
        var tail = new StaffSummaryTailData()
        {
            num = 2, 
            nextCallback = GoCheckHireStaffsToWaitingAnimation, 
            previousCallback = GoCheckHireStaffsToMergeStaffList
        };
        _selectedStaffs.tailType = tail;
        _selectedStaffs.selectable = false;
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.StaffCandidateUI, _selectedStaffs);
        await WaitProcess();
        if (_conditionGoback) await MergeStaffList();
        else await Prcessing();
    }

    private async UniTaskVoid GoCheckHireStaffsToWaitingAnimation()
    {
        _waiting = false;
        _conditionGoback = false;
        
    }
    
    private async UniTaskVoid GoCheckHireStaffsToMergeStaffList()
    {
        _waiting = false;
        _conditionGoback = true;
    }

    private async UniTask Prcessing()
    {
        _waiting = true;
        // ToDO. Animation 이 들어올 경우 대비 해야함.
        var data = new SimpleUIRenderData(9900020, 9900007, GoProcess);
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcessSimpleUI, data);
        foreach (var staff in _totalCandidateStaffs.staffSummaryData)
        {
            if (staff.selected && !staff.hired) // 고용 
                await ServiceLocater.Get<IStaffRecruit>().ConfirmHireAsync(staff.viewData.Staff_ID);
            else if (!staff.selected && staff.hired) // 해고
                ServiceLocater.Get<IStaffHireService>().FireStaff(staff.viewData.Staff_ID);
        }
        await WaitProcess();
        await CheckHiring();
    }

    private void GoProcess()
    {
        _waiting = false;
    }

    private async UniTask CheckHiring()
    {
        _waiting = true;
        var tail = new StaffSummaryTailData()
        {
            num = 3, 
            nextCallback = GoToNextProcess,
        };
        StaffSummaryRenderData sd = new();
        sd.staffSummaryData = new();
        foreach (var staff in ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList())
            sd.staffSummaryData.Add(new StaffSummaryData() { viewData = staff });   
        await UniTask.Yield();
        sd.tailType = tail;
        sd.selectable = false;
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.StaffCandidateUI, sd);
        await WaitProcess();
    }

    private async UniTaskVoid GoToNextProcess()
    {
        _waiting = false;
        _endProcess = true;
    }
}