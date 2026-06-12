using UnityEngine;

public class RewardApply
{
    
}

public class QualityReward : IEventRouter
{
    public void Apply(EventButtonData btn)
    {
        var qt = ServiceLocater.Get<IProjectManager>();
        qt.UpdateTotalQuality(qt.TotalQuality, btn.effectRatio);
        Debug.Log(qt.TotalQuality);
    }
}