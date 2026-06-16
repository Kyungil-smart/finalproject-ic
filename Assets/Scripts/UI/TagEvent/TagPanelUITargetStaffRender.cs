using R3;
using TMPro;
using UnityEngine;

public class TagPanelUITargetStaffRender : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI staffName;
    [SerializeField] private TextMeshProUGUI staffJob;
    [SerializeField] private TextMeshProUGUI staffLevel;
    [SerializeField] private TextMeshProUGUI staffDev;
    [SerializeField] private TextMeshProUGUI staffArt;
    [SerializeField] private TextMeshProUGUI staffDesign;
    [SerializeField] private TextMeshProUGUI staffConcentration;
    [SerializeField] private TextMeshProUGUI staffCommunity;
    [SerializeField] private TextMeshProUGUI staffCreativity;
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start() => ServiceLocater.Get<IGameManager>().Money.Subscribe(UpdateMoney).AddTo(this);
    
    public void Render(StaffEntity staffEntity)
    {
        staffName.text = staffEntity.init.Staff_Name;
        staffJob.text = staffEntity.init.Job.ToString();
        staffLevel.text = $"Lv. {staffEntity.init.Level:D2}";
        staffDev.text = $"개발력\n{staffEntity.GetDevelopment()}";
        staffDesign.text = $"기획력\n{staffEntity.GetDesign()}";
        staffArt.text = $"예술성\n{staffEntity.GetArt()}";
        if (staffConcentration != null) staffConcentration.text = $"집중력\n{staffEntity.GetConcentration()}";
        if (staffCommunity != null) staffCommunity.text = $"소통력\n{staffEntity.GetCommunication()}";
        if (staffCreativity != null) staffCreativity.text = $"창조력\n{staffEntity.GetCreativity()}";
    }

    private void UpdateMoney(int money) => moneyText.text = $"자금\n{money}";
}