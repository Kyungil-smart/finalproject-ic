using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class GameManager : Manager, IGameManager, IResettable
{
    private const int   InitLevel = 1;
    private const int   InitMoney = 30000;
    private const int   InitHeart = 0;
    private const int   InitFloor = 2;
    private const int StartYear = 2026;
    
    private ReactiveProperty<int> _playerLevel = new(InitLevel); // 회사 레벨
    private int _playerMaxLevel = 15; // 최대 회사 레벨
    private ReactiveProperty<int> _money = new(InitMoney); // 재화 ; Test 목적으로 일단 많이 넣어둠.
    private ReactiveProperty<int> _heart = new(InitHeart);
    
    private ReactiveProperty<DateTime> _date = new(new DateTime(StartYear, 1, 1));
    private List<ProjectData> _projects = new(); // 프로젝트 리스트
    private ReactiveProperty<GameDevProcName> _procName = new();
    private bool _inputProjectNameActive;
    private int _floor = InitFloor;  // 현재 층수
   
    public string PlayerName { get; private set; } // 회사 이름
    public ReadOnlyReactiveProperty<int> PlayerLevel => _playerLevel;
    private float _exp;
    public ReadOnlyReactiveProperty<int> Money => _money;
    public ReadOnlyReactiveProperty<int> Heart => _heart;
    public ReadOnlyReactiveProperty<DateTime> Date => _date;
    public IReadOnlyList<ProjectData> Projects => _projects;
    public ReadOnlyReactiveProperty<GameDevProcName> ProcName => _procName;
    public bool InputProjectNameActive => _inputProjectNameActive;
    public int Floor => _floor;
    private int _maxSlotNum = 8;

    public bool IsNeedTutorial { get; set; } = true;    // 튜토리얼 발동 여부 (제어는 SlotUIController 에서 함)
    
    // ======================= 변수 입력 부 완료 ======================== //


    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    protected override void Register() => ServiceLocater.Register<IGameManager>(this);

    protected override void Unregister() => ServiceLocater.Unregister<IGameManager>(this);

    public void SetPlayerName(string playerName) => PlayerName = playerName;

    public void AddPlayerLevel(int playerLevel)
    {
        if (_playerLevel.Value >= _playerMaxLevel) _playerLevel.Value = _playerMaxLevel;
        _playerLevel.Value += playerLevel;
    }

    public void AddMoney(int money) => _money.Value += money;
    public void AddHeart(int heart) => _heart.Value += heart;
    public void AddProject(ProjectData project) => _projects.Add(project);
    public void UnlockFloor() => _floor++;
    public void ChangeState(GameDevProcName state)
    {
        bool changed = _procName.Value != state;
        _procName.Value = state;                              
        if (changed) ServiceLocater.Get<ISaveManager>()?.Save();
    }

    public int GetProjectYear() => _projects.Count == 0 ? 1 : _projects.Count + 1;


    public void AddExp(float exp)
    {
        _exp += exp;
        var lvData = ServiceLocater.Get<IStaffDataManager>()
            .LevelExpList.Find(x => x.level == _playerLevel.Value + 1);
        if (_exp >= lvData.requiredExp)
            _playerLevel.Value++;
    }

    public void UpdateInputProjectNameActive(bool active) => _inputProjectNameActive = active;

    public void AddAYear() => _date.Value = _date.Value.AddYears(1);
    public int GetCalendarYear() => _date.Value.Year - StartYear + 1;
    
    public GameManagerSaveData CaptureSaveData() => new()
    {
        playerName  = PlayerName,
        floor       = _floor,
        exp         = _exp,
        money       = _money.Value,
        heart       = _heart.Value,
        procDate    = _date.Value,
        procName    = _procName.Value,
        projects    = _projects.ConvertAll(ProjectSaveData.From),
    };
    
    public void RestoreSaveData(GameManagerSaveData dto)
    {
        PlayerName         = dto.playerName;
        _exp               = dto.exp;
        _floor             = dto.floor;
        _money.Value       = dto.money;
        _heart.Value       = dto.heart;
        _date.Value        = dto.procDate;
        _procName.Value    = dto.procName;

        _projects.Clear();
        foreach (var p in dto.projects) _projects.Add(p.ToProjectData());
    }

    [ContextMenu("게임 데이터 확인")]
    private void ShowGameData()
    {
        Debug.Log("[GameManager] ------ 게임 매니저 데이터 확인 ------");
        Debug.Log("[GameManager] PlayerName: " + PlayerName);
        Debug.Log("[GameManager] ---------------------------------");
    }

    public void ResetData()
    {
        _playerLevel.Value = InitLevel;
        _money.Value       = InitMoney;
        _heart.Value       = InitHeart;
        _date.Value        = new DateTime(StartYear, 1, 1);
        _procName.Value    = default;

        _exp        = 0f;
        _floor      = InitFloor;
        PlayerName  = null;
        _inputProjectNameActive = false;
        _projects.Clear();
    }
}
