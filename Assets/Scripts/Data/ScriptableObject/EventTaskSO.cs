using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventButtonData
{
    public int textId;
    public string target;
    public int effectValue;
    public float effectRatio;
}

[Serializable]
public class EventTaskData
{
    public int id;
    public int titleTextId;
    public int categoryId;
    public int descTextId;
    public List<EventButtonData> buttons;
}

[CreateAssetMenu(fileName = "EventTaskSO", menuName = "Scriptable Objects/EventTaskSO")]
public class EventTaskSO : ScriptableObject
{
    public EventType eventType;
    public List<EventTaskData> tasks;
}
