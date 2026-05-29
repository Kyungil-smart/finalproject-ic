using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct NormalEventChoices
{
    public int btId;
    public Button button;   // 데모용. 추후 개발 방향에 따라 변경 가능.
    public TextLoader textLoader;
}

[Serializable]
public class RewardEventOptions
{
    public int btId;
    public Button button;
    public Image image;
    public TextLoader textLoader;
}

public class EventUIRenderer : MonoBehaviour, IUIRender
{
    [Header("Main Panel")]
    [SerializeField] private GameObject _mainPanel;
    
    [Header("Normal Common Event")]
    [SerializeField] private GameObject _normalPanel;
    [SerializeField] private TextLoader _normalMainTextLoader;
    
    [Header("Normal Confirm Event")]
    [SerializeField] private GameObject _confirmPanel;
    [SerializeField] private NormalEventChoices _confirm; 
    
    [Header("Normal Choice Event")]
    [SerializeField] private GameObject _choicePanel;
    [SerializeField] private NormalEventChoices[] _choices;

    [Header("Reward Event")]
    [SerializeField] private GameObject _rewardPanel;
    [SerializeField] private TextLoader _rewardMainTextLoader;
    [SerializeField] private Image _gradeImage;
    [SerializeField] private RewardEventOptions[] _options;
    
    public void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>()
            .RegisterUIRender(UIType.EventUI, this);
    }
    
    public void Render(UIRenderData renderData)
    {
        if (renderData is EventUIRenderData data)
        {
            _normalPanel.SetActive(data.eventType != EventType.Reward);
            _rewardPanel.SetActive(data.eventType == EventType.Reward);

            if (data.eventType == EventType.Reward && renderData is RewardEventUIRenderData rewardEventParams)
            {
                _rewardMainTextLoader.TextId = rewardEventParams.mainTextId;
                _gradeImage.sprite = rewardEventParams.gradeImage;
                RenderRewardEvent(rewardEventParams);
            }
            else if (data.eventType != EventType.Reward && renderData is NormalEventUIRenderData normalEventParams)
            {
                _normalMainTextLoader.TextId = normalEventParams.mainTextId;
                RenderNormalEvent(normalEventParams);
            }
            else
            {
                Debug.LogError($"[EventUIRenderer] Not Supported Data Type {renderData.GetType().Name}");    
            }
        }
        else
        {
            Debug.LogError($"[EventUIRenderer] Not Supported Data Type {renderData.GetType().Name}");
        }
    }

    private void RenderNormalEvent(NormalEventUIRenderData data)
    {
        Open();
        if (data.choices.Count == 1) // confirm 
        {
            _confirmPanel.SetActive(true);
            _choicePanel.SetActive(false);
            
            _confirm.textLoader.TextId = data.choices[0].textId;
            _confirm.btId = data.choices[0].id;
            _confirm.button.onClick.RemoveAllListeners();
            _confirm.button.onClick.AddListener(() => data.callback(_confirm.btId));
            _confirm.button.onClick.AddListener(Close);
        }
        else // choice
        {
            _confirmPanel.SetActive(false);
            _choicePanel.SetActive(true);
            for (int i = 0; i < data.choices.Count; i++)
            {
                var choice = _choices[i];
                choice.textLoader.TextId = data.choices[i].textId;
                choice.button.onClick.RemoveAllListeners();
                choice.btId = data.choices[i].id;
                choice.button.onClick.AddListener(() => data.callback(choice.btId));
                choice.button.onClick.AddListener(Close);
            }
        }
    }
    
    private void RenderRewardEvent(RewardEventUIRenderData data)
    {
        Open();
        for (int i = 0; i < _options.Length; i++)
        {
            var opt = _options[i];
            opt.image.sprite = data.options[i].icon;
            opt.textLoader.TextId = data.options[i].textId;
            opt.button.onClick.RemoveAllListeners();
            opt.button.onClick.AddListener(() => data.callback(opt.btId));
            opt.button.onClick.AddListener(Close);
        }
    }

    private void Open()
    {
        _mainPanel.SetActive(true);
    }

    private void Close()
    {
        _mainPanel.SetActive(false);
    }
}