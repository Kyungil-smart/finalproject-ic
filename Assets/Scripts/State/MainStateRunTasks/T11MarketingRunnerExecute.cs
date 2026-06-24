using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using UnityEngine;

// TODO : 어딘가에서 마케팅 받아서 SO에 저장해야함 -> 저장한 SO 불러와야함 -> 불러온 SO 여기에 들어와야 함 -> 랜더 데이터로 UI 쪽에 전달해줘야함


public class T11MarketingRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    private bool _endProcess;
    private bool _conditionGoback;


    public async UniTask Execute()
    {
        _endProcess = false;

        await SelectMarketing();

        await UniTask.WaitUntil(() => _endProcess);
    }

    public async UniTask SelectMarketing()
    {
        _waiting = true;

        // TODO : 마케팅 방식 선택
        GoProcessA();

        await WaitProcess();
        await MarketingProcessing();
    }

    private void GoProcessA()
    {
        _waiting = false;
    }

    private async UniTask MarketingProcessing()
    {
        _waiting = true;

        // ToDO. Animation 추가 작업 필요.

        var data = new ProgressAnimationRenderData()
        {
            staticImage = null,
            progressTexts = new() { "홍보 페이지 오픈", "트레일러 영상 제작", "데모 배포 및 인플루언서 섭외", "게임쇼 출품" },
            callback = GoProcessB,
        };
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcAnimationUI, data);

        CalculateMarketingResult();   // TODO : 현재는 계산 함수를 뺐는데, 기능이 많이 없으면 합칠 수 있음

        await WaitProcess();
        await CheckMarketing();
    }

    private void GoProcessB()
    {
        _waiting = false;
    }

    private void CalculateMarketingResult()
    {
        // TODO : 마케팅 효과 산출 및 저장
    }

    private async UniTask CheckMarketing()
    {
        _waiting = true;

        // TODO : 마케팅 완료 및 UI 에 랜더 데이터 제공
        GoToNextProcess();

        await UniTask.Yield();
        await WaitProcess();
    }

    private void GoToNextProcess()
    {
        _waiting = false;
        _endProcess = true;
    }
}