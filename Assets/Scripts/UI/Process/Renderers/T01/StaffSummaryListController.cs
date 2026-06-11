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
    [SerializeField] private GameObject mainPanel;

    [Header("Header Panels")] 
    [SerializeField] private TextLoader titleTl;
    [SerializeField] private TextMeshProUGUI selectedCountText;
    [Header("Tail 1 Panel")]
    [SerializeField] private GameObject tail1Panel;
    [SerializeField] private Button selectBtn;
    
    [Header("Tail 2 Panel")]
    [SerializeField] private GameObject tail2Panel;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button nextBtn;
    
    [Header("Tail 3 Panel")]
    [SerializeField] private GameObject tail3Panel;
    [SerializeField] private Button okBtn;
    
    private List<int> _selectedStaffs = new();

    private void Awake()
    {
        ServiceLocater.Get<IUIRouter>().RegisterUIRender(UIType.StaffCandidateUI, this);    
    }

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
            var cnt = 0;

            for (int i = 0; i < renderData.staffSummaryData.Count; i++)
            {
                SetUpPanel(i, renderData.staffSummaryData[i], staffSummaryPanels[i], renderData.selectable);
                staffSummaryPanels[i].gameObject.SetActive(true);
                if (renderData.staffSummaryData[i].selected)
                {
                    _selectedStaffs.Add(i);
                    cnt++;
                }
            }
            
            switch (renderData.tailType.num)
            {
                case 1:
                    tail1Panel.SetActive(true);
                    selectBtn.onClick.RemoveAllListeners();
                    selectBtn.onClick.AddListener(Close);
                    selectBtn.onClick.AddListener(() => renderData.tailType.confirmCallback(_selectedStaffs));
                    selectBtn.onClick.AddListener(() => mainPanel.SetActive(false));
                    break;
                case 2:
                    tail2Panel.SetActive(true);
                    previousBtn.onClick.RemoveAllListeners();
                    previousBtn.onClick.AddListener(Close);
                    previousBtn.onClick.AddListener(() => renderData.tailType.previousCallback());
                    previousBtn.onClick.AddListener(() => mainPanel.SetActive(false));
                    
                    nextBtn.onClick.RemoveAllListeners();
                    nextBtn.onClick.AddListener(Close);
                    nextBtn.onClick.AddListener(() => renderData.tailType.nextCallback());
                    nextBtn.onClick.AddListener(() => mainPanel.SetActive(false));
                    break;
                case 3:
                    tail3Panel.SetActive(true);
                    okBtn.onClick.RemoveAllListeners();
                    okBtn.onClick.AddListener(Close);
                    okBtn.onClick.AddListener(() => renderData.tailType.nextCallback());
                    okBtn.onClick.AddListener(() => mainPanel.SetActive(false));
                    break;
            }
            
            selectedCountText.text = $"{cnt} / {ServiceLocater.Get<IGameManager>().SlotNum}";
        }
    }

    private void SetUpPanel(int index, StaffSummaryData data, StaffSummaryPanelRender panel, bool selectable)
    {
        panel.SetUp(index);
        
        panel.OnItemSelecte
            .Subscribe(SelectItem)
            .AddTo(panel);
        
        panel.Render(data.viewData, data.hired, data.selected, selectable);
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
        var totalCnt = ServiceLocater.Get<IGameManager>().SlotNum;
        selectedCountText.text = $"{_selectedStaffs.Count} / {totalCnt}";
    }

    private void Close()
    {
        foreach (var staffSummaryPanel in staffSummaryPanels)
        {
            staffSummaryPanel.gameObject.SetActive(false);
        }
        tail1Panel.SetActive(false);
        tail2Panel.SetActive(false);
        tail3Panel.SetActive(false);
    }

    [ContextMenu("RenderTest")]
    private async UniTaskVoid RenderTest()
    {
        async UniTaskVoid GetData(List<int> staffs)
        {
            Debug.Log(String.Join(", ", staffs));
        }
        await ServiceLocater.Get<IStaffRecruit>().GenerateRecruitCandidatesAsync(1, 2);
        var ls = ServiceLocater.Get<IStaffRecruit>().GetAvailableStaffList();
        foreach (var staff in ls)
            await ServiceLocater.Get<IStaffRecruit>().ConfirmHireAsync(staff.Staff_ID, false);
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
        
        sd.tailType = new StaffSummaryTailData() { confirmCallback = GetData};
        Render(sd);
    }
}
