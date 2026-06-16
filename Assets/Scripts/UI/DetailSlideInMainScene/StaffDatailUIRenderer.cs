using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffDatailUIRenderer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI salaryText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI genderText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI discText;
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
    
    public void Render(StaffViewData viewData, Action onCloseCallback = null)
    {
        foreach (var tag in tags) tag.SetActive(false);
        closeBtn.onClick.RemoveAllListeners();
        if (onCloseCallback != null) closeBtn.onClick.AddListener(() => onCloseCallback());
        closeBtn.onClick.AddListener(() => gameObject.SetActive(false));
        
        nameText.text = viewData.Staff_Name;
        genderText.text = viewData.Staff_Gender ? "M" : "F";
        levelText.text = viewData.Level.ToString();
        gradeText.text = viewData.Grade;
        discText.text = viewData.DISC_Type;
        jobText.text = viewData.Job_Name;
        concentratText.text = viewData.Final_Common_Concentration.ToString();
        careerText.text = viewData.Final_Career.ToString();
        creativityText.text = viewData.Final_Common_Creativity.ToString();
        communicationText.text = viewData.Final_Common_Communication.ToString();
        designText.text = viewData.Final_Job_Planning.ToString();
        developText.text = viewData.Final_Job_Development.ToString();
        artText.text = viewData.Final_Job_Art.ToString();
        salaryText.text = viewData.Salary.ToString();
        
        for (int i = 0; i < viewData.All_Tags.Count; i++)
        {
            var tmp = tags[i].GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = viewData.All_Tags[i];
            tags[i].SetActive(true);
        }
        
        gameObject.SetActive(true);
    }
}