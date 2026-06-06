using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ProcessTaskRunner : MonoBehaviour, IProcessTaskRunnerEnterExit
{
    protected ProcessStateSO psSO;
    private bool _canGoing;

    public async UniTask Enter(ProcessStateSO so)
    {
        await UniTask.WaitForSeconds(1f);
        psSO = so;
        var data = new SimpleUIRenderData(so.stateNameId, 9900007, GoProcess);
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
        var data = new SimpleUIRenderData(psSO.stateNameId, 9900008, GoProcess);
        Debug.Log($"[ProcessTaskRunner:Exit] {psSO.eventType}");
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcessSimpleUI, data);
        await UniTask.WaitUntil(() => _canGoing);
        _canGoing = false;
    }

    private void GoProcess()
    {
        _canGoing = true;
    }
}