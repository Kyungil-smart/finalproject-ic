using System.Collections.Generic;
using UnityEngine;

public class EventDataLoader
{
    private Dictionary<int, EventData> _eventData = new();
    public void LoadEvent(GSheetManager gsheet)
    {
        var data = gsheet.GetData();

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
                        EffectValue = float.Parse(row["Btn_A_Effect_Value"]),
                        EffectRatio = float.Parse(row["Btn_A_Effect_Ratio"]),
                    },
                    new ButtonData
                    {
                        TxtId = (int)float.Parse(row["Btn_B_Txt_ID"]),
                        Target = row["Btn_B_Target"],
                        EffectValue = float.Parse(row["Btn_B_Effect_Value"]),
                        EffectRatio = float.Parse(row["Btn_B_Effect_Ratio"]),
                    },
                    new ButtonData
                    {
                        TxtId = (int)float.Parse(row["Btn_C_Txt_ID"]),
                        Target = row["Btn_C_Target"],
                        EffectValue = float.Parse(row["Btn_C_Effect_Value"]),
                        EffectRatio = float.Parse(row["Btn_C_Effect_Ratio"]),
                    }
                }
            };
            
            _eventData[eventData.EventId] = eventData;
        }
        Debug.Log($"Loaded {_eventData.Count}개");
        ServiceLocater.Register(this);
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
    public float EffectValue;
    public float EffectRatio;
}