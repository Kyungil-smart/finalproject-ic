using UnityEngine;
using Cysharp.Threading.Tasks;
using DataDispatcher;
using Channel = DataDispatcher.Channel;

// 모바일/에디터 공용
public class StaffDataFetcher
{
    private const string SHEET_ID = "1kkqhlNdWRSD3DCiUQlRI0cTI6uT9WmA7jKO4kawTdOo"; // 시트자체 ID 
    private const string STAFF_GID = "0"; // 각 탭별 GID
    private const string TAG_GID = "393559418";
    private const string LEVEL_STAT_GID = "1433961869";
    private const string GRADE_GID = "1616210531";
    private const string GRADE_RATIO_GID = "671267036";
    private const string LEVEL_EXP_GID = "264636132";

    private string GetString(string textId)
    {
        var postManager = ServiceLocater.Get<IPostManager>();
        if (postManager == null)
        {
            Debug.LogError("유니티 게임 실행을 한 후 진행 바랍니다.");
            return null;
        }
        return postManager.Request<int, string>(Channel.GetUIText, int.Parse(textId));
    }
    public async UniTask<FetchedStaffData> FetchAllDataAsync()
    {
        var result = new FetchedStaffData();
        Debug.Log("[StaffDataFetcher] 구글 시트 데이터 다운로드 및 파싱 시작...");

        Debug.Log("[StaffDataFetcher] Staff Data ... ");
        // 1. 스태프 데이터 파싱
        GSheetManager staffSheet = new GSheetManager(SHEET_ID, STAFF_GID);
        await UniTask.WaitUntil(() => staffSheet.IsDownload);
        foreach (var row in staffSheet.GetData())
        {
            result.Staffs.Add(new StaffRow {
                Staff_ID = int.Parse(row["Staff_ID"]),
                
                Staff_Name_ID = row["Staff_Name_ID"],
                Staff_Name = GetString(row["Staff_Name_ID"]),
                
                Staff_Job_ID = row["Staff_Job_ID"],
                Staff_Job = GetString(row["Staff_Job_ID"]),
                
                Staff_Gender_ID = row["Staff_Gender_ID"],
                Staff_Gender = GetString(row["Staff_Gender_ID"])
            });
        }
        Debug.Log("[StaffDataFetcher] Staff Data ... Done");

        Debug.Log("[StaffDataFetcher] Tag Data ... ");
        // 2. 태그 데이터 파싱
        GSheetManager tagSheet = new GSheetManager(SHEET_ID, TAG_GID);
        await UniTask.WaitUntil(() => tagSheet.IsDownload);
        foreach (var row in tagSheet.GetData())
        {
            result.Tags.Add(new TagRow {
                Tag_Id = int.Parse(row["Tag_ID"]),
                Tag_Type = int.Parse(row["Tag_Type"]),
                
                Tag_Name_ID = row["Tag_Name_ID"],
                Tag_Name = GetString(row["Tag_Name_ID"]),
                
                // Tag_Desc_ID = row["Tag_Desc_ID"],
                // Tag_Desc = GetString(row["Tag_Desc_ID"]),
                
                Tag_A_Effect_Name = row["Tag_A_Effect_Name"],
                Tag_A_Effect_Value = int.Parse(row["Tag_A_Effect_Value"]),
                Tag_A_Effect_Ratio = float.Parse(row["Tag_A_Effect_Ratio"]),
                
                Tag_B_Effect_Name = row["Tag_B_Effect_Name"],
                Tag_B_Effect_Value = int.Parse(row["Tag_B_Effect_Value"]),
                Tag_B_Effect_Ratio = float.Parse(row["Tag_B_Effect_Ratio"])
            });
        }
        Debug.Log("[StaffDataFetcher] Tag Data ... Done");

        Debug.Log("[StaffDataFetcher] Level Stat Data ... ");
        // 3. 레벨 스탯 파싱
        GSheetManager levelSheet = new GSheetManager(SHEET_ID, LEVEL_STAT_GID);
        await UniTask.WaitUntil(() => levelSheet.IsDownload);
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
        Debug.Log("[StaffDataFetcher] Level State Data ... Done");

        Debug.Log("[StaffDataFetcher] Grade Data ... ");
        // 4. 등급 파싱
        GSheetManager gradeSheet = new GSheetManager(SHEET_ID, GRADE_GID);
        await UniTask.WaitUntil(() => gradeSheet.IsDownload);
        foreach (var row in gradeSheet.GetData())
        {
            result.Grades.Add(new GradeRow {
                Grade = row["Grade"],
                Tag_Min = int.Parse(row["Tag_Min"]),
                Tag_Max = int.Parse(row["Tag_Max"]),
                Grade_XP = float.Parse(row["Grade_XP"])
            });
        }
        Debug.Log("[StaffDataFetcher] Grade Data ... Done");

        Debug.Log("[StaffDataFetcher] Grade Ratio Data ... ");
        // 5. 등급 비율 파싱
        GSheetManager ratioSheet = new GSheetManager(SHEET_ID, GRADE_RATIO_GID);
        await UniTask.WaitUntil(() => ratioSheet.IsDownload);
        foreach (var row in ratioSheet.GetData())
        {
            result.GradeRatios.Add(new GradeRatioRow()
            {
                Level = int.Parse(row["Level"]),
                Grade = row["Grade"],
                Ratio = float.Parse(row["Ratio"]),
            });
        }
        Debug.Log("[StaffDataFetcher] Grade Ratio Data ... Done");
        
        Debug.Log("[StaffDataFetcher] Level Exp Data ... Done");
        // 6. Level Exp 파싱
        GSheetManager levelExpSheet = new GSheetManager(SHEET_ID, LEVEL_EXP_GID);
        await UniTask.WaitUntil(() => levelExpSheet.IsDownload);
        foreach (var row in levelExpSheet.GetData())
        {
            result.LevelExps.Add(new LevelExpRow() {
                level = int.Parse(row["Level"]),
                requiredExp = int.Parse(row["Required_XP"]),
                cumulativeExp = int.Parse(row["Cumulative_XP"]),
                isTag = row["Is_Tag"].ToUpper() == "TRUE"
            });
        }
        Debug.Log("[StaffDataFetcher] Staff Data ... Done");
        
        

        return result;
    }
}