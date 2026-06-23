using System.Collections.Generic;
using UnityEngine;

public class EventRouter : IEventRouter
{
    private Dictionary<string, IEventRouter> _router = new()
    {
        {"Total_Quality", new QualityReward()},
        {"Art_Quality", new ArtQualityReward()},
        {"Dev_Quality", new DevQualityReward()},
        {"Design_Quality", new DesignQualityReward()},
        {"Money", new GoldReward()},
        {"Heart", new HeartReward()},
    };
    public void Apply(EventButtonData btn)
    {
        if (string.IsNullOrEmpty(btn.target)) return;
        if (_router.TryGetValue(btn.target, out var target)) target.Apply(btn);
        else Debug.LogWarning("Target not found: " + btn.target);
    }
}
