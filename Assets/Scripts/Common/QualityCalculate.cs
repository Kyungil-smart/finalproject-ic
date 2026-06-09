using UnityEngine;

public class QualityCalculate
{
    private QualityDataSO _qualityData;
    
    private float _designQuality;
    private float _devQuality;
    private float _artQuality;
    private float _totalQuality;
    private float _achieve;

    public QualityCalculate(QualityDataSO qualityData)
    {
        _qualityData = qualityData;
    }
    
    // 유효스탯 계산
    private float CalCuateArrange(StaffEntity main, StaffEntity sub)
    {
        var data = _qualityData.rates[0];
        float mainStat = main.GetJob() switch
        {
            JobType.Artist => main.GetArt(),
            JobType.Developer => main.GetDevelopment(),
            JobType.Planner => main.GetPlanning()
        };
        float subStat = sub.GetJob() switch
        {
            JobType.Artist => sub.GetArt(),
            JobType.Developer => sub.GetDevelopment(),
            JobType.Planner => sub.GetPlanning()
        };
        return (mainStat * data.arrageCase1) + (subStat * data.arrageCase2);
    }
    
    // 파트별 퀄리티
    
}
