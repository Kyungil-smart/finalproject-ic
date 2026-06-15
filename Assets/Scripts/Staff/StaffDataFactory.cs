using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 스태프 스탯에 대한 생성 및 계산 담당.
/// </summary>
public class StaffDataFactory
{
    // 특정 StaffID를 지정해서 능력치 및 등급을 랜덤 결정하는 핵심 함수
    // 지정된 한명의 능력치 및 등급을 랜덤 결정
    public async UniTask<StaffInitData> CreateDataByStaffIDAsync(int staffID, int playerLevel)
    {
        // 비동기 구조 유지를 위해 1프레임 대기
        await UniTask.Yield();

        var dataManager = ServiceLocater.Get<IStaffDataManager>();
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

        data.Level = playerLevel + Random.Range(-1, 2);
        if (data.Level <= 0) data.Level = 1;
        
        // 레벨별 스탯 구간 지정
        LevelStatRow levelData = dataManager.LevelStatsDict.ContainsKey(data.Level) 
            ? dataManager.LevelStatsDict[data.Level] 
            : dataManager.LevelStatsDict[1];

        data.Base_Common_Concentration = Random.Range(levelData.Common_Min, levelData.Common_Max + 1);
        data.Base_Common_Creativity = Random.Range(levelData.Common_Min, levelData.Common_Max + 1);
        data.Base_Common_Communication = Random.Range(levelData.Common_Min, levelData.Common_Max + 1);
        data.Base_Job_Planning = Random.Range(levelData.Job_Min, levelData.Job_Max + 1);
        data.Base_Job_Development = Random.Range(levelData.Job_Min, levelData.Job_Max + 1);
        data.Base_Job_Art = Random.Range(levelData.Job_Min, levelData.Job_Max + 1);
        
        // 비용 계산
        CalculateCosts(data);
        
        return data;
    }
    
    // -- 계산 관련 함수 -----------------
    
    // 확률에 따른 등급 결정 (시트 데이터 기준)
    private GradeRow RollGradeFromTable(IStaffDataManager dataManager)
    {
        int currentLevel = 1; 
        float roll = Random.value; 
        float cumulative = 0f;

        if (dataManager.GradeRatiosDict.TryGetValue(currentLevel, out List<GradeRatioRow> ratioList))
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
    public void ApplyTagEffect(StaffInitData init, StaffRuntimeData runtime, string effectName, int addValue, float ratioValue)
    {
        if (string.IsNullOrEmpty(effectName) || effectName == "Staff_Effect_None") return;
        ratioValue -= 1;
        switch (effectName)
        {
            case "Staff_Concentration":
                runtime.Added_Common_Concentration += addValue;
                runtime.Added_Common_Concentration += Mathf.RoundToInt(init.Base_Common_Concentration * ratioValue);
                break;
            case "Staff_Creativity":
                runtime.Added_Common_Creativity += addValue;
                runtime.Added_Common_Creativity += Mathf.RoundToInt(init.Base_Common_Creativity * ratioValue);
                break;
            case "Staff_Communication":
                runtime.Added_Common_Communication += addValue;
                runtime.Added_Common_Communication += Mathf.RoundToInt(init.Base_Common_Communication * ratioValue);
                break;
            case "Staff_Design":
                runtime.Added_Job_Design += addValue;
                runtime.Added_Job_Design += Mathf.RoundToInt(init.Base_Job_Planning * ratioValue);
                break;
            case "Staff_Dev":
                runtime.Added_Job_Development += addValue;
                runtime.Added_Job_Development += Mathf.RoundToInt(init.Base_Job_Development * ratioValue);
                break;
            case "Staff_Art":
                runtime.Added_Job_Art += addValue;
                runtime.Added_Job_Art += Mathf.RoundToInt(init.Base_Job_Art * ratioValue);
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
    public void CalculateCosts(StaffInitData data)
    {
        float gradeCost = data.Grade switch { StaffGrade.D => 0.5f, StaffGrade.C => 0.75f, StaffGrade.B => 1.0f, StaffGrade.A => 1.5f, StaffGrade.S => 2.0f, _ => 1.0f };
        int totalBaseStats = data.Base_Common_Concentration + data.Base_Common_Creativity + data.Base_Job_Planning; // 약식 합산
        // totalBaseStats 식은 나중에 직업군 별 스탯 배율?을 적용 (자신의 직업과 연관되지 않은 스탯 * 0.X)
        
        data.Salary = Mathf.RoundToInt((2000 + (totalBaseStats * gradeCost)) / 100f) * 100;
        data.Hire_Cost = Mathf.RoundToInt(data.Salary * gradeCost);
    }
    
    // 신규 스태프의 초기 RuntimeData 생성. 초기 레벨/보너스 공식이 생기면 이 함수 안에만 추가한다.
    public StaffRuntimeData CreateInitialRuntimeData(StaffInitData init)
    {
        var runtime = new StaffRuntimeData();
        var dataManager = ServiceLocater.Get<IStaffDataManager>();
        
        GradeRow gradeData = RollGradeFromTable(dataManager);
        // 등급 스탯 보너스 배율 적용
        float gradeMultiplier = gradeData.Grade_XP;
        if (gradeMultiplier > 1) gradeMultiplier -= 1f;
        runtime.Added_Common_Concentration = Mathf.RoundToInt(init.Base_Common_Concentration * gradeMultiplier);
        runtime.Added_Common_Creativity = Mathf.RoundToInt(init.Base_Common_Creativity * gradeMultiplier);
        runtime.Added_Common_Communication = Mathf.RoundToInt(init.Base_Common_Communication * gradeMultiplier);
        runtime.Added_Job_Design = Mathf.RoundToInt(init.Base_Job_Planning * gradeMultiplier);
        runtime.Added_Job_Development = Mathf.RoundToInt(init.Base_Job_Development * gradeMultiplier);
        runtime.Added_Job_Art = Mathf.RoundToInt(init.Base_Job_Art * gradeMultiplier);

        // // 태그 뽑기 및 효과 적용 -> 테그는 나중에
        // Tag 가져오고
        
        // 가져온 Tag 기반으로 Runtime 데이터 추가하기.
        foreach (var tag in runtime.Added_Tags)
        {
            ApplyTagEffect(init, runtime, tag.Tag_A_Effect_Name, tag.Tag_A_Effect_Value, tag.Tag_A_Effect_Ratio);
            ApplyTagEffect(init, runtime, tag.Tag_B_Effect_Name, tag.Tag_B_Effect_Value, tag.Tag_B_Effect_Ratio);
        }
        return runtime;
    }
}