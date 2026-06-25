using System;
using R3;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketingPanelRender : MonoBehaviour
{
    [Header("Render Data")]
    [SerializeField] private Image marketImage;
    [SerializeField] private TextMeshProUGUI marketName;
    [SerializeField] private TextMeshProUGUI marketCost;
    [SerializeField] private TextMeshProUGUI marketEffect;
    
    [Header("Toggle")]
    [SerializeField] private Toggle panelToggle;
    [SerializeField] private Image imageToggle;
    [SerializeField] private Color trueColor = new Color(0.7f, 0.9f, 0.7f, 0.5f);
    [SerializeField] private Color falseColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);

    public Observable<(bool isOn, int index)> OnItemSelect => onItemSelected;
    private readonly Subject<(bool isOn, int index)> onItemSelected = new();
    private int _itemIndex;
    private bool _isRendering;
    
    public void SetUp(int index) => _itemIndex = index;
    
    public void OnClickSelect() => onItemSelected.OnNext((panelToggle.isOn, _itemIndex));

    private bool _selectable;

    private void Start()
    {
        if (panelToggle)
        {
            panelToggle.onValueChanged
                .AsObservable()
                .Subscribe(isOn =>
                {
                    if (_isRendering) return;
                    if (_selectable) imageToggle.color = isOn ? trueColor : falseColor;
                    onItemSelected.OnNext((isOn, _itemIndex));
                })
                .AddTo(this);
        }
    }

    public void Render(MarketingData data, bool selectable)
    {
        _isRendering = true;
        marketName.text = data.marketType;
        marketCost.text = $"{data.moneyMarket}";
        marketEffect.text = $"{data.moneyMarket} / 비율 : {data.rateMarket}";
        if (panelToggle)
        {
            panelToggle.interactable = selectable;
            _selectable = selectable;
            panelToggle.isOn = data.selected;
            imageToggle.color = data.selected ? trueColor : falseColor;
        }
        _isRendering = false;
    }

    public void RevertSelection()
    {
        _isRendering = true;
        panelToggle.isOn = false;
        imageToggle.color = falseColor;
        _isRendering = false;
    }

    private void OnDestroy()
    {
        onItemSelected.OnCompleted();
    }
}
