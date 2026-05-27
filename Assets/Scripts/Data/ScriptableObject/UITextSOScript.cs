using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Line
{
    public int id;
    public string text;
}

[CreateAssetMenu(fileName = "UITextSO", menuName = "ScriptableObject/UITextSO")]
public class UITextSOScript : ScriptableObject
{
    public List<Line> lines;
}
