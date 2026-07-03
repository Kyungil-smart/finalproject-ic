using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AchieveEventRunner
{
    private AchieveTaskData _taskData;
    
    public AchieveEventRunner(AchieveTaskData taskData)
    {
        _taskData = taskData;
    }

    public async UniTask Execute()
    {
        var tcs = new UniTaskCompletionSource<int>();

        var renderData = new NormalEventUIRenderData(
            eventType: EventType.Reward,
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
