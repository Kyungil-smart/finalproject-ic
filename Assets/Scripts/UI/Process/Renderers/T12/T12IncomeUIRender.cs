using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class T12IncomeUIRender : MonoBehaviour
{
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
        incomeValue.text = $"{projectManager.Income} 원";
        projectCostValue.text = $"{projectManager.Cost} 원";
        staffsCostValue.text = $"{projectManager.StaffsCost} 원";
        earningsValue.text = $"{projectManager.Earnings} 원";
        
        incomeConfirmBtn.onClick.RemoveAllListeners();
        incomeConfirmBtn.onClick.AddListener(() => data.btCallback?.Invoke());
        incomeConfirmBtn.onClick.AddListener(() => gameObject.SetActive(false));
    }
}