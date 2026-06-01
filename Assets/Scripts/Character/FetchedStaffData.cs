using System.Collections.Generic;

// 구글 시트에서 파싱된 전체 데이터를 임시로 담아둘 묶음 상자 (DTO)
public class FetchedStaffData
{
    public List<StaffRow> Staffs = new List<StaffRow>();
    public List<TagRow> Tags = new List<TagRow>();
    public List<LevelStatRow> LevelStats = new List<LevelStatRow>();
    public List<GradeRow> Grades = new List<GradeRow>();
    public List<GradeRatioRow> GradeRatios = new List<GradeRatioRow>();
}