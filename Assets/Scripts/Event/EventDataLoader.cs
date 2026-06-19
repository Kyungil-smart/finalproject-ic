using System.Collections.Generic;
using UnityEngine;

public class EventDataLoader
{
    public EventTaskSO staffTaskSO;
    public EventTaskSO regularTaskSO;

    private void ClearData()
    {
        staffTaskSO.tasks.Clear();
        regularTaskSO.tasks.Clear();
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

            EventTaskData taskData = new()
            {
                id = int.Parse(row["Event_ID"]),
                titleTextId = int.Parse(row["Event_Title_ID"]),
                categoryId =  int.Parse(row["Event_Cat"]),
                descTextId = int.Parse(row["Event_Desc_ID"]),
                resultId = int.Parse(row["Event_Result_ID"]),
                buttons = new List<EventButtonData>
                {
                    eventAButton,
                    eventBButton,
                }
            };

            if (taskData.categoryId % 10 == 1 || taskData.categoryId % 10 == 2 || taskData.categoryId % 10 == 3) staffTaskSO.tasks.Add(taskData);
            else if (taskData.categoryId % 10 == 0) regularTaskSO.tasks.Add(taskData);
            // Todo. 일단 시너지랑 외부요인만 넣어뒀습니다. 카테고리로 분류했습니다 아래에 새로 아이디로 추가해도 카테고리만 잘 넣어두면
            // Todo. 순서가 바껴도 되어서 카테고리로 분류했습니다.
        }
        
        ServiceLocater.Register(this);
        Debug.Log($"[EventDataLoader] Load - StaffTask: {staffTaskSO.tasks.Count}");
        Debug.Log($"[EventDataLoader] Load - RegularTask: {regularTaskSO.tasks.Count}");
        Debug.Log($"[EventDataLoader] Complete load and save the event to ScriptableObject.");
    }
}