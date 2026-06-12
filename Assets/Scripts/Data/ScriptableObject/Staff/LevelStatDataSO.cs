using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelStatDataSO", menuName = "Scriptable Object/Staff/LevelStatDataSO")]
public class LevelStatDataSO : ScriptableObject
{
    public List<LevelStatRow> levelStatList = new List<LevelStatRow>();
}