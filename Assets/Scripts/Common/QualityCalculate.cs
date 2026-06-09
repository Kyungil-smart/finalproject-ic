using UnityEngine;

public class QualityCalculate
{
    private QualityDataSO _qualityData;

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
    // 세부 프로세스 구분이 가능하면 합치는 걸로 변경가능성 있음
    public void CalculateDesign(StaffEntity main, StaffEntity sub)
    {
        var data = _qualityData.rates[0];
        float arrange = CalCuateArrange(main, sub);
        float boost = 1 + ((main.GetTotalCommunication() * data.boostCase1) 
                           + (main.GetTotalCreativity() * data.boostCase2) 
                           + (main.GetTotalConcentration() * data.boostCase3)) / 100;
        ServiceLocater.Get<IProjectManager>().DesignQuality = arrange * boost;
    }
    public void CalculateDev(StaffEntity main, StaffEntity sub)
    {
        var data = _qualityData.rates[0];
        float arrange = CalCuateArrange(main, sub);
        float boost = 1 + ((main.GetTotalCreativity() * data.boostCase1) 
                           + (main.GetTotalConcentration() * data.boostCase2)) / 100;
        ServiceLocater.Get<IProjectManager>().DevQuality = arrange * boost;
    }
    public void CalculateArt(StaffEntity main, StaffEntity sub)
    {
        var data = _qualityData.rates[0];
        float arrange = CalCuateArrange(main, sub);
        float boost = 1 + ((main.GetTotalConcentration() * data.boostCase1) 
                           + (main.GetTotalCreativity() * data.boostCase2)) / 100;
        ServiceLocater.Get<IProjectManager>().ArtQuality = arrange * boost;
    }
    
    // 합산(초반부 후반부 합산 퀄리티 계산식이 동일해서 +=으로 더하도록 함)
    public void CalculateTotal()
    {
        var data = _qualityData.rates[0];
        float noise = Random.Range(data.noiseMin, data.noiseMax);
        var qt = ServiceLocater.Get<IProjectManager>();
        float result = (qt.DesignQuality + qt.ArtQuality + qt.DevQuality) * noise;
        qt.UpdateTotalQuality(result);
    }
}
