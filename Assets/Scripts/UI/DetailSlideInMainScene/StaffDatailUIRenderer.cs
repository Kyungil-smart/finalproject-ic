using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffDatailUIRenderer : MonoBehaviour
{
    [SerializeField] private Image staffImage;
    [SerializeField] private TextMeshProUGUI salaryText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject genderMale;
    [SerializeField] private GameObject genderFemale;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI[] discTexts;
    [SerializeField] private TextMeshProUGUI jobText;
    [SerializeField] private TextMeshProUGUI concentratText;
    [SerializeField] private TextMeshProUGUI careerText;
    [SerializeField] private TextMeshProUGUI creativityText;
    [SerializeField] private TextMeshProUGUI communicationText;
    [SerializeField] private TextMeshProUGUI designText;
    [SerializeField] private TextMeshProUGUI developText;
    [SerializeField] private TextMeshProUGUI artText;
    [SerializeField] private Button closeBtn; 
    [SerializeField] private GameObject[] tags;
        
    [Header("직업 별 값 표기")]
    [SerializeField] private GameObject artPanel;
    [SerializeField] private TextMeshProUGUI artValue;
    [SerializeField] private GameObject designPanel;
    [SerializeField] private TextMeshProUGUI designValue;
    [SerializeField] private GameObject devPanel;
    [SerializeField] private TextMeshProUGUI devValue;

    [Header("DISC 컬러 표기")] 
    [SerializeField] private Color discDisableColor;
    [SerializeField] private Color discEnableColor;
    
    public void Render(StaffViewData viewData, Action onCloseCallback = null)
    {
        foreach (var tag in tags) tag.SetActive(false);
        closeBtn.onClick.RemoveAllListeners();
        if (onCloseCallback != null) closeBtn.onClick.AddListener(() => onCloseCallback());
        closeBtn.onClick.AddListener(() => gameObject.SetActive(false));
        staffImage.sprite = viewData.Thumbnail; 
        nameText.text = viewData.Staff_Name;
        genderMale.SetActive(viewData.Staff_Gender);
        genderFemale.SetActive(!viewData.Staff_Gender);
        levelText.text = viewData.Level.ToString();
        gradeText.text = viewData.Grade;

        foreach (var textObj in discTexts)
        {
            textObj.color = discDisableColor;
        }
        switch (viewData.DISC_Type)
        {
            case DiscType.D:
                discTexts[0].color = discEnableColor;
                break;
            case DiscType.I:
                discTexts[1].color = discEnableColor;
                break;
            case DiscType.S:
                discTexts[2].color = discEnableColor;
                break;
            case DiscType.C:
                discTexts[3].color = discEnableColor;
                break;
        }
        jobText.text = viewData.Job_Name;
        concentratText.text = viewData.Final_Common_Concentration.ToString();
        careerText.text = viewData.Final_Career.ToString();
        creativityText.text = viewData.Final_Common_Creativity.ToString();
        communicationText.text = viewData.Final_Common_Communication.ToString();
        designText.text = viewData.Final_Job_Planning.ToString();
        developText.text = viewData.Final_Job_Development.ToString();
        artText.text = viewData.Final_Job_Art.ToString();
        salaryText.text = viewData.Salary.ToString();

        artPanel.SetActive(false);
        designPanel.SetActive(false);
        devPanel.SetActive(false);
        
        switch (viewData.Job_Type)
        {
            case JobType.Artist:
                artPanel.SetActive(true);
                break;
            case JobType.Designer:
                designPanel.SetActive(true);
                break;
            default:
                devPanel.SetActive(true);
                break;
        }
        
        for (int i = 0; i < viewData.All_Tags.Count; i++)
        {
            var tmp = tags[i].GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = viewData.All_Tags[i];
            tags[i].SetActive(true);
        }
        
        gameObject.SetActive(true);
    }
}