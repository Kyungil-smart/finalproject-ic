using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 프로세스에서 보여야할 내용 중 간단한 애니메이션 출력 + 확인 버튼만 있는 UI 에 대한 매니저
/// </summary>
public class ProcessSimpleUIRenderer : MonoBehaviour, IUIRender
{
    [Header("UI Elements - Panel")]
    [SerializeField] private GameObject panelObject;

    [Header("UI Elements - Main Session")] 
    [SerializeField] private TextLoader titleTl;
    [SerializeField] private TextLoader mainTxtLd;
    [SerializeField] private ImageLoader imageLoader;
    
    [Header("UI Elements - Button Session")]
    [SerializeField] private Button confirmBt;
    [SerializeField] private TextLoader confirmBtTxtLd;
    
    private void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>()
            .RegisterUIRender(UIType.ProcessSimpleUI, this);
    }

    private void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.ProcessSimpleUI);
    }

    public void Render(UIRenderData renderData)
    {
        if (renderData is SimpleUIRenderData data)
        {
            mainTxtLd.gameObject.SetActive(false);
            titleTl.TextId = data.titleTextId;
            if (!String.IsNullOrEmpty(data.mainText))
            {
                mainTxtLd.Text = data.mainText;
                imageLoader.gameObject.SetActive(false);
                mainTxtLd.gameObject.SetActive(true);
            }
            else if (imageLoader != null)
            {
                imageLoader.ImageId = data.imageId;
                imageLoader.gameObject.SetActive(true);
                mainTxtLd.gameObject.SetActive(false);
            }
            confirmBtTxtLd.TextId = data.btTextId;
            if (data.btTextId < 0)
                confirmBtTxtLd.GetComponent<TextMeshProUGUI>().text = data.text;
            confirmBt.onClick.RemoveAllListeners();
            confirmBt.onClick.AddListener(() => data.btCallback());
            confirmBt.onClick.AddListener(() => panelObject.SetActive(false));
            panelObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"[ProcessSimpleUIRenderer] Not Supported Data Type {renderData.GetType().Name}");
        }
    }
}