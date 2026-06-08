using R3;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaffSummaryListController : MonoBehaviour, IUIRender
{
    [SerializeField] private List<StaffSummaryPanelRender> staffSummaryPanels;
    [SerializeField] private TextMeshProUGUI selectedCountText;
    [SerializeField] private StaffSummaryTailOnClickController tailPanel;
    [SerializeField] private Button selectBtn;
    private List<StaffSummaryData> _renderData = new();
    private List<int> _selectedStaffs = new();
    private IGameManager _gameManager;

    private void Start()
    {
        _gameManager = ServiceLocater.Get<IGameManager>();
        
    }
    
    public void Render(UIRenderData data)
    {
        if (data is StaffSummaryRenderData renderData)
        {
            _selectedStaffs.Clear();
            _renderData.Clear();
            _renderData = renderData.staffSummaryData;

            for (int i = 0; i < renderData.staffSummaryData.Count; i++)
            {
                SetUpPanel(i, renderData.staffSummaryData[i], staffSummaryPanels[i]);
                staffSummaryPanels[i].gameObject.SetActive(true);
                if (renderData.staffSummaryData[i].selected)
                    _selectedStaffs.Add(i);
            }
            
            var callBackLength = renderData.callbacks.Length; 
            Array.Resize(ref renderData.callbacks, callBackLength + 1);
            renderData.callbacks[callBackLength] = Close;
            tailPanel.SetOnClickCallBack(renderData.callbacks);
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
            if (_selectedStaffs.Count >= _gameManager.SlotNum) return;
            _selectedStaffs.Add(data.index); 
        }
        else
        {
            _selectedStaffs.Remove(data.index);
        }
    }

    private void Close()
    {
        foreach (var staffSummaryPanel in staffSummaryPanels)
        {
            staffSummaryPanel.gameObject.SetActive(false);
        }
    }
}
