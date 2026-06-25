using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class T11UIController : MonoBehaviour
{
    [SerializeField] private List<MarketingPanelRender> marketingPanels;
    [SerializeField] private GameObject mainPanel;

    [Header("Header Panels")] 
    [SerializeField] private TextLoader titleTl;
    // [SerializeField] private TextMeshProUGUI selectedCountText;
    
    [Header("Tail 1 Panels")]
    [SerializeField] private GameObject tail1Panel;
    [SerializeField] private Button selectBtn;
    
    [Header("Tail 2 Panels")]
    [SerializeField] private GameObject tail2Panel;
    [SerializeField] private Button selectBtn2;

    private List<int> _selectedMarketings = new();

    private void Awake()
    {
        // Todo. 마케팅 UIType 성우님이 만드신다고 하셔서 놔둠
        // ServiceLocater.Get<IUIRouter>().RegisterUIRender();
        // foreach (var panel in marketingPanels)
        //     panel.OnItemSelect.Subscribe(SelectItem)
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void Render(UIRenderData data)
    {
        if (data is MarketingRenderData renderData)
        {
            mainPanel.SetActive(true);
            _selectedMarketings.Clear();
            var cnt = 0;

            for (int i = 0; i < renderData.marketingData.Count; i++)
            {
                SetUpPanel(i, renderData.marketingData[i], marketingPanels[i], renderData.selectable);
                marketingPanels[i].gameObject.SetActive(true);
                if (renderData.marketingData[i].selected
                    && _selectedMarketings.Contains(i))
                {
                    _selectedMarketings.Add(i);
                    cnt++;
                }
            }

            switch (renderData.tailType.num)
            {
                case 1:
                    tail1Panel.SetActive(true);
                    selectBtn.onClick.RemoveAllListeners();
                    selectBtn.onClick.AddListener(Close);
                    selectBtn.onClick.AddListener(() => renderData.tailType.nextCallback());
                    selectBtn.onClick.AddListener(() => mainPanel.SetActive(false));
                    break;
                case 2:
                    tail2Panel.SetActive(true);
                    selectBtn.onClick.RemoveAllListeners();
                    selectBtn.onClick.AddListener(Close);
                    selectBtn.onClick.AddListener(() => renderData.tailType.nextCallback());
                    selectBtn.onClick.AddListener(() => mainPanel.SetActive(false));
                    break;
            }
        }
    }

    private void SetUpPanel(int index, MarketingData data, MarketingPanelRender panel, bool selectable)
    {
        panel.SetUp(index);
        panel.Render(data, selectable);
    }

    private void SelectItem((bool isOn, int index) data)
    {
        if (data.isOn)
        {
            if (_selectedMarketings.Contains(data.index)) return;
            
            _selectedMarketings.Add(data.index);
        }
        else
        {
            _selectedMarketings.Remove(data.index);
        }
        
    }
    
    private void Close()
    {
        foreach (var marketingPanel in marketingPanels)
        {
            marketingPanel.gameObject.SetActive(false);
        }
        tail1Panel.SetActive(false);
        tail2Panel.SetActive(false);
    }
}
