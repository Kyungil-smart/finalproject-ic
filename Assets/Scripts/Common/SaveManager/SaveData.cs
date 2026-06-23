using R3;
using System;
using System.Collections.Generic;


[Serializable]
public class ProjectSaveData
{
    public Quality quality;        // Qualities.Value (struct → 직렬화 OK)
    public bool    isCompleted;    // IsCompleted.Value
    public string  name;
    public NameTag genre;
    public NameTag theme;
    public ProjectGrade grade;
    public uint cost;
    public uint income;
    public uint staffCost;
    public AwardsData award;
    public NameTag trendGenre;
    public NameTag trendTheme;

    public static ProjectSaveData From(ProjectData p) => new()
    {
        quality = p.Qualities.Value,
        isCompleted = p.IsCompleted.Value,
        name = p.name, genre = p.genre, theme = p.theme, grade = p.grade,
        cost = p.cost, income = p.income, staffCost = p.staffCost,
        award = p.award, trendGenre = p.trendGenre, trendTheme = p.trendTheme,
    };

    public ProjectData ToProjectData() => new()
    {
        Qualities   = new ReactiveProperty<Quality>(quality),
        IsCompleted = new ReactiveProperty<bool>(isCompleted),
        name = name, genre = genre, theme = theme, grade = grade,
        cost = cost, income = income, staffCost = staffCost,
        award = award, trendGenre = trendGenre, trendTheme = trendTheme,
    };
}

[Serializable]
public class GameManagerSaveData 
{
    public string playerName;
    public int    playerLevel;
    public float  exp;
    public int    money;
    public int    heart;
    public DateTime procDate;          // _date (게임 내 날짜)
    public GameDevProcName procName;   // 진행 단계 (StringEnumConverter로 이름 저장)
    public List<ProjectSaveData> projects = new();
}

[Serializable]
public class ProjectManagerSaveData 
{
    public ProjectSaveData currentProject;   // 진행 중 프로젝트 (없으면 null)
    public Dictionary<GameDevProcName, List<int>> assignedStaff = new();
}

[Serializable]
public class StaffManagerSaveData
{
    public List<StaffSaveData> staff = new();
    public int slotIndex;
    public List<SlotSaveData> slots = new();
}

[Serializable]
public class StaffSaveData
{
    public StaffInitData init;            // 전부 primitive → 통째 저장 (AvatarKey/AssetId 포함 = 비주얼 복원 열쇠)
    public StaffRuntimeSaveData runtime;
}

[Serializable]
public class StaffRuntimeSaveData
{
    public StaffState currentState;
    public int addedCareer;
    public int addedCommonConcentration;
    public int addedCommonCreativity;
    public int addedCommonCommunication;
    public int addedJobDesign;
    public int addedJobDevelopment;
    public int addedJobArt;
    public List<int> tagIds = new();      // Added_Tags(TagRow) → Tag_Id만 (SO 데이터 규칙)
}

[Serializable]
public class SlotSaveData
{
    public int  id;          // SlotDef.id (어느 슬롯인지)
    public bool unlocked;
    public int  staffId;     // 슬롯-직원 매핑
}

[Serializable]
public class EventManagerSaveData 
{
    public Dictionary<EventType, List<int>> runIds = new();
}

[Serializable]
public class SaveMeta              // 슬롯 카드 표시용 스냅샷 (저장 시 같이 기록)
{
    public string playerName;          // 회사 이름 (구분 + 표시)
    public int    playerLevel;         // 2F / 1F
    public int    completedProjectCount; // 완성한 프로젝트 N개
    public int    year;                // N년차
    public int    money;               // 보유 재화
    public string savedAt;             // 저장 시각(표시/정렬용)  
    // public string thumbnailKey;     // 썸네일 미정 → 확정되면 추가
}

[Serializable]
public class SaveRoot
{
    public int version = 1;               // 마이그레이션 대비
    public SaveMeta meta;
    public GameManagerSaveData game;
    public ProjectManagerSaveData project;
    public StaffManagerSaveData staff;
    public EventManagerSaveData events;
}