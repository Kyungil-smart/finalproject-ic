using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StaffDataSO", menuName = "Scriptable Objects/Staff/StaffDataSO")]
public class StaffDataSO : ScriptableObject
{
    public List<StaffRow> staffList = new List<StaffRow>();
}

