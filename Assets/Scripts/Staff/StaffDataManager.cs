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

[Serializable]
public class LevelExpRow
{
    public int level;
    public int requiredExp;
    public int cumulativeExp;
    public bool isTag;
}

/// <summary>
/// 스태프 원본 정보같은 시트의 내용을 꺼내볼 수 있게 들고 있는 역할
/// 시작 시에는 SO의 데이터를 참조해서 시트의 전체 내용을 가져오고. (InitData)
/// 나중에 런타임 중에는 구글 시트에서 바로 가져옴. (아직 런타임 중 SO에 저장은 구현 X, SyncDataFromSheetAsync)
/// </summary>
public class StaffDataManager : MonoBehaviour, IStaffDataManager, IStaffCodex
{
    [Header("구워진 SO들 (베이크 툴로 자동 연결)")]
    [SerializeField] private StaffDataSO staffDataSO;
    [SerializeField] private TagDataSO tagDataSO;
    [SerializeField] private LevelStatDataSO levelStatDataSO;
    [SerializeField] private GradeDataSO gradeDataSO;
    [SerializeField] private GradeRatioDataSO gradeRatioDataSO;
    [SerializeField] private LevelExpSO levelExpSo;
    
    // 데이터들 파싱해서 저장. 이 리스트, 딕셔너리 들에 시트의 내용들 전체 저장. 
    private List<StaffRow> _staffList = new ();
    private List<TagRow> _tagList = new ();
    private Dictionary<int, LevelStatRow> _levelStatDict = new ();
    private List<GradeRow> _gradeList = new ();
    private Dictionary<int, List<GradeRatioRow>> _gradeRatioDict = new ();
    private List<LevelExpRow> _levelExpList = new ();
    
    public List<StaffRow> StaffList => _staffList;
    public List<TagRow> TagList => _tagList;
    public Dictionary<int, LevelStatRow> LevelStatsDict => _levelStatDict;
    public List<GradeRow> GradeList => _gradeList;
    public Dictionary<int, List<GradeRatioRow>> GradeRatiosDict => _gradeRatioDict;
    public List<LevelExpRow> LevelExpList => _levelExpList;


    private void Awake()
    {
        ServiceLocater.Register<IStaffDataManager>(this);
    }
    
    private void Start()
    {
        InitData();
    }
    
    private void OnDestroy()
    {
        ServiceLocater.Unregister<IStaffDataManager>(this);
    }


    // 초기 데이터는 SO에서 가져오고 실시간 데이터는 시트에서 StaffDataFetcher를 통해서 가져옴.
    // SO에 저장은 아직 안하는 중. 변경하려면 #if UnityEditor 전처리 기문을 사용해야 해서 아직은 고민중.  
    private async UniTaskVoid SyncDataFromSheetAsync()
    {
        Debug.Log("런타임 실시간 데이터 동기화 시작...");
        
        StaffDataFetcher fetcher = new StaffDataFetcher();
        var fetchedData = await fetcher.FetchAllDataAsync(); //fetchedData에 시트에서 가져온 파싱값들 저장. 

        if (fetchedData == null) return;

        // 적용
        staffDataSO.staffList.Clear();
        staffDataSO.staffList = fetchedData.Staffs;
        tagDataSO.tagList.Clear();
        tagDataSO.tagList = fetchedData.Tags;
        gradeDataSO.gradeList.Clear();
        gradeDataSO.gradeList = fetchedData.Grades;
        gradeRatioDataSO.ratioList.Clear();
        gradeRatioDataSO.ratioList = fetchedData.GradeRatios;
        levelStatDataSO.levelStatList.Clear();
        levelStatDataSO.levelStatList = fetchedData.LevelStats;
        levelExpSo.levelExpList.Clear();
        levelExpSo.levelExpList = fetchedData.LevelExps;
        Debug.Log("런타임 실시간 데이터 동기화 시작... 완료");
        
        _gradeRatioDict.Clear();
        foreach (var row in fetchedData.GradeRatios)
        {
            if (!_gradeRatioDict.ContainsKey(row.Level))
                _gradeRatioDict[row.Level] = new List<GradeRatioRow>();
            _gradeRatioDict[row.Level].Add(row);
        }
    }
    

    private void InitData()
    {
        if (staffDataSO != null) _staffList = staffDataSO.staffList;
        if (tagDataSO != null) _tagList = tagDataSO.tagList;
        if (gradeDataSO != null) _gradeList = gradeDataSO.gradeList;
        
        if (gradeRatioDataSO != null) 
        {
            _gradeRatioDict.Clear();
            foreach (var row in gradeRatioDataSO.ratioList)
            {
                if (!_gradeRatioDict.ContainsKey(row.Level))
                {
                    _gradeRatioDict[row.Level] = new List<GradeRatioRow>();
                }
                _gradeRatioDict[row.Level].Add(row);
            }
        }
        
        if (levelStatDataSO != null)
        {
            _levelStatDict.Clear();
            foreach (var stat in levelStatDataSO.levelStatList)
            {
                _levelStatDict[stat.Level] = stat;
            }
        }

        if (levelExpSo != null)
        {
            _levelExpList.Clear();
            _levelExpList = levelExpSo.levelExpList;
        }
        Debug.Log($"모든 데이터 메모리 로드 완료 (스태프:{_staffList.Count}개, 태그:{_tagList.Count}개, 레벨스탯:{_levelStatDict.Count}개, 등급:{_gradeList.Count}개)");
    }
    
    // 시트에서 받은 전체 스태프들 목록. 데이터 반환형식은 다른 UI 인터페이스와의 호환 을 생각해서 StaffViewData형식으로 작성.
    public List<StaffViewData> GetAllStaffViewDataList()
    {
        List<StaffViewData> viewList = new List<StaffViewData>();

        foreach (var row in _staffList)
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
    
    // 모바일 환경에서도 기획자가 시트 데이터를 실시간으로 동기화할 수 있는 함수
    [ContextMenu("모바일 런타임 시트 실시간 동기화")]
    public void SyncDataFromSheetRuntime()
    {
        SyncDataFromSheetAsync().Forget();
    }
}