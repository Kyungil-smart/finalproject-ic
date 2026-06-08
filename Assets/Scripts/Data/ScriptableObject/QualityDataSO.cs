using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QualityDataSO", menuName = "Scriptable Objects/QualityDataSO")]
public class QualityDataSO : ScriptableObject
{
    public List<QualityRateData> rates;
}

[Serializable]
public struct QualityRateData
{
    public float arrageCase1;
    public float arrageCase2;
    public float boostCase1;
    public float boostCase2;
    public float boostCase3;
    public float noiseMin;
    public float noiseMax;
    public float gtNeither;
    public float gtEither;
    public float gtBoth;
    
}