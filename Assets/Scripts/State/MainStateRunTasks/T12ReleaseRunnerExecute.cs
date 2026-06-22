using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 출시(T12)
/// </summary>
public class T12ReleaseRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    AwardsData curAwardsData;

    private bool _endProcess;
    private bool _conditionGoback;

    // (유저 / 평론가 리뷰 생략) -> 매출 집계 애니메이션 -> 매출 결과 확인 -> 어워즈 심사 애니메이션 -> 어워즈 결과 확인 -> 프로젝트 총 결산 -> 프로젝트 종료
    public async UniTask Execute()
    {
        _endProcess = false;
        await CalculateRevenue();
        await UniTask.WaitUntil(() => _endProcess);
    }

    // 매출 받아오기 및 매출 집계 애니메이션 완료 대기
    private async UniTask CalculateRevenue()
    {
        _waiting = true;

        // TODO: 매출 건내주는 기능이 필요할 수도 있음
        // ToDO. Animation 이 들어올 경우 대비 해야함.

        // await WaitProcess();
        await CheckRevenue();
    }

    // 매출 결과 확인
    private async UniTask CheckRevenue()
    {
        _waiting = true;

        T12IncomeUIRenderData t12IncomeUIRenderData = new T12IncomeUIRenderData();
        t12IncomeUIRenderData.btCallback = GoProcessA;

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ReleaseUI, t12IncomeUIRenderData);

        await WaitProcess();
        await CalculateAwards();
    }

    private void GoProcessA()
    {
        _waiting = false;
    }

    // 어워즈 및 어워즈 애니메이션 완료 대기
    private async UniTask CalculateAwards()
    {
        _waiting = true;

        Debug.Log($"[디버그] ServiceLocater 체크: {ServiceLocater.Get<IQualityManager>() != null}");
        Debug.Log($"[디버그] Calculator 체크: {ServiceLocater.Get<IQualityManager>()?.Calculator != null}");


        // 어워즈 판단해서 프로젝트 매니저에 넣기
        curAwardsData.reqDesign = (int)ServiceLocater.Get<IQualityManager>().Calculator.GetDesignAchieve();
        curAwardsData.reqArt = (int)ServiceLocater.Get<IQualityManager>().Calculator.GetArtAchieve();
        curAwardsData.reqDev = (int)ServiceLocater.Get<IQualityManager>().Calculator.GetDevAchieve();


        Debug.Log($"[디버그] AwardsDataSO 체크: {ServiceLocater.Get<IProjectDataManager>()?.AwardsDataSO != null}");
        Debug.Log($"[디버그] AwardsDataSO 리스트 체크: {ServiceLocater.Get<IProjectDataManager>()?.AwardsDataSO.awardsDataList != null}");
        Debug.Log($"[디버그] AwardsDataSO 리스트 숫자 체크: {ServiceLocater.Get<IProjectDataManager>()?.AwardsDataSO.awardsDataList.Count}");


        // 기준이 되는 어워즈 SO(리스트) 불러오기
        List <AwardsData> awardsList  = ServiceLocater.Get<IProjectDataManager>().AwardsDataSO.awardsDataList;

        int index = awardsList.Count - 1;  // 수상 판단용 인덱스, 디폴트는 어워즈 SO(리스트) 마지막 값

        for (int i = 0; i < awardsList.Count; i++)
        {
            AwardsData curElement = awardsList[i];

            if (curAwardsData.reqDesign >= curElement.reqDesign &&
            curAwardsData.reqArt >= curElement.reqArt &&
            curAwardsData.reqDev >= curElement.reqDev)
            {
                index = i; 
                break;
            }
        }

        // 프로젝트 매니저에 수상 넣어주기
        ServiceLocater.Get<IProjectManager>().SetAwards(awardsList[index]);
        Debug.Log($"어워즈 번호 : {awardsList[index]}");


        // 수상에 따른 금액 추가하기
        if(awardsList[index].target == "Money")
        {
            ServiceLocater.Get<IGameManager>().AddMoney(awardsList[index].value);
            Debug.Log($"어워즈 보상 : {awardsList[index].target} | {awardsList[index].value}");
        }

        // ToDO. Animation 이 들어올 경우 대비 해야함.

        // await WaitProcess();
        await CheckAwards();
    }


    // 어워즈 결과 확인
    private async UniTask CheckAwards()
    {
        _waiting = true;

        T12AwardsUIRenderData t12AwardsUIRenderData = new T12AwardsUIRenderData();
        t12AwardsUIRenderData.btCallback = GoProcessB;

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ReleaseUI, t12AwardsUIRenderData);

        await WaitProcess();
        await CheckTotal();
    }

    private void GoProcessB()
    {
        _waiting = false;
    }

    // 프로젝트 총 결산 확인
    private async UniTask CheckTotal()
    {
        _waiting = true;

        // TODO : 장르테마 & 등급 & 투자비 & 매출 & 시상기록 받아오기

        T12ProjectDetailUIRenderData t12ProjectDetailUIRenderData = new T12ProjectDetailUIRenderData();
        t12ProjectDetailUIRenderData.btCallback = GoProcessC;

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ReleaseUI, t12ProjectDetailUIRenderData);

        await WaitProcess();
        await EndProject();
    }

    private void GoProcessC()
    {
        _waiting = false;
    }

    // 프로젝트 종료
    private async UniTask EndProject()
    {
        _waiting = true;

        // 전체 스텝에게 경험치 주기
        ServiceLocater.Get<IStaffRegister>().GetExpAllStaffs();
        await UniTask.Yield();
        
        // 프로젝트에 추가하기
        ServiceLocater.Get<IGameManager>().AddProject(ServiceLocater.Get<IProjectManager>().GetProjectData());
        await UniTask.Yield();
        
        // 1년 지나기
        ServiceLocater.Get<IGameManager>().AddAYear();
        await UniTask.Yield();
        
        await GoToNextProcess();
        await WaitProcess();
    }

    private async UniTask GoToNextProcess()
    {
        _waiting = false;
        _endProcess = true;
    }

}