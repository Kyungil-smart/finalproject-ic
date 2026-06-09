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
    public uint Cost {get; set;}
    public uint Income {get; set;}
    public void UpdateTotalQuality(float value);
    
    // Method Part
    public void NewProject(string projectName);
    public ProjectData FinishProject();
    public void LoadProject(string jsonData);
    public string ToJsonData();
    public ProjectData GetProjectData();
    
    // Staff Part
    public void AssignStaff(int staffId);
    public void ClearStaffs();
    public IReadOnlyList<int> GetAssignedStaff();
}