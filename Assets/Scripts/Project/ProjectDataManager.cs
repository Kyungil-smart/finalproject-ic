using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using DataDispatcher;
using Channel = DataDispatcher.Channel;

[Serializable]
public struct NameTag
{
    public int id;
    public string name;
    public int textId;
}

[Serializable]
public struct Quality
{
    public float total;
    public float design;
    public float development;
    public float art;
}

[Serializable]
public class ProjectData
{
    public ReactiveProperty<Quality> Qualities = new();  // 프로젝트 퀄리티
    public ReactiveProperty<bool> IsCompleted = new();  // 프로젝트 완료 여부
    public string name;  // 프로젝트 이름
    public NameTag genre;  // 장르
    public NameTag theme;  // 테마
    public ProjectGrade grade;  // 등급 D ~ S
    public uint cost;  // 투자된 금액
    public uint income;  // 매출 금액
    public uint staffCost;  // 직원 연봉 합산
    public uint marketingCost;  // 마케팅 비용
    public uint marketingBonus;  // 마케팅 보너스 수익
    public AwardsData award;  // 수상 경력
    public NameTag trendGenre;  // 트랜드장르
    public NameTag trendTheme;  // 트랜드테마
    public List<ReviewResult> reviewResults = new();    // 유저 평점
}


public class ProjectDataManager : Manager, IProjectDataManager, IReadyStatus
{
    [Serializable]
    class GsheedInfo
    {
        public string gsheedId;
        public string gid;
    } 
        
    [Header("Gsheet Info")]
    [SerializeField] private GsheedInfo awardsGsheetInfo;
    [SerializeField] private GsheedInfo genreThemeGsheetInfo; 
    [SerializeField] private GsheedInfo incomeRatioGsheetInfo;
    
    [Header("ScriptableObject Info")]
    [SerializeField] private AwardsDataSO _awardsDataSO;
    [SerializeField] private GenreThemeTypeDataSO _genreThemeDataSO;
    [SerializeField] private IncomeRatioDataSO _incomeRatioDataSO;
    
    public AwardsDataSO AwardsDataSO => _awardsDataSO;
    private Dictionary<string, bool> _readyStatus = new();
    public Dictionary<string, bool> ReadyStatus => _readyStatus;
    private void OnEnable()
    {
        Register();
    }

    private void OnDisable()
    {
        Unregister();
    }

    protected override void Register() => ServiceLocater.Register<IProjectDataManager>(this);
    protected override void Unregister() => ServiceLocater.Unregister<IProjectDataManager>(this);

    private void Start()
    {
        _readyStatus.Clear();
        if (Utils.Environment.isDevelopment)
        {
            UniTask.Void(async() =>
            {
                await UniTask.WaitUntil(() => ServiceLocater.Get<IUITextManager>().IsDataUpdated);
                DownloadAwardData();
                DownloadGenreThemeData();
                DownloadIncomeData();
            });
        }
    }
    
    private string GetString(string textId)
    {
        var postManager = ServiceLocater.Get<IPostManager>();
        if (postManager == null)
        {
            Debug.LogError("유니티 게임 실행을 한 후 진행 바랍니다.");
            return null;
        }
        return postManager.Request<int, string>(Channel.GetUIText, int.Parse(textId));
    }

    private async UniTask DownloadAwardData()
    {
        if (!Utils.Environment.isDevelopment) return;
        _readyStatus.Add("AwardsData", false);
        if (_awardsDataSO.awardsDataList == null) _awardsDataSO.awardsDataList = new();
        _awardsDataSO.awardsDataList.Clear();
        GSheetManager gSheetManager = new(awardsGsheetInfo.gsheedId, awardsGsheetInfo.gid);
        await UniTask.WaitUntil(() => gSheetManager.IsDownload);
        var dataList = gSheetManager.GetData();
        foreach (var data in dataList)
        {
            _awardsDataSO.awardsDataList.Add(new AwardsData()
            {
                name = new NameTag()
                {
                    id = int.Parse(data["Award_ID"]),
                    name = GetString(data["Award_Title_ID"]),
                    textId = int.Parse(data["Award_Title_ID"]),
                },
                reqDesign = int.Parse(data["Req_Design"]),
                reqArt = int.Parse(data["Req_Art"]),
                reqDev = int.Parse(data["Req_Dev"]),
                descId = int.Parse(data["Award_Desc_ID"]),
                target = data["Award_Reward"],
                value = int.Parse(data["Reward_Value"]),
                resultId = int.Parse(data["Award_Result_ID"]),
            });
        }
        _readyStatus["AwardsData"] = true;
    }
    
    private async UniTask DownloadGenreThemeData()
    {
        if (!Utils.Environment.isDevelopment) return;
        _readyStatus.Add("GenreThemeData", false);
        if (_genreThemeDataSO.genreThemeList == null) _genreThemeDataSO.genreThemeList = new();
        _genreThemeDataSO.genreThemeList.Clear();
        GSheetManager gSheetManager = new(genreThemeGsheetInfo.gsheedId, genreThemeGsheetInfo.gid);
        await UniTask.WaitUntil(() => gSheetManager.IsDownload);
        var dataList = gSheetManager.GetData();
        foreach (var data in dataList)
        {
            _genreThemeDataSO.genreThemeList.Add(new GenreThemeRow()
            {
                GT_ID = int.Parse(data["GT_Id"]),
                Type = int.Parse(data["Type"]),
                GT_Name_ID = int.Parse(data["GT_Name_ID"]),
                GT_Cost =  int.Parse(data["GT_Cost"]),
                GT_Cost_Ratio = float.Parse(data["GT_Cost_Ratio"]),
            });
        }
        _readyStatus["GenreThemeData"] = true;
    }
    
    private async UniTask DownloadIncomeData()
    {
        _readyStatus.Add("IncomeData", false);
        if (_incomeRatioDataSO.ratioList == null) _incomeRatioDataSO.ratioList = new();
        _incomeRatioDataSO.ratioList.Clear();
        GSheetManager gSheetManager = new(incomeRatioGsheetInfo.gsheedId, incomeRatioGsheetInfo.gid);
        await UniTask.WaitUntil(() => gSheetManager.IsDownload);
        var dataList = gSheetManager.GetData();
        foreach (var data in dataList)
        {
            Debug.Log($"[IncomeData] More:'{data["More"]}', Under:'{data["Under"]}', Money:'{data["Money"]}', Heart:'{data["Heart"]}'");
            _incomeRatioDataSO.ratioList.Add(new IncomeRatioRow()
            {
                achieveMin = float.Parse(data["More"].Replace("%", "")),
                achieveMax = float.Parse(data["Under"].Replace("%", "")),
                moneyRatio = float.Parse(data["Money"]),
                heartRatio = float.Parse(data["Heart"]),
            });
        }
        _readyStatus["IncomeData"] = true;
    }
}