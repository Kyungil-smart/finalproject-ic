using System;
using System.Collections.Generic;
using UnityEngine;


// 마케팅 읽기
// https://docs.google.com/spreadsheets/d/18pW0HvqUVgkDqUf6f7a9AZl7ZWxIZpGgOkGL5sexvbw/edit?gid=735612401#gid=735612401 칼럼 명 그대로 사용

[Serializable]
public class MarketingRow
{
    public string Marketing_Type;
    public float Money_Marketing;
    public float Heart_Marketing;
    public float Rate_Marketing;
    public int EffectID_Marketing;
}

[CreateAssetMenu(fileName = "MarketingDataSO", menuName = "Data/MarketingDataSO")]
public class MarketingDataSO : ScriptableObject
{
    public List<MarketingRow> MarketingList = new List<MarketingRow>();

}
