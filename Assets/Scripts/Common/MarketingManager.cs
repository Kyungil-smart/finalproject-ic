using Cysharp.Threading.Tasks;
using DataDispatcher;
using System.Collections.Generic;
using System.Threading;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public struct MarketingResult
{
    public string typeName;
    public uint costResult;
    public uint bonusResult;
    public string bonusName;
}


// 마케팅 SO를 받아서 실제 마케팅에 들어가도록 계산하는 기능 필요
public class MarketingManager : Manager, IMarketingManager, IReadyStatus
{
    [SerializeField] private MarketingDataSO marketingTasks;

    [SerializeField] private string gSheetId;
    [SerializeField] private string gid;

    private Dictionary<string, bool> _readyStatus = new();
    public Dictionary<string, bool> ReadyStatus => _readyStatus;


    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Register()
    {
        ServiceLocater.Register<IMarketingManager>(this);
        ServiceLocater.Register<IEventRouter>(new EventRouter());
    }

    protected override void Unregister()
    {
        ServiceLocater.Unregister<IMarketingManager>(this);
        ServiceLocater.Unregister<IEventRouter>(new EventRouter());
    }

    protected override void Init()
    {
        Debug.Log("[MarketingManager] Initializing...");
        DownloadData();
    }

    private async UniTaskVoid DownloadData()
    {
        if (!Utils.Environment.isDevelopment) return;
        _readyStatus["MarketingData"] = false;

        GSheetManager gsManager = new GSheetManager(gSheetId, gid);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(
            "MarketingData", () => gsManager.IsDownload, GSheetManager.MAX_TIMEOUT_SECONDS);
        var dataList = gsManager.GetData();
        marketingTasks.marketingList.Clear();
        foreach (var data in dataList)
        {
            marketingTasks.marketingList.Add( new MarketingRow()
            {
                marketingType = data["Marketing_Type"],
                moneyMarketing = float.Parse(data["Money_Marketing"]),
                heartMarketing = float.Parse(data["Heart_Marketing"]),
                rateMarketing = float.Parse(data["Rate_Marketing"]),
                effectIDMarketing = int.Parse(data["EffectID_Marketing"])       
            });    
        }
        _readyStatus["MarketingData"] = true;
    }


    // 비용 및 효과 계산하기
    public List<MarketingResult> CalculateCostBonus()
    {
        List<MarketingResult> resultList = new List<MarketingResult>();
        var postManager = ServiceLocater.Get<IPostManager>();

        if (marketingTasks != null)
        {
            foreach (var item in marketingTasks.marketingList)
            {
                MarketingResult result = new MarketingResult();

                result.typeName = item.marketingType;
                result.bonusName = postManager.Request<int, string>(DataDispatcher.Channel.GetUIText, item.effectIDMarketing);

                result.costResult = (uint)(ServiceLocater.Get<IProjectManager>().Cost * (item.rateMarketing));
                result.bonusResult = (uint)(ServiceLocater.Get<IProjectManager>().Cost * (item.moneyMarketing - 1));

                resultList.Add(result);
            }
        }

        return resultList;
    }


    // TODO : SO 다운로드 받기 기능 추가 필요
    [ContextMenu("데이터 다운로드")]
    private void DataDownload()
    {
        DownloadData();
    }
}