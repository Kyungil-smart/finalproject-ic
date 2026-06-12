using System.Collections.Generic;
using UnityEngine;

public class EventRouter : IEventRouter
{
    private Dictionary<string, IEventRouter> _router = new()
    {
        {
            "Total_Quality", new QualityReward()
        }
    };
    public void Apply(EventButtonData btn)
    {
        
    }
}
