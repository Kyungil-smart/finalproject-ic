using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class QualityManager : Manager, IQualityManager
{
    [SerializeField] private string _gSheetId;
    [SerializeField] private string _gidQuality;
    [SerializeField] private string _gidTarget;
    [SerializeField] private bool _wasDownloaded;
    [SerializeField] private QualityDataSO _qData;

    public QualityCalculate Calculator { get; }

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    
    protected override void Init()
    {
        DownloadData().Forget();
    }
    private async UniTaskVoid DownloadData()
    {
        if (!Utils.Environment.isDevelopment) return;
        if (_wasDownloaded) return;
        var loader = new QualityDataLoader { qualityData = _qData };
        var gsQuality = new GSheetManager(_gSheetId, _gidQuality);
        var gsTarget = new GSheetManager(_gSheetId, _gidTarget);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(() => gsQuality.IsDownload);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(() => gsTarget.IsDownload);
        loader.LoadQulityData(gsQuality);
        loader.LoadTargetData(gsTarget);
        _wasDownloaded = true;
    }
    
    [ContextMenu("데이터 다운로드")]
    private void DataDownload()
    {
        DownloadData();
    }

    protected override void Register() => ServiceLocater.Register<IQualityManager>(this);

    protected override void Unregister() => ServiceLocater.Unregister<IQualityManager>(this);
}