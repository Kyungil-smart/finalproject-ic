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
    
    private Dictionary<EventType, EventDataStruct> _eventTasks;
    private CancellationTokenSource _cts;
    private bool _running;
    private EventRandom _eventRandom = new();
    
    public bool IsRunning => _running;
    
    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Register() => ServiceLocater.Register<IEventManager>(this);

    protected override void Unregister() => ServiceLocater.Unregister<IEventManager>(this);

    protected override void Init()
    {
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
        if (_eventTasks != null) return;
        _eventTasks = new Dictionary<EventType, EventDataStruct>();
        _eventTasks[EventType.Staff] = new EventDataStruct
        {
            so = staffTasks,
            TaskRunner = new StaffEventTaskRunner()
        };
        _eventTasks[EventType.Linkage] = new EventDataStruct
        {
            so = linkageTasks,
            TaskRunner = new LinkageEventTaskRunner()
        };
        _eventTasks[EventType.Regular] = new EventDataStruct
        {
            so = regularTasks,
            TaskRunner = new RegularEventTaskRunner()
        };
        _eventTasks[EventType.Reward] = new EventDataStruct
        {
            so = rewardTasks,
            TaskRunner = new RewardEventTaskRunner()
        };
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
        if (!_eventTasks.TryGetValue(evtType, out var dataStruct)) return;
        // if (_running) CancelCurrentEvent();
        // _cts = new CancellationTokenSource();
        _running = true;
        var data = evtType == EventType.Staff
            ? await _eventRandom.GetStaffRandomly(dataStruct.so.tasks, dataStruct.runIds, GetSynergy())
            : await _eventRandom.GetRandomly(dataStruct.so.tasks, dataStruct.runIds);
        // var data = await GetEventTaskDataRandomly(evtType, dataStruct.runIds.Count);
        if (data == null)
        {
            Debug.Log("[EventManager] 실행 가능한 이벤트가 존재하지 않습니다.");
            return;
        }
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
    
    // private async UniTask<EventTaskData> GetEventTaskDataRandomly(EventType evtType, int totalEventCount)
    // {
    //     while (true)
    //     {
    //         var index = UnityEngine.Random.Range(0, totalEventCount);
    //         if (!_eventTasks.TryGetValue(evtType, out var mdStruct)) return null;
    //         var task = mdStruct.so.tasks[index];
    //         if (mdStruct.runIds.Count < totalEventCount)
    //         {
    //             if (!mdStruct.runIds.Contains(task.id))
    //                 return task;
    //             await UniTask.Yield();
    //         }
    //         return null;
    //     }
    // }

    // Todo. 범위지정 및 데이터 조회가 아직 안되어 에러가 떠서 주석처리해둠.
    // private async UniTask<EventTaskData> GetStaffEventTaskDataRandomly(EventType evtType)
    // {
    //     if (!_eventTasks.TryGetValue(evtType, out var mdStruct)) return null;
    //     var synergy = GetSynergy();
    //     var filtered = synergy switch
    //     {
    //         Synergy.Good   => mdStruct.so.tasks.FindAll(t => t.id),
    //         Synergy.Normal => mdStruct.so.tasks.FindAll(t => t.id),
    //         Synergy.Bad    => mdStruct.so.tasks.FindAll(t => t.id), Todo. 기획에서 범위가 아직 안나옴
    //         _              => mdStruct.so.tasks
    //     };
    //     int totalEventCount = filtered.Count;
    //     while (true)
    //     {
    //         var index = UnityEngine.Random.Range(0, totalEventCount);
    //         var task = mdStruct.so.tasks[index];
    //         if (mdStruct.runIds.Count < totalEventCount)
    //         {
    //             if (!mdStruct.runIds.Contains(task.id))
    //                 return task;
    //             await UniTask.Yield();
    //         }
    //         return null;
    //     }
    // }

    private Synergy GetSynergy()
    {
        var assignedIds = ServiceLocater.Get<IProjectManager>().GetAssignedStaff();
        int discSum = 0;

        foreach (var id in assignedIds)
        {
            // Todo. assignedIds로 DISC값 조회 및 계산
             
        }
        
        return discSum switch
        {
            6 or 9 => Synergy.Good,
            5 or 10 => Synergy.Bad,
            _ => Synergy.Normal
        };
    }
    
    [ContextMenu("데이터 다운로드")]
    private void DataDownload()
    {
        DownloadData();
    }
}