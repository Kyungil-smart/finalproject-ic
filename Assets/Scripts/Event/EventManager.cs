using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class EventManager : Manager, IEventManager
{
    [Serializable]
    public class EventDataStruct
    {
        public List<int> runIds;
        public EventTaskSO so;
        public IEventTaskRunner TaskRunner;
    }
    
    [SerializeField] private string gSheetId;
    [SerializeField] private string gid;
    [SerializeField] private bool _wasDownloaded;
    private GSheetManager _gsheet;
    
    [SerializeField] private EventTaskSO staffTasks;
    [SerializeField] private EventTaskSO linkageTasks;
    [SerializeField] private EventTaskSO regularTasks;
    [SerializeField] private EventTaskSO rewardTasks;
    
    private Dictionary<EventType, EventDataStruct> _eventTasks = new();
    private CancellationTokenSource _cts;
    private bool _running;
    
    public bool IsRunning => _running;
    
    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Register() => ServiceLocater.Register<IEventManager>(this);

    protected override void Unregister() => ServiceLocater.Unregister<IEventManager>(this);

    protected override void Init()
    {
        Debug.Log("[EventManager] Initializing...");
        InitEvent();
        DownloadData().Forget();
    }

    private async UniTaskVoid DownloadData()
    {
        if (!Utils.Environment.isDevelopment) return;
        if (_wasDownloaded) return;
        var loader = new EventDataLoader
        {
            staffTaskSO = staffTasks,
            regularTaskSO = regularTasks,
            linkageTaskSO = linkageTasks,
            rewardTaskSO = rewardTasks
        };
        var gsManager = new GSheetManager(gSheetId, gid);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(() => gsManager.IsDownload);
        loader.LoadEvent(gsManager);
        _wasDownloaded = true;
    }

    private void InitEvent()
    {
        SetEventTask(EventType.Staff, staffTasks, new StaffEventTaskRunner());
        SetEventTask(EventType.Linkage, linkageTasks, new LinkageEventTaskRunner());
        SetEventTask(EventType.Regular, regularTasks, new RegularEventTaskRunner());
        SetEventTask(EventType.Reward, rewardTasks, new RewardEventTaskRunner());
        return;

        void SetEventTask(EventType evtType, EventTaskSO so, IEventTaskRunner runner)
        {
            _eventTasks[evtType] = new EventDataStruct
            {
                runIds = new(),
                so = so,
                TaskRunner = runner
            };
        }
    }
    
    public void ResetRunId()
    {
        if (_eventTasks == null) return;
        EventType[] eventTypes = { EventType.Staff, EventType.Linkage, EventType.Regular, EventType.Reward };
        foreach (var evtType in eventTypes)
            _eventTasks[evtType].runIds.Clear();
    }
    
    private void OnDestroy()
    {
        CancelCurrentEvent();
        _cts?.Dispose();
        _cts = null;
        
        ServiceLocater.Unregister(this);
    }

    public async UniTask OccurEvent(EventType evtType)
    {
        Debug.Log($"[EventManager:OccurEvent] {evtType} 이벤트 동작 신청");
        if (!_eventTasks.TryGetValue(evtType, out var dataStruct)) return;
        // if (_running) CancelCurrentEvent();
        // _cts = new CancellationTokenSource();
        _running = true;
        var data = await GetEventTaskDataRandomly(evtType, dataStruct.so.tasks.Count);
        if (data == null)
        {
            Debug.Log("[EventManager] 실행 가능한 이벤트가 존재하지 않습니다.");
            return;
        }
        Debug.Log($"[EventManager:OccurEvent] title text id: {data.titleTextId} 이벤트 동작");
        var task = dataStruct.TaskRunner;
        task.SetEventData(data);
        try
        {
            await task.Execute();
            dataStruct.runIds.Add(data.id);
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
    
    private async UniTask<EventTaskData> GetEventTaskDataRandomly(EventType evtType, int totalEventCount)
    {
        Debug.Log($"[EventManager:GetEventTaskDataRandomly] {evtType} {totalEventCount}");
        while (true)
        {
            var index = UnityEngine.Random.Range(0, totalEventCount);
            if (!_eventTasks.TryGetValue(evtType, out var mdStruct)) return null;
            var task = mdStruct.so.tasks[index];
            if (mdStruct.runIds.Count < totalEventCount)
            {
                if (!mdStruct.runIds.Contains(task.id))
                    return task;
                await UniTask.Yield();
            }
            else
            {
                return null;    
            }
        }
    }

    [ContextMenu("데이터 다운로드")]
    private void DataDownload()
    {
        DownloadData();
    }
}