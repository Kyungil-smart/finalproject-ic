using System.Collections.Generic;
using R3;

public interface IGameManager
{
    string PlayerName { get; }
    ReadOnlyReactiveProperty<int> Money { get; }
    ReadOnlyReactiveProperty<int> Heart { get; }
    // IReadOnlyList<ProjectData> Projects { get; }
    
    void AddMoney(int money);
    void AddHeart(int heart);
    // void AddProject(ProjectData project);
}
