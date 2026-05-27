using System.Collections.Generic;
using UnityEngine;

public abstract class EventParams { }

public class NormalEventParams : EventParams
{   // 외부 요인, 직원, 연계 이벤트 관련
    public int mainTextId;
    public List<(int id, int textId)> choices = new();
}

public class RewardEventParams : EventParams
{
    public int mainTextId;
    public Sprite gradeImage;
    public List<(int id, Sprite icon, int textId)> options = new();
}
