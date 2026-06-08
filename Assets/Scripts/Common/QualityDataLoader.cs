using UnityEngine;

public class QualityDataLoader
{
    public QualityDataSO qualityData;

    private void ClearData()
    {
        qualityData.rates.Clear();
    }

    public void LoadQulityData(GSheetManager gsheet)
    {
        ClearData();
        
    }
}
