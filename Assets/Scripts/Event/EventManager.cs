using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EventManager : Manager, IEventManager
{
    // [SerializeField] private EventTaskSO _staff;
    // [SerializeField] private EventTaskSO _linkage;
    // [SerializeField] private EventTaskSO _regular;
    // [SerializeField] private EventTaskSO _reward;
    private CancellationTokenSource _cts;
    private bool _running;
    public bool IsRunning => _running;
    
    private Dictionary<EventType, IEventTask>  _eventTasks = new();

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Register() => ServiceLocater.Register<IEventManager>(this);

    protected override void Unregister() => ServiceLocater.Unregister<IEventManager>(this);

    protected override void Init()
    {
        InitEvent();
        ResetEvent();
    }

    private void InitEvent()
    {
        // if (_staff)   _eventTasks[EventType.Staff]   = _staff;
        // if (_linkage) _eventTasks[EventType.Linkage] = _linkage;
        // if (_regular) _eventTasks[EventType.Regular] = _regular;
        // if (_reward)  _eventTasks[EventType.Reward]  = _reward;
        _eventTasks[EventType.Staff] = new StaffEventTask();
        _eventTasks[EventType.Linkage] = new LinkageEventTask();
        _eventTasks[EventType.Regular] = new RegularEventTask();
        _eventTasks[EventType.Reward] = new RewardEventTask();
    }

    public void InitEventIds(EventType eventType, List<string> ids)
    {
        if (_eventTasks.TryGetValue(eventType, out var task)) task.Init(ids);
    }
    
    private void ResetEvent()
    {
        foreach (var task in _eventTasks.Values) task.Reset();
    }
    
    private void OnDestroy()
    {
        CancelCurrentEvent();
        _cts?.Dispose();
        _cts = null;
        
        ServiceLocater.Unregister(this);
    }
    
    public async UniTaskVoid OccurEvent(EventType type)
    {
        if (!_eventTasks.TryGetValue(type, out var task)) return;
        // if (_running) CancelCurrentEvent();
        // _cts = new CancellationTokenSource();
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
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void CancelCurrentEvent()
    {
        _cts?.Cancel();
    }
}