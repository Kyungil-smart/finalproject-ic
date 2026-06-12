using System.Collections.Generic;

// 구글 시트에서 파싱된 전체 데이터를 임시로 담아둘 묶음 상자 (DTO)
public class FetchedStaffData
{
    public List<StaffRow> Staffs = new ();
    public List<TagRow> Tags = new ();
    public List<LevelStatRow> LevelStats = new ();
    public List<GradeRow> Grades = new ();
    public List<GradeRatioRow> GradeRatios = new ();
    public List<LevelExpRow> LevelExps = new ();
    public List<GetExpRow> GetExps = new ();
}