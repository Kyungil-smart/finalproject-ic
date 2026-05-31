using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;


public class EventManager : MonoBehaviour
{
    [SerializeField] private EventTask _staff;
    [SerializeField] private EventTask _linkage;
    [SerializeField] private EventTask _regular;
    [SerializeField] private EventTask _reward;
    
    private CancellationTokenSource _cts;
    private bool _running;
    public bool IsRunning => _running;
    
    private Dictionary<EventType, EventTask>  _eventTasks = new();
    
    private void Awake()
    {
        ServiceLocater.Register(this);
        InitEvent();
        ResetEvent();
    }

    private void InitEvent()
    {
        if (_staff)   _eventTasks[EventType.Staff]   = _staff;
        if (_linkage) _eventTasks[EventType.Linkage] = _linkage;
        if (_regular) _eventTasks[EventType.Regular] = _regular;
        if (_reward)  _eventTasks[EventType.Reward]  = _reward;
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
        if (_running) CancelCurrentEvent();
        _cts = new CancellationTokenSource();
        _running = true;

        try
        {
            await task.Execute(_cts.Token);
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