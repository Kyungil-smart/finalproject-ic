using System.Collections.Generic;
using UnityEngine;

public abstract class EventParams { }

public class NormalEventParams : EventParams
{   // 외부 요인, 직원, 연계 이벤트 관련
    public string mainText;
    public List<(int id, string text)> choices = new();
}

public class RewardEventParams : EventParams
{
    public string mainText;
    public Sprite gradeImage;
    public List<(int id, Sprite icon, string text)> options = new();
}
