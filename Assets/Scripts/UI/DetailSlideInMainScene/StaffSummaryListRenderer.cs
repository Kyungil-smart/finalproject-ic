using System.Collections.Generic;
using UnityEngine;

public class StaffSummaryListRenderer : MonoBehaviour
{
    [SerializeField] private GameObject staffSummaryHeadPrefab;
    [SerializeField] private StaffSummaryPanelRender staffSummaryPanel;
    [SerializeField] private GameObject staffSummaryTailPrefab;
    [SerializeField] private Transform contentsObject;

    public void Render(List<(StaffViewData viewData, bool hired)> dataList)
    {
        Instantiate(staffSummaryHeadPrefab, contentsObject);
        foreach (var (viewData, hired) in dataList)
        {
            var sr = Instantiate(staffSummaryPanel, contentsObject);
            sr.Render(viewData, hired);
        }
        Instantiate(staffSummaryTailPrefab, contentsObject);
    }
}
