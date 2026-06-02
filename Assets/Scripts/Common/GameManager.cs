using R3;
using UnityEngine;

public class GameManager : MonoBehaviour, IGameManager
{
    private ReactiveProperty<int> _money = new (0);
    private ReactiveProperty<int> _heart = new (0);
    
    public string PlayerName { get; private set; }
    public ReadOnlyReactiveProperty<int> Money => _money;
    public ReadOnlyReactiveProperty<int> Heart => _heart;

    public void SetPlayerName(string playerName) => PlayerName = playerName;
    public void AddMoney(int money) => _money.Value += money;
    public void AddHeart(int heart) => _heart.Value += heart;
}
