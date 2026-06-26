using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class T11UIController : MonoBehaviour, IUIRender
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
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button nextBtn;

    [Header("Tail 3 Panels")]
    [SerializeField] private GameObject tail3Panel;
    [SerializeField] private Button okBtn;


    private List<int> _selectedMarketings = new();
    
    private void Awake()
    {
        foreach (var panel in marketingPanels)
            panel.OnItemSelect.Subscribe(SelectItem).AddTo(panel);
    }

    private void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>().RegisterUIRender(UIType.MarketingUI, this);
    }

    private void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.MarketingUI);
    }

    public void Render(UIRenderData data)
    {
        if (data is MarketingRenderData renderData)
        {
            mainPanel.SetActive(true);
            _selectedMarketings.Clear();
            var cnt = 0;

            for (int i = 0; i < marketingPanels.Count; i++)
            {
                if (i < renderData.marketingData.Count)
                {
                    SetUpPanel(i, renderData.marketingData[i], marketingPanels[i], renderData.selectable);
                    marketingPanels[i].gameObject.SetActive(true);
                    if (renderData.marketingData[i].selected
                        && !_selectedMarketings.Contains(i))
                    {
                        _selectedMarketings.Add(i);
                        cnt++;
                    }
                }
                else
                {
                    marketingPanels[i].gameObject.SetActive(false);
                }
            }
            
            switch (renderData.tailType.num)
            {
                case 1:
                    tail1Panel.SetActive(true);
                    selectBtn.onClick.RemoveAllListeners();
                    selectBtn.onClick.AddListener(Close);
                    selectBtn.onClick.AddListener(() =>
                    {
                        renderData.tailType.confirmCallback(_selectedMarketings);
                    });
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
        tail3Panel.SetActive(false);
    }
}
