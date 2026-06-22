using System.Collections.Generic;

public interface IStaffDataManager
{
    public List<StaffRow> StaffList { get; }
    public List<TagRow> TagList { get; }
    public Dictionary<int, LevelStatRow> LevelStatsDict { get; }
    public List<GradeRow> GradeList { get; }
    public Dictionary<int, List<GradeRatioRow>> GradeRatiosDict { get; }
    public List<LevelExpRow> LevelExpList { get; }
    public List<GetExpRow> GetExpList { get; }
    public List<SynergyRow> SynergyList { get; }
}