using System.Collections.Generic;
using R3;

public interface IGameManager
{
    string PlayerName { get; }
    ReadOnlyReactiveProperty<int> Money { get; }
    ReadOnlyReactiveProperty<int> Heart { get; }
    public IReadOnlyList<ProjectData> Projects { get; }
    
    void SetPlayerName(string playerName);
    void AddMoney(int money);
    void AddHeart(int heart);
    void AddProject(ProjectData project);
}
