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
    private List<StaffViewData> _candidateStaffList;
    private List<StaffViewData> _tempHiredList;
    private StaffSummaryRenderData _staffSummaryRenderData;

    private List<int> _selectedStaffs;



    // 후보 리스트 생성 및 변환 -> 직원 리스트 확인 -> 기존 직원과 리스트 통합 -> 계약할 캐릭터 선택 -> 계약 및 채용 확정 -> 채용 진행 애니메이션 -> 채용 확인
    public async UniTask Execute()
    {
        _staffSummaryRenderData = new StaffSummaryRenderData();
        _selectedStaffs = new List<int>();


        MergeStaffList();
        // List<int> number = new List<int> { 0, 2, 3 };    // !! TODO: 임시용, 테스트 완료 후 삭제 요망
        //HireStaff(number);

        WaitingAnimation();
        CheckHiring();
    }

    // TODO: 임시로 이곳에 넣음, 추후 Enter로 이동 필요
    private async UniTask<List<StaffViewData>> CreateCandidateList()
    {
        // TODO : 후보 생성
        // 필요 Input 데이터
        //   슬롯 : 고용된 직원 슬롯, 최대 슬롯
        //   회사 레벨

        // 필요 기능
        //   카드 수만큼 채용 후보 리스트 생성 (staffManager.GenerateRecruitCandidatesAsync())
        //   뽑힌 후보 리스트를 StaffViewData 형태로 반환 (staffManager.GetAvailableStaffList())

        // 필요 Output 데이터
        //   UI용으로 변환된 후보 리스트 (staffManager.GetAvailableStaffList() -> List<StaffViewData>)



        // 아래는 필요한 데이터 위치 확인 + ServiceLocater / R3 / UniTask 기능 작동하는지 보기 위해서 예시로 넣음
     

        // 필요한 인터페이스 불러오기
        var gameManager = ServiceLocater.Get<IGameManager>();
        var staffManager = ServiceLocater.Get<IStaffRecruit>();

        await staffManager.GenerateRecruitCandidatesAsync(playerLevel: gameManager.PlayerLevel.CurrentValue, cardCount: 8);

        // Debug.Log($"회사 레벨: {gameManager.PlayerLevel.CurrentValue} | 회사 슬롯: {_totalSlotCount - _hiredSlotCount}");

        return staffManager.GetAvailableStaffList();
    }

    private List<StaffViewData> CheckStaffList()
    {
        // TODO : 보유 직원 리스트 확인
        // 필요 Input 데이터
        //   직원 리스트 ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList() -> List<StaffViewData>

        // 필요 기능
        //   

        // 필요 Output 데이터

        return ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
    }

    private async void MergeStaffList()
    {
        // TODO : 후보 + 보유 직원 리스트 합치기
        // 필요 Input 데이터
        //   currentAvailableStaff, CheckStaffList() 의 리스트

        // 필요 기능
        //   두 리스트 합치기
        //   보내주기

        // 필요 Output 데이터
        //   합쳐진 형태의 리스트

        // UI 인덱스 번호랑 같아야

        List<StaffViewData> candidateList = await CreateCandidateList();

        List<StaffViewData> staffList = CheckStaffList();

        _candidateStaffList = new List<StaffViewData>();
        _candidateStaffList.AddRange(staffList);
        _candidateStaffList.AddRange(candidateList);

        // var data = new SimpleUIRenderData(so.stateNameId, 9900007, GoProcess);  // TODO: 확인 필요


        _staffSummaryRenderData.staffSummaryData = new List<StaffSummaryData>();

        if (_staffSummaryRenderData == null)
        {
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

        _staffSummaryRenderData.callbacks = SelectedStaffCallback;


        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.StaffCandidateUI, _staffSummaryRenderData);

    }

    private List<int> SelectedStaffCallback(List<int> staffs)
    {
        _selectedStaffs = staffs;
        return staffs;
    }


    // 가채용
    public void HireStaff(List<int> indexNumber)
    {
        // TODO : 직원 관리 4: 계약 및 채용(실제 확정은 아니고 가채용, 문서 명칭대로 함)
        // 필요 Input 데이터
        //   UI에서 선택 받은 인덱스 넘버

        // 필요 기능
        //   UI에서 선택 받은 인덱스 넘버를 통해 합쳐진 리스트에서 추려낸 가 리스트 만들기

        // 필요 Output 데이터
        //   리스트업 해서 던지기
        //   UI에도 전달

        _tempHiredList = new List<StaffViewData>();

        // 순회하며 해당 인덱스의 데이터를 _tempHiredList에 추가
        foreach (int idx in indexNumber)
        {
            if (idx >= 0 && idx < _candidateStaffList.Count)
            {
                _tempHiredList.Add(_candidateStaffList[idx]);
            }
        }
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