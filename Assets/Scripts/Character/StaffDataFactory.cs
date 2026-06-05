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
public class StaffDataFactory
{
    // 기존 무작위 가챠 생성 파이프라인. HashSet hiredIDs를 받아서 중복되지 않는 직원 중 랜덤 한명 선택 후 값 넣기
    // 현재 사용 안하는 중.
    public async UniTask<StaffInitData> CreateRandomDataAsync(int playerLevel, HashSet<int> hiredIDs)
    {
        await UniTask.Yield(); 
        
        var dataManager = ServiceLocater.Get<StaffDataManager>();
        if (dataManager == null) return null;
        
        // 미고용 인원 추출 .
        var availableStaff = dataManager.StaffList
            .Where(s => hiredIDs == null || !hiredIDs.Contains(s.Staff_ID))
            .ToList();
        
        if (availableStaff.Count == 0)
        {
            Debug.LogWarning("더 이상 고용할 직원이 없음 (남은 미고용자 0명)");
            return null; 
        }

        // 미고용자 중 랜덤 1명 선택
        var picked = availableStaff[Random.Range(0, availableStaff.Count)];

        // 가챠로직은 아래 함수로
        return await CreateDataByStaffIDAsync(picked.Staff_ID, playerLevel);
    }

    // 특정 StaffID를 지정해서 능력치 및 등급을 랜덤 결정하는 핵심 함수
    // 지정된 한명의 능력치 및 등급을 랜덤 결정
    public async UniTask<StaffInitData> CreateDataByStaffIDAsync(int staffID, int playerLevel)
    {
        // 비동기 구조 유지를 위해 1프레임 대기
        await UniTask.Yield();

        var dataManager = ServiceLocater.Get<StaffDataManager>();
        if (dataManager == null)
        {
            Debug.LogError("ServiceLocater에서 StaffDataManager를 찾을 수 없습니다");
            return null;
        }

        // 전체 명부에서 지정된 ID의 Row 찾기
        var pickedStaff = dataManager.StaffList.FirstOrDefault(s => s.Staff_ID == staffID);
        if (pickedStaff == null)
        {
            Debug.LogError($"StaffList에서 ID {staffID}에 해당하는 기본 스태프 정보를 찾을 수 없습니다.");
            return null;
        }

        StaffInitData data = new StaffInitData();
        
        // StaffID에 따른 시트 기본 정보 매핑  
        data.Staff_ID = pickedStaff.Staff_ID;
        data.Staff_Name = pickedStaff.Staff_Name;
        data.Job = ParseJobType(pickedStaff.Staff_Job);
        data.Staff_Gender = (pickedStaff.Staff_Gender == "남"); // String -> Bool
        data.Avatar_ID = Random.Range(1, 1000); // 3D 모델 연결용 임시값

        // 등급 결정 (여기부턴 시트 결정 방식에 따른 랜덤 결정)
        GradeRow gradeData = RollGradeFromTable(dataManager);
        data.Grade = gradeData.GradeEnum; 
        data.DISC_Type = (DiscType)Random.Range(0, 4); 
        data.Base_Career = 0; 

        // 레벨별 스탯 구간 지정
        LevelStatRow levelData = dataManager.LevelStatDict.ContainsKey(playerLevel) 
            ? dataManager.LevelStatDict[playerLevel] 
            : dataManager.LevelStatDict[1];

        data.Base_Common_Concentration = Random.Range(levelData.Common_Min, levelData.Common_Max + 1);
        data.Base_Common_Creativity = Random.Range(levelData.Common_Min, levelData.Common_Max + 1);
        data.Base_Common_Communication = Random.Range(levelData.Common_Min, levelData.Common_Max + 1);
        data.Base_Job_Planning = Random.Range(levelData.Job_Min, levelData.Job_Max + 1);
        data.Base_Job_Development = Random.Range(levelData.Job_Min, levelData.Job_Max + 1);
        data.Base_Job_Art = Random.Range(levelData.Job_Min, levelData.Job_Max + 1);

        // 등급 스탯 보너스 배율 적용
        float gradeMultiplier = gradeData.Grade_XP;
        data.Base_Common_Concentration = Mathf.RoundToInt(data.Base_Common_Concentration * gradeMultiplier);
        data.Base_Common_Creativity = Mathf.RoundToInt(data.Base_Common_Creativity * gradeMultiplier);
        data.Base_Common_Communication = Mathf.RoundToInt(data.Base_Common_Communication * gradeMultiplier);
        data.Base_Job_Planning = Mathf.RoundToInt(data.Base_Job_Planning * gradeMultiplier);
        data.Base_Job_Development = Mathf.RoundToInt(data.Base_Job_Development * gradeMultiplier);
        data.Base_Job_Art = Mathf.RoundToInt(data.Base_Job_Art * gradeMultiplier);

        // 태그 뽑기 및 효과 적용
        int tagCountToDraw = Random.Range(gradeData.Tag_Min, gradeData.Tag_Max + 1);
        List<TagRow> pickedTags = dataManager.TagList.OrderBy(t => Random.value).Take(tagCountToDraw).ToList();

        // 고정 태그 유형 할당
        var type2Tags = dataManager.TagList.Where(t => t.Tag_Type == 2).ToList();
        if (type2Tags.Count > 0)
        {
            data.Fixed_Tag = type2Tags[Random.Range(0, type2Tags.Count)].Tag_Id; 
        }

        foreach (var tag in pickedTags)
        {
            ApplyTagEffect(data, tag.Tag_A_Effect_Name, tag.Tag_A_Effect_Value, tag.Tag_A_Effect_Ratio);
            ApplyTagEffect(data, tag.Tag_B_Effect_Name, tag.Tag_B_Effect_Value, tag.Tag_B_Effect_Ratio);
        }

        // 비용 계산
        CalculateCosts(data);

        return data;
    }
    
    // -- 계산 관련 함수 -----------------
    
    // 확률에 따른 등급 결정 (시트 데이터 기준)
    private GradeRow RollGradeFromTable(StaffDataManager dataManager)
    {
        int currentLevel = 1; 
        float roll = Random.value; 
        float cumulative = 0f;

        if (dataManager.GradeRatioDict.TryGetValue(currentLevel, out List<GradeRatioRow> ratioList))
        {
            foreach (var ratioData in ratioList)
            {
                cumulative += ratioData.Ratio;
                if (roll <= cumulative)
                {
                    return dataManager.GradeList.Find(g => g.Grade == ratioData.Grade);
                }
            }
        }
        return dataManager.GradeList.Last(); 
    }

    // 문자열로 된 태그 효과를 실제 스탯에 계산.
    private void ApplyTagEffect(StaffInitData data, string effectName, int addValue, float ratioValue)
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
    private JobType ParseJobType(string jobString)
    {
        if (jobString == "기획") return JobType.Planner;
        if (jobString == "개발") return JobType.Developer;
        return JobType.Artist;
    }

    // 비용 계산식에 따른 Salary(연봉), Hire_Cost(초기 계약금) 계산.
    private void CalculateCosts(StaffInitData data)
    {
        float gradeCost = data.Grade switch { StaffGrade.D => 0.5f, StaffGrade.C => 0.75f, StaffGrade.B => 1.0f, StaffGrade.A => 1.5f, StaffGrade.S => 2.0f, _ => 1.0f };
        int totalBaseStats = data.Base_Common_Concentration + data.Base_Common_Creativity + data.Base_Job_Planning; // 약식 합산
        // totalBaseStats 식은 나중에 직업군 별 스탯 배율?을 적용 (자신의 직업과 연관되지 않은 스탯 * 0.X)
        
        data.Salary = Mathf.RoundToInt((2000 + (totalBaseStats * gradeCost)) / 100f) * 100;
        data.Hire_Cost = Mathf.RoundToInt(data.Salary * gradeCost);
    }
}