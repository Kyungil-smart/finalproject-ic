using System.Linq;
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
            JobType.Planner => main.GetPlanning(),
            _ => default
        };
        float subStat = sub.GetJob() switch
        {
            JobType.Artist => sub.GetArt(),
            JobType.Developer => sub.GetDevelopment(),
            JobType.Planner => sub.GetPlanning(),
            _ => default
        };
        return (mainStat * data.arrageCase1) + (subStat * data.arrageCase2);
    }
    
    // 파트별 퀄리티
    // 세부 프로세스 구분이 가능하면 합치는 걸로 변경가능성 있음
    public void CalculateDesign()
    {
        var data = _qualityData.rates[0];
        var ids = ServiceLocater.Get<IProjectManager>().GetAssignedStaff();
        var main = ServiceLocater.Get<StaffManager>().GetStaffEntity(ids[0]);
        var sub = ServiceLocater.Get<StaffManager>().GetStaffEntity(ids[1]);
        float arrange = CalCuateArrange(main, sub);
        float boost = 1 + ((main.GetTotalCommunication() * data.boostCase1) 
                           + (main.GetTotalCreativity() * data.boostCase2) 
                           + (main.GetTotalConcentration() * data.boostCase3)) / 100;
        ServiceLocater.Get<IProjectManager>().DesignQuality = arrange * boost;
    }
    public void CalculateDev()
    {
        var data = _qualityData.rates[0];
        var ids = ServiceLocater.Get<IProjectManager>().GetAssignedStaff();
        var main = ServiceLocater.Get<StaffManager>().GetStaffEntity(ids[0]);
        var sub = ServiceLocater.Get<StaffManager>().GetStaffEntity(ids[1]);
        float arrange = CalCuateArrange(main, sub);
        float boost = 1 + ((main.GetTotalCreativity() * data.boostCase1) 
                           + (main.GetTotalConcentration() * data.boostCase2)) / 100;
        ServiceLocater.Get<IProjectManager>().DevQuality = arrange * boost;
    }
    public void CalculateArt()
    {
        var data = _qualityData.rates[0];
        var ids = ServiceLocater.Get<IProjectManager>().GetAssignedStaff();
        var main = ServiceLocater.Get<StaffManager>().GetStaffEntity(ids[0]);
        var sub = ServiceLocater.Get<StaffManager>().GetStaffEntity(ids[1]);
        float arrange = CalCuateArrange(main, sub);
        float boost = 1 + ((main.GetTotalConcentration() * data.boostCase1) 
                           + (main.GetTotalCreativity() * data.boostCase2)) / 100;
        ServiceLocater.Get<IProjectManager>().ArtQuality = arrange * boost;
    }
    
    // 합산
    public void CalculateTotal()
    {
        var data = _qualityData.rates[0];
        float noise = Random.Range(data.noiseMin, data.noiseMax);
        var qt = ServiceLocater.Get<IProjectManager>();
        float result = (qt.DesignQuality + qt.ArtQuality + qt.DevQuality) * noise;
        float begin = qt.TotalQuality;
        qt.UpdateTotalQuality(begin + result);
    }
    
    // 트랜드, 소통시너지는 호출시기에 따라 한번에 계산할수도 있음.
    // 트랜드 배수 적용
    public void ApplyGt()
    {
        // 트랜드의 장르랑 테마랑 프로젝트의 장르랑 테마랑 일치하는지 여부로 계산
        var data = _qualityData.rates[0];
        var qt = ServiceLocater.Get<IProjectManager>();
        var genreMatch = qt.Genre.id == qt.GetProjectData().trendGenre.id;
        var themeMatch = qt.Theme.id == qt.GetProjectData().trendTheme.id;
        if (genreMatch && themeMatch) qt.UpdateTotalQuality(qt.TotalQuality * data.gtBoth);
        else if (genreMatch || themeMatch) qt.UpdateTotalQuality(qt.TotalQuality * data.gtEither);
        else qt.UpdateTotalQuality(qt.TotalQuality * data.gtNeither);
        // 소통시너지 계산
        var staffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        float avgCom = (float)staffList.Average(s => s.Final_Common_Communication);
        qt.UpdateTotalQuality(qt.TotalQuality * (1 + (avgCom / data.commSynergy)));
    }
    
    // 소통 시너지 적용
    // public void ApplySynergy()
    // {
    //     var data = _qualityData.rates[0];
    //     var staffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
    //     float avgCom = (float)staffList.Average(s => s.Final_Common_Communication);
    //     var qt = ServiceLocater.Get<IProjectManager>();
    //     qt.UpdateTotalQuality(qt.TotalQuality * (1 + (avgCom / data.commSynergy)));
    // }
    
    // Total퀄리티 지표계산
    public void CalculateAchieve()
    {
        var staffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        float avgLevel = (float)staffList.Sum(s => s.Current_Level) /  staffList.Count;
        var target = _qualityData.targets.Find(t => t.avgLevel == avgLevel);
        var qt = ServiceLocater.Get<IProjectManager>();
        float achieve = (qt.TotalQuality / target.targetTotal) * 100;
    }
}
