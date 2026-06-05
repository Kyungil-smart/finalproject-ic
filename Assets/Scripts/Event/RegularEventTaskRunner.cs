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
        Apply(_taskData.buttons[selectedIndex]);
    }

    private void Apply(EventButtonData btn)
    {
        // ToDo. Staff 나 ProjectManager 상 데이터 형태에 따라 적용이 달라짐. 적용 관련 모듈 개발 필요
        var _manage = ServiceLocater.Get<ManagementData>();
        switch (btn.target)
        {
            case "Money":
                _manage.AddMoney(btn.effectValue);
                Debug.Log($"돈 지급, {_manage.Money}");
                break;
            case "Heart":
                _manage.AddHeart(btn.effectValue);
                Debug.Log("하트 지급");
                break;
            case "Total_Quality":
                Debug.Log("퀄리티 증가");
                break;
        }
    }
}
