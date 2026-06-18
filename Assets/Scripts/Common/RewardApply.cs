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
        Debug.Log($"[EventManager:ApplyData] Total; {qt.TotalQuality}");
    }
}

public class GoldReward : IEventRouter
{
    public void Apply(EventButtonData btn)
    {
        ServiceLocater.Get<IGameManager>().AddMoney(btn.effectValue);
        Debug.Log(ServiceLocater.Get<IGameManager>().Money);
    }
}

public class HeartReward : IEventRouter
{
    public void Apply(EventButtonData btn)
    {
        ServiceLocater.Get<IGameManager>().AddHeart(btn.effectValue);
        Debug.Log(ServiceLocater.Get<IGameManager>().Heart);
    }
}

public class DesignQualityReward : IEventRouter
{
    public void Apply(EventButtonData btn)
    {
        var qt = ServiceLocater.Get<IProjectManager>();
        qt.UpdateDesignQuality(qt.DesignQuality, btn.effectRatio);
        Debug.Log($"[EventManager:ApplyData] Design; {qt.DesignQuality}");
    }
}

public class ArtQualityReward : IEventRouter
{
    public void Apply(EventButtonData btn)
    {
        var qt = ServiceLocater.Get<IProjectManager>();
        qt.UpdateArtQuality(qt.DesignQuality, btn.effectRatio);
        Debug.Log($"[EventManager:ApplyData] Art; {qt.DesignQuality}");
    }
}

public class DevQualityReward : IEventRouter
{
    public void Apply(EventButtonData btn)
    {
        var qt = ServiceLocater.Get<IProjectManager>();
        qt.UpdateDevQuality(qt.DevQuality, btn.effectRatio);
        Debug.Log($"[EventManager:ApplyData] Dev; {qt.DevQuality}");
    }
}
