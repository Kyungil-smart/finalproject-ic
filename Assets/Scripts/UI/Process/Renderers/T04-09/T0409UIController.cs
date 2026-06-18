using System;
using R3;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class T0409UIController : MonoBehaviour, IUIRender
{
    [SerializeField] private GameObject mainPanel;
    
    [Header("Staff View")]
    [SerializeField] private GameObject staffListPanel;
    [SerializeField] private List<StaffSummaryPanelRender> ssPanels;
    [SerializeField] private Button confirmBtn;
    
    [Header("Leader Result")]
    [SerializeField] private GameObject leaderResultPanel;
    [SerializeField] private List<TextMeshProUGUI> leaderResults;
    [SerializeField] private Button goBackBtn;
    [SerializeField] private Button goNextBtn;

    private List<int> _selectedStaffs = new();
    private const int LeaderCount = 2;

    private void Awake()
    {
        foreach (var panel in ssPanels)
            panel.OnItemSelecte.Subscribe(SelectItem).AddTo(panel);
    }
    
    private void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>().RegisterUIRender(UIType.ProductionUI, this);
    }

    private void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.ProductionUI);
    }
    
    public void Render(UIRenderData data)
    {
        mainPanel.SetActive(true);
        if (data is T0409ProductionStaffListRenderData renderStaffData)
        {
            staffListPanel.SetActive(true);
            _selectedStaffs.Clear();
            for (int i = 0; i < renderStaffData.staffList.Count; i++)
            {
                SetUpPanel(i, renderStaffData.staffList[i], ssPanels[i]);
                ssPanels[i].gameObject.SetActive(true);
                if (renderStaffData.staffList[i].selected)
                {
                    _selectedStaffs.Add(i);
                }
            }
            confirmBtn.onClick.RemoveAllListeners();
            confirmBtn.onClick.AddListener(() =>
            {
                if (_selectedStaffs.Count != LeaderCount) return;
                renderStaffData.onSelectCallback?.Invoke(_selectedStaffs);
                StaffListClose();
            });
        } 
        else if (data is T0409ProductionLeaderResultRenderData renderLeaderData)
        {
            leaderResultPanel.SetActive(true);
            for (int i = 0; i < renderLeaderData.leaderList.Count; i++)
            {
                var leader = renderLeaderData.leaderList[i];
                leaderResults[i].text = leader.Staff_Name;
            }
            
            goBackBtn.onClick.RemoveAllListeners();
            goBackBtn.onClick.AddListener(() => renderLeaderData.onGoBackCallback?.Invoke());
            goBackBtn.onClick.AddListener(() => leaderResultPanel.SetActive(false));
            goBackBtn.onClick.AddListener(() => mainPanel.SetActive(false));
            
            goNextBtn.onClick.RemoveAllListeners();
            goNextBtn.onClick.AddListener(() => GoNext(renderLeaderData.onGoNextCallback));
        }
    }

    private void GoNext(Action onGoNextCallback)
    {
        if (_selectedStaffs.Count != LeaderCount)
        {
            // ToDO. 경고 메시지 필요.
            return;
        }
        if (onGoNextCallback != null) onGoNextCallback.Invoke();
        leaderResultPanel.SetActive(false);
        mainPanel.SetActive(false);
    }
    
    private void SetUpPanel(int index, StaffSummaryData data, StaffSummaryPanelRender panel)
    {
        panel.SetUp(index);
        panel.Render(data.viewData, true, data.selected, true);
        panel.gameObject.SetActive(true);
    }
    
    private void SelectItem((bool isOn, int index) data)
    {
        if (data.isOn)
        {
            if (_selectedStaffs.Count >= LeaderCount)
            {
                ssPanels[data.index].RevertSelection(); 
                return;
            }
            _selectedStaffs.Add(data.index); 
        }
        else
        {
            _selectedStaffs.Remove(data.index);
        }
    }

    private void StaffListClose()
    {
        foreach (var staffPanel in ssPanels)
            staffPanel.gameObject.SetActive(false);
        staffListPanel.SetActive(false);
        mainPanel.SetActive(false);
    }
}
