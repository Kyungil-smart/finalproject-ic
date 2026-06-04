using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IEventManager
{
    void InitEventIds(EventType eventType, List<string> ids);
    UniTask OccurEvent(EventType eventType);
    bool IsRunning { get; }
}
