using System;
using System.Collections.Generic;
using R3;

public interface IGameManager
{
    public string PlayerName { get; }
    public ReadOnlyReactiveProperty<int> Money { get; }
    public ReadOnlyReactiveProperty<int> Heart { get; }
    public ReadOnlyReactiveProperty<DateTime> Date { get; }
    public IReadOnlyList<ProjectData> Projects { get; }
    public ReadOnlyReactiveProperty<GameDevProcName> ProcName {get; }
    public int SlotNum { get; set; }
    
    public void SetPlayerName(string playerName);
    public void AddMoney(int money);
    public void AddHeart(int heart);
    public void AddProject(ProjectData project);
    public void ChangeState(GameDevProcName newState);
    public bool UnlockSlotNum();
}
