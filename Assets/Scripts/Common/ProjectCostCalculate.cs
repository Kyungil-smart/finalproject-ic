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

    public void CalculateCost()
    {
        var pm = ServiceLocater.Get<IProjectManager>();
        var genRow = _genreThemeData.genreThemeList.Find(r => r.GT_ID == pm.Genre.id);
        var theRow = _genreThemeData.genreThemeList.Find(r => r.GT_ID == pm.Theme.id);

        uint gtCost = (uint)(genRow.GT_Cost * theRow.GT_Cost_Ratio);
        pm.Cost = gtCost + pm.StaffsCost;
    }

    public void CalculateIncome()
    {
        float achieve = ServiceLocater.Get<IQualityManager>().Calculator.CalculateFullAchieve();
        var pm = ServiceLocater.Get<IProjectManager>();
        
        var income = _incomeRatioDataSO.ratioList.Find(i => achieve >= i.achieveMin && achieve < i.achieveMax);
        if (income == null) return;

        pm.Income = (uint)(pm.Cost * income.moneyRatio);
    }
}
