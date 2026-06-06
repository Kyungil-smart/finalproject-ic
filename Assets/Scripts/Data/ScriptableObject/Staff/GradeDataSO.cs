using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GradeDataSO", menuName = "Data/GradeDataSO")]
public class GradeDataSO : ScriptableObject
{
    public List<GradeRow> gradeList = new List<GradeRow>();
}