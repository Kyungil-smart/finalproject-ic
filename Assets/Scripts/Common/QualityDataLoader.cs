using UnityEngine;

public class QualityDataLoader
{
    public QualityDataSO qualityData;

    private void ClearData()
    {
        qualityData.rates.Clear();
        qualityData.targets.Clear();
    }

    public void LoadQualityData(GSheetManager gsheet)
    {
        ClearData();
        var data = gsheet.GetData();
        foreach (var row in data)
        {
            QualityRateData dataRate = new()
            {
                arrageCase1 = float.Parse(row["Arrange_Case1"]),
                arrageCase2 = float.Parse(row["Arrange_Case2"]),
                boostCase1 =  float.Parse(row["Boost_Case1"]),
                boostCase2 = float.Parse(row["Boost_Case2"]),
                boostCase3 = float.Parse(row["Boost_Case3"]),
                noiseMin = float.Parse(row["Noise_Min"]),
                noiseMax = float.Parse(row["Noise_Max"]),
                gtNeither = float.Parse(row["GT_Neither"]),
                gtEither = float.Parse(row["GT_Either"]),
                gtBoth = float.Parse(row["GT_Both"]),
                commSynergy = float.Parse(row["Comm_Synergy"]),
            };
            qualityData.rates.Add(dataRate);
        }
        Debug.Log($"[QualityDataLoader] Loading rates for {qualityData.rates.Count}");
    }
    public void LoadTargetData(GSheetManager gsheet)
    {
        var data = gsheet.GetData();
        foreach (var row in data)
        {
            QualityTargetData dataTarget = new()
            {
                avgLevel = float.Parse(row["Avg_Level"]),
                levelMultiplier = float.Parse(row["Level_Multiple"]),
                targetTotal = float.Parse(row["Target_Quality_Total"]),
                targetDesign = float.Parse(row["Target_Quality_Design"]),
                targetDev = float.Parse(row["Target_Quality_Dev"]),
                targetArt = float.Parse(row["Target_Quality_Art"]),
                targetDesignPre = float.Parse(row["Target_Quality_Design_Pre"]),
                targetDevPre = float.Parse(row["Target_Quality_Dev_Pre"]),
                targetArtPre = float.Parse(row["Target_Quality_Art_Pre"]),
                targetTotalPre = float.Parse(row["Target_Quality_Total_Pre"]),
            };
            qualityData.targets.Add(dataTarget);
        }
        Debug.Log($"[QualityDataLoader] Loading rates for {qualityData.rates.Count}");
    }
    
}
