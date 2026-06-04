using UnityEngine;

public interface IMainStateMachine
{
    public StateViewData StateViewData { get; }
    public void SetCurrentMainState(GameDevProcName stepName);
    public void Run();
}
