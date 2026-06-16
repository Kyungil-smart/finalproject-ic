using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct AwardsData
{
    public NameTag name;
    public int reqDesign;
    public int reqArt;
    public int reqDev;
    public int descId;
    public string target;
    public int value;
    public int resultId;
}


[CreateAssetMenu(fileName = "AwardsDataSO", menuName = "Scriptable Objects/Project/AwardsDataSO")]
public class AwardsDataSO : ScriptableObject
{
    public List<AwardsData> awardsDataList;
}