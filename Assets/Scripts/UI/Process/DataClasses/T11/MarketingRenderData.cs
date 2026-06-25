using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MarketingData
{
    public bool selected;
    public string marketType;
    public uint moneyMarket;
    public uint heartMarket;
    public uint rateMarket;
    public string moneyResultType;
}

public class MarketingTailData
{
    public int num = 1;
    public Func<UniTaskVoid> previousCallback;
    public Func<UniTaskVoid> nextCallback;
    public Func<List<int>, UniTaskVoid> confirmCallback;
}

public class MarketingRenderData : UIRenderData
{
    public bool selectable;
    public MarketingTailData tailType;
    public List<MarketingData> marketingData;
}
