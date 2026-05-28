using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventUIParams
{
    public int mainTextId;
    public Action<int> callback;
}

public class NormalEventUIParams : EventUIParams
{   // 외부 요인, 직원, 연계 이벤트 관련
    public List<(int id, int textId)> choices = new();
}

public class RewardEventUIParams : EventUIParams
{
    public Sprite gradeImage;
    public List<(int id, Sprite icon, int textId)> options = new();
}
