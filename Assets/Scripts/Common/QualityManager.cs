using UnityEngine;

public class QualityManager : Manager, IQualityManager
{
    [SerializeField] private string _gSheetId;
    [SerializeField] private string _gid;
    [SerializeField] private bool _wasDownloaded;
    
    protected override void Register() => ServiceLocater.Register<IQualityManager>(this);
    protected override void Unregister() => ServiceLocater.Unregister<IQualityManager>(this);
}
