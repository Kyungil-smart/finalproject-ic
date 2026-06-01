using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

// 모바일/에디터 공용
public class StaffDataFetcher
{
    private const string SHEET_ID = "1IuTAkokLxpxIBwdIwTID_xZ0_3VCJKKEjA1GAd2Q_ec";
    private const string STAFF_GID = "0";
    private const string TAG_GID = "708511556";
    private const string LEVEL_STAT_GID = "563784454";
    private const string GRADE_GID = "123973711";
    private const string GRADE_RATIO_GID = "1946541515";
    
    public async UniTask<FetchedStaffData> FetchAllDataAsync()
    {
        var result = new FetchedStaffData();
        
        Debug.Log("[StaffDataFetcher] 구글 시트 데이터 다운로드 및 파싱 시작...");

        // 1. 스태프 데이터 파싱
        GSheetManager staffSheet = new GSheetManager(SHEET_ID, STAFF_GID);
        while (!staffSheet.IsDownload) await UniTask.Yield();
        foreach (var row in staffSheet.GetData())
        {
            result.Staffs.Add(new StaffRow {
                Staff_ID = int.Parse(row["Staff_ID"]),
                Staff_Name = row["Staff_Name"],
                Staff_Name_ID = row["Staff_Name_ID"],
                Staff_Job = row["Staff_Job"],
                Staff_Job_ID = row["Staff_Job_ID"],
                Staff_Gender = row["Staff_Gender"],
                Staff_Gender_ID = row["Staff_Gender_ID"]
            });
        }

        // 2. 태그 데이터 파싱
        GSheetManager tagSheet = new GSheetManager(SHEET_ID, TAG_GID);
        while (!tagSheet.IsDownload) await UniTask.Yield();
        foreach (var row in tagSheet.GetData())
        {
            result.Tags.Add(new TagRow {
                Tag_Id = int.Parse(row["Tag_Id"]),
                Tag_Type = int.Parse(row["Tag_Type"]),
                Tag_Name = row["Tag_Name"],
                Tag_Name_ID = row["Tag_Name_ID"],
                Tag_Desc_ID = row["Tag_Desc_ID"],
                Tag_Desc = row["Tag_Desc"],
                Tag_A_Effect_ID = row["Tag_A_Effect_ID"],
                Tag_A_Effect_Name = row["Tag_A_Effect_Name"],
                Tag_A_Effect_Value = int.Parse(row["Tag_A_Effect_Value"]),
                Tag_A_Effect_Ratio = float.Parse(row["Tag_A_Effect_Ratio"]),
                Tag_B_Effect_ID = row["Tag_B_Effect_ID"],
                Tag_B_Effect_Name = row["Tag_B_Effect_Name"],
                Tag_B_Effect_Value = int.Parse(row["Tag_B_Effect_Value"]),
                Tag_B_Effect_Ratio = float.Parse(row["Tag_B_Effect_Ratio"])
            });
        }

        // 3. 레벨 스탯 파싱
        GSheetManager levelSheet = new GSheetManager(SHEET_ID, LEVEL_STAT_GID);
        while (!levelSheet.IsDownload) await UniTask.Yield();
        foreach (var row in levelSheet.GetData())
        {
            result.LevelStats.Add(new LevelStatRow {
                Level = int.Parse(row["Level"]),
                Common_Min = int.Parse(row["Common_Min"]),
                Common_Max = int.Parse(row["Common_Max"]),
                Job_Min = int.Parse(row["Job_Min"]),
                Job_Max = int.Parse(row["Job_Max"])
            });
        }

        // 4. 등급 파싱
        GSheetManager gradeSheet = new GSheetManager(SHEET_ID, GRADE_GID);
        while (!gradeSheet.IsDownload) await UniTask.Yield();
        foreach (var row in gradeSheet.GetData())
        {
            result.Grades.Add(new GradeRow {
                Grade = row["Grade"],
                Tag_Min = int.Parse(row["Tag_Min"]),
                Tag_Max = int.Parse(row["Tag_Max"]),
                Grade_XP = float.Parse(row["Grade_XP"])
            });
        }

        // 5. 등급 비율 파싱
        GSheetManager ratioSheet = new GSheetManager(SHEET_ID, GRADE_RATIO_GID);
        while (!ratioSheet.IsDownload) await UniTask.Yield();
        foreach (var row in ratioSheet.GetData())
        {
            result.GradeRatios.Add(new GradeRatioRow {
                Level = int.Parse(row["Level"]),
                Grade = row["Grade"],
                Ratio = float.Parse(row["Ratio"])
            });
        }

        return result;
    }
}