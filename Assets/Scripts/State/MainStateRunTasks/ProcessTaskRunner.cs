using System;
using Cysharp.Threading.Tasks;
using DataDispatcher;
using UnityEngine;
using Channel = DataDispatcher.Channel;

public class ProcessTaskRunner : MonoBehaviour, IProcessTaskRunnerEnterExit
{
    protected ProcessStateSO psSO;
    private bool _canGoing;
    protected bool _waiting;

    public async UniTask Enter(ProcessStateSO so)
    {
        await UniTask.WaitForSeconds(1f);
        psSO = so;
        var data = new SimpleUIRenderData()
        {
            btCallback =  GoProcess,
            titleTextId = so.stateNameId,
            imageId = so.imageID,
            btTextId = 9900043
        };
        Debug.Log($"[ProcessTaskRunner:Enter] {so.eventType}");
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcessSimpleUI, data);
        await UniTask.WaitUntil(() => _canGoing);
        _canGoing = false;
    }

    public async UniTask EventPreExecute()
    {
        // Event Manager 내 이미 진행했던 Regular Event 에 대한 Reset
        if (psSO.resetEvent) ServiceLocater.Get<IEventManager>().ResetRunId();
        
        if (psSO.eventType.Contains(EventType.Staff))
        {
            Debug.Log($"[ProcessTaskRunner:EventPreExecute] {EventType.Staff} 진행");
            // ServiceLocater.Get<IEventManager>().OccurEvent(EventType.Staff);
        } 
        
        if (psSO.eventType.Contains(EventType.Regular))
        {
            if (UnityEngine.Random.Range(0, 100) < 60)  // 확률적 동작 60%
            {
                Debug.Log($"[ProcessTaskRunner:EventPreExecute] {EventType.Regular} 진행");
                await ServiceLocater.Get<IEventManager>().OccurEvent(EventType.Regular);
                Debug.Log($"[ProcessTaskRunner:EventPreExecute] {EventType.Regular} 완료");
            }
        }
    }

    public async UniTask EventPostExecute()
    {
        if (psSO.eventType.Contains(EventType.Reward))
        {
            Debug.Log($"[ProcessTaskRunner:EventPreExecute] {EventType.Reward} 진행");
            // ServiceLocater.Get<IEventManager>().OccurEvent(EventType.Reward);
        }
    }
    
    public async UniTask Exit()
    {
        // var ptm = ServiceLocater.Get<IPostManager>();
        // string fmt = ptm.Request<int, string>(Channel.GetUIText, 9900045);
        // string procName = ptm.Request<int, string>(Channel.GetUIText, psSO.stateNameId);
        
        var data = new SimpleUIRenderData()
        {
            titleTextId = psSO.stateNameId,
            imageId = "rimg_TA_E",
            btTextId = 9900044,
            btCallback = GoProcess,
        };
        Debug.Log($"[ProcessTaskRunner:Exit] {psSO.eventType}");
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcessSimpleUI, data);
        await UniTask.WaitUntil(() => _canGoing);
        _canGoing = false;
    }

    private void GoProcess()
    {
        _canGoing = true;
    }
    
    protected async UniTask WaitProcess() => await UniTask.WaitUntil(() => !_waiting);
}