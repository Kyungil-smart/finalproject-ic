using Cysharp.Threading.Tasks;
using DataDispatcher;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직원 관리
/// </summary>
public class T01HumanResourceRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    private List<StaffViewData> _candidateStaffList;



    // 후보 리스트 생성 및 변환 -> 직원 리스트 확인 -> 기존 직원과 리스트 통합 -> 계약할 캐릭터 선택 -> 계약 및 채용 확정 -> 채용 진행 애니메이션 -> 채용 확인
    public async UniTask Execute()
    {
        await CreateCandidateList();
        CheckStaffList();
        MergeStaffList();
        List<int> number = new List<int> { 0, 2, 3 };    // !! TODO: 임시용, 테스트 완료 후 삭제 요망
        HireStaff(number);
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

        // 슬롯 개수
        int _totalSlotCount = 8;    // 전체 슬롯 개수(TODO : 어디서 불러오는지 확인 필요)   
        int _hiredSlotCount = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList().Count;    // 이미 고용된 슬롯 개수        

        // 필요한 인터페이스 불러오기
        var gameManager = ServiceLocater.Get<IGameManager>();
        var staffManager = ServiceLocater.Get<IStaffRecruit>();

        await staffManager.GenerateRecruitCandidatesAsync(playerLevel: gameManager.PlayerLevel.CurrentValue, cardCount: _totalSlotCount - _hiredSlotCount);

        Debug.Log($"회사 레벨: {gameManager.PlayerLevel.CurrentValue} | 회사 슬롯: {_totalSlotCount - _hiredSlotCount}");

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
        _candidateStaffList.AddRange(candidateList);
        _candidateStaffList.AddRange(staffList);

    }

    public void HireStaff(List<int> indexNumber)
    {
        // TODO : 직원 관리 4: 계약 및 채용 확정(실제 확정은 아니고 문서 명칭대로 함)
        // 필요 Input 데이터
        //   UI에서 선택 받은 인덱스 넘버

        // 필요 기능
        //   UI에서 선택 받은 인덱스 넘버를 통해 합쳐진 리스트에서 추려낸 가 리스트 만들기

        // 필요 Output 데이터
        //   리스트업 해서 던지기
        //   UI에도 전달
    }

    private void WaitingAnimation()
    {
        // TODO : 채용 진행 애니메이션
        // 필요 Input 데이터
        //   

        // 필요 기능
        //   채용 진행
        //   해고 진행
        //   채용 진행 애니메이션 실행 및 대기


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