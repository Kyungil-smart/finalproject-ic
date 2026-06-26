using R3;
using UnityEngine;


public interface IMinigameManager
{
    public ReactiveProperty<int> TotalBugs { get; }
    public ReactiveProperty<int> CatchBugs { get; }
    public ReactiveProperty<float> CurrentTime { get; }
    public ReactiveProperty<bool> IsGameOver { get; }
    public ReactiveProperty<int> CountDown { get;  }
    public bool GameStart { get; set; }
    public void OnBugCaught(GameObject bugInstance);
    public void ApplyResult();
}