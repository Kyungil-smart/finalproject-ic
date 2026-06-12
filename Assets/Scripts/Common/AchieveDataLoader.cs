using System.Collections.Generic;
using UnityEngine;

public class AchieveDataLoader
{
    public AchieveRewardSO achieveRewardSO;

    private void ClearData()
    {
        achieveRewardSO.tasks.Clear();
    }

    public void LoadAchieveData(GSheetManager gSheet)
    {
        ClearData();
        var data = gSheet.GetData();
        foreach (var row in data)
        {
            AchieveButtonData buttonA = new()
            {
                textId      = int.Parse(row["Btn_A_Txt_ID"]),
                target      = row["Btn_A_Target"],
                effectValue = int.Parse(row["Btn_A_Effect_Value"]),
                effectRatio = float.Parse(row["Btn_A_Effect_Ratio"])
            };
            AchieveButtonData buttonB = new()
            {
                textId      = int.Parse(row["Btn_B_Txt_ID"]),
                target      = row["Btn_B_Target"],
                effectValue = int.Parse(row["Btn_B_Effect_Value"]),
                effectRatio = float.Parse(row["Btn_B_Effect_Ratio"])
            };
            AchieveTaskData task = new()
            {
                id = int.Parse(row["Event_Id"]),
                projectYear = int.Parse(row["Project_Year"]),
                threshold = float.Parse(row["Event_Achieve"]),
                buttons = new List<AchieveButtonData>(){buttonA, buttonB}
            };
            achieveRewardSO.tasks.Add(task);
        }
        Debug.Log("데이터 등록 완료");
    }
}
