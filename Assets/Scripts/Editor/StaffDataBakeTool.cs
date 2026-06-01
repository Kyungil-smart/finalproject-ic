#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.IO;

/// <summary>
/// StafDataFetcher로 데이터를 받아와서 SO 파일로 굽는 역할만 수행하게 변경.
/// </summary>
public static class StaffDataBakeTool
{
    [MenuItem("CONTEXT/StaffDataManager/시트의 스태프 관련 데이터 SO 굽기")]
    public static void BakeFromContextMenu(MenuCommand command)
    {
        StaffDataManager targetManager = (StaffDataManager)command.context;
        BakeAndSetupAllDataAsync(targetManager).Forget();
    }

    private static async UniTaskVoid BakeAndSetupAllDataAsync(StaffDataManager manager)
    {
        // 저장할 베이스 폴더 생성 로직
        string basePath = "Assets/SOAssets/StaffSO";
        string[] subFolders = { "Staff", "Tag", "LevelStat", "Grade", "GradeRatio" };
        
        bool folderCreated = false;
        if (!Directory.Exists(basePath)) { Directory.CreateDirectory(basePath); folderCreated = true; }
        
        foreach(var sub in subFolders)
        {
            string path = $"{basePath}/{sub}";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); folderCreated = true; }
        }

        if (folderCreated) AssetDatabase.Refresh();

        // 파싱된 데이터 받아오기
        StaffDataFetcher fetcher = new StaffDataFetcher();
        var fetchedData = await fetcher.FetchAllDataAsync();

        // 받아온 데이터를 SO에 덮어씌우기
        StaffDataSO staffSO = GetOrCreateSO<StaffDataSO>($"{basePath}/Staff/StaffDataSO.asset");
        staffSO.staffList = fetchedData.Staffs;
        EditorUtility.SetDirty(staffSO);
        
        TagDataSO tagSO = GetOrCreateSO<TagDataSO>($"{basePath}/Tag/TagDataSO.asset");
        tagSO.tagList = fetchedData.Tags;
        EditorUtility.SetDirty(tagSO);
        
        LevelStatDataSO levelSO = GetOrCreateSO<LevelStatDataSO>($"{basePath}/LevelStat/LevelStatDataSO.asset");
        levelSO.levelStatList = fetchedData.LevelStats;
        EditorUtility.SetDirty(levelSO);

        GradeDataSO gradeSO = GetOrCreateSO<GradeDataSO>($"{basePath}/Grade/GradeDataSO.asset");
        gradeSO.gradeList = fetchedData.Grades;
        EditorUtility.SetDirty(gradeSO);

        GradeRatioDataSO ratioSO = GetOrCreateSO<GradeRatioDataSO>($"{basePath}/GradeRatio/GradeRatioDataSO.asset");
        ratioSO.ratioList = fetchedData.GradeRatios;
        EditorUtility.SetDirty(ratioSO);
        
        // 변경사항 일괄 저장 및 씬 반영
        AssetDatabase.SaveAssets();
        AutoConnectAllToDataManager(manager, staffSO, tagSO, levelSO, gradeSO, ratioSO);

        Debug.Log("모든 데이터 SO 굽기 완료");
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

    private static void AutoConnectAllToDataManager(StaffDataManager manager, StaffDataSO staffSO, TagDataSO tagSO, LevelStatDataSO levelSO, GradeDataSO gradeSO, GradeRatioDataSO ratioSO)
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