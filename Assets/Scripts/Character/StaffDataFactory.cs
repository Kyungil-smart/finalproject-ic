using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 스태프 생성 시 초기 랜덤값(가챠)을 결정해서 반환하는 팩토리.
/// 
/// DataManager에서 파싱한 시트 데이터(CSV)를 기반으로 동작함.
/// (이전의 단순 랜덤 생성에서 데이터 연동 방식으로 변경 완료)
/// </summary>
public static class StaffDataFactory
{
    // 고용된 스태프 ID 저장용 (중복 뽑기 방지)
    private static HashSet<int> _hiredStaffIDs = new HashSet<int>();

    public static async UniTask<StaffInitData> CreateRandomDataAsync(int playerLevel)
    {
        // 비동기 구조(가챠 연출 등) 유지를 위해 1프레임 대기.
        await UniTask.Yield(); 
        
        StaffInitData data = new StaffInitData();
        
        // 직원 뽑기 (남은 인원 중 랜덤)
        var availableStaff = DataManager.StaffList.Where(s => !_hiredStaffIDs.Contains(s.Staff_ID)).ToList();
        
        if (availableStaff.Count == 0)
        {
            Debug.LogWarning("더 이상 고용할 직원이 없음.");
            return null; 
        }

        var pickedStaff = availableStaff[Random.Range(0, availableStaff.Count)];
        _hiredStaffIDs.Add(pickedStaff.Staff_ID);

        // 시트 데이터 매핑
        data.Staff_ID = pickedStaff.Staff_ID;
        data.Staff_Name = pickedStaff.Staff_Name;
        data.Job = ParseJobType(pickedStaff.Staff_Job);
        data.Staff_Gender = (pickedStaff.Staff_Gender == "남");
        data.Avatar_ID = Random.Range(1, 1000); // 3D 모델 연결용 임시값 (나중에 어드레서블 키 등으로 수정)

        // 등급 결정 (시트 확률 기반)
        GradeRow gradeData = RollGradeFromTable();
        data.Grade = gradeData.GradeEnum; 
        data.DISC_Type = (DiscType)Random.Range(0, 4); 
        data.Base_Career = 0; // 신입 기준 (나중에 기획에 맞게 스탯 수치에 비례하게 공식적용해서 수정)

        // 레벨별 스탯 구간 지정 (시트에 데이터 없으면 1레벨로)
        LevelStatRow levelData = DataManager.LevelStatDict.ContainsKey(playerLevel) 
            ? DataManager.LevelStatDict[playerLevel] 
            : DataManager.LevelStatDict[1];

        data.Base_Common_Concentration = Random.Range(levelData.Common_Min, levelData.Common_Max + 1);
        data.Base_Common_Creativity = Random.Range(levelData.Common_Min, levelData.Common_Max + 1);
        data.Base_Common_Communication = Random.Range(levelData.Common_Min, levelData.Common_Max + 1);
        data.Base_Job_Planning = Random.Range(levelData.Job_Min, levelData.Job_Max + 1);
        data.Base_Job_Development = Random.Range(levelData.Job_Min, levelData.Job_Max + 1);
        data.Base_Job_Art = Random.Range(levelData.Job_Min, levelData.Job_Max + 1);

        // 등급 스탯 보너스(Grade_XP) 배율 적용
        float gradeMultiplier = gradeData.Grade_XP;
        data.Base_Common_Concentration = Mathf.RoundToInt(data.Base_Common_Concentration * gradeMultiplier);
        data.Base_Common_Creativity = Mathf.RoundToInt(data.Base_Common_Creativity * gradeMultiplier);
        data.Base_Common_Communication = Mathf.RoundToInt(data.Base_Common_Communication * gradeMultiplier);
        data.Base_Job_Planning = Mathf.RoundToInt(data.Base_Job_Planning * gradeMultiplier);
        data.Base_Job_Development = Mathf.RoundToInt(data.Base_Job_Development * gradeMultiplier);
        data.Base_Job_Art = Mathf.RoundToInt(data.Base_Job_Art * gradeMultiplier);

        // 태그 뽑기 및 효과 적용
        int tagCountToDraw = Random.Range(gradeData.Tag_Min, gradeData.Tag_Max + 1);
        List<TagRow> pickedTags = DataManager.TagList.OrderBy(t => Random.value).Take(tagCountToDraw).ToList();

        // 지금은 Fixed_Tag
        var type2Tags = DataManager.TagList.Where(t => t.Tag_Type == 2).ToList();

        // 필터링된 리스트 중 랜덤으로 1개 뽑아서 ID 할당
        data.Fixed_Tag = type2Tags[Random.Range(0, type2Tags.Count)].Tag_Id; 

        foreach (var tag in pickedTags)
        {
            ApplyTagEffect(data, tag.Tag_A_Effect_Name, tag.Tag_A_Effect_Value, tag.Tag_A_Effect_Ratio);
            ApplyTagEffect(data, tag.Tag_B_Effect_Name, tag.Tag_B_Effect_Value, tag.Tag_B_Effect_Ratio);
        }

        // 비용 계산
        CalculateCosts(data);

        return data;
    }

    // 확률에 따른 등급 결정 (시트 데이터 기준)
    private static GradeRow RollGradeFromTable()
    {
        int currentLevel = 1; 

        float roll = Random.value; 
        float cumulative = 0f;

        // 1레벨 확률표 데이터가 존재하는지 확인
        if (DataManager.GradeRatioDict.TryGetValue(currentLevel, out List<GradeRatioRow> ratioList))
        {
            // 1레벨 확률표를 순회하며 가챠 실행
            foreach (var ratioData in ratioList)
            {
                cumulative += ratioData.Ratio;
                if (roll <= cumulative)
                {
                    // 당첨된 등급 이름("S" 같은 것)으로 등급 상세 정보 테이블(GradeList)을 검색해서 반환
                    return DataManager.GradeList.Find(g => g.Grade == ratioData.Grade);
                }
            }
        }
        
        // 예외 처리 (데이터가 없거나 확률 계산이 어긋났을 때 기본으로 반환)
        return DataManager.GradeList.Last();
    }

    // 문자열로 된 태그 효과를 실제 스탯에 계산.
    private static void ApplyTagEffect(StaffInitData data, string effectName, int addValue, float ratioValue)
    {
        if (string.IsNullOrEmpty(effectName) || effectName == "Staff_Effect_None") return;

        switch (effectName)
        {
            case "Staff_Concentration":
                data.Base_Common_Concentration = Mathf.RoundToInt((data.Base_Common_Concentration + addValue) * ratioValue);
                break;
            case "Staff_Creativity":
                data.Base_Common_Creativity = Mathf.RoundToInt((data.Base_Common_Creativity + addValue) * ratioValue);
                break;
            case "Staff_Communication":
                data.Base_Common_Communication = Mathf.RoundToInt((data.Base_Common_Communication + addValue) * ratioValue);
                break;
            case "Staff_Design": 
                data.Base_Job_Planning = Mathf.RoundToInt((data.Base_Job_Planning + addValue) * ratioValue);
                break;
            case "Staff_Dev": 
                data.Base_Job_Development = Mathf.RoundToInt((data.Base_Job_Development + addValue) * ratioValue);
                break;
            case "Staff_Art": 
                data.Base_Job_Art = Mathf.RoundToInt((data.Base_Job_Art + addValue) * ratioValue);
                break;
        }
    }

    // 한글 직무명을 JobType Enum으로 변환. 
    private static JobType ParseJobType(string jobString)
    {
        if (jobString == "기획") return JobType.Planner;
        if (jobString == "개발") return JobType.Developer;
        return JobType.Artist;
    }

    // 비용 계산식에 따른 Salary(연봉), Hire_Cost(초기 계약금) 계산.
    private static void CalculateCosts(StaffInitData data)
    {
        float gradeCost = data.Grade switch { StaffGrade.D => 0.5f, StaffGrade.C => 0.75f, StaffGrade.B => 1.0f, StaffGrade.A => 1.5f, StaffGrade.S => 2.0f, _ => 1.0f };
        int totalBaseStats = data.Base_Common_Concentration + data.Base_Common_Creativity + data.Base_Job_Planning; // 약식 합산
        // totalBaseStats 식은 나중에 직업군 별 스탯 배율?을 적용 (자신의 직업과 연관되지 않은 스탯 * 0.X)
        
        data.Salary = Mathf.RoundToInt((2000 + (totalBaseStats * gradeCost)) / 100f) * 100;
        data.Hire_Cost = Mathf.RoundToInt(data.Salary * gradeCost);
    }
}