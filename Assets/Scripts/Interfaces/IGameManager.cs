using System.Collections.Generic;
using R3;

public interface IGameManager
{
    // ProjectData 개발시 리스트와 함수 주석해제
    string PlayerName { get; }
    ReadOnlyReactiveProperty<int> Money { get; }
    ReadOnlyReactiveProperty<int> Heart { get; }
    // IReadOnlyList<ProjectData> Projects { get; }
    
    void SetPlayerName(string playerName);
    void AddMoney(int money);
    void AddHeart(int heart);
    // void AddProject(ProjectData project);
}
