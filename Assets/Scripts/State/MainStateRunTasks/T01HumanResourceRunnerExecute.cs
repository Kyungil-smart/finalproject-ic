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
    private List<int> _selectedStaffIdxs = new();  // UI에 콜백을 보내기 위한 List<int> 타입의 인스턴스 변수
    private bool _endProcess;
    private bool _conditionGoback;
    private List<StaffViewData> _candidateList;

    // 후보 리스트 생성 및 변환 -> 직원 리스트 확인 -> 기존 직원과 리스트 통합 -> 계약할 캐릭터 선택 -> 계약 및 채용 확정 -> 채용 진행 애니메이션 -> 채용 확인
    public async UniTask Execute()
    {
        _endProcess = false;
        await CreateNewProject();
        await UniTask.WaitUntil(() => _endProcess);
    }

    private async UniTask CreateNewProject()
    {
        var projectManager = ServiceLocater.Get<IProjectManager>();
        projectManager.NewProject();
        _candidateList = await CreateCandidateList();
        await MergeStaffList();
    }

    private async UniTask<List<StaffViewData>> CreateCandidateList()
    {
        // 프로젝트 생성부터 시작.
        ServiceLocater.Get<IProjectManager>().NewProject();
        
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

        foreach (var item in _candidateList)
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
        StaffSummaryRenderData selSsrd = new () { staffSummaryData = new List<StaffSummaryData>() };
        foreach (var idx in _selectedStaffIdxs)
        {
            selSsrd.staffSummaryData.Add(_totalCandidateStaffs.staffSummaryData[idx]);
            _totalCandidateStaffs.staffSummaryData[idx].selected = true;
        }
        
        var tail = new StaffSummaryTailData()
        {
            num = 2, 
            nextCallback = GoCheckHireStaffsToWaitingAnimation, 
            previousCallback = GoCheckHireStaffsToMergeStaffList
        };
        selSsrd.tailType = tail;
        selSsrd.selectable = false;
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.StaffCandidateUI,selSsrd);
        await WaitProcess();
        if (_conditionGoback) await MergeStaffList();
        else await RecruitProcessing();
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

    private async UniTask RecruitProcessing()
    {
        _waiting = true;
        // ToDO. Animation 추가 작업 필요.
        
        var data = new ProgressAnimationRenderData()
        {
            staticImageId = null,
            progressTexts = new() { "기존 직원 해고", "새 직원 고용", "사무실 자리 셋팅", "Welcome 킷 제공" },
            callback = GoProcess,
        };
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcAnimationUI, data);
        foreach (var staff in _totalCandidateStaffs.staffSummaryData)
        {
            Debug.Log($"[T01] {staff.viewData.Staff_Name} - h;{staff.hired}|s;{staff.selected}");
            if (!staff.selected && staff.hired) // 해고
            {
                Debug.Log($"[T01] {staff.viewData.Staff_ID} {staff.viewData.Staff_Name} 해고");
                await ServiceLocater.Get<IStaffHireService>().FireStaff(staff.viewData.Staff_ID);
            }
            await UniTask.Yield();
                
            if (staff.selected && !staff.hired) // 고용 
            {
                Debug.Log($"[T01] {staff.viewData.Staff_ID} {staff.viewData.Staff_Name} 고용");
                await ServiceLocater.Get<IStaffRecruit>().ConfirmHireAsync(staff.viewData.Staff_ID, false);
            }
            await UniTask.Yield();
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