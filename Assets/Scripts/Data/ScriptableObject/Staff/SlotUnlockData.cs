using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SlotData
{
    public int id;
    public int cost;
    public bool unlocked;
}

[CreateAssetMenu(fileName = "LevelStatDataSO", menuName = "Scriptable Object/Staff/SlotUnlockSO")]
public class SlotUnlockDataSO : ScriptableObject
{
    public List<SlotData> slots;
}
