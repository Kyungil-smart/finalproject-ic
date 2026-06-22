using R3;
using TMPro;
using UnityEngine;

public class TagPanelUITargetStaffRender : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI staffName;
    [SerializeField] private TextMeshProUGUI staffJob;
    [SerializeField] private TextMeshProUGUI staffLevel;
    [SerializeField] private TextMeshProUGUI staffConcent;
    [SerializeField] private TextMeshProUGUI staffCreative;
    [SerializeField] private TextMeshProUGUI staffCommunication;
    [SerializeField] private TextMeshProUGUI staffJobValue;
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start() => ServiceLocater.Get<IGameManager>().Money.Subscribe(UpdateMoney).AddTo(this);
    
    public void Render(StaffEntity staffEntity)
    {
        staffName.text = staffEntity.init.Staff_Name;
        staffJob.text = staffEntity.init.Job.ToString();
        staffLevel.text = $"Lv. {staffEntity.init.Level:D2}";
        staffConcent.text = $"{staffEntity.GetConcentration()}";
        staffCommunication.text = $"{staffEntity.GetCommunication()}";
        staffCreative.text = $"{staffEntity.GetCreativity()}";
        switch (staffEntity.init.Job)
        {
            case JobType.Artist:
                staffJobValue.text = $"{staffEntity.GetArt()}";
                break;
            case JobType.Designer:
                staffJobValue.text = $"{staffEntity.GetDesign()}";
                break;
            case JobType.Developer:
                staffJobValue.text = $"{staffEntity.GetDevelopment()}";
                break;
        }
    }

    private void UpdateMoney(int money) => moneyText.text = $"자금\n{money}";
}