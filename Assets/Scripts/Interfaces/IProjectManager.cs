using System.Collections.Generic;

public interface IProjectManager
{
    // Data Update Part
    public float TotalQuality { get; }
    public float DevQuality {get; set;}
    public float ArtQuality {get; set;}
    public float DesignQuality {get; set;}
    public NameTag Genre {get; set;}
    public NameTag Theme {get; set;}
    public NameTag TrendGenre { get; set; }
    public NameTag TrendTheme { get; set; }
    public AwardsData Awards { get; }
    public uint Cost {get; set;}
    public uint Income {get; set;}
    public uint StaffsCost {get; }
    public uint Earnings { get; }
    public uint MarketingCost { get; set; }
    public uint MarketingBonus { get; set; }
    public float QAResult  {get; set;}

    public void UpdateTotalQuality(float value, float ratio = 1f);
    public void UpdateDesignQuality(float value, float ratio = 1f);
    public void UpdateArtQuality(float value, float ratio = 1f);
    public void UpdateDevQuality(float value, float ratio = 1f);
    
    // Method Part
    public void SetProjectName(string projectName);
    public void NewProject();
    public ProjectData FinishProject();
    public void LoadProject(string jsonData);
    public string ToJsonData();
    public ProjectData GetProjectData();
    public void SetAwards(AwardsData awardsData);
    public ProjectCostCalculate CostCalculator {get;}

    // Staff Part
    public void SetStaffsCost();  // 직원 고용 마지막에 한번 실행해놓기.
    public void AssignStaff(GameDevProcName procName, int staffId);
    public void ClearStaffs();
    public IReadOnlyList<int> GetAssignedStaffIds(GameDevProcName procName);
    public ProjectManagerSaveData CaptureSaveData();
    public void RestoreSaveData(ProjectManagerSaveData dto);
}