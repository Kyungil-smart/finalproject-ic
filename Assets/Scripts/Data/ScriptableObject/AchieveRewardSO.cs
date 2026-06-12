using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AchieveTaskData
{
    public int id;
    public int projectYear;
    public float threshold;
    public List<AchieveButtonData> buttons;
}

[Serializable]
public class AchieveButtonData
{
    public int textId;
    public string target;
    public int effectValue;
    public float effectRatio;
}

[CreateAssetMenu(fileName = "AchieveRewardSO", menuName = "Scriptable Objects/AchieveRewardSO")]
public class AchieveRewardSO : ScriptableObject
{
    public List<AchieveTaskData> tasks;
}
