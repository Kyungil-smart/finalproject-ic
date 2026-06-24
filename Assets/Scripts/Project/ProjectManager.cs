using System.Collections.Generic;
using UnityEngine;

public class ProjectManager : Manager, IProjectManager
{
    private ProjectData _projectData;
    private ProjectDataManager _projectDataManager;
    
    [SerializeField] private GenreThemeTypeDataSO _genreThemeDataSO;
    [SerializeField] private IncomeRatioDataSO _incomeRatioDataSO;
    private Dictionary<GameDevProcName, List<int>> _assignedStaff = new();
    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    public ProjectCostCalculate CostCalculator { get; private set; }
    public float TotalQuality => _projectData.Qualities.Value.total;

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

    public void LoadProject(string jsonData)
    {
        // ToDo. 현재 진행중인 프로젝트의 JSON Data Load
        _projectData = JsonUtility.FromJson<ProjectData>(jsonData);
    }

    public string ToJsonData()
    {
        return JsonUtility.ToJson(_projectData);
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
}