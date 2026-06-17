using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


/// <summary>
/// 프로덕션(T04 ~ T09)
/// </summary>
public class T04To09ProductionRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    T0409ProductionStaffListRenderData _t0409ProductionStaffListRenderData;
    T0409ProductionLeaderResultRenderData selectedStaffs;
    private List<int> _selectedStaffIdxs = new();   // UI에 콜백을 보내기 위한 List<int> 타입의 인스턴스 변수
    GameDevProcName curGameDevProcName; // 현재 상태 T04 ~ T09 를 확인하기 위한 enum 변수

    private bool _endProcess;
    private bool _conditionGoback;

    // 직원 리스트 만들어 보내기 -> (유저의 선택 대기) -> 프로덕션 2: 스태프에서 리더 2명 선택 -> 프로덕션 4: 선택한 리더 2명 확인하기
    //   -> 직원 간 상호작용 이벤트 실행 -> 프로덕션 5: 진행 애니메이션 대기 -> 지표 결과 이벤트 발생(프리 프로덕션 마지막에만 1번) -> 프로덕션 6: 지표 결과 출력하기
    public async UniTask Execute()
    {
        _endProcess = false;
        curGameDevProcName = ServiceLocater.Get<IGameManager>().ProcName.CurrentValue;

        await GetStaffList();
        await UniTask.WaitUntil(() => _endProcess);
    }

    private async UniTask GetStaffList()
    {
        _waiting = true;
        await UniTask.Yield();
        if (_t0409ProductionStaffListRenderData == null)
        {
            _t0409ProductionStaffListRenderData = new T0409ProductionStaffListRenderData();
            List<StaffViewData> curStaffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList(); // 매니저에 저장된 직원 리스트        
            _t0409ProductionStaffListRenderData.staffList = new List<StaffSummaryData>(); // 랜더용 직원 리스트

            // 매니저에 저장된 직원을 랜더용으로 변환
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
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProductionUI, _t0409ProductionStaffListRenderData);
        await WaitProcess();
        await CheckSelectedLeaders();
    }

    private void SelectedStaffCallback(List<int> staffs)
    {
        _waiting = false;
        _selectedStaffIdxs = staffs;
    }

    // 선택된 리더들 체크 기능(뒤로가기 / 확정)
    public async UniTask CheckSelectedLeaders()
    {
        _waiting = true;
        selectedStaffs = new T0409ProductionLeaderResultRenderData() { leaderList = new List<StaffViewData>() };
        foreach (var idx in _selectedStaffIdxs)
        {
            selectedStaffs.leaderList.Add(_t0409ProductionStaffListRenderData.staffList[idx].viewData);
            _t0409ProductionStaffListRenderData.staffList[idx].selected = true;
        }

        selectedStaffs.onGoBackCallback = GoCheckSelectLeadersToCreateStaffList;
        selectedStaffs.onGoNextCallback = GoCheckSelectLeadersToWaitingAnimation;

        await UniTask.Yield();
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProductionUI, selectedStaffs);
        await WaitProcess();
        if (_conditionGoback) await GetStaffList();
        else await LeadersEvent();
    }

    private void GoCheckSelectLeadersToCreateStaffList()
    {
        _waiting = false;
        _conditionGoback = true;
    }

    private void GoCheckSelectLeadersToWaitingAnimation()
    {
        // 투입한 직원 ID 보내기
        ServiceLocater.Get<IProjectManager>().AssignStaff(curGameDevProcName, selectedStaffs.leaderList[0].Staff_ID);
        ServiceLocater.Get<IProjectManager>().AssignStaff(curGameDevProcName, selectedStaffs.leaderList[1].Staff_ID);
        Debug.Log($"직원 투입 : {selectedStaffs.leaderList[0].Staff_ID} : {selectedStaffs.leaderList[0].Staff_Name}, " +
            $"{selectedStaffs.leaderList[1].Staff_ID} : {selectedStaffs.leaderList[1].Staff_Name} ");

        _waiting = false;
        _conditionGoback = false;
    }

    // 직원간 이벤트 발생
    private async UniTask LeadersEvent()
    {
        _waiting = true;

        await ServiceLocater.Get<IEventManager>().OccurEvent(EventType.Staff);  // 직원간 이벤트 발생

        await UniTask.Yield();
        // await WaitProcess();
        await SelectLeaderProcessing();
    }

    // 프로덕션 애니메이션과 T06 여부 확인
    private async UniTask SelectLeaderProcessing()
    {
        _waiting = true;

        Debug.Log("SelectLeaderProcessing() 시작");

        // ToDO. Animation 이 들어올 경우 대비 해야함.
        var data = new SimpleUIRenderData(9900020, 9900007, GoProcess); // TODO : 애니메이션 적용 중 단순 텍스트 출력, ID 는 바꿔야 함
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcessSimpleUI, data);

        // 리더 되는 프로세스는 각 프로덕션 내에서만 저장되면 되기 때문에 매니저에 저장될 필요 없을 듯

        // 점수 계산 진행
        
        Debug.Log($"{curGameDevProcName} | {selectedStaffs.leaderList[0].Staff_Name} | {selectedStaffs.leaderList[1].Staff_Name}"); // 테스트용 임시 로그
        await UniTask.Yield();
        // Todo. 한번에 불러오는 시스템 만들어서 원래 있던거 지움.
        ServiceLocater.Get<IQualityManager>().Calculator.CalculateQuality(curGameDevProcName);
        await UniTask.Yield();
        // 배치된 스태프 경험치 주기 기능 추가 필요 (누적 되어야 함)
        // ServiceLocater.Get<IStaffDataManager>().GetExpList();    // TODO : Staff 경험치 주는 함수 수정되었는지 확인 필요
        List<int> staffList = new List<int>
        {
            selectedStaffs.leaderList[0].Staff_ID,
            selectedStaffs.leaderList[1].Staff_ID
        };
        ServiceLocater.Get<IStaffRegister>().GetExpInProduction(curGameDevProcName, staffList);  // 배치된 스텝 경험치 주기, 뒤에껀 배치된 스태프 ID를 리스트 int로 보내기

        await UniTask.Yield();
        await WaitProcess();

        // T06 이면 EmitResultEvent 로 가고, 아니면 CheckResult 로
        if (curGameDevProcName == GameDevProcName.DevelopmentPreProduction) await EmitResultEvent();
        else await CheckResult();
    }

    private void GoProcess()
    {
        _waiting = false;
    }


    // T06 (프리 프로덕션의 마지막 단계) 에서만 발생하는 이벤트
    private async UniTask EmitResultEvent()
    {
        _waiting = true;
        // Todo. 어디서 넣어야할지 몰라서 일단 Total을 여기서 넣어서 확인했습니다.
        ServiceLocater.Get<IQualityManager>().Calculator.CalculateTotal();
        Debug.Log($"[PreProduction] 계산 완료 달성률 : {ServiceLocater.Get<IQualityManager>().Calculator.CalculateAchieve()}");
        Debug.Log("EmitResultEvent() 시작");

        await ServiceLocater.Get<IEventManager>().OccurEvent(EventType.Reward); // 지표 이벤트 발생
        
        await UniTask.Yield();
        // await WaitProcess();
        await GoProcessForT06();
        await CheckResult();
    }

    private UniTask GoProcessForT06()
    {
        _waiting = false;
        return UniTask.CompletedTask;
    }


    private async UniTask CheckResult()
    {
        _waiting = true;

        Debug.Log("CheckResult() 시작");

        GameDevProcName curGameDevProcName = ServiceLocater.Get<IGameManager>().ProcName.CurrentValue;

        if (curGameDevProcName == GameDevProcName.DevelopmentPreProduction)
        {
            // TODO : T06 이면 추가로 지표 달성 이벤트 결과 UI가 떠야 함 -> 기다렸다 만들어지면 하기
        }

        await UniTask.Yield();
        await GoToNextProcess();
        await WaitProcess();
    }

    // 마지막에 다음 프로세스 상태로 가기 위한 기능
    private async UniTask GoToNextProcess()
    {
        _waiting = false;
        _endProcess = true;
    }
}