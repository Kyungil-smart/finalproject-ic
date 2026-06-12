using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GetExpSO", menuName = "Scriptable Objects/Staff/GetExpSO")]
public class GetExpSO : ScriptableObject
{
    public List<GetExpRow> getExpList;
}