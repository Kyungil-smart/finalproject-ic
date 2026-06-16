using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

[Serializable]
public struct NameTag
{
    public int id;
    public string name;
    public int textId;
}

[Serializable]
public class Quality
{
    public float total;
    public float design;
    public float development;
    public float art;
}

[Serializable]
public class ProjectData
{
    public ReactiveProperty<Quality> Qualities = new();  // 프로젝트 퀄리티
    public ReactiveProperty<bool> IsCompleted = new();  // 프로젝트 완료 여부
    public string name;  // 프로젝트 이름
    public NameTag genre;  // 장르
    public NameTag theme;  // 테마
    public ProjectGrade grade;  // 등급 F ~ SSSS
    public uint cost;  // 투자된 금액
    public uint income;  // 매출 금액
    public NameTag award;  // 수상 경력
    public NameTag trendGenre;  // 트랜드장르
    public NameTag trendTheme;  // 트랜드테마
}

public class ProjectManager : Manager, IProjectManager
{
    private ProjectData _projectData;
    
    private Dictionary<GameDevProcName, List<int>> _assignedStaff = new();
    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

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
    
    public uint Earnings => _projectData.income - _projectData.cost; // 수익

    public void UpdateTotalQuality(float value, float ratio = 1f)
    {
        var data = _projectData.Qualities.Value;
        data.total = value * ratio;
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

    public void JudgingAward()
    {
        // ToDo. 수상 받는 계산 식 넣기 (애매하네 이건... 외부에서 주입하는게 좋을까?)
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

    public void ClearStaffs() => _assignedStaff.Clear();
    public IReadOnlyList<int> GetAssignedStaff(GameDevProcName procName) => _assignedStaff[procName];
    protected override void Register() => ServiceLocater.Register<IProjectManager>(this);
    protected override void Unregister()=> ServiceLocater.Unregister<IProjectManager>(this);
}