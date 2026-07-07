using System;
using UnityEngine;

public class ProjectCostCalculate
{
    private GenreThemeTypeDataSO _genreThemeData;
    private IncomeRatioDataSO _incomeRatioDataSO;

    public ProjectCostCalculate(GenreThemeTypeDataSO genreThemeData, IncomeRatioDataSO incomeRatioDataSO)
    {
        _genreThemeData = genreThemeData;
        _incomeRatioDataSO = incomeRatioDataSO;
    }

    public uint CalculateDevCost(int genreId, int themeId)
    {
        var genre = _genreThemeData.genreThemeList.Find(r => r.GT_ID == genreId);
        var theme = _genreThemeData.genreThemeList.Find(r => r.GT_ID == themeId);
        if (genre == null || theme == null) return 0;
        return (uint)Math.Round(genre.GT_Cost * theme.GT_Cost_Ratio);
    }
    
    public void CalculateCost()
    {
        var pm = ServiceLocater.Get<IProjectManager>();
        var genRow = _genreThemeData.genreThemeList.Find(r => r.GT_ID == pm.Genre.id);
        var theRow = _genreThemeData.genreThemeList.Find(r => r.GT_ID == pm.Theme.id);

        uint gtCost = (uint)(genRow.GT_Cost * theRow.GT_Cost_Ratio);
        pm.Cost = gtCost;
        Debug.Log($"[CalculateCost] pm.Cost: {pm.Cost}");
    }

    public void CalculateIncome()
    {
        float achieve = ServiceLocater.Get<IQualityManager>().Calculator.CalculateFullAchieve();
        var pm = ServiceLocater.Get<IProjectManager>();
        
        var income = _incomeRatioDataSO.ratioList.Find(i => achieve >= i.achieveMin && achieve < i.achieveMax);
        if (income == null) return;
        
        pm.Income = (uint)Math.Round((pm.Cost + pm.StaffsCost) * income.moneyRatio + ServiceLocater.Get<IProjectManager>().MarketingBonus);
        Debug.Log($"[CalculateIncome] pm.Income: {pm.Income}");
    }
}
