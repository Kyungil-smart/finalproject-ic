using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

public interface IGameManager
{
    public string PlayerName { get; }
    public ReadOnlyReactiveProperty<int> PlayerLevel { get; }
    public ReadOnlyReactiveProperty<int> Money { get; }
    public ReadOnlyReactiveProperty<int> Heart { get; }
    public ReadOnlyReactiveProperty<DateTime> Date { get; }
    public IReadOnlyList<ProjectData> Projects { get; }
    public ReadOnlyReactiveProperty<GameDevProcName> ProcName {get; }
    public bool InputProjectNameActive { get; }
    
    public void SetPlayerName(string playerName);
    public void AddPlayerLevel(int playerLevel);
    public void AddMoney(int money);
    public void AddHeart(int heart);
    public void AddAYear();
    public void AddProject(ProjectData project);
    public void ChangeState(GameDevProcName newState);
    public int GetProjectYear();
    public void AddExp(float exp);
    public void UpdateInputProjectNameActive(bool active);
    public GameManagerSaveData CaptureSaveData();
    public void RestoreSaveData(GameManagerSaveData dto);
}
