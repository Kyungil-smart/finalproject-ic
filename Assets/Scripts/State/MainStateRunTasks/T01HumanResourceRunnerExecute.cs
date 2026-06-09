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
    private List<StaffViewData> _candidateStaffList;    // 고용된 스태프 + 후보 8인 리스트
    private List<StaffViewData> _tempHiredList;
    private StaffSummaryRenderData _staffSummaryRenderData; //
    private List<int> _selectedStaffs;  // UI에 콜백을 보내기 위한 List<int> 타입의 인스턴스 변수


    // 후보 리스트 생성 및 변환 -> 직원 리스트 확인 -> 기존 직원과 리스트 통합 -> 계약할 캐릭터 선택 -> 계약 및 채용 확정 -> 채용 진행 애니메이션 -> 채용 확인
    public async UniTask Execute()
    {
        _selectedStaffs = new List<int>();
        await MergeStaffList();
        await UniTask.WaitUntil(() => !_waiting);
        await CheckHireStaffs();
        WaitingAnimation();
        CheckHiring();
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

        List<StaffViewData> candidateList = await CreateCandidateList();

        List<StaffViewData> staffList = CheckStaffList();

        _candidateStaffList = new List<StaffViewData>();
        _candidateStaffList.AddRange(staffList);
        _candidateStaffList.AddRange(candidateList);
        await UniTask.Yield();
        if (_staffSummaryRenderData == null)
        {
            _staffSummaryRenderData = new StaffSummaryRenderData();
            _staffSummaryRenderData.staffSummaryData = new List<StaffSummaryData>();
            foreach (var item in staffList)
            {
                StaffSummaryData staffSummaryData = new StaffSummaryData();
                staffSummaryData.selected = false;
                staffSummaryData.hired = true;
                staffSummaryData.viewData = item;

                _staffSummaryRenderData.staffSummaryData.Add(staffSummaryData);
            }

            foreach (var item in candidateList)
            {
                StaffSummaryData staffSummaryData = new StaffSummaryData();
                staffSummaryData.selected = false;
                staffSummaryData.hired = false;
                staffSummaryData.viewData = item;

                _staffSummaryRenderData.staffSummaryData.Add(staffSummaryData);
            }
        }
        await UniTask.Yield();
        var tail = new StaffSummaryTailData() { num = 1, confirmCallback = SelectedStaffCallback };
        _staffSummaryRenderData.tailType = tail;
        _staffSummaryRenderData.selectable = true;
        await UniTask.Yield();
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.StaffCandidateUI, _staffSummaryRenderData);
    }

    private void SelectedStaffCallback(List<int> staffs)
    {
        _selectedStaffs = staffs;
        _waiting = false;
    }

    public async UniTask CheckHireStaffs()
    {
        // TODO : 직원 관리 4: 계약 및 채용(실제 확정은 아니고 가채용, 문서 명칭대로 함)
        // 필요 Input 데이터
        //   UI에서 받은 정보

        // 필요 기능
        //   UI에서 선택 받은 인덱스 넘버를 통해 합쳐진 리스트에서 추려낸 가 리스트 만들기

        // 필요 Output 데이터
        //   리스트업 해서 던지기
        //   UI에도 전달

        _tempHiredList = new List<StaffViewData>();

        // 순회하며 해당 인덱스의 데이터를 _tempHiredList에 추가

        
    }

    private void WaitingAnimation()
    {
        // TODO : 채용 진행 애니메이션
        // 필요 Input 데이터
        //   

        // 필요 기능
        //   채용 진행
        //   해고 진행
        //   채용 진행 애니메이션 실행 및 대기 -> 어떻게 하지?? 애니메이션 실제 진행은 에셋 나온 후에 TODO


        // 필요 Output 데이터


    }

    private void CheckHiring()
    {
        // TODO : 채용 확인
        // 필요 Input 데이터

        // 필요 기능
        //    GetAllHiredStaffList() 최신화

        // 필요 Output 데이터
        //   최신화된 GetAllHiredStaffList() 전달하기



    }
}