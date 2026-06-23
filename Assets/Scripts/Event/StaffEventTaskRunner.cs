using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StaffEventTaskRunner : IEventTaskRunner
{
    private EventTaskData _taskData;

    public void SetEventData(EventTaskData taskData) => _taskData = taskData;

    public async UniTask Execute()
    {
        Debug.Log("[StaffEventTaskRunner] Start to run");
        
        Debug.Log($"[StaffEventTaskRunner] main: ID:{_taskData.id} MainText:{_taskData.titleTextId} Desc:{_taskData.descTextId}");
        foreach (var btn in _taskData.buttons)
            Debug.Log($"[StaffEventTaskRunner] btns - Target:{btn.target} Value:{btn.effectValue} Ratio:{btn.effectRatio}");
        
        var tcs = new UniTaskCompletionSource<int>();
        
        var renderData = new NormalEventUIRenderData(
            eventType: EventType.Regular,
            mainTextId: _taskData.titleTextId,
            callback: (selectedId) => tcs.TrySetResult(selectedId)
        );
        
        for (int i = 0; i < _taskData.buttons.Count; i++)
        {
            var btn = _taskData.buttons[i];
            if (btn.textId != 0) renderData.choices.Add((i, btn.textId));
        }
        
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.EventUI, renderData);
        
        var selectedIndex = await tcs.Task;
        ServiceLocater.Get<IEventRouter>().Apply(_taskData.buttons[selectedIndex]);
    }
}

public enum Synergy {Good, Normal, Bad}