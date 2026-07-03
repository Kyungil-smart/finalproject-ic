using System.Collections.Generic;
using UnityEngine;

public class ProjectManager : Manager, IProjectManager
{
    private ProjectData _projectData;
    private ProjectDataManager _projectDataManager;
    
    [SerializeField] private GenreThemeTypeDataSO _genreThemeDataSO;
    [SerializeField] private IncomeRatioDataSO _incomeRatioDataSO;
    private Dictionary<GameDevProcName, List<int>> _assignedStaff = new();
    private bool _isLoaded;
    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    public ProjectCostCalculate CostCalculator { get; private set; }
    public float TotalQuality => _projectData.Qualities.Value.total;

    private float _qaResult;
    public float QAResult 
    {
        get => _qaResult;
        set => _qaResult = value;
    }
    
    public float DevQuality
    {
        get => _projectData.Qualities.Value.development;
        set
        {
            var data = _projectData.Qualities.Value;
            data.development = value;
            _projectData.Qualities.Value = data;
        }
    } 
    
    public float ArtQuality
    {
        get => _projectData.Qualities.Value.art;
        set
        {
            var data = _projectData.Qualities.Value;
            data.art = value;
            _projectData.Qualities.Value = data;
        }
    } 
    
    public float DesignQuality
    {
        get => _projectData.Qualities.Value.design;
        set
        {
            var data = _projectData.Qualities.Value;
            data.design = value;
            _projectData.Qualities.Value = data;
        }
    } 
    
    public NameTag Genre
    {
        get => _projectData.genre; 
        set => _projectData.genre = value;   
    }

    public NameTag Theme
    {
        get => _projectData.theme; 
        set => _projectData.theme = value;
    }
    public NameTag TrendGenre
    {
        get => _projectData.trendGenre;
        set => _projectData.trendGenre = value;
    }

    public NameTag TrendTheme
    {
        get => _projectData.trendTheme;
        set => _projectData.trendTheme = value;
    }

    public uint Cost
    {
        get => _projectData.cost;
        set => _projectData.cost = value;
    }

    public uint Income
    {
        get => _projectData.income;
        set => _projectData.income = value;
    }

    public uint MarketingCost
    {
        get => _projectData.marketingCost;
        set => _projectData.marketingCost = value;
    }

    public uint MarketingBonus
    {
        get => _projectData.marketingBonus;
        set => _projectData.marketingBonus = value;
    }

    public List<ReviewResult> ReviewResults
    {
        get => _projectData.reviewResults;
        set => _projectData.reviewResults = value;
    }

    public AwardsData Awards => _projectData.award;
    
    public uint StaffsCost => _projectData.staffCost;
    
    public uint Earnings => _projectData.income - _projectData.cost; // 수익

    protected override void Init() => CostCalculator = new ProjectCostCalculate(_genreThemeDataSO, _incomeRatioDataSO);

    public void UpdateTotalQuality(float value, float ratio = 1f)
    {
        var data = _projectData.Qualities.Value;
        data.total = value * ratio;
        _projectData.Qualities.Value = data;
    }
    
    public void UpdateArtQuality(float value, float ratio = 1f)
    {
        var data = _projectData.Qualities.Value;
        data.art = value * ratio;
        _projectData.Qualities.Value = data;
    }
    
    public void UpdateDesignQuality(float value, float ratio = 1f)
    {
        var data = _projectData.Qualities.Value;
        data.design = value * ratio;
        _projectData.Qualities.Value = data;
    }
    
    public void UpdateDevQuality(float value, float ratio = 1f)
    {
        var data = _projectData.Qualities.Value;
        data.development = value * ratio;
        _projectData.Qualities.Value = data;
    }

    public void SetProjectName(string projectName) => _projectData.name = projectName;

    public void NewProject()
    {
        _projectData = new ProjectData();
        _assignedStaff.Clear();
    }

    public ProjectData FinishProject()
    {
        _projectData.IsCompleted.Value = true;
        return _projectData;
    }

    public void CalculateGrade()
    {
        // ToDo. 등급 받는 계산 식 넣기
    }

    public void SetAwards(AwardsData awardsData)
    {
        _projectData.award = awardsData;
    }

    public ProjectData GetProjectData() => _projectData;
    public void AssignStaff(GameDevProcName procName, int staffId)
    {
        if (!_assignedStaff.ContainsKey(procName))
            _assignedStaff.Add(procName, new());
        if (_assignedStaff[procName].Count >= 2)
        {
            Debug.LogWarning("[ProjectManager] 각 프로세스 당 2명씩만 배치 가능합니다.");
            return;
        }
        _assignedStaff[procName].Add(staffId);
    }

    public void SetStaffsCost()
    {
        var staffManager = ServiceLocater.Get<IStaffRegister>();
        uint staffCost = 0;
        foreach (var staff in staffManager.GetAllHiredStaffList())
            staffCost += (uint)staff.Salary;
        _projectData.staffCost = staffCost;
    }
    
    public void ClearStaffs() => _assignedStaff.Clear();
    public IReadOnlyList<int> GetAssignedStaffIds(GameDevProcName procName) => _assignedStaff[procName];
    protected override void Register() => ServiceLocater.Register<IProjectManager>(this);
    protected override void Unregister()=> ServiceLocater.Unregister<IProjectManager>(this);
    public ProjectManagerSaveData CaptureSaveData()
    {
        var dto = new ProjectManagerSaveData
        {
            currentProject = _projectData != null ? ProjectSaveData.From(_projectData) : null,
        };
        foreach (var (proc, ids) in _assignedStaff)
            dto.assignedStaff[proc] = new List<int>(ids);   // 내부 리스트 복사
        return dto;
    }

    public void RestoreSaveData(ProjectManagerSaveData dto)
    {
        _isLoaded = false;
        if (dto == null)
        {
            _isLoaded = true;
            return;
        }

        // 진행 중 프로젝트 (없으면 null = NewProject 전 상태와 동일)
        _projectData = dto.currentProject?.ToProjectData();

        _assignedStaff.Clear();
        if (dto.assignedStaff != null)
            foreach (var (proc, ids) in dto.assignedStaff)
                _assignedStaff[proc] = new List<int>(ids);
        _isLoaded = true;
    }

    public bool IsLoaded() => _isLoaded;

    [ContextMenu("프로젝트 데이터 확인")]
    private void ShowProjectData()
    {
        Debug.Log("[ProjectManager] ------- 프로젝트 데이터 --------");
        if (_projectData == null) Debug.Log("[ProjectManager] 데이터 없음 ");
        else
        {
            Debug.Log("[ProjectManager] Name: " + _projectData.name);
            Debug.Log("[ProjectManager] Marketing Cost: " + _projectData.marketingCost);
            Debug.Log("[ProjectManager] Marketing Bonus: " + _projectData.marketingBonus);
        }
        Debug.Log("[ProjectManager] -----------------------------");
    }
}