using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class QualityManager : Manager, IQualityManager, IReadyStatus
{
    [SerializeField] private string _gSheetId;
    [SerializeField] private string _gidQuality;
    [SerializeField] private string _gidTarget;
    [SerializeField] private string _gsheetReward;
    [SerializeField] private string _gidReward;
    [SerializeField] private bool _wasDownloaded;
    [SerializeField] private QualityDataSO _qData;
    [SerializeField] private AchieveRewardSO _reward;

    public QualityCalculate Calculator { get; private set; }
    public Dictionary<string, bool> ReadyStatus => _readyStatus;
    private Dictionary<string, bool> _readyStatus = new ();

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    
    protected override void Init()
    {
        Calculator = new QualityCalculate(_qData);
        DownloadData().Forget();
    }
    private async UniTaskVoid DownloadData()
    {
        if (!Utils.Environment.isDevelopment) return;
        if (_wasDownloaded) return;
        _readyStatus["QualityManager"] = false;
        var loader = new QualityDataLoader { qualityData = _qData };
        var achieveLoader = new AchieveDataLoader { achieveRewardSO = _reward };
        var gsQuality = new GSheetManager(_gSheetId, _gidQuality);
        var gsTarget = new GSheetManager(_gSheetId, _gidTarget);
        var gsAchieve = new GSheetManager(_gsheetReward, _gidReward);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(() => gsQuality.IsDownload);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(() => gsTarget.IsDownload);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(() => gsAchieve.IsDownload);
        loader.LoadQualityData(gsQuality);
        loader.LoadTargetData(gsTarget);
        achieveLoader.LoadAchieveData(gsAchieve);
        _readyStatus["QualityManager"] = true;
        _wasDownloaded = true;
    }

    public async UniTask ShowAchieveResult()
    {
        int _projectYear = ServiceLocater.Get<IGameManager>().GetProjectYear();
        float _achieve = Calculator.CalculateAchieve();
        
        var yearTask = _reward.tasks.FindAll(t => t.projectYear == _projectYear);
        var taskData = yearTask.FindLast(t => _achieve >= t.threshold);

        if (taskData == null)
        {
            Debug.LogWarning("taskData 없음");
            return;
        }
        var runner = new AchieveEventRunner(taskData);
        await runner.Execute();
    }
    
    [ContextMenu("데이터 다운로드")]
    private void DataDownload()
    {
        DownloadData();
    }

    protected override void Register() => ServiceLocater.Register<IQualityManager>(this);

    protected override void Unregister() => ServiceLocater.Unregister<IQualityManager>(this);
}