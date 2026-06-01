using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelStatDataSO", menuName = "Data/LevelStatDataSO")]
public class LevelStatDataSO : ScriptableObject
{
    public List<LevelStatRow> levelStatList = new List<LevelStatRow>();
}