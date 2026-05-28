using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem; 


// 시트에서 가져온 정보를 저장하기 위한 클래스들

// 스태프 읽기 테이블에서 파싱하는 데이터
[Serializable]
public class StaffRow
{
    public int Staff_ID;
    public string Staff_Name;
    public string Staff_Job;
    public string Staff_Gender;
}

// 태그 읽기 테이블에서 파싱 
[Serializable]
public class TagRow
{
    public int Tag_Id; 
    public int Tag_Type; // Fixed = 1, Added = 2
    public string Tag_Name; 
    public string Tag_A_Effect_Name; // 태그의 효과는 A, B 두개까지 가질 수 있음. 
    public int Tag_A_Effect_Value;
    public float Tag_A_Effect_Ratio;
    public string Tag_B_Effect_Name;
    public int Tag_B_Effect_Value;
    public float Tag_B_Effect_Ratio;
}

// 레벨스텟 읽기
[Serializable]
public class LevelStatRow
{
    public int Level;
    public int Common_Min; // 공통 스탯 최솟값
    public int Common_Max; //
    public int Job_Min; // 직업 스탯 최솟값
    public int Job_Max;
}

// 등급 읽기
[Serializable]
public class GradeRow
{
    public string Grade;       // "D", "C", "B", "A", "S"
    public float Ratio;        // 등장 확률
    public float Stat_Bonus;   // 능력치 배율 가산점
    public int Tag_Min;        // 태그 최소 개수
    public int Tag_Max;        // 태그 최대 
    
    public StaffGrade GradeEnum => (StaffGrade)System.Enum.Parse(typeof(StaffGrade), Grade);
}


public class DataManager : MonoBehaviour
{
    //데이터 창고 : 데이터들 파싱해서 저장 Static으로 선언.
    public static List<StaffRow> StaffList = new List<StaffRow>();
    public static List<TagRow> TagList = new List<TagRow>();
    public static Dictionary<int, LevelStatRow> LevelStatDict = new Dictionary<int, LevelStatRow>();
    public static List<GradeRow> GradeList = new List<GradeRow>();

    private async void Start()
    {
        // 게임이 켜지면 런타임 메모리에 CSV 데이터를 비동기로 로드합니다.
        await LoadAllTablesAsync();
    }

    // 테스트용
    private void Update()
    {
        // T키를 누르면 메모리에 저장된 데이터들 뽑아봄.
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("==== 📊 데이터 로드 테스트 시작 ====");

            // 스태프 데이터 테스트 (첫 번째 직원이 잘 들어왔는지)
            if (StaffList.Count > 0)
            {
                var staff = StaffList[0];
                Debug.Log($"[스태프 테스트] 사번: {staff.Staff_ID} | 이름: {staff.Staff_Name} | 직업: {staff.Staff_Job}");
            }

            // 레벨별 스탯 테스트 (1레벨 스탯 범위가 잘 들어왔는지)
            if (LevelStatDict.ContainsKey(1))
            {
                var level1 = LevelStatDict[1];
                Debug.Log($"[레벨 테스트] 1레벨 공통스탯 범위: {level1.Common_Min} ~ {level1.Common_Max}");
            }

            // 등급 데이터 테스트 (마지막 등급(S)의 확률이 잘 들어왔는지)
            if (GradeList.Count > 0)
            {
                var topGrade = GradeList[GradeList.Count - 1]; // 리스트의 맨 마지막 요소
                Debug.Log($"[등급 테스트] 최고 등급: {topGrade.Grade} | 등장 확률: {topGrade.Ratio} | 보너스: {topGrade.Stat_Bonus}");
            }

            // 태그 데이터 테스트 (첫 번째 태그의 효과가 잘 들어왔는지)
            if (TagList.Count > 0)
            {
                var tag = TagList[0];
                Debug.Log($"[태그 테스트] 태그명: {tag.Tag_Name} | 효과1: {tag.Tag_A_Effect_Name} (+{tag.Tag_A_Effect_Value})");
            }
            
            Debug.Log("====================================");
        }
    }
    
    /// <summary>
    /// 로컬 Resources/Data 폴더에 있는 4개의 CSV 파일을 동시에 비동기로 읽어오는 역할. 
    /// </summary>
    public async UniTask LoadAllTablesAsync()
    {
        Debug.Log("[DataManager] 로컬 CSV 파일 읽기 시작");

        // UniTask.WhenAll은 작업이 끝나면 결과물들을 튜플로 한꺼번에 반환
        var (staffAsset, tagAsset, levelAsset, gradeAsset) = await UniTask.WhenAll(
            LoadCSVAsync("Data/스태프_읽기"),
            LoadCSVAsync("Data/태그_읽기"),
            LoadCSVAsync("Data/레벨별스탯_읽기"),
            LoadCSVAsync("Data/등급_읽기")
        );

        // 읽어온 텍스트 데이터를 쪼개서 메모리 리스트/딕셔너리에 주입
        if (staffAsset != null) ParseStaffData(staffAsset.text);
        if (tagAsset != null) ParseTagData(tagAsset.text);
        if (levelAsset != null) ParseLevelStatData(levelAsset.text);
        if (gradeAsset != null) ParseGradeData(gradeAsset.text);

        Debug.Log($"[DataManager] 모든 데이터 메모리 로드 완료 " +
                  $"(스태프:{StaffList.Count}개, 태그:{TagList.Count}개, 레벨스탯:{LevelStatDict.Count}개, 등급:{GradeList.Count}개)");
    }

    
    // CSV 파싱 로직 
    private void ParseStaffData(string csvText)
    {
        StaffList.Clear();
        // 한 줄씩 Split
        string[] lines = csvText.Replace("\r", "").Split('\n'); 

        for (int i = 1; i < lines.Length; i++) // i = 0 column 헤더 제외하고 읽기. 
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split(',');

            StaffList.Add(new StaffRow {
                Staff_ID = int.Parse(cols[0].Trim()),
                Staff_Name = cols[1].Trim(),
                Staff_Job = cols[2].Trim(),
                Staff_Gender = cols[3].Trim()
            });
        }
    }

    private void ParseTagData(string csvText)
    {
        TagList.Clear();
        string[] lines = csvText.Replace("\r", "").Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split(',');

            TagList.Add(new TagRow {
                Tag_Id = int.Parse(cols[0].Trim()),
                Tag_Type = int.Parse(cols[1].Trim()),
                Tag_Name = cols[2].Trim(),
                Tag_A_Effect_Name = cols[3].Trim(),
                Tag_A_Effect_Value = int.Parse(cols[4].Trim()),
                Tag_A_Effect_Ratio = float.Parse(cols[5].Trim()),
                Tag_B_Effect_Name = cols[6].Trim(),
                Tag_B_Effect_Value = int.Parse(cols[7].Trim()),
                Tag_B_Effect_Ratio = float.Parse(cols[8].Trim())
            });
        }
    }

    private void ParseLevelStatData(string csvText)
    {
        LevelStatDict.Clear();
        string[] lines = csvText.Replace("\r", "").Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split(',');

            int level = int.Parse(cols[0].Trim());
            LevelStatDict.Add(level, new LevelStatRow {
                Level = level,
                Common_Min = int.Parse(cols[1].Trim()),
                Common_Max = int.Parse(cols[2].Trim()),
                Job_Min = int.Parse(cols[3].Trim()),
                Job_Max = int.Parse(cols[4].Trim())
            });
        }
    }

    private void ParseGradeData(string csvText)
    {
        GradeList.Clear();
        string[] lines = csvText.Replace("\r", "").Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split(',');

            GradeList.Add(new GradeRow {
                Grade = cols[0].Trim(),
                Ratio = float.Parse(cols[1].Trim()),
                Stat_Bonus = float.Parse(cols[2].Trim()),
                Tag_Min = int.Parse(cols[3].Trim()),
                Tag_Max = int.Parse(cols[4].Trim())
            });
        }
    }

    
    
    private async UniTask<TextAsset> LoadCSVAsync(string path)
    {
        ResourceRequest request = Resources.LoadAsync<TextAsset>(path);
        await request.ToUniTask(); // 여기서 안전하게 딱 한 번만 기다립니다.
        return request.asset as TextAsset;
    }
    
    
    // ️ 에디터 전용 기능: 인스펙터에서 스크립트 우클릭하면 뜨는 최신 다운로드 버튼
#if UNITY_EDITOR
    // 구글 시트에서 파일 - 공유 - 웹게시 클릭 후 해당하는 시트 선택, CSV형식으로 한뒤에 게시. 나오는 URL 붙여넣기
    [Header("구글 시트 웹 게시 CSV 웹 링크 (URL)")]
    [SerializeField] private string staffSheetUrl = "스태프_읽기_CSV_링크_붙여넣기";
    [SerializeField] private string tagSheetUrl = "태그_읽기_CSV_링크_붙여넣기";
    [SerializeField] private string levelStatSheetUrl = "레벨별스탯_읽기_CSV_링크_붙여넣기";
    [SerializeField] private string gradeSheetUrl = "등급_읽기_CSV_링크_붙여넣기";

    // 스크립트 우클하고 해당 버튼 누르면 실행
    [ContextMenu("구글 시트에서 스태프 데이터 다운로드")]
    public async void DownloadCSVFromInspector()
    {
        Debug.Log("[Editor] 구글 시트 웹 서버에서 최신 CSV 파일 다운로드 요청 시작...");

        // .NET HttpClient를 이용해 실시간으로 다운로드 후 로컬 에셋 폴더에 덮어씁니다.
        await DownloadAndSaveCSVAsync(staffSheetUrl, "스태프_읽기");
        await DownloadAndSaveCSVAsync(tagSheetUrl, "태그_읽기");
        await DownloadAndSaveCSVAsync(levelStatSheetUrl, "레벨별스탯_읽기");
        await DownloadAndSaveCSVAsync(gradeSheetUrl, "등급_읽기");

        // 다운로드가 끝나면 유니티 에디터의 프로젝트 창을 자동으로 새로고침(F5) 해줍니다.
        UnityEditor.AssetDatabase.Refresh();
        
        Debug.Log("[Editor] 모든 시트 파일 다운로드 완료 및 Resources/Data 폴더 갱신 성공");
    }

    private async Task DownloadAndSaveCSVAsync(string url, string fileName)
    {
        if (string.IsNullOrEmpty(url) || url.StartsWith("여기에"))
        {
            Debug.LogWarning($"[{fileName}] 링크 주소가 비어있거나 초기 상태라 다운로드를 보류합니다.");
            return;
        }

        using (HttpClient client = new HttpClient())
        {
            try
            {
                string csvContent = await client.GetStringAsync(url);
                
                // 저장될 로컬 폴더 경로 설정 (Assets/Resources/Data)
                string folderPath = Path.Combine(Application.dataPath, "Resources/Data");
                
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, $"{fileName}.csv");
                await File.WriteAllTextAsync(filePath, csvContent);
                Debug.Log($"[성공] {fileName}.csv 파일 로컬 쓰기 완료.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[실패] {fileName} 다운로드 실패. 에러 원인: {e.Message}");
            }
        }
    }
#endif
}
