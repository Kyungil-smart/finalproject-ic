using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
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
        await UserReview();
        await UniTask.WaitUntil(() => _endProcess);
    }

    // 유저 리뷰
    private async UniTask UserReview()
    {
        _waiting = true;

        List<ReviewResult> reviewResults = ServiceLocater.Get<IReviewManager>().CheckRequirements();
        // 유저 리뷰 UI 랜더 데이터 보내기
        T12ReviewUIRenderData reviewRD = new T12ReviewUIRenderData();
        reviewRD.reviews = new List<ReviewData>();

        // reviewRD.reviews에 맞게 reviewResults 변환하기(일단 nickNameId 와 commentId 만 넣기)
        foreach (var item in reviewResults)
        {
            ReviewData data = new ReviewData();
            data.profileImg = await ServiceLocater.Get<IReviewManager>().RandomUserImgLoad();
            data.nickNameId = item.userReviewRow.userId;

            // 긍정/부정 분기
            if (item.isPositiveComment)
            {
                data.commentId = item.userReviewRow.positiveCommentId;
                data.isPositive = true;
            }
            else
            {
                data.commentId = item.userReviewRow.negativeCommentId;
                data.isPositive = false;
            }

            reviewRD.reviews.Add(data);
        }

        reviewRD.btCallback = GoProcessF;

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ReleaseUI, reviewRD);

        // 리뷰 매니저에게도 보내주기(출시 이후에도 이전 프로젝트 보여줘야해서 저장 필요)
        ServiceLocater.Get<IProjectManager>().ReviewResults = reviewResults;

        await WaitProcess();
        await CalculateRevenue();
    }

    private void GoProcessF()
    {
        _waiting = false;
    }

    // 매출 받아오기 및 매출 집계 애니메이션 완료 대기
    private async UniTask CalculateRevenue()
    {
        var calculate = ServiceLocater.Get<IProjectManager>().CostCalculator;
        ServiceLocater.Get<IQualityManager>().Calculator.ApplyGenreAndTheme();
        calculate.CalculateCost();
        calculate.CalculateIncome();
        _waiting = true;
        // ToDO. Animation 추가 작업 필요.
        
        var data = new ProgressAnimationRenderData()
        {
            staticImageId = null,
            progressTexts = new() { "플랫폼 수수료 제외 수익 집계", "광고 효율 산출", "프로젝트 비용 및 임금 계산", "공유용 최종 매출 리포트 작성" },
            callback = GoProcessD,
        };
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcAnimationUI, data);

        await WaitProcess();
        await CheckRevenue();
    }

    private void GoProcessD()
    {
        _waiting = false;
    }


    // 매출 결과 확인
    private async UniTask CheckRevenue()
    {
        _waiting = true;

        T12IncomeUIRenderData t12IncomeUIRenderData = new T12IncomeUIRenderData();
        t12IncomeUIRenderData.btCallback = GoProcessA;

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ReleaseUI, t12IncomeUIRenderData);
        ServiceLocater.Get<IGameManager>().AddMoney((int)ServiceLocater.Get<IProjectManager>().Earnings);
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

        // 어워즈 판단해서 프로젝트 매니저에 넣기
        curAwardsData.reqDesign = (int)ServiceLocater.Get<IQualityManager>().Calculator.GetDesignAchieve();
        curAwardsData.reqArt = (int)ServiceLocater.Get<IQualityManager>().Calculator.GetArtAchieve();
        curAwardsData.reqDev = (int)ServiceLocater.Get<IQualityManager>().Calculator.GetDevAchieve();

        Debug.Log($"[T12ReleaseRunnerExecute] 현재 점수 : {curAwardsData.reqDesign} {curAwardsData.reqArt} {curAwardsData.reqDev}");

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

        Debug.Log($"[T12ReleaseRunnerExecute] 어워즈 번호 : {awardsList[index]} | 보상 {awardsList[index].target} | {awardsList[index].value}");

        // ToDO. Animation 추가 작업 필요.

        var data = new ProgressAnimationRenderData()
        {
            staticImageId = null,
            progressTexts = new() { "시상 후보 명단 등재", "비공개 기술 심사 진행", "유저 투표 점수 합산", "최종 수상작 발표" },
            callback = GoProcessE,
        };
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcAnimationUI, data);

        await WaitProcess();
        if (awardsList[index].target == "Money")
            ServiceLocater.Get<IGameManager>().AddMoney(awardsList[index].value);
        await CheckAwards();
    }

    private void GoProcessE()
    {
        _waiting = false;
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
        Debug.Log($"0번 스태프 경험치 주기 전 : {ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList()[0].Staff_Name} | {ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList()[0].Exp}");
        ServiceLocater.Get<IStaffRegister>().GetExpAllStaffs();
        await UniTask.Yield();
        
        Debug.Log($"0번 스태프 경험치 준 후 : {ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList()[0].Staff_Name} | {ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList()[0].Exp}");

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