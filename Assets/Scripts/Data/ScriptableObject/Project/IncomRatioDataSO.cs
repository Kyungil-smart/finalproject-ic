using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IncomeRatioRow
{
    public float achieveThreshold;
    public float ratio;
}

[CreateAssetMenu(fileName = "IncomeRatioDataSO", menuName = "Scriptable Objects/Project/IncomeRatioDataSO")]
public class IncomeRatioDataSO : ScriptableObject
{
    public List<IncomeRatioRow> ratioList = new();
}
