using System;
using System.Collections.Generic;
using UnityEngine;

public class EventUIParams : UIRenderData
{
    public EventType eventType;
    public int mainTextId;
    public Action<int> callback;
}

public class NormalEventUIParams : EventUIParams
{   // 외부 요인, 직원, 연계 이벤트 관련
    public List<(int id, int textId)> choices = new();

    public NormalEventUIParams(EventType eventType, int mainTextId, Action<int> callback)
    {
        this.eventType = eventType;
        this.mainTextId = mainTextId;
        this.callback = callback;
    }
}

public class RewardEventUIParams : EventUIParams
{
    public Sprite gradeImage;
    public List<(int id, Sprite icon, int textId)> options = new();

    public RewardEventUIParams(EventType eventType, int mainTextId, Action<int> callback, Sprite gradeImage)
    {
        this.eventType = eventType;
        this.mainTextId = mainTextId;
        this.callback = callback;
        this.gradeImage = gradeImage;
    }
}
