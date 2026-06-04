using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem; 

// 시트에서 가져온 정보를 저장하기 위한 직렬화된 클래스들

// 스태프 읽기 테이블에서 파싱하는 데이터
[Serializable]
public class StaffRow
{
    public int Staff_ID;
    public string Staff_Name;
    public string Staff_Name_ID; // 아직 시트에 값이 안들어가있음.
    public string Staff_Job;
    public string Staff_Job_ID;  // 아직 시트에 값이 안들어가있음.
    public string Staff_Gender;
    public string Staff_Gender_ID; // 아직 시트에 값이 안들어가있음.
}

// 태그 읽기 테이블에서 파싱 
[Serializable]
public class TagRow
{
    public int Tag_Id; 
    public int Tag_Type; // Fixed = 1, Added = 2
    public string Tag_Name; 
    public string Tag_Name_ID;     
    public string Tag_Desc_ID;     
    public string Tag_Desc;        
    public string Tag_A_Effect_ID; 
    public string Tag_A_Effect_Name; // 태그의 효과는 A, B 두개 가질 수 있음. 
    public int Tag_A_Effect_Value;
    public float Tag_A_Effect_Ratio;
    public string Tag_B_Effect_ID; 
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
    public int Tag_Min;        // 태그 최소 개수
    public int Tag_Max;        // 태그 최대 
    public float Grade_XP;     
    
    public StaffGrade GradeEnum => (StaffGrade)System.Enum.Parse(typeof(StaffGrade), Grade);
}

// 등급 출현 확률 읽기 (새로 추가된 표..)
[Serializable]
public class GradeRatioRow
{
    public int Level;
    public string Grade;
    public float Ratio;
    
    public StaffGrade GradeEnum => (StaffGrade)System.Enum.Parse(typeof(StaffGrade), Grade);
}

/// <summary>
/// 스태프 원본 정보같은 시트의 내용을 꺼내볼 수 있게 들고 있는 역할
/// </summary>
public class StaffDataManager : MonoBehaviour, IStaffCodex
{
    [Header("구워진 SO들 (베이크 툴로 자동 연결)")]
    [SerializeField] private StaffDataSO staffDataSO;
    [SerializeField] private TagDataSO tagDataSO;
    [SerializeField] private LevelStatDataSO levelStatDataSO;
    [SerializeField] private GradeDataSO gradeDataSO;
    [SerializeField] private GradeRatioDataSO gradeRatioDataSO;
    
    // 데이터들 파싱해서 저장
    public List<StaffRow> StaffList = new List<StaffRow>();  
    public List<TagRow> TagList = new List<TagRow>();
    public Dictionary<int, LevelStatRow> LevelStatDict = new Dictionary<int, LevelStatRow>();
    public List<GradeRow> GradeList = new List<GradeRow>();
    public Dictionary<int, List<GradeRatioRow>> GradeRatioDict = new Dictionary<int, List<GradeRatioRow>>();
    
    private void Awake()
    {
        ServiceLocater.Register<StaffDataManager>(this);
    }
    
    private void Start()
    {
        InitData();
    }
    
    private void OnDestroy()
    {
        ServiceLocater.Unregister<StaffDataManager>(this);
    }

    // 모바일 환경에서도 기획자가 시트 데이터를 실시간으로 동기화할 수 있는 함수
    [ContextMenu("모바일 런타임 시트 실시간 동기화")]
    public void SyncDataFromSheetRuntime()
    {
        SyncDataFromSheetAsync().Forget();
    }

    private async UniTaskVoid SyncDataFromSheetAsync()
    {
        Debug.Log("런타임 실시간 데이터 동기화 시작...");
        
        StaffDataFetcher fetcher = new StaffDataFetcher();
        var fetchedData = await fetcher.FetchAllDataAsync(); //fetchedData에 시트에서 가져온 파싱값들 저장. 

        if (fetchedData == null) return;

        // 적용
        this.StaffList = fetchedData.Staffs;
        this.TagList = fetchedData.Tags;
        this.GradeList = fetchedData.Grades;

        this.GradeRatioDict.Clear();
        foreach (var row in fetchedData.GradeRatios)
        {
            if (!this.GradeRatioDict.ContainsKey(row.Level))
                this.GradeRatioDict[row.Level] = new List<GradeRatioRow>();
            this.GradeRatioDict[row.Level].Add(row);
        }

        this.LevelStatDict.Clear();
        foreach (var stat in fetchedData.LevelStats)
        {
            this.LevelStatDict[stat.Level] = stat;
        }

        Debug.Log("런타임 동기화 완료. 다음 액션부터 수정된 밸런싱이 즉시 적용됩니다.");
    }
    
    

    private void InitData()
    {
        if (staffDataSO != null) StaffList = staffDataSO.staffList;
        if (tagDataSO != null) TagList = tagDataSO.tagList;
        if (gradeDataSO != null) GradeList = gradeDataSO.gradeList;
        
        if (gradeRatioDataSO != null) 
        {
            GradeRatioDict.Clear();
            foreach (var row in gradeRatioDataSO.ratioList)
            {
                if (!GradeRatioDict.ContainsKey(row.Level))
                {
                    GradeRatioDict[row.Level] = new List<GradeRatioRow>();
                }
                GradeRatioDict[row.Level].Add(row);
            }
        }
        
        if (levelStatDataSO != null)
        {
            LevelStatDict.Clear();
            foreach (var stat in levelStatDataSO.levelStatList)
            {
                LevelStatDict[stat.Level] = stat;
            }
        }

        Debug.Log($"모든 데이터 메모리 로드 완료 (스태프:{StaffList.Count}개, 태그:{TagList.Count}개, 레벨스탯:{LevelStatDict.Count}개, 등급:{GradeList.Count}개)");
    }
    
    // 시트에서 받은 전체 스태프들 목록. 데이터 반환형식은 다른 UI 인터페이스와의 호환 을 생각해서 StaffViewData형식으로 작성.
    public List<StaffViewData> GetAllStaffViewDataList()
    {
        List<StaffViewData> viewList = new List<StaffViewData>();

        foreach (var row in StaffList)
        {
            StaffViewData viewData = new StaffViewData();

            // 고정 신상 정보 매핑 (시트 원본 데이터)
            viewData.Staff_ID = row.Staff_ID;
            viewData.Staff_Name = row.Staff_Name;
            viewData.Job_Name = row.Staff_Job;
            viewData.Staff_Gender = (row.Staff_Gender == "남" || row.Staff_Gender == "Male");

            // 가챠 전이므로 알 수 없는 데이터들은 도감같은 것들 용도로 기본값(??? 또는 0) 처리
            viewData.Avatar_ID = 0; 
            viewData.Grade = "???";
            viewData.DISC_Type = "???";
            
            viewData.Current_State = "None";
            viewData.Current_Level = 0;
            viewData.Current_Exp = 0;
            
            viewData.Salary = 0;
            viewData.Hire_Cost = 0;
            
            viewData.Final_Career = 0;
            viewData.Final_Common_Concentration = 0;
            viewData.Final_Common_Creativity = 0;
            viewData.Final_Common_Communication = 0;
            viewData.Final_Job_Planning = 0;
            viewData.Final_Job_Development = 0;
            viewData.Final_Job_Art = 0;
            
            viewData.All_Tags = new List<int>();

            viewList.Add(viewData);
        }

        return viewList;
    }
    
    
    [ContextMenu("Load Data Test")]
    public void TestLoadData()
    {
        Debug.Log("데이터 로드 테스트 시작");

        if (StaffList.Count > 0)
        {
            var staff = StaffList[0];
            Debug.Log($"[스태프 테스트] 사번: {staff.Staff_ID} | 이름: {staff.Staff_Name} | 직업: {staff.Staff_Job}");
        }

        if (LevelStatDict.ContainsKey(1))
        {
            var level1 = LevelStatDict[1];
            Debug.Log($"[레벨 테스트] 1레벨 공통스탯 범위: {level1.Common_Min} ~ {level1.Common_Max}");
        }

        if (GradeList.Count > 0)
        {
            var topGrade = GradeList[GradeList.Count - 1]; 
            Debug.Log($"[등급 테스트] 최고 등급: {topGrade.Grade} | 경험치 배율(XP): {topGrade.Grade_XP} | 태그 수: {topGrade.Tag_Min}~{topGrade.Tag_Max}");
        }
            
        if (GradeRatioDict.ContainsKey(1))
        {
            var level1Ratios = GradeRatioDict[1];
            var sGradeRatio = level1Ratios.Find(x => x.Grade == "S");
            if (sGradeRatio != null)
            {
                Debug.Log($"[가챠 확률 테스트] 1레벨 S등급 등장 확률: {sGradeRatio.Ratio}");
            }
        }

        if (TagList.Count > 0)
        {
            var tag = TagList[0];
            Debug.Log($"[태그 테스트] 태그명: {tag.Tag_Name} | 효과1: {tag.Tag_A_Effect_Name} (+{tag.Tag_A_Effect_Value})");
        }
    }
}