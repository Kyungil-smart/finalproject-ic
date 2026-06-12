using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelStatDataSO", menuName = "Scriptable Object/Staff/LevelStatDataSO")]
public class LevelExpSO : ScriptableObject
{
    public List<LevelExpRow> levelExpList;
}