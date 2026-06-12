using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 빌드할 때 생성.
/// 스태프가 가지는 인터페이스 기능 구현, MonoBehaviour를 상속받아 유니티에서 오브젝트로 존재할 수 있게 구현.
/// </summary>
public class StaffEntity : MonoBehaviour, IStaffInfo, ISavableStaff
{
    // 빌더를 통해서 빌드할 때 StaffManager의 데이터를 참조로 데이터를 받아 StaffManager의 데이터와 동기화 되어있음.
    // 해당 InitData, RuntimeData 수정할 시 StaffManager에도 반영됨.  
    public StaffInitData init;
    public StaffRuntimeData runtime;
    public Action OnLevelUp;
    private IStaffDataManager _staffDataManager;

    private void Start()
    {
        _staffDataManager = ServiceLocater.Get<IStaffDataManager>();
    }

    // IStaffInfo 구현 (읽기 전용)
    public int GetStaffID() => init.Staff_ID;
    public string GetFullName() => init.Staff_Name;
    public JobType GetJob() => init.Job;
    public StaffGrade GetGrade() => init.Grade;
    public int GetSalary() => init.Salary;
    public int GetHireCost() => init.Hire_Cost;
    public DiscType GetDiscType() => init.DISC_Type;
    public void DisplayInfo() => Debug.Log($"[{init.Grade}급 {init.Job}] 사번:{init.Staff_ID} / 연봉:{init.Salary}");

    public int GetTotalCareer() => init.Base_Career + runtime.Added_Career;
    
    public int GetTotalConcentration() => init.Base_Common_Concentration + runtime.Added_Common_Concentration;
    public int GetTotalCreativity() => init.Base_Common_Creativity + runtime.Added_Common_Creativity;
    public int GetTotalCommunication() => init.Base_Common_Communication + runtime.Added_Common_Communication; 
    public int GetPlanning() => init.Base_Job_Planning + runtime.Added_Job_Planning;
    public int GetDevelopment() => init.Base_Job_Development + runtime.Added_Job_Development;
    public int GetArt() => init.Base_Job_Art + runtime.Added_Job_Art;
    
    // Set 인터페이스 추가.
    private void LevelUp()
    {
        init.Level++;
        
        // Apply Common Status
        init.Base_Common_Communication = ApplyState(init.Base_Common_Communication, isCommon: true);
        init.Base_Common_Creativity = ApplyState(init.Base_Common_Creativity, isCommon: true);
        init.Base_Common_Concentration = ApplyState(init.Base_Common_Concentration, isCommon: true);

        // Apply Job Status
        init.Base_Job_Art = ApplyState(init.Base_Job_Art, isCommon: false);
        init.Base_Job_Development = ApplyState(init.Base_Job_Development, isCommon: false);
        init.Base_Job_Planning = ApplyState(init.Base_Job_Planning, isCommon: false);
        
        OnLevelUp?.Invoke();
        // 만렙 고정치
        if (init.Level >= 15)
            init.Exp = _staffDataManager.LevelExpList.Find(x => x.level == init.Level).cumulativeExp;
    }

    private int ApplyState(int baseValue, bool isCommon)
    {
        int min = 0;
        int max = 0;
        if (isCommon)
        {
            min = _staffDataManager.LevelStatsDict[init.Level].Common_Min;
            max = _staffDataManager.LevelStatsDict[init.Level].Common_Max;
        }
        else
        {
            min = _staffDataManager.LevelStatsDict[init.Level].Job_Min;
            max = _staffDataManager.LevelStatsDict[init.Level].Job_Max;
        }
        if (baseValue < min) baseValue = min;
        return Random.Range(++baseValue, max);
    }
    
    // ISavableStaff 구현 (데이터 세이브, 로드용도)
    public StaffInitData GetInitData() => init;
    public StaffRuntimeData GetRuntimeData() => runtime;

    public void ApplyExp(float exp)
    {
        // 만렙이면 더 경험치 얻지 않음
        if (init.Level >= 15) return;
        
        init.Exp += exp;
        var requiredExp = _staffDataManager
            .LevelExpList
            .Find(x => x.level == init.Level + 1)
            .requiredExp;
        if (requiredExp >= init.Exp) LevelUp();
    }
}

// 직군별 전략 패턴 컴포넌트 (IJobAction 구현. 나중에 커지면 따로 스크립트 만들예정)
public class PlannerAction : MonoBehaviour, IJobAction
{
    private IStaffInfo _myInfo;
    void Start() => _myInfo = GetComponent<IStaffInfo>(); // Entity와 연결
    
    public void DoWork() => Debug.Log($"{_myInfo?.GetFullName()} 기획자가 기획을 수정");
}

public class DeveloperAction : MonoBehaviour, IJobAction
{
    private IStaffInfo _myInfo;
    void Start() => _myInfo = GetComponent<IStaffInfo>();
    
    public void DoWork() => Debug.Log($"{_myInfo?.GetFullName()} 개발자가 코드를 작성");
}

public class ArtistAction : MonoBehaviour, IJobAction
{
    private IStaffInfo _myInfo;
    void Start() => _myInfo = GetComponent<IStaffInfo>();
    
    public void DoWork() => Debug.Log($"{_myInfo?.GetFullName()} 아티스트가 리소스를 생성");
}