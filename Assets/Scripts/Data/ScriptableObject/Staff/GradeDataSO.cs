using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GradeDataSO", menuName = "Scriptable Object/Staff/GradeDataSO")]
public class GradeDataSO : ScriptableObject
{
    public List<GradeRow> gradeList = new List<GradeRow>();
}