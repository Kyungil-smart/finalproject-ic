using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TagDataSO", menuName = "Scriptable Object/Staff/TagDataSO")]
public class TagDataSO : ScriptableObject
{
    public List<TagRow> tagList = new List<TagRow>();
}