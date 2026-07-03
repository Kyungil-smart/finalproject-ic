using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RegularEventTaskRunner : IEventTaskRunner
{
    private List<int> _remainingIds = new();
    private EventTaskData _taskData;

    public void SetEventData(EventTaskData taskData) => _taskData = taskData;

    public async UniTask Execute()
    {
        Debug.Log("[RegularEventTaskRunner] Start to run");
        
        Debug.Log($"[RegularEventTaskRunner] main: ID:{_taskData.id} MainText:{_taskData.titleTextId} Desc:{_taskData.descTextId}");
        foreach (var btn in _taskData.buttons)
            Debug.Log($"[RegularEventTaskRunner] btns - Target:{btn.target} Value:{btn.effectValue} Ratio:{btn.effectRatio}");
        
        var tcs = new UniTaskCompletionSource<int>();
        
        var renderData = new NormalEventUIRenderData(
            eventType: EventType.Regular,
            mainTextId: _taskData.descTextId,
            callback: (selectedId) => tcs.TrySetResult(selectedId)
        );
        
        for (int i = 0; i < _taskData.buttons.Count; i++)
        {
            var btn = _taskData.buttons[i];
            EventEffectData effectData = new()
            {
                btId = i,
                ratio = _taskData.buttons[i].effectRatio,
                target = _taskData.buttons[i].target,
                value = btn.effectValue
            };
            if (btn.textId != 0) renderData.choices.Add((i, btn.textId, effectData));
        }
        
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.EventUI, renderData);
        
        var selectedIndex = await tcs.Task;
        ServiceLocater.Get<IEventRouter>().Apply(_taskData.buttons[selectedIndex]);
    }
}
