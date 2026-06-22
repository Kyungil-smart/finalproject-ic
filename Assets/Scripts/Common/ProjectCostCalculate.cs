using UnityEngine;

public class ProjectCostCalculate
{
    private GenreThemeTypeDataSO _genreThemeData;

    public ProjectCostCalculate(GenreThemeTypeDataSO genreThemeData)
    {
        _genreThemeData = genreThemeData;
    }

    public void CalculateCost()
    {
        var pm = ServiceLocater.Get<IProjectManager>();
        var genRow = _genreThemeData.genreThemeList.Find(r => r.GT_ID == pm.Genre.id);
        var theRow = _genreThemeData.genreThemeList.Find(r => r.GT_ID == pm.Theme.id);

        uint gtCost = (uint)(genRow.GT_Cost * theRow.GT_Cost_Ratio);
        pm.Cost = gtCost + pm.StaffsCost;
    }
}
