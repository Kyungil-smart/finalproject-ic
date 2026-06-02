using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IEventManager
{
    void InitEventIds(EventType eventType, List<string> ids);
    UniTaskVoid OccurEvent(EventType eventType);
    bool IsRunning { get; }
}
