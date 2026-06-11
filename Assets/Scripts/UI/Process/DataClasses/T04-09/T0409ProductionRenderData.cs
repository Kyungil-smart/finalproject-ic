using System;
using System.Collections.Generic;
using UnityEngine;

public class T0409ProductionStaffListRenderData : UIRenderData
{
    public List<StaffSummaryData> staffList;
    public Action<List<int>> onSelectCallback;
}

public class T0409ProductionLeaderResultRenderData : UIRenderData
{
    public List<StaffViewData> leaderList;
    public Action onGoBackCallback;
    public Action onGoNextCallback;
}