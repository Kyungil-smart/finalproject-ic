using System;
using System.Collections.Generic;
using UnityEngine;


// 마케팅 읽기
// https://docs.google.com/spreadsheets/d/18pW0HvqUVgkDqUf6f7a9AZl7ZWxIZpGgOkGL5sexvbw/edit?gid=735612401#gid=735612401 칼럼 명 그대로 사용

[Serializable]
public class MarketingRow
{
    public string marketingType;
    public float moneyMarketing;
    public float heartMarketing;
    public float rateMarketing;
    public int effectIDMarketing;
}

[CreateAssetMenu(fileName = "MarketingDataSO", menuName = "Data/MarketingDataSO")]
public class MarketingDataSO : ScriptableObject
{
    public List<MarketingRow> marketingList = new List<MarketingRow>();

}
