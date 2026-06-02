using System.Collections.Generic;
using UnityEngine;

public class EventDataLoader
{
    private Dictionary<int, EventData> _eventData = new();
    public void LoadEvent(GSheetManager gsheet)
    {
        var data = gsheet.GetData();

        var staffIds = new List<string>();
        var linkageIds = new List<string>();
        var regularIds = new List<string>();
        var rewardIds = new List<string>();
        
        foreach (var row in data)
        {
            var eventData = new EventData
            {
                EventId = (int)float.Parse(row["Event_ID"]),
                MainTextId = (int)float.Parse(row["Event_Title_ID"]),
                DescId = (int)float.Parse(row["Event_Desc_ID"]),
                Buttons = new List<ButtonData>
                {
                    new ButtonData
                    {
                        TxtId = (int)float.Parse(row["Btn_A_Txt_ID"]),
                        Target = row["Btn_A_Target"],
                        EffectValue = int.Parse(row["Btn_A_Effect_Value"]),
                        EffectRatio = float.Parse(row["Btn_A_Effect_Ratio"]),
                    },
                    new ButtonData
                    {
                        TxtId = (int)float.Parse(row["Btn_B_Txt_ID"]),
                        Target = row["Btn_B_Target"],
                        EffectValue = int.Parse(row["Btn_B_Effect_Value"]),
                        EffectRatio = float.Parse(row["Btn_B_Effect_Ratio"]),
                    },
                    new ButtonData
                    {
                        TxtId = (int)float.Parse(row["Btn_C_Txt_ID"]),
                        Target = row["Btn_C_Target"],
                        EffectValue = int.Parse(row["Btn_C_Effect_Value"]),
                        EffectRatio = float.Parse(row["Btn_C_Effect_Ratio"]),
                    }
                }
            };
            
            _eventData[eventData.EventId] = eventData;
            var id = eventData.EventId;
            if (id > 31000 && id < 32001) staffIds.Add(id.ToString());
            else if (id > 32000 && id < 33001) linkageIds.Add(id.ToString());
            else if (id > 33000 && id < 34001) regularIds.Add(id.ToString());
            else if (id > 34000 && id < 35001) rewardIds.Add(id.ToString());
        }
        
        ServiceLocater.Register(this);
        Debug.Log($"Load {_eventData.Count}");
        Debug.Log($"Staff:{staffIds.Count} Linkage:{linkageIds.Count} Regular:{regularIds.Count} Reward:{rewardIds.Count}");
        var eventManager = ServiceLocater.Get<IEventManager>();
        eventManager.InitEventIds(EventType.Staff, staffIds);
        eventManager.InitEventIds(EventType.Linkage, linkageIds);
        eventManager.InitEventIds(EventType.Regular, regularIds);
        eventManager.InitEventIds(EventType.Reward, rewardIds);
        Debug.Log($"이벤트 매니저등록 완료");
    }

    public EventData GetEventData(int eventId) => _eventData.TryGetValue(eventId, out var data) ? data : null;
}

public class EventData
{
    public int EventId;
    public int MainTextId;
    public int DescId;
    public List<ButtonData> Buttons = new();
}

public class ButtonData
{
    public int TxtId;
    public string Target;
    public int EffectValue;
    public float EffectRatio;
}