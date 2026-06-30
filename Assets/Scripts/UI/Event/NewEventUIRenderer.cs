using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewEventUIRenderer : MonoBehaviour, IUIRender
{
    [Header("Main Panel")]
    [SerializeField] private GameObject mainPanel;

    [Header("Context Items")] 
    [SerializeField] private TextLoader title;
    [SerializeField] private TextLoader descriptionTl;
    [SerializeField] private Image image;
    [SerializeField] private EventAbilityUIHandler eventAbilityUIHandler;
    
    [Header("Select Sliders")]
    [SerializeField] private TwoSelectScrollBarController twoScrollBarController;
    [SerializeField] private OneSelectScrollBarController oneScrollBarController;
    
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
        mainPanel.SetActive(true);
        if (renderData is NormalEventUIRenderData normalEventParams)
        {
            descriptionTl.TextId = normalEventParams.mainTextId;
            // image.sprite =  // ToDo. 그냥 종류마다 다를듯? related image 참조 하면 될 듯?
            RenderNormalEvent(normalEventParams);
        }
        else
        {
            Debug.LogError($"[EventUIRenderer] Not Supported Data Type {renderData.GetType().Name}");    
        }
    }

    private void RenderNormalEvent(NormalEventUIRenderData data)
    {
        List<EventEffectData> effectDataList = new();
        if (data.choices.Count == 1)
        {
            oneScrollBarController.gameObject.SetActive(true);
            var choice = data.choices[0];
            oneScrollBarController.SetData(choice.id, choice.textId, data.callback);
            effectDataList.Add(choice.effectData);
            eventAbilityUIHandler.SetData(effectDataList);
        }
        else if (data.choices.Count == 2)
        {
            twoScrollBarController.gameObject.SetActive(true);
            for (int i = 0; i < data.choices.Count; i++)
            {
                effectDataList.Add(data.choices[i].effectData);
                twoScrollBarController.SetData(i, data.choices[i].id, data.choices[i].textId, data.callback);
            }
            eventAbilityUIHandler.SetData(effectDataList);
        }
    }
}