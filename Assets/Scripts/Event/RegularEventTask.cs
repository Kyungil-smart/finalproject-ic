using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RegularEventTask : IEventTask
{
    private List<int> _ids = new();
    private List<int> _remainingIds = new();
    
    public void Init(List<string> ids)
    {
        _ids.Clear();
        foreach(var id in ids)
            if (int.TryParse(id, out var intId)) _ids.Add(intId);
        Reset();
    }

    public async UniTask Execute()
    {
        Debug.Log("Execute 진입");
        var id = GetRandomId();
        if (id < 0) return;
        
        var eventData = ServiceLocater.Get<EventDataLoader>().GetEventData(id);
        if (eventData == null) return;

        Debug.Log($"ID:{eventData.EventId} MainText:{eventData.MainTextId} Desc:{eventData.DescId}");
        foreach (var btn in eventData.Buttons)
            Debug.Log($"  Target:{btn.Target} Value:{btn.EffectValue} Ratio:{btn.EffectRatio}");
        
        var tcs = new UniTaskCompletionSource<int>();
        
        var renderData = new NormalEventUIRenderData(
            eventType: EventType.Regular,
            mainTextId: eventData.MainTextId,
            callback: (selectedId) => tcs.TrySetResult(selectedId)
        );
        
        for (int i = 0; i < eventData.Buttons.Count; i++)
        {
            var btn = eventData.Buttons[i];
            if (btn.TxtId != 0) renderData.choices.Add((i, btn.TxtId));
        }
        
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.EventUI, renderData);
        
        var selectedIndex = await tcs.Task;
        Apply(eventData.Buttons[selectedIndex]);
    }

    private void Apply(ButtonData btn)
    {
        var _manage = ServiceLocater.Get<ManagementData>();
        switch (btn.Target)
        {
            case "Money":
                _manage.AddMoney(btn.EffectValue);
                Debug.Log($"돈 지급, {_manage.Money}");
                break;
            case "Heart":
                _manage.AddHeart(btn.EffectValue);
                Debug.Log("하트 지급");
                break;
            case "Total_Quality":
                Debug.Log("퀄리티 증가");
                break;
        }
    }
    
    public void Reset()
    {
        _remainingIds = new List<int>(_ids);
    }

    private int GetRandomId()
    {
        if (_remainingIds.Count == 0) return -1;
        var randomIndex = Random.Range(0, _remainingIds.Count);
        var id = _remainingIds[randomIndex];
        _remainingIds.RemoveAt(randomIndex);
        return id;
    }
}
