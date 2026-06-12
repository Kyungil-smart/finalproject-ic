using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelStatDataSO", menuName = "Scriptable Objects/Staff/LevelStatDataSO")]
public class LevelStatDataSO : ScriptableObject
{
    public List<LevelStatRow> levelStatList = new List<LevelStatRow>();
}