using System.Collections.Generic;
using UnityEngine;

public class EventRouter : IEventRouter
{
    private Dictionary<string, IEventRouter> _router = new()
    {
        {"Total_Quality", new QualityReward()},
        {"Money", new GoldReward()}
    };
    public void Apply(EventButtonData btn)
    {
        if (string.IsNullOrEmpty(btn.target)) return;
        if (_router.TryGetValue(btn.target, out var target)) target.Apply(btn);
        else Debug.LogWarning("Target not found: " + btn.target);
    }
}
