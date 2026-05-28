using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Line
{
    public int id;
    public string text;

    public Line(string id, string text)
    {
        this.id = int.Parse(id);
        this.text = text;
    }
}

[CreateAssetMenu(fileName = "UITextSO", menuName = "ScriptableObject/UITextSO")]
public class UITextSOScript : ScriptableObject
{
    public LanguageType language;
    public List<Line> lines;
}
