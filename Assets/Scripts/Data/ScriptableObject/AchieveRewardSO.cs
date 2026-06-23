using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AchieveTaskData
{
    public int id;
    public int projectYear;
    public float threshold;
    public int titleTextId;
    public int descTextId;
    public List<EventButtonData> buttons;
}

[CreateAssetMenu(fileName = "AchieveRewardSO", menuName = "Scriptable Objects/AchieveRewardSO")]
public class AchieveRewardSO : ScriptableObject
{
    public List<AchieveTaskData> tasks;
}
