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

    // TODO : SO 받도록 수정 필요
    [SerializeField] private string gSheetId;
    [SerializeField] private string gid;
    [SerializeField] private bool _wasDownloaded;
    private GSheetManager _gsheet;

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
        // TODO : MarketingDataLoader 만들어서 수정 필요
        /*
        if (!Utils.Environment.isDevelopment) return;
        if (_wasDownloaded) return;
        
        _readyStatus["MarketingData"] = false;

        GSheetManager gsManager = new GSheetManager(gSheetId, gid);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(() => gsManager.IsDownload);
        var dataList = gsManager.GetData();
        loader.LoadEvent(gsManager);
        _wasDownloaded = true;
        _readyStatus["MarketingData"] = true;
        */
    }


    // 비용 및 효과 계산하기
    public List<MarketingResult> CalculateCostBonus()
    {
        List<MarketingResult> resultList = new List<MarketingResult>();
        var postManager = ServiceLocater.Get<IPostManager>();

        if (marketingTasks != null)
        {
            foreach (var item in marketingTasks.MarketingList)
            {
                MarketingResult result = new MarketingResult();

                result.typeName = item.Marketing_Type;
                result.bonusName = postManager.Request<int, string>(DataDispatcher.Channel.GetUIText, item.EffectID_Marketing);

                result.costResult = (uint)(ServiceLocater.Get<IProjectManager>().Cost * (item.Money_Marketing - 1));
                result.bonusResult = (uint)(ServiceLocater.Get<IProjectManager>().Income * (item.Rate_Marketing - 1));

                Debug.Log($"[MarketingManager] : {result.typeName} | {result.costResult} = {ServiceLocater.Get<IProjectManager>().Cost} * {item.Money_Marketing - 1}");

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