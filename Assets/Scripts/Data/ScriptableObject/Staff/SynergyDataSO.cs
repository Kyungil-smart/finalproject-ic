using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SynergyDataSO", menuName = "Scriptable Objects/Staff/SynergyDataSO")]
public class SynergyDataSO : ScriptableObject
{
    public List<SynergyRow> synergyList = new();
}
