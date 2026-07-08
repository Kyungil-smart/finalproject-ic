using System;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class QualityCalculate
{
    private QualityDataSO _qualityData;
    public QualityCalculate(QualityDataSO qualityData)
    {
        _qualityData = qualityData;
    }
    
    // 유효스탯 계산
    private float CalculateArrange(StaffEntity main, StaffEntity sub, GameDevProcName procName)
    {
        var data = _qualityData.rates[0];
        float mainStat = procName switch
        {
            GameDevProcName.ConceptPreProduction or GameDevProcName.ConceptFullProduction => main.GetDesign(),
            GameDevProcName.DevelopmentPreProduction or GameDevProcName.DevelopmentFullProduction => main.GetDevelopment(),
            GameDevProcName.ArtPreProduction or GameDevProcName.ArtFullProduction => main.GetArt(),
            _ => default
        };
        float subStat = procName switch
        {
            GameDevProcName.ConceptPreProduction or GameDevProcName.ConceptFullProduction => sub.GetDesign(),
            GameDevProcName.DevelopmentPreProduction or GameDevProcName.DevelopmentFullProduction => sub.GetDevelopment(),
            GameDevProcName.ArtPreProduction or GameDevProcName.ArtFullProduction => sub.GetArt(),
            _ => default
        };
        return (mainStat * data.arrageCase1) + (subStat * data.arrageCase2);
    }
    
    // Todo. 편하게 부를수 있게 만든 함수 프로세스 네임으로 부를수 있음.
    public void CalculateQuality(GameDevProcName procName)
    {
        switch (procName)
        {
            case GameDevProcName.ConceptPreProduction:
            case GameDevProcName.ConceptFullProduction:
                CalculateDesign(procName);
                break;
            case GameDevProcName.DevelopmentPreProduction:
            case GameDevProcName.DevelopmentFullProduction:
                CalculateDev(procName);
                break;
            case GameDevProcName.ArtPreProduction:
            case GameDevProcName.ArtFullProduction:
                CalculateArt(procName);
                break;
        }
    }
    
    // 파트별 퀄리티
    // 세부 프로세스 구분이 가능하면 합치는 걸로 변경가능성 있음
    public void CalculateDesign(GameDevProcName procName)
    {
        var data = _qualityData.rates[0];
        var ids = ServiceLocater.Get<IProjectManager>()
            .GetAssignedStaffIds(ServiceLocater.Get<IGameManager>().ProcName.CurrentValue);
        var main = ServiceLocater.Get<IStaffRegister>().GetStaffEntity(ids[0]);
        var sub = ServiceLocater.Get<IStaffRegister>().GetStaffEntity(ids[1]);
        float noise = Random.Range(data.noiseMin, data.noiseMax);
        float arrange = CalculateArrange(main, sub, procName);
        float boost = 1 + ((main.GetCommunication() * data.boostCase1) 
                           + (main.GetCreativity() * data.boostCase2) 
                           + (main.GetConcentration() * data.boostCase3)) / 100;
        ServiceLocater.Get<IProjectManager>().DesignQuality = (float)Math.Round(arrange * boost * noise, 1);
        Debug.Log($"{ServiceLocater.Get<IProjectManager>().DesignQuality}");
        Debug.Log("[DesignQuality]품질계산 완료");
    }
    public void CalculateDev(GameDevProcName procName)
    {
        var data = _qualityData.rates[0];
        var ids = ServiceLocater.Get<IProjectManager>()
            .GetAssignedStaffIds(ServiceLocater.Get<IGameManager>().ProcName.CurrentValue);
        var main = ServiceLocater.Get<IStaffRegister>().GetStaffEntity(ids[0]);
        var sub = ServiceLocater.Get<IStaffRegister>().GetStaffEntity(ids[1]);
        float noise = Random.Range(data.noiseMin, data.noiseMax);
        float arrange = CalculateArrange(main, sub, procName);
        float boost = 1 + ((main.GetCreativity() * data.boostCase1) 
                           + (main.GetConcentration() * data.boostCase2)) / 100;
        ServiceLocater.Get<IProjectManager>().DevQuality = (float)Math.Round(arrange * boost * noise, 1);
        Debug.Log($"{ServiceLocater.Get<IProjectManager>().DevQuality}");
        Debug.Log("[DevQuality]품질계산 완료");
    }
    public void CalculateArt(GameDevProcName procName)
    {
        var data = _qualityData.rates[0];
        var ids = ServiceLocater.Get<IProjectManager>()
            .GetAssignedStaffIds(ServiceLocater.Get<IGameManager>().ProcName.CurrentValue);
        var main = ServiceLocater.Get<IStaffRegister>().GetStaffEntity(ids[0]);
        var sub = ServiceLocater.Get<IStaffRegister>().GetStaffEntity(ids[1]);
        float noise = Random.Range(data.noiseMin, data.noiseMax);
        float arrange = CalculateArrange(main, sub, procName);
        float boost = 1 + ((main.GetConcentration() * data.boostCase1) 
                           + (main.GetCreativity() * data.boostCase2)) / 100;
        ServiceLocater.Get<IProjectManager>().ArtQuality = (float)Math.Round(arrange * boost * noise, 1);
        Debug.Log($"{ServiceLocater.Get<IProjectManager>().ArtQuality}");
        Debug.Log("[ArtQuality]품질계산 완료");
    }
    
    // 합산
    public void CalculateTotal()
    {
        var qt = ServiceLocater.Get<IProjectManager>();
        float result = (qt.DesignQuality + qt.ArtQuality + qt.DevQuality);
        qt.UpdateTotalQuality(qt.TotalQuality + result);
        Debug.Log("[CalculateTotal] 퀄리티 합산 완료");
        Debug.Log($"{ServiceLocater.Get<IProjectManager>().TotalQuality}");
    }
    
    // QA계산식 추가 예정(기획이 나오면)
    
    // 트랜드, 소통시너지는 호출시기에 따라 한번에 계산할수도 있음.
    // 트랜드 배수 적용
    public void ApplyGenreAndTheme()
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

    public void ApplyTotalQualityWithQAResult()
    {
        var qt = ServiceLocater.Get<IProjectManager>();
        var totalQuality = qt.TotalQuality * (1 + qt.QAResult);
        qt.UpdateTotalQuality(totalQuality);
    }
    
    // PreTotal퀄리티 달성률 계산
    public float CalculateAchieve()
    {
        var staffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        float avgLevel = (float)staffList.Sum(s => s.Level) / staffList.Count;
        var target = _qualityData.targets.Find(t => t.avgLevel == avgLevel);
        var qt = ServiceLocater.Get<IProjectManager>();
        return Attainment(qt.TotalQuality, target.targetTotalPre);
    }
    
    // FullTotal퀄리티 달성률 계산
    public float CalculateFullAchieve()
    {
        var staffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        float avgLevel = (float)staffList.Sum(s => s.Level) / staffList.Count;
        var target = _qualityData.targets.Find(t => t.avgLevel == avgLevel);
        var qt = ServiceLocater.Get<IProjectManager>();
        return Attainment(qt.TotalQuality, target.targetTotal);
    }
    
    // 파트별 달성률 계산
    // Todo. 어워즈용으로 남겨둠
    public float GetDesignAchieve()
    {
        var qt = ServiceLocater.Get<IProjectManager>();
        var staffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        float avgLevel = (float)staffList.Sum(s => s.Level) / staffList.Count;
        var target = _qualityData.targets.Find(t => t.avgLevel == avgLevel);
        float achieve = Attainment(qt.DesignQuality, target.targetDesign);
        return achieve;
    }
    
    public float GetDevAchieve()
    {
        var qt = ServiceLocater.Get<IProjectManager>();
        var staffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        float avgLevel = (float)staffList.Sum(s => s.Level) / staffList.Count;
        var target = _qualityData.targets.Find(t => t.avgLevel == avgLevel);
        float achieve = Attainment(qt.DevQuality, target.targetDev);
        return achieve;
    }
    
    public float GetArtAchieve()
    {
        var qt = ServiceLocater.Get<IProjectManager>();
        var staffList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        float avgLevel = (float)staffList.Sum(s => s.Level) / staffList.Count;
        var target = _qualityData.targets.Find(t => t.avgLevel == avgLevel);
        float achieve = Attainment(qt.ArtQuality, target.targetArt);
        return achieve;
    }
    
    // 달성률 계산 함수
    public float Attainment(float qa, float target)
    {
        float achieve = (float)Math.Round((qa / target) * 100, 1);
        return achieve;
    }
}
