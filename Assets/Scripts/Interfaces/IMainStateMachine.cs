using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IMainStateMachine
{
    public StateViewData StateViewData { get; }
    public UniTask SetCurrentMainState(GameDevProcName stepName);
    public void Run();
}
