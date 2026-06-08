using System;
using System.Collections.Generic;
using UnityEditor.Profiling;

public struct StaffSummaryData
{
    public bool selected;
    public bool hired;
    public StaffViewData viewData;
}


public class StaffSummaryRenderData : UIRenderData
{
    public Func<List<int>, List<int>> callbacks;  // selected Index List 받을 수 있도록..
    public List<StaffSummaryData> staffSummaryData;
}