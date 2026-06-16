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


    // 매출 발생 및 매출 집계 애니메이션 완료 대기
    private async UniTask CalculateRevenue()
    {
        _waiting = true;
        // ToDO. Animation 이 들어올 경우 대비 해야함.
        var data = new SimpleUIRenderData(9900020, 9900007, GoProcess);
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcessSimpleUI, data);

        // TODO: 매출 발생 기능 추가 필요

        await WaitProcess();
        await CheckRevenue();
    }

    private void GoProcess()
    {
        _waiting = false;
    }

    // 매출 결과 확인
    private async UniTask CheckRevenue()
    {
        _waiting = true;


        await WaitProcess();
        await CalculateAwards();
    }


    // 어워즈 및 어워즈 애니메이션 완료 대기
    private async UniTask CalculateAwards()
    {
        _waiting = true;
        // ToDO. Animation 이 들어올 경우 대비 해야함.
        var data = new SimpleUIRenderData(9900020, 9900007, GoProcessB);
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcessSimpleUI, data);

        // TODO: 어워즈 기능 추가 필요

        await WaitProcess();
        await CheckAwards();
    }

    private void GoProcessB()
    {
        _waiting = false;
    }

    // 어워즈 결과 확인
    private async UniTask CheckAwards()
    {
        _waiting = true;

        // TODO : 어워즈 결과 UI 추가되면 넣어야 함

        await WaitProcess();
        await CheckTotal();
    }

    // 프로젝트 총 결산 확인
    private async UniTask CheckTotal()
    {
        _waiting = true;

        // TODO : 총결산 UI 추가되면 넣어야 함

        await WaitProcess();
        await EndProject();
    }

    // 프로젝트 종료
    private async UniTask EndProject()
    {
        _waiting = true;



        // ServiceLocater.Get<IStaffRegister>().GetExpAllStaffs() TODO : 릴리즈 때 전체 스텝에게 경험치 주기


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