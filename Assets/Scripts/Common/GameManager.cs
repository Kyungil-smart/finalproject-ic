using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class GameManager : Manager, IGameManager
{
    private ReactiveProperty<int> _playerLevel = new(1);      // 회사 레벨
    private int _playerMaxLevel = 15;                   // 최대 회사 레벨
    private ReactiveProperty<int> _money = new (1000000);     // 재화 ; Test 목적으로 일단 많이 넣어둠.
    private ReactiveProperty<int> _heart = new (0);
    private ReactiveProperty<DateTime> _date = new ();
    private List<ProjectData> _projects = new();        // 프로젝트 리스트
    private ReactiveProperty<GameDevProcName> _procName = new();
    
    public string PlayerName { get; private set; }      // 회사 이름
    public ReadOnlyReactiveProperty<int> PlayerLevel => _playerLevel;
    public ReadOnlyReactiveProperty<int> Money => _money;
    public ReadOnlyReactiveProperty<int> Heart => _heart;
    public ReadOnlyReactiveProperty<DateTime> Date => _date;
    public IReadOnlyList<ProjectData> Projects => _projects;
    public ReadOnlyReactiveProperty<GameDevProcName> ProcName => _procName;
    private int _maxSlotNum = 8;
    private int _slotNum = 2;
    public int SlotNum => _slotNum;

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
    public void ChangeState(GameDevProcName state) => _procName.Value = state;

    private void Start()
    {
        // ToDo. Save Data 를 Load 했을 경우 해당 내용 적용 하기
        if (_procName.Value == GameDevProcName.Initialization)
        {
            _procName.Value = GameDevProcName.HumanResources;
            ServiceLocater.Get<IMainStateMachine>().SetCurrentMainState(GameDevProcName.HumanResources);
        }
    }

    public bool UnlockSlotNum()
    {
        if (_slotNum < _maxSlotNum)
        {
            _slotNum++;
            return true;
        }
        return false;
    }
}
