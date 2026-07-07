using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class EventManager : Manager, IEventManager, IReadyStatus
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
    [SerializeField] private EventTaskSO regularTasks;
    
    private Dictionary<EventType, EventDataStruct> _eventTasks = new();
    private CancellationTokenSource _cts;
    private bool _running;
    private EventRandom _eventRandom = new();
    
    private Dictionary<string, bool> _readyStatus = new();
    public Dictionary<string, bool> ReadyStatus => _readyStatus;
    
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
        DownloadData();
    }

    private async UniTaskVoid DownloadData()
    {
        if (!Utils.Environment.isDevelopment) return;
        if (_wasDownloaded) return;
        _readyStatus.Add("EventData", false);
        var loader = new EventDataLoader
        {
            staffTaskSO = staffTasks,
            regularTaskSO = regularTasks,
        };
        var gsManager = new GSheetManager(gSheetId, gid);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(
            "EventManager", () => gsManager.IsDownload, GSheetManager.MAX_TIMEOUT_SECONDS);
        loader.LoadEvent(gsManager);
        _wasDownloaded = true;
        _readyStatus["EventData"] = true;
    }

    private void InitEvent()
    {
        SetEventTask(EventType.Staff, staffTasks, new StaffEventTaskRunner());
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
        EventType[] eventTypes = { EventType.Staff, EventType.Regular};
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
        var row = ServiceLocater.Get<IStaffDataManager>().SynergyList.Find(r => r.discSum == discSum);
        return row.synergyType switch
        {
            200 => Synergy.Good,
            300 => Synergy.Bad,
            100 => Synergy.Normal
        };
    }
    
    public EventManagerSaveData CaptureSaveData()
    {
        var dto = new EventManagerSaveData();
        foreach (var (type, data) in _eventTasks)
            dto.runIds[type] = new List<int>(data.runIds);  // 내부 리스트 복사(앨리어싱 방지)
        return dto;
    }

    public void RestoreSaveData(EventManagerSaveData dto)
    {
        if (dto?.runIds == null) return;
        foreach (var (type, ids) in dto.runIds)
        {
            // InitEvent()가 Init(Awake)에서 _eventTasks(so/runner 포함)를 이미 만들어 둠 → runIds만 채움
            if (_eventTasks.TryGetValue(type, out var data))
            {
                data.runIds.Clear();
                data.runIds.AddRange(ids);
            }
        }
    }
    
    [ContextMenu("데이터 다운로드")]
    private void DataDownload()
    {
        DownloadData();
    }
}