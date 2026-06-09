using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor.Profiling;

public class StaffSummaryData
{
    public bool selected;
    public bool hired;
    public StaffViewData viewData;
}

public class StaffSummaryTailData
{
    public int num = 1;
    public Func<UniTaskVoid> previousCallback;
    public Func<UniTaskVoid> nextCallback;  // 일반 확인도 여기에 포함.
    public Func<List<int>, UniTaskVoid> confirmCallback;  
}


public class StaffSummaryRenderData : UIRenderData
{
    public bool selectable;
    public StaffSummaryTailData tailType;
    public List<StaffSummaryData> staffSummaryData;
}