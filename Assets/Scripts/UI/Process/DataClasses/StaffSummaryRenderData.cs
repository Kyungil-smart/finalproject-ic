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
    public Action[] callbacks;
    public List<StaffSummaryData> staffSummaryData;
}