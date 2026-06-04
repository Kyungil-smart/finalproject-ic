using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class GameManager : Manager, IGameManager
{
    private ReactiveProperty<int> _money = new (0);     // 재화
    private ReactiveProperty<int> _heart = new (0);
    private ReactiveProperty<DateTime> _date = new ();
    private List<ProjectData> _projects = new();        // 프로젝트 리스트
    private ReactiveProperty<GameDevProcName> _procName = new();
    
    public string PlayerName { get; private set; }      // 회사 이름
    public ReadOnlyReactiveProperty<int> Money => _money;
    public ReadOnlyReactiveProperty<int> Heart => _heart;
    public ReadOnlyReactiveProperty<DateTime> Date => _date;
    public IReadOnlyList<ProjectData> Projects => _projects;
    public ReadOnlyReactiveProperty<GameDevProcName> ProcName => _procName;

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    protected override void Register() => ServiceLocater.Register<IGameManager>(this);
    
    protected override void Unregister() => ServiceLocater.Unregister<IGameManager>(this);

    public void SetPlayerName(string playerName) => PlayerName = playerName;
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
}
