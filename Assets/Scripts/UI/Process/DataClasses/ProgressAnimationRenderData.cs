using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressAnimationRenderData : UIRenderData
{
    public GameDevProcName gameDevProcName;
    public List<string> progressTexts;
    public string staticImageId;
    public Action callback;
    public List<StaffEntity> staffIds;
}