using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class NewEventUIRenderer : MonoBehaviour, IUIRender
{
    [Header("Main Panel")]
    [SerializeField] private GameObject mainPanel;

    [Header("Context Items")] 
    [SerializeField] private TextLoader title;
    [SerializeField] private TextLoader descriptionTl;
    [SerializeField] private ImageLoader imageLoader;
    [SerializeField] private EventAbilityUIHandler eventAbilityUIHandler;
    
    [Header("Select Sliders")]
    [SerializeField] private TwoSelectScrollBarController twoScrollBarController;
    [SerializeField] private OneSelectScrollBarController oneScrollBarController;
    
    public void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>()?.RegisterUIRender(UIType.EventUI, this);
    }
    
    public void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>()?.UnregisterUIRender(UIType.EventUI);
    }
    
    public void Render(UIRenderData renderData)
    {
        mainPanel.SetActive(true);
        if (renderData is NormalEventUIRenderData normalEventParams)
        {
            descriptionTl.TextId = normalEventParams.mainTextId;
            imageLoader.ImageId = normalEventParams.eventType == EventType.Regular
                ? $"rimg_issue_{Random.Range(1, 3)}"
                : $"rimg_milestone_{Random.Range(1, 3)}";
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
    
    [ContextMenu("SetTestData/TwoData")]
    public void SetTestTwoData()
    {
        var dataA = new EventEffectData
        {
            btId = 0,
            target = "Design_Quality",
            value = 12,
            ratio = 1
        };
        var dataB = new EventEffectData
        {
            btId = 1,
            target = "Design_Quality",
            value = -12,
            ratio = 1
        };
        var renderData = new NormalEventUIRenderData
        (
            eventType: EventType.Reward,
            mainTextId: 1210252,
            callback: TestCallback
        );
        renderData.choices.Add((0, 1210253, dataA));
        renderData.choices.Add((0, 1210254, dataB));
        Render(renderData);
    }
    
    [ContextMenu("SetTestData/OneData")]
    public void SetTestOneData()
    {
        var dataA = new EventEffectData
        {
            btId = 0,
            target = "Money",
            value = -250,
            ratio = 1
        };
        var renderData = new NormalEventUIRenderData
        (
            eventType: EventType.Regular,
            mainTextId: 1230782,
            callback: TestCallback
        );
        renderData.choices.Add((0, 1210253, dataA));
        Render(renderData);
    }

    private void TestCallback(int btnId) => Debug.Log($"TestCallback => {btnId}");
}