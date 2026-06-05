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