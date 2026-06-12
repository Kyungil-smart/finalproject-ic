using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelExpSO", menuName = "Scriptable Objects/Staff/LevelExpSO")]
public class LevelExpSO : ScriptableObject
{
    public List<LevelExpRow> levelExpList;
}