using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TagDataSO", menuName = "Scriptable Objects/Staff/TagDataSO")]
public class TagDataSO : ScriptableObject
{
    public List<TagRow> tagList = new List<TagRow>();
}