using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IncomeRatioRow
{
    public float achieveMin;
    public float achieveMax;
    public float moneyRatio;
    public float heartRatio;
}

[CreateAssetMenu(fileName = "IncomeRatioDataSO", menuName = "Scriptable Objects/Project/IncomeRatioDataSO")]
public class IncomeRatioDataSO : ScriptableObject
{
    public List<IncomeRatioRow> ratioList = new();
}
