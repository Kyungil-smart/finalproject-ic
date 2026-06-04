using UnityEngine;

/// <summary>
/// 빌드할 때 생성.
/// 스태프가 가지는 인터페이스 기능 구현, MonoBehaviour를 상속받아 유니티에서 오브젝트로 존재할 수 있게 구현.
/// </summary>
public class StaffEntity : MonoBehaviour, IStaffInfo, ISavableStaff
{
    // 빌더를 통해서 빌드할 때 StaffManager의 데이터를 참조로 데이터를 받아 StaffManager의 데이터와 동기화 되어있음.
    // 해당 InitData, RuntimeData 수정할 시 StaffManager에도 반영됨.  
    private StaffInitData _initData;
    private StaffRuntimeData _runtimeData;

    public void Initialize(StaffInitData init, StaffRuntimeData runtime)
    {
        _initData = init;
        _runtimeData = runtime;
    }

    // IStaffInfo 구현 (읽기 전용)
    public int GetStaffID() => _initData.Staff_ID;
    public string GetFullName() => _initData.Staff_Name;
    public JobType GetJob() => _initData.Job;
    public StaffGrade GetGrade() => _initData.Grade;
    public int GetSalary() => _initData.Salary;
    public DiscType GetDiscType() => _initData.DISC_Type;
    public void DisplayInfo() => Debug.Log($"[{_initData.Grade}급 {_initData.Job}] 사번:{_initData.Staff_ID} / 연봉:{_initData.Salary}");

    public int GetTotalCareer() => _initData.Base_Career + _runtimeData.Added_Career;
    
    public int GetTotalConcentration() => _initData.Base_Common_Concentration + _runtimeData.Added_Common_Concentration;
    public int GetTotalCreativity() => _initData.Base_Common_Creativity + _runtimeData.Added_Common_Creativity;
    public int GetTotalCommunication() => _initData.Base_Common_Communication + _runtimeData.Added_Common_Communication; 
    public int GetPlanning() => _initData.Base_Job_Planning + _runtimeData.Added_Job_Planning;
    public int GetDevelopment() => _initData.Base_Job_Development + _runtimeData.Added_Job_Development;
    public int GetArt() => _initData.Base_Job_Art + _runtimeData.Added_Job_Art;
    
    // Set 인터페이스 추가. 
    
    // ISavableStaff 구현 (데이터 세이브, 로드용도)
    public StaffInitData GetInitData() => _initData;
    public StaffRuntimeData GetRuntimeData() => _runtimeData;
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