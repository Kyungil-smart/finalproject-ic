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
    [SerializeField] private Button selectBtn2;

    private List<int> _selectedMarketings = new();
    private MarketingData _defaultMarketing;
    public MarketingData defaultMarketing => _defaultMarketing;

    private int _selectedIndex = -1;
    private void Awake()
    {
        // Todo. 마케팅 UIType 성우님이 만드신다고 하셔서 놔둠
        // ServiceLocater.Get<IUIRouter>().RegisterUIRender();
        foreach (var panel in marketingPanels)
            panel.OnItemSelect.Subscribe(SelectItem).AddTo(panel);
    }

    // Todo. Enable / Disalble 성우님이 UIType Enum생성한다고 하셔서 완성되시면 추가할려고 남겨뒀습니다.
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

            for (int i = 0; i < marketingPanels.Count; i++)
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
            
            _defaultMarketing = renderData.marketingData[marketingPanels.Count];
            switch (renderData.tailType.num)
            {
                case 1:
                    tail1Panel.SetActive(true);
                    selectBtn.onClick.RemoveAllListeners();
                    selectBtn.onClick.AddListener(Close);
                    selectBtn.onClick.AddListener(() =>
                    {
                        if (_selectedMarketings.Count == 0)
                        {
                            // 디폴트값 처리
                        }
                        renderData.tailType.nextCallback();
                    });
                    selectBtn.onClick.AddListener(() => mainPanel.SetActive(false));
                    break;
                case 2:
                    tail2Panel.SetActive(true);
                    selectBtn2.onClick.RemoveAllListeners();
                    selectBtn2.onClick.AddListener(Close);
                    selectBtn2.onClick.AddListener(() => renderData.tailType.nextCallback());
                    selectBtn2.onClick.AddListener(() => mainPanel.SetActive(false));
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
        if (data.isOn) _selectedIndex = data.index;
        else if(_selectedIndex == data.index) _selectedIndex = -1;
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
