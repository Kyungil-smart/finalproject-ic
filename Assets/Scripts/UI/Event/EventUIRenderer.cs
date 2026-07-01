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
    [SerializeField] private TextLoader _titleTl;
    [SerializeField] private TextLoader _normalMainTextLoader;
    
    [Header("Normal Choice Event")]
    [SerializeField] private GameObject _choicePanel;
    [SerializeField] private GameObject _choice1Btns;
    [SerializeField] private GameObject _choice2Btns;
    [SerializeField] private GameObject _choice3Btns;
    [SerializeField] private NormalEventChoices[] _confirm;
    [SerializeField] private NormalEventChoices[] _2choices;
    [SerializeField] private NormalEventChoices[] _3choices;
    [SerializeField] private AudioClip _goldClip;
    [SerializeField] private AudioClip _btnClip;

    [Header("Reward Event")]
    [SerializeField] private GameObject _rewardPanel;
    [SerializeField] private TextLoader _rewardMainTextLoader;
    [SerializeField] private Image _gradeImage;
    [SerializeField] private RewardEventOptions[] _options;
    
    public void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>().RegisterUIRender(UIType.EventUI, this);
    }
    
    public void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.EventUI);
    }
    
    public void Render(UIRenderData renderData)
    {
        if (renderData is EventUIRenderData data)
        {
            // Todo. 리워드 이벤트가 기존의 이벤트랑 같은 ui디자인이라 주석처리해둠.
            _normalPanel.SetActive(true);
            // _rewardPanel.SetActive(data.eventType == EventType.Reward);

            if (
                // data.eventType == EventType.Reward && 
                renderData is RewardEventUIRenderData rewardEventParams)
            {
                _rewardMainTextLoader.TextId = rewardEventParams.mainTextId;
                _gradeImage.sprite = rewardEventParams.gradeImage;
                RenderRewardEvent(rewardEventParams);
            }
            else if (
                // data.eventType != EventType.Reward && 
                renderData is NormalEventUIRenderData normalEventParams)
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
        if (!(data.choices.Count > 0 && data.choices.Count < 4))
            throw new Exception($"[EventUIRenderer] Not Supported Choices {data.choices.Count}");
        
        Open();
        NormalEventChoices[] choices;
        _choicePanel.SetActive(true);

        switch (data.choices.Count)
        {
            case 2:
                choices = _2choices;
                _choice1Btns.SetActive(false);
                _choice2Btns.SetActive(true);
                _choice3Btns.SetActive(false);
                break;
            case 3:
                choices = _3choices;
                _choice1Btns.SetActive(false);
                _choice2Btns.SetActive(false);
                _choice3Btns.SetActive(true);
                break;
            default:
                choices = _confirm;
                _choice1Btns.SetActive(true);
                _choice2Btns.SetActive(false);
                _choice3Btns.SetActive(true);
                break;
        }
        
        for (int i = 0; i < data.choices.Count; i++)
        {
            var choice = choices[i];
            choice.textLoader.TextId = data.choices[i].textId;
            choice.button.onClick.RemoveAllListeners();
            choice.btId = data.choices[i].id;
            choice.button.onClick.AddListener(() =>
            {
                var clip = data.choices[i].effectData?.target == "Money" ? _goldClip : _btnClip;
                ServiceLocater.Get<ISoundManager>().PlaySfx(clip);
            });
            choice.button.onClick.AddListener(() => data.callback(choice.btId));
            choice.button.onClick.AddListener(Close);
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
        _choicePanel.SetActive(false);
        _normalPanel.SetActive(false);
        _rewardPanel.SetActive(false);
    }
}