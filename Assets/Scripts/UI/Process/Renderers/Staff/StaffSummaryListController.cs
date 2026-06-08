using R3;
using TMPro;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StaffSummaryListController : MonoBehaviour, IUIRender
{
    [SerializeField] private List<StaffSummaryPanelRender> staffSummaryPanels;
    [SerializeField] private TextMeshProUGUI selectedCountText;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Button selectBtn;
    private List<StaffSummaryData> _renderData = new();
    private List<int> _selectedStaffs = new();

    private void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>().RegisterUIRender(UIType.StaffCandidateUI, this);    
    }

    private void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.StaffCandidateUI);
    }


    
    public void Render(UIRenderData data)
    {
        if (data is StaffSummaryRenderData renderData)
        {
            mainPanel.SetActive(true);
            _selectedStaffs.Clear();
            _renderData.Clear();
            _renderData = renderData.staffSummaryData;
            var cnt = 0;

            for (int i = 0; i < renderData.staffSummaryData.Count; i++)
            {
                SetUpPanel(i, renderData.staffSummaryData[i], staffSummaryPanels[i]);
                staffSummaryPanels[i].gameObject.SetActive(true);
                if (renderData.staffSummaryData[i].selected)
                {
                    _selectedStaffs.Add(i);
                    cnt++;
                }
                else if (renderData.staffSummaryData[i].hired)
                {
                    cnt++;
                }
            }
            selectBtn.onClick.RemoveAllListeners();
            selectBtn.onClick.AddListener(Close);
            selectBtn.onClick.AddListener(() => renderData.callbacks(_selectedStaffs));
            selectBtn.onClick.AddListener(() => mainPanel.SetActive(false));
            
            selectedCountText.text = $"{cnt} / {_renderData.Count}";
        }
    }

    private void SetUpPanel(int index, StaffSummaryData data, StaffSummaryPanelRender panel)
    {
        panel.SetUp(index);
        
        panel.OnItemSelecte
            .Subscribe(SelectItem)
            .AddTo(panel);
        
        panel.Render(data.viewData, data.hired, data.selected);
        panel.gameObject.SetActive(true);
    }

    private void SelectItem((bool isOn, int index) data)
    {
        if (data.isOn)
        {
            if (_selectedStaffs.Count >= ServiceLocater.Get<IGameManager>().SlotNum) return;
            _selectedStaffs.Add(data.index); 
        }
        else
        {
            _selectedStaffs.Remove(data.index);
        }

        // var totalCnt = ServiceLocater.Get<IGameManager>().SlotNum;
        var totalCnt = 8;
        selectedCountText.text = $"{_selectedStaffs.Count} / {totalCnt}";
    }

    private void Close()
    {
        foreach (var staffSummaryPanel in staffSummaryPanels)
        {
            staffSummaryPanel.gameObject.SetActive(false);
        }
    }

    [ContextMenu("RenderTest")]
    private async UniTaskVoid RenderTest()
    {
        List<int> GetData(List<int> staffs)
        {
            Debug.Log(String.Join(", ", staffs));
            return staffs;
        }
        await ServiceLocater.Get<IStaffRecruit>().GenerateRecruitCandidatesAsync(1, 2);
        var ls = ServiceLocater.Get<IStaffRecruit>().GetAvailableStaffList();
        foreach (var staff in ls)
            await ServiceLocater.Get<IStaffRecruit>().ConfirmHireAsync(staff.Staff_ID);
        await UniTask.Yield();
        await ServiceLocater.Get<IStaffRecruit>().GenerateRecruitCandidatesAsync(1, 8);
        StaffSummaryRenderData sd = new ();
        sd.staffSummaryData = new();

        foreach (var d in ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList())
        {
            Debug.Log($"{d.Staff_Name} {d.Staff_ID}");
            sd.staffSummaryData.Add(new StaffSummaryData { selected = false, hired = true, viewData = d });
        }

        foreach (var d in ServiceLocater.Get<IStaffRecruit>().GetAvailableStaffList())
        {
            sd.staffSummaryData.Add(new StaffSummaryData { selected = false, hired = false, viewData = d });
        }
            

        sd.callbacks = GetData;
        Render(sd);
    }
}
