#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.IO;

/// <summary>
/// 구글에서 GSheetManager로 다운 받은 파싱된 딕셔너리를 받아서 SO로 만듦
/// </summary>
public static class StaffDataBakeTool
{
    private const string SHEET_ID = "1IuTAkokLxpxIBwdIwTID_xZ0_3VCJKKEjA1GAd2Q_ec";
    private const string STAFF_GID = "0";
    private const string TAG_GID = "708511556";
    private const string LEVEL_STAT_GID = "563784454";
    private const string GRADE_GID = "123973711";
    private const string GRADE_RATIO_GID = "1946541515";
    
    [MenuItem("CONTEXT/DataManager/시트의 스태프 관련 데이터 SO 굽기")]
    public static void BakeFromContextMenu(MenuCommand command)
    {
        DataManager targetManager = (DataManager)command.context;
        BakeAndSetupAllDataAsync(targetManager).Forget();
    }

    private static async UniTaskVoid BakeAndSetupAllDataAsync(DataManager manager)
    {
        // 저장할 베이스 폴더 및 각각의 서브 폴더 생성 로직
        string basePath = "Assets/SOAssets/StaffSO";
        string[] subFolders = { "Staff", "Tag", "LevelStat", "Grade", "GradeRatio" };
        
        bool folderCreated = false;
        
        // 베이스 폴더 확인
        if (!Directory.Exists(basePath)) 
        { 
            Directory.CreateDirectory(basePath); 
            folderCreated = true; 
        }

        // 각각의 서브 폴더 확인 및 생성
        foreach(var sub in subFolders)
        {
            string path = $"{basePath}/{sub}";
            if (!Directory.Exists(path)) 
            { 
                Directory.CreateDirectory(path); 
                folderCreated = true; 
            }
        }

        // 폴더를 새로 만들었다면 에셋 데이터베이스 새로고침 (에러 방지)
        if (folderCreated) AssetDatabase.Refresh();


        // -------------------------------------------------------------
        // 스태프 데이터 굽기
        GSheetManager staffSheet = new GSheetManager(SHEET_ID, STAFF_GID);
        while (!staffSheet.IsDownload) await UniTask.Yield();
        var staffRawData = staffSheet.GetData();
        
        string staffAssetPath = $"{basePath}/Staff/StaffDataSO.asset"; // 지정된 서브폴더로 경로 변경
        StaffDataSO staffSO = GetOrCreateSO<StaffDataSO>(staffAssetPath);
        staffSO.staffList.Clear();
        foreach (var row in staffRawData)
        {
            staffSO.staffList.Add(new StaffRow {
                Staff_ID = int.Parse(row["Staff_ID"]),
                Staff_Name = row["Staff_Name"],
                Staff_Name_ID = row["Staff_Name_ID"],
                Staff_Job = row["Staff_Job"],
                Staff_Job_ID = row["Staff_Job_ID"],
                Staff_Gender = row["Staff_Gender"],
                Staff_Gender_ID = row["Staff_Gender_ID"]
            });
        }
        EditorUtility.SetDirty(staffSO);
        
        // -------------------------------------------------------------
        // 태그 데이터 굽기
        GSheetManager tagSheet = new GSheetManager(SHEET_ID, TAG_GID);
        while (!tagSheet.IsDownload) await UniTask.Yield();
        var tagRawData = tagSheet.GetData();
        
        string tagAssetPath = $"{basePath}/Tag/TagDataSO.asset"; // 지정된 서브폴더로 경로 변경
        TagDataSO tagSO = GetOrCreateSO<TagDataSO>(tagAssetPath);
        tagSO.tagList.Clear();
        foreach (var row in tagRawData)
        {
            tagSO.tagList.Add(new TagRow {
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
        EditorUtility.SetDirty(tagSO);
        
        // -------------------------------------------------------------
        // 레벨별 스탯 데이터 굽기
        GSheetManager levelSheet = new GSheetManager(SHEET_ID, LEVEL_STAT_GID);
        while (!levelSheet.IsDownload) await UniTask.Yield();
        var levelRawData = levelSheet.GetData();
        
        string levelAssetPath = $"{basePath}/LevelStat/LevelStatDataSO.asset"; // 지정된 서브폴더로 경로 변경
        LevelStatDataSO levelSO = GetOrCreateSO<LevelStatDataSO>(levelAssetPath);
        levelSO.levelStatList.Clear();
        foreach (var row in levelRawData)
        {
            levelSO.levelStatList.Add(new LevelStatRow {
                Level = int.Parse(row["Level"]),
                Common_Min = int.Parse(row["Common_Min"]),
                Common_Max = int.Parse(row["Common_Max"]),
                Job_Min = int.Parse(row["Job_Min"]),
                Job_Max = int.Parse(row["Job_Max"])
            });
        }
        EditorUtility.SetDirty(levelSO);

        // -------------------------------------------------------------
        // 등급 데이터 굽기
        GSheetManager gradeSheet = new GSheetManager(SHEET_ID, GRADE_GID);
        while (!gradeSheet.IsDownload) await UniTask.Yield();
        var gradeRawData = gradeSheet.GetData();
        
        string gradeAssetPath = $"{basePath}/Grade/GradeDataSO.asset"; // 지정된 서브폴더로 경로 변경
        GradeDataSO gradeSO = GetOrCreateSO<GradeDataSO>(gradeAssetPath);
        gradeSO.gradeList.Clear();
        foreach (var row in gradeRawData)
        {
            gradeSO.gradeList.Add(new GradeRow {
                Grade = row["Grade"],
                Tag_Min = int.Parse(row["Tag_Min"]),
                Tag_Max = int.Parse(row["Tag_Max"]),
                Grade_XP = float.Parse(row["Grade_XP"])
            });
        }
        EditorUtility.SetDirty(gradeSO);

        // -------------------------------------------------------------
        // 등급 출현 확률 데이터 굽기
        GSheetManager ratioSheet = new GSheetManager(SHEET_ID, GRADE_RATIO_GID);
        while (!ratioSheet.IsDownload) await UniTask.Yield();
        var ratioRawData = ratioSheet.GetData();
        
        string ratioAssetPath = $"{basePath}/GradeRatio/GradeRatioDataSO.asset"; // 지정된 서브폴더로 경로 변경
        GradeRatioDataSO ratioSO = GetOrCreateSO<GradeRatioDataSO>(ratioAssetPath);
        ratioSO.ratioList.Clear();
        foreach (var row in ratioRawData)
        {
            ratioSO.ratioList.Add(new GradeRatioRow {
                Level = int.Parse(row["Level"]),
                Grade = row["Grade"],
                Ratio = float.Parse(row["Ratio"])
            });
        }
        EditorUtility.SetDirty(ratioSO);
        
        // -------------------------------------------------------------
        // 변경사항 일괄 저장 및 씬 반영
        AssetDatabase.SaveAssets();
        AutoConnectAllToDataManager(manager, staffSO, tagSO, levelSO, gradeSO, ratioSO);

        Debug.Log("[BakeTool] 모든 데이터 서브폴더별 SO 굽기 및 연결 완료");
    }

    private static T GetOrCreateSO<T>(string path) where T : ScriptableObject
    {
        T so = AssetDatabase.LoadAssetAtPath<T>(path);
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, path);
        }
        return so;
    }

    private static void AutoConnectAllToDataManager(DataManager manager, StaffDataSO staffSO, TagDataSO tagSO, LevelStatDataSO levelSO, GradeDataSO gradeSO, GradeRatioDataSO ratioSO)
    {
        SerializedObject serializedManager = new SerializedObject(manager);
        
        SerializedProperty staffProp = serializedManager.FindProperty("staffDataSO");
        SerializedProperty tagProp = serializedManager.FindProperty("tagDataSO");
        SerializedProperty levelProp = serializedManager.FindProperty("levelStatDataSO");
        SerializedProperty gradeProp = serializedManager.FindProperty("gradeDataSO");
        SerializedProperty ratioProp = serializedManager.FindProperty("gradeRatioDataSO"); 
        
        if (staffProp != null) staffProp.objectReferenceValue = staffSO;
        if (tagProp != null) tagProp.objectReferenceValue = tagSO;
        if (levelProp != null) levelProp.objectReferenceValue = levelSO;
        if (gradeProp != null) gradeProp.objectReferenceValue = gradeSO;
        if (ratioProp != null) ratioProp.objectReferenceValue = ratioSO; 

        serializedManager.ApplyModifiedProperties();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
    }
}
#endif