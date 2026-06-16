using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct StaffTagUIRenderData
{
    public GameDevProcName procName;
    public string staffName;
    public int staffLevel;
}


public class T12ProjectDetailUIRender : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI projectNameTxt;    
    [SerializeField] private TextMeshProUGUI genreTxt;
    [SerializeField] private TextMeshProUGUI themeTxt;
    [SerializeField] private TextMeshProUGUI gradeTxt;
    [SerializeField] private TextMeshProUGUI costTxt;
    [SerializeField] private TextMeshProUGUI incomeTxt;
    [SerializeField] private TextMeshProUGUI awardsTxt;
    
    [SerializeField] private T12StaffRenderer[] staffTags;

    [SerializeField] private Button confirmBtn; 
    
    public void Render(T12ProjectDetailUIRenderData data)
    {
        var projectManager = ServiceLocater.Get<IProjectManager>();

        projectNameTxt.text = projectManager.GetProjectData().name;
        genreTxt.text = projectManager.Genre.name;
        themeTxt.text = projectManager.Theme.name;
        gradeTxt.text = projectManager.GetProjectData().grade.ToString();
        costTxt.text = projectManager.Cost.ToString();
        incomeTxt.text = projectManager.Income.ToString();
        awardsTxt.text = projectManager.Awards.name.name;
        
        // 유저 평론
        // 전문가 평론
        
        List<StaffTagUIRenderData> staffList = new();
        MakeStaffTagList(GameDevProcName.ConceptPreProduction, staffList);
        MakeStaffTagList(GameDevProcName.DevelopmentPreProduction, staffList);
        MakeStaffTagList(GameDevProcName.ArtPreProduction, staffList);
        MakeStaffTagList(GameDevProcName.ConceptFullProduction, staffList);
        MakeStaffTagList(GameDevProcName.DevelopmentFullProduction, staffList);
        MakeStaffTagList(GameDevProcName.ArtFullProduction, staffList);

        for (int i = 0; i < 12; i++)  // 총 12개.
            staffTags[i].Render(staffList[i].procName, staffList[i].staffName, staffList[i].staffLevel);
        
        confirmBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.AddListener(() => data.btCallback?.Invoke() );
        confirmBtn.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void MakeStaffTagList(GameDevProcName procName, List<StaffTagUIRenderData> staffList)
    {
        var projectManager = ServiceLocater.Get<IProjectManager>();
        foreach (var staffId in projectManager.GetAssignedStaffIds(procName))
            staffList.Add(ExtractStaffData(procName, staffId));
    }
    
    private StaffTagUIRenderData ExtractStaffData(GameDevProcName proc, int staffId)
    {
        var staff = ServiceLocater.Get<IStaffRegister>().GetStaffEntity(staffId);
        return new StaffTagUIRenderData() 
            { procName = proc, staffName = staff.init.Staff_Name, staffLevel = staff.init.Level};
    }
}