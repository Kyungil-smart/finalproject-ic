using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MarketingData
{
    public bool selected;
    public string marketType;
    public float moneyMarket;
    public float heartMarket;
    public float rateMarket;
}

public class MarketingTailData
{
    public int num = 1;
    public Func<UniTaskVoid> nextCallback;
}

public class MarketingRenderData : UIRenderData
{
    public bool selectable;
    public MarketingTailData tailType;
    public List<MarketingData> marketingData;
}
