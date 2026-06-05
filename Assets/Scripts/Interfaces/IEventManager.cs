using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IEventManager
{
    public UniTask OccurEvent(EventType evtType);
    public void ResetRunId();
    public bool IsRunning { get; }
}
