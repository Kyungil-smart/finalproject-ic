using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

// TODO : 어딘가에서 마케팅 받아서 SO에 저장해야함 -> 저장한 SO 불러와야함 -> 불러온 SO 여기에 들어와야 함 -> 랜더 데이터로 UI 쪽에 전달해줘야함


public class T11MarketingRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    private bool _endProcess;
    private bool _conditionGoback;

    MarketingRenderData _marketingRenderData;
    private List<int> _selectedMarketingIndex = new();  // UI에 콜백을 보내기 위한 변수, 실제로는 Staff와 다르개 1개만 선택 가능


    public async UniTask Execute()
    {
        _endProcess = false;

        await SelectMarketingList();

        await UniTask.WaitUntil(() => _endProcess);
    }

    public async UniTask SelectMarketingList()
    {
        _waiting = true;

        _marketingRenderData = new MarketingRenderData();
        _marketingRenderData.marketingData = new List<MarketingData>();

        List<MarketingResult> resultList = ServiceLocater.Get<IMarketingManager>().CalculateCostBonus();    // 마케팅 매니저에서 각각 광고 결과 가져오기

        // 매니저에 저장된 마케팅을 랜더용으로 변환
        foreach (var item in resultList)
        {
            MarketingData marketingData = new MarketingData();

            // 리스트에서 한줄 씩 데이터 넣기
            marketingData.selected = false; // 선택 여부
            marketingData.marketType = item.typeName;   // 광고 타입 (A ~ D 사용)
            marketingData.moneyMarket = item.bonusResult;   // 광고 수익 보너스
            marketingData.heartMarket = 0;  // 광고 하트 보너스, 칼럼에는 있지만 사용 안함
            marketingData.rateMarket = item.costResult; // 광고 비용
            marketingData.moneyResultType = item.bonusName; // 광고 효과 표시 (T11에서 사용)

            _marketingRenderData.marketingData.Add(marketingData);
        }
        await UniTask.Yield();

        var tail = new MarketingTailData() { num = 1, confirmCallback = SelectedMarketingCallback };
        _marketingRenderData.tailType = tail;
        _marketingRenderData.selectable = true;

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.MarketingUI, _marketingRenderData);

        await WaitProcess();
        await CheckSelectMarketing();
    }

    private async UniTaskVoid SelectedMarketingCallback(List<int> index)
    {
        _waiting = false;
        _selectedMarketingIndex = index;
    }

    public async UniTask CheckSelectMarketing()
    {
        _waiting = true;

        MarketingRenderData selectedmarketingRD = new MarketingRenderData();
        selectedmarketingRD.marketingData = new List<MarketingData>();

        foreach (var idx in _selectedMarketingIndex)
        {
            selectedmarketingRD.marketingData.Add(_marketingRenderData.marketingData[idx]);
            _marketingRenderData.marketingData[idx].selected = false;
        }

        var tail = new MarketingTailData()
        {
            num = 2,
            nextCallback = GoCheckSelectMarketingToWaitingAnimation,
            previousCallback = GoCheckSelectMarketingToBack
        };
        selectedmarketingRD.tailType = tail;
        selectedmarketingRD.selectable = false;

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.MarketingUI, selectedmarketingRD);

        await WaitProcess();
        if (_conditionGoback) await SelectMarketingList();
        await MarketingProcessing();
    }

    private async UniTaskVoid GoCheckSelectMarketingToWaitingAnimation()
    {
        _waiting = false;
        _conditionGoback = false;

    }

    private async UniTaskVoid GoCheckSelectMarketingToBack()
    {
        _waiting = false;
        _conditionGoback = true;
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

        // 선택된 광고 저장
        foreach(var marketing in _marketingRenderData.marketingData)
        {
            if(marketing.selected)
            {
                ServiceLocater.Get<IProjectManager>().MarketingCost = marketing.rateMarket;
                ServiceLocater.Get<IProjectManager>().MarketingBonus = marketing.moneyMarket;

                Debug.Log($"[T11] 저장된 코스트 : {ServiceLocater.Get<IProjectManager>().MarketingCost} | 저장된 보너스 : {ServiceLocater.Get<IProjectManager>().MarketingBonus}");
            }
        }

        await UniTask.Yield();
        await WaitProcess();
        await CheckMarketing();
    }

    private void GoProcessB()
    {
        _waiting = false;
    }
    
    private async UniTask CheckMarketing()
    {
        _waiting = true;

        MarketingRenderData FinalmarketingRD = new MarketingRenderData();
        FinalmarketingRD.marketingData = new List<MarketingData>();

        foreach (var idx in _selectedMarketingIndex)
        {
            FinalmarketingRD.marketingData.Add(_marketingRenderData.marketingData[idx]);
            _marketingRenderData.marketingData[idx].selected = false;
        }
        await UniTask.Yield();

        // 마케팅 완료 및 UI 에 랜더 데이터 제공
        var tail = new MarketingTailData()
        {
            num = 3,
            nextCallback = GoToNextProcess,
        };

        FinalmarketingRD.tailType = tail;
        FinalmarketingRD.selectable = false;

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.MarketingUI, FinalmarketingRD);

        await WaitProcess();
    }
    
    private async UniTaskVoid GoToNextProcess()
    {
        _waiting = false;
        _endProcess = true;
    }
}