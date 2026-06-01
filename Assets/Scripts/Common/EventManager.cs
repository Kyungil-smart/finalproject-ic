using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;


public class EventManager : MonoBehaviour
{
    private bool _running;
    public bool IsRunning => _running;
    
    private Dictionary<EventType, IEventTask>  _eventTasks = new();
    
    private void Awake()
    {
        ServiceLocater.Register(this);
        InitEvent();
        ResetEvent();
    }

    private void InitEvent()
    {
        _eventTasks[EventType.Staff] = new StaffEventTask();
        _eventTasks[EventType.Linkage] = new LinkageEventTask();
        _eventTasks[EventType.Regular] = new RegularEventTask();
        _eventTasks[EventType.Reward] = new RewardEventTask();
    }

    public void ResetEvent()
    {
        foreach (var task in _eventTasks.Values) task.Reset();
    }
    
    private void OnDestroy()
    {
        ServiceLocater.Unregister(this);
    }
    
    public async UniTaskVoid OccurEvent(EventType type)
    {
        if (!_eventTasks.TryGetValue(type, out var task)) return;
        _running = true;

        try
        {
            await task.Execute();
        }
        catch (Exception e)
        {
            Debug.LogError($"[EventManager] 오류: {e}");
        }
        finally
        {
            _running = false;
        }
    }
}