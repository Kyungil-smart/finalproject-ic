using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

// 시트에서 가져온 정보를 저장하기 위한 직렬화된 클래스들

// 스태프 읽기 테이블에서 파싱하는 데이터
[Serializable]
public class StaffRow
{
    public int Staff_ID;
    public string Staff_Name;
    public string Staff_Name_ID; 
    public string Staff_Job;
    public string Staff_Job_ID;  
    public string Staff_Gender;
    public string Staff_Gender_ID; 
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
    public string Tag_A_Effect_Name; // 태그의 효과는 A, B 두개까지 가질 수 있음. 
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

public class DataManager : MonoBehaviour
{
    [Header("구워진 SO들 (베이크  툴로 자동 연결")]
    [SerializeField] private StaffDataSO staffDataSO;
    [SerializeField] private TagDataSO tagDataSO;
    [SerializeField] private LevelStatDataSO levelStatDataSO;
    [SerializeField] private GradeDataSO gradeDataSO;
    [SerializeField] private GradeRatioDataSO gradeRatioDataSO;
    
    //데이터 창고 : 데이터들 파싱해서 저장 Static으로 선언.
    public static List<StaffRow> StaffList = new List<StaffRow>();
    public static List<TagRow> TagList = new List<TagRow>();
    public static Dictionary<int, LevelStatRow> LevelStatDict = new Dictionary<int, LevelStatRow>();
    public static List<GradeRow> GradeList = new List<GradeRow>();
    public static Dictionary<int, List<GradeRatioRow>> GradeRatioDict = new Dictionary<int, List<GradeRatioRow>>();
    
    private void Start()
    {
        // 게임이 켜지면 인스펙터에 연결된 SO 데이터를 즉시 메모리에 초기화
        InitData();
    }

    // 테스트용
    private void Update()
    {
        // T키를 누르면 메모리에 저장된 데이터들 뽑아봄.
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("==== 데이터 로드 테스트 시작 ====");

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

            // 등급 데이터 테스트 (마지막 등급 S의 확률이 잘 들어왔는지)
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

            // 태그 데이터 테스트 (첫 번째 태그의 효과가 잘 들어왔는지)
            if (TagList.Count > 0)
            {
                var tag = TagList[0];
                Debug.Log($"[태그 테스트] 태그명: {tag.Tag_Name} | 효과1: {tag.Tag_A_Effect_Name} (+{tag.Tag_A_Effect_Value})");
            }
            
            Debug.Log("====================================");
        }
    }
    
    
    // 인스펙터에 미리 연결된 SO 파일에서 데이터를 꺼내 static 변수에 담아주는 역할 
    private void InitData()
    {
        // 로드한 SO의 데이터를 static 변수에 넣기 
        if (staffDataSO != null) StaffList = staffDataSO.staffList;
        if (tagDataSO != null) TagList = tagDataSO.tagList;
        if (gradeDataSO != null) GradeList = gradeDataSO.gradeList;
        if (gradeRatioDataSO != null) // 등급 출현 확률 초기화는 레벨별로 묶어서 저장
        {
            GradeRatioDict.Clear();
            foreach (var row in gradeRatioDataSO.ratioList)
            {
                // 해당 레벨의 키가 없으면 새 리스트 생성
                if (!GradeRatioDict.ContainsKey(row.Level))
                {
                    GradeRatioDict[row.Level] = new List<GradeRatioRow>();
                }
                // 리스트에 데이터 추가
                GradeRatioDict[row.Level].Add(row);
            }
        }
        
        
        // 레벨 스탯은 검색 속도를 위해 SO의 List를 Dictionary로 한 번만 변환 처리.
        if (levelStatDataSO != null)
        {
            LevelStatDict.Clear();
            foreach (var stat in levelStatDataSO.levelStatList)
            {
                LevelStatDict[stat.Level] = stat;
            }
        }

        Debug.Log($"모든 데이터 메모리 로드 완료 " +
                  $"(스태프:{StaffList.Count}개, 태그:{TagList.Count}개, 레벨스탯:{LevelStatDict.Count}개, 등급:{GradeList.Count}개)");
    }
}