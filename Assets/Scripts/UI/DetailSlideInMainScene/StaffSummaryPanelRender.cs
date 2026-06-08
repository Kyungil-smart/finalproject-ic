using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffSummaryPanelRender : MonoBehaviour
{
    [SerializeField] private Image faseImage;
    [SerializeField] private TextMeshProUGUI staffIdText;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI hiredText;
    [SerializeField] private TextMeshProUGUI jobTxt;
    [SerializeField] private TextMeshProUGUI gradeTxt;
    [SerializeField] private TextMeshProUGUI tag1Txt;
    [SerializeField] private TextMeshProUGUI tag2Txt;

    private string StaffId(int id) => $"ID: {id}";
    private string Hired(bool hired) => hired ? "" : "New";

    public void Render(StaffViewData data, bool hired)
    {
        faseImage.sprite = null;  // ToDo. 추후 addressable 로 불러오는 것 확인하기
        staffIdText.text = StaffId(data.Staff_ID);
        nameTxt.text = data.Staff_Name;
        hiredText.text = Hired(hired);
        jobTxt.text = data.Job_Name;
        gradeTxt.text = data.Grade;
        tag1Txt.text = null;  // ToDo. Tag 개발 완료시 추가
        tag2Txt.text = null;
    }
}
