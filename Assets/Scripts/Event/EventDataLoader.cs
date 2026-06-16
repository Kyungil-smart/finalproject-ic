using System.Collections.Generic;
using UnityEngine;

public class EventDataLoader
{
    public EventTaskSO staffTaskSO;
    public EventTaskSO regularTaskSO;
    public EventTaskSO linkageTaskSO;

    private void ClearData()
    {
        staffTaskSO.tasks.Clear();
        regularTaskSO.tasks.Clear();
        linkageTaskSO.tasks.Clear();
    }
    
    public void LoadEvent(GSheetManager gsheet)
    {
        ClearData();
        var data = gsheet.GetData();
        Debug.Log($"[EventDataLoader] Download items => {data.Count}");
        foreach (var row in data)
        {
            EventButtonData eventAButton = new()
            {
                textId = int.Parse(row["Btn_A_Txt_ID"]),
                target = row["Btn_A_Target"],
                effectValue = int.Parse(row["Btn_A_Effect_Value"]),
                effectRatio = float.Parse(row["Btn_A_Effect_Ratio"])
            };

            EventButtonData eventBButton = new()
            {
                textId = int.Parse(row["Btn_B_Txt_ID"]),
                target = row["Btn_B_Target"],
                effectValue = int.Parse(row["Btn_B_Effect_Value"]),
                effectRatio = float.Parse(row["Btn_B_Effect_Ratio"])
            };

            EventButtonData eventCButton = new()
            {
                textId = int.Parse(row["Btn_C_Txt_ID"]),
                target = row["Btn_C_Target"],
                effectValue = int.Parse(row["Btn_C_Effect_Value"]),
                effectRatio = float.Parse(row["Btn_C_Effect_Ratio"])
            };

            EventTaskData taskData = new()
            {
                id = int.Parse(row["Event_ID"]),
                titleTextId = int.Parse(row["Event_Title_ID"]),
                descTextId = int.Parse(row["Event_Desc_ID"]),
                buttons = new List<EventButtonData>
                {
                    eventAButton,
                    eventBButton,
                    eventCButton
                }
            };

            if (taskData.id >= 310000 && taskData.id < 320000) staffTaskSO.tasks.Add(taskData);
            else if (taskData.id >= 320000 && taskData.id < 330000) linkageTaskSO.tasks.Add(taskData);
            else if (taskData.id >= 330000 && taskData.id < 340000) regularTaskSO.tasks.Add(taskData);
        }
        
        ServiceLocater.Register(this);
        Debug.Log($"[EventDataLoader] Load - StaffTask: {staffTaskSO.tasks.Count}");
        Debug.Log($"[EventDataLoader] Load - LinkageTask: {linkageTaskSO.tasks.Count}");
        Debug.Log($"[EventDataLoader] Load - RegularTask: {regularTaskSO.tasks.Count}");
        Debug.Log($"[EventDataLoader] Complete load and save the event to ScriptableObject.");
    }
}