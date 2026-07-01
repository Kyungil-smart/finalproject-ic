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
        return (uint)(genre.GT_Cost * theme.GT_Cost_Ratio);
    }
    
    public void CalculateCost()
    {
        var pm = ServiceLocater.Get<IProjectManager>();
        var genRow = _genreThemeData.genreThemeList.Find(r => r.GT_ID == pm.Genre.id);
        var theRow = _genreThemeData.genreThemeList.Find(r => r.GT_ID == pm.Theme.id);

        uint gtCost = (uint)(genRow.GT_Cost * theRow.GT_Cost_Ratio);
        pm.Cost = gtCost + pm.StaffsCost;
        Debug.Log($"[CalculateCost] pm.Cost: {pm.Cost}");
    }

    public void CalculateIncome()
    {
        float achieve = ServiceLocater.Get<IQualityManager>().Calculator.CalculateFullAchieve();
        var pm = ServiceLocater.Get<IProjectManager>();
        
        var income = _incomeRatioDataSO.ratioList.Find(i => achieve >= i.achieveMin && achieve < i.achieveMax);
        if (income == null) return;

        // Todo. 계산식 현재 마케팅이 없어서 간단하게만 계산했습니다. 나중에 마케팅 추가되면 계산식 수정할 예정입니다.
        pm.Income = (uint)(pm.Cost * income.moneyRatio);
        Debug.Log($"[CalculateIncome] pm.Income: {pm.Income}");
    }
}
