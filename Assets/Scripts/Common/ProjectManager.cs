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
public struct Quality
{
    public float total;
    public float design;
    public float development;
    public float art;
}

[Serializable]
public struct ProjectData
{
    public ReactiveProperty<Quality> Qualities;  // 프로젝트 퀄리티
    public ReactiveProperty<bool> IsCompleted { get; }  // 프로젝트 완료 여부
    public string name;  // 프로젝트 이름
    public NameTag genre;  // 장르
    public NameTag theme;  // 테마
    public ProjectGrade grade;  // 등급 F ~ SSSS
    public uint cost;  // 투자된 금액
    public uint income;  // 매출 금액
    public NameTag award;  // 수상 경력
    // Todo. 품질산출 시 데이터 비교를 위해 선언함
    public string trendGenre;  // 트랜드장르
    public string trendTheme;  // 트랜드테마

    public ProjectData(string name)
    {
        this.name = name;
        genre = default;
        theme = default;
        grade = ProjectGrade.F;
        cost = 0;
        income = 0;
        award = default;
        Qualities = new();
        IsCompleted = new();
        trendGenre = default;
        trendTheme = default;
    }
}

public struct ProjectDataForUI
{
    public float totalQuality;
    public float devQuality;
    public float artQuality;
    public float designQuality;
    public bool isCompleted;
    public string name;
    public int genreTextId;
    public int themeTextId;
    public string gradeText;
    public uint cost;
    public uint income;
    public int awardTextId;
}

public class ProjectManager : Manager, IProjectManager
{
    private ProjectData _projectData;
    // Todo.메인스태프와 서브스태프를 구분할 필요가 있음 ex) 0번이 메인, 1번이 서브로 스탯값 계산.
    private List<int> _assignedStaff = new();
    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    public float TotalQuality
    {
        get => _projectData.Qualities.Value.total;
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

    public void UpdateTotalQuality(float value)
    {
        var data = _projectData.Qualities.Value;
        data.total = value;
        _projectData.Qualities.Value = data;
    }

    public void NewProject(string projectName)
    {
        _projectData = new ProjectData(projectName);
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
    public void AssignStaff(int staffId) => _assignedStaff.Add(staffId);

    public void ClearStaffs() => _assignedStaff.Clear();

    public IReadOnlyList<int> GetAssignedStaff() => _assignedStaff;


    protected override void Register() => ServiceLocater.Register<IProjectManager>(this);
    protected override void Unregister()=> ServiceLocater.Unregister<IProjectManager>(this);
}