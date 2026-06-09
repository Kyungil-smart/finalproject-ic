using System;
using System.Collections.Generic;
using UnityEditor.Profiling;

public struct StaffSummaryData
{
    public bool selected;
    public bool hired;
    public StaffViewData viewData;
}

public class StaffSummaryTailData
{
    public int num = 1;
    public Action previousCallback;
    public Action nextCallback;  // 일반 확인도 여기에 포함.
    public Action<List<int>> confirmCallback;  
}


public class StaffSummaryRenderData : UIRenderData
{
    public StaffSummaryTailData tailType;
    public List<StaffSummaryData> staffSummaryData;
}