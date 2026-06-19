using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SlotDef
{
    public int id;
    public int cost;
}

[CreateAssetMenu(fileName = "SlotUnlockDataSO", menuName = "Scriptable Objects/Staff/SlotUnlockDataSO")]
public class SlotUnlockDataSO : ScriptableObject
{
    public List<SlotDef> slots;
}
