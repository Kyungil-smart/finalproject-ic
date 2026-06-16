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
public class Quality
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
    public ProjectGrade grade;  // 등급 F ~ SSSS
    public uint cost;  // 투자된 금액
    public uint income;  // 매출 금액
    public uint staffCost;  // 직원 연봉 합산
    public AwardsData award;  // 수상 경력
    public NameTag trendGenre;  // 트랜드장르
    public NameTag trendTheme;  // 트랜드테마
}


public class ProjectDataManager : Manager, IProjectDataManager
{
    [Header("Gsheet Info")]
    [SerializeField] private string gsheetId;
    [SerializeField] private string awardsGId;
    
    [Header("ScriptableObject Info")]
    [SerializeField] private AwardsDataSO _awardsDataSO;
    
    private Dictionary<string, bool> _readyStatues = new();
    
    public AwardsDataSO AwardsDataSO => _awardsDataSO;
    public Dictionary<string, bool> ReadyStatues => _readyStatues;
    
    protected override void Register() => ServiceLocater.Register<IProjectDataManager>(this);
    protected override void Unregister() => ServiceLocater.Unregister<IProjectDataManager>(this);

    private void Start()
    {
        _readyStatues.Clear();
        if (Utils.Environment.isDevelopment)
            DownloadAwardData();
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
        _readyStatues.Add("AwardsData", false);
        if (_awardsDataSO.awardsDataList == null) _awardsDataSO.awardsDataList = new();
        _awardsDataSO.awardsDataList.Clear();
        GSheetManager gSheetManager = new(gsheetId, awardsGId);
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
        _readyStatues["AwardsData"] = true;
    }
}