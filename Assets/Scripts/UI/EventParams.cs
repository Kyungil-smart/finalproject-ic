using System.Collections.Generic;
using UnityEngine;

public abstract class EventParams { }

public class RegularEvent : EventParams
{
    public string mainText;
    public List<(int id, string text)> choices = new();
}

public class Staff : RegularEvent { }
public class Linkage : RegularEvent { }

public class RewardEvent : EventParams
{
    public string mainText;
    public Sprite gradeSprite;
    public List<(int id, Sprite icon)> options = new();
}
