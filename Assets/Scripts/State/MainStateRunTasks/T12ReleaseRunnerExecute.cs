using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using UnityEngine;


/// <summary>
/// 출시(T12)
/// </summary>
public class T12ReleaseRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
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

        // ServiceLocater.Get<IProjectManager>().JudgingAward();    // TODO: 어워즈 기능 추가되면 수정 필요
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

        // 프로젝트에 추가하기
        ServiceLocater.Get<IGameManager>().AddProject(ServiceLocater.Get<IProjectManager>().GetProjectData());

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