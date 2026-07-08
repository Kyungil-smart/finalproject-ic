using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class T12IncomeUIRender : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameTitleValue;
    [SerializeField] private TextMeshProUGUI incomeValue;
    [SerializeField] private TextMeshProUGUI marketBonusValue;
    [SerializeField] private TextMeshProUGUI marketCostValue;
    [SerializeField] private TextMeshProUGUI projectCostValue;
    [SerializeField] private TextMeshProUGUI staffsCostValue;
    [SerializeField] private TextMeshProUGUI earningsValue;
    [SerializeField] private Button incomeConfirmBtn;

    public void Render(T12IncomeUIRenderData data)
    {
        // ToDo. 마케팅은 추후에.
        var projectManager = ServiceLocater.Get<IProjectManager>();
        gameTitleValue.text = $"{projectManager.GetProjectData()?.name}";
        incomeValue.text = $"{projectManager.Income} 원";
        projectCostValue.text = $"{projectManager.Cost} 원";
        staffsCostValue.text = $"{projectManager.StaffsCost} 원";
        earningsValue.text = $"{projectManager.Earnings} 원";
        marketCostValue.text = $"{projectManager.MarketingCost} 원";
        marketBonusValue.text = $"{projectManager.MarketingBonus} 원";
        
        incomeConfirmBtn.onClick.RemoveAllListeners();
        incomeConfirmBtn.onClick.AddListener(() => data.btCallback?.Invoke());
        incomeConfirmBtn.onClick.AddListener(() => gameObject.SetActive(false));
    }
}