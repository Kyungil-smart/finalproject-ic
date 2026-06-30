using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventEffectData
{
    public int btId;
    public string target;
    public int value;
    public float ratio;
}

public class EventUIRenderData : UIRenderData
{
    public EventType eventType;
    public int mainTextId;
    public Action<int> callback;
}

public class NormalEventUIRenderData : EventUIRenderData
{   // 외부 요인, 직원, 연계 이벤트 관련
    public List<(int id, int textId, EventEffectData effectData)> choices = new();

    public NormalEventUIRenderData(EventType eventType, int mainTextId, Action<int> callback)
    {
        this.eventType = eventType;
        this.mainTextId = mainTextId;
        this.callback = callback;
    }
}

public class RewardEventUIRenderData : EventUIRenderData
{
    public Sprite gradeImage;
    public List<(int id, Sprite icon, int textId)> options = new();

    public RewardEventUIRenderData(EventType eventType, int mainTextId, Action<int> callback, Sprite gradeImage)
    {
        this.eventType = eventType;
        this.mainTextId = mainTextId;
        this.callback = callback;
        this.gradeImage = gradeImage;
    }
}
