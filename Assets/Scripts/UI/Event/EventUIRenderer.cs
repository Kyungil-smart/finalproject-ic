using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct NormalEventChoices
{
    public Button button;   // 데모용. 추후 개발 방향에 따라 변경 가능.
    public TextMeshProUGUI tmPro;
}

[Serializable]
public struct RewardEventOptions
{
    public Button button;
    public Image image;
    public TextMeshProUGUI tmPro;
}

public class EventUIRenderer : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject _mainPanel;
    
    [Header("Normal Common Event")]
    [SerializeField] private GameObject _normalPanel;
    [SerializeField] private TextMeshProUGUI _normalMainText;
    
    [Header("Normal Confirm Event")]
    [SerializeField] private GameObject _confirmPanel;
    [SerializeField] private NormalEventChoices _confirm; 
    
    [Header("Normal Choice Event")]
    [SerializeField] private GameObject _choicePanel;
    [SerializeField] private NormalEventChoices[] _choices;

    [Header("Reward Event")]
    [SerializeField] private GameObject _rewardPanel;
    [SerializeField] private TextMeshProUGUI _rewardMainText;
    [SerializeField] private Image _gradeImage;
    [SerializeField] private RewardEventOptions[] _options;
    
    public Action<int> OnItemSelected;
    
    public void Render(EventType eventType, EventParams @eventParams)
    {
        _normalPanel.SetActive(eventType != EventType.Reward);
        _rewardPanel.SetActive(eventType == EventType.Reward);

        if (eventType == EventType.Reward && @eventParams is RewardEventParams rewardEventParams)
        {
            _rewardMainText.text = rewardEventParams.mainText;
            _gradeImage.sprite = rewardEventParams.gradeImage;
            RenderRewardEvent(rewardEventParams);
        }
        else if (eventType != EventType.Reward && @eventParams is NormalEventParams normalEventParams)
        {
            _normalMainText.text = normalEventParams.mainText;
            RenderNormalEvent(normalEventParams);
        }
    }

    private void RenderNormalEvent(NormalEventParams data)
    {
        if (data.choices.Count == 1) // confirm 
        {
            _confirmPanel.SetActive(true);
            _choicePanel.SetActive(false);
            
            _confirm.tmPro.text = data.choices[0].text;
            _confirm.button.onClick.RemoveAllListeners();
            _confirm.button.onClick.AddListener(() => OnItemSelected?.Invoke(data.choices[0].id));
        }
        else // choice
        {
            _confirmPanel.SetActive(false);
            _choicePanel.SetActive(true);
            for (int i = 0; i < data.choices.Count; i++)
            {
                _choices[i].tmPro.text = data.choices[i].text;
                _choices[i].button.onClick.RemoveAllListeners();
                _choices[i].button.onClick.AddListener(() => OnItemSelected?.Invoke(data.choices[i].id));
            }
        }
    }

    private void RenderRewardEvent(RewardEventParams data)
    {
        for (int i = 0; i < _options.Length; i++)
        {
            var opt = _options[i];
            opt.image.sprite = data.options[i].icon;
            opt.tmPro.text = data.options[i].text;
            opt.button.onClick.RemoveAllListeners();
            opt.button.onClick.AddListener(() => OnItemSelected?.Invoke(data.options[i].id));
        }
    }
    
    public void Open() => _mainPanel.SetActive(true);
    public void Close() => _mainPanel.SetActive(false);
}