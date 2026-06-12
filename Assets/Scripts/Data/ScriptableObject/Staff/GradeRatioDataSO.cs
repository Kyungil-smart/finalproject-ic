using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GradeRatioDataSO", menuName = "Scriptable Object/Staff/GradeRatioDataSO")]
public class GradeRatioDataSO : ScriptableObject
{
    public List<GradeRatioRow> ratioList = new List<GradeRatioRow>();
}