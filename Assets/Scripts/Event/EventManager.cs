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
    
    private Dictionary<EventType, EventDataStruct> _eventTasks = new();
    private CancellationTokenSource _cts;
    private bool _running;
    private EventRandom _eventRandom = new();
    
    public bool IsRunning => _running;
    
    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Register()
    {
        ServiceLocater.Register<IEventManager>(this);
        ServiceLocater.Register<IEventRouter>(new EventRouter());
    }

    protected override void Unregister()
    {
        ServiceLocater.Unregister<IEventManager>(this);
        ServiceLocater.Unregister<IEventRouter>(new EventRouter());
    }

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
        EventType[] eventTypes = { EventType.Staff, EventType.Linkage, EventType.Regular};
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
        // 이벤트 타입이 직원간 이벤트면 직원간 이벤트 랜덤뽑기
        var data = evtType == EventType.Staff
            ? await _eventRandom.GetStaffRandomly(dataStruct.so.tasks, dataStruct.runIds, GetSynergy())
            : await _eventRandom.GetRandomly(dataStruct.so.tasks, dataStruct.runIds);
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

    // 시너지 반환
    private Synergy GetSynergy()
    {
        // ProjectManager에 투입된 직원의 id값 가져오기
        var assignedIds = ServiceLocater.Get<IProjectManager>()
            .GetAssignedStaffIds(ServiceLocater.Get<IGameManager>().ProcName.CurrentValue);
        int discSum = 0;
        
        // 투입된 직원의 id값으로 해당 직원의 DISC값 가져오기
        foreach (var id in assignedIds)
        {
            // Todo. assignedIds로 DISC값 조회 및 계산
            var entity = ServiceLocater.Get<IStaffRegister>().GetStaffEntity(id);
            if (entity != null) discSum += (int)entity.GetDiscType();
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