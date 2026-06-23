using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUIRenderData : UIRenderData
{
    public bool Open { get; set; }

    public LoadingUIRenderData(bool open)
    {
        Open = open;
    }
}


public class LoadingUIController : MonoBehaviour, IUIRender
{
    [SerializeField] private GameObject loadingPage;
    [Header("Background Image")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Sprite dayBackground;
    [SerializeField] private Sprite nightBackground;
    
    [Header("Dot Img Ctrl")]
    [SerializeField] private GameObject[] dots;
    [SerializeField][Range(0.1f, 1f)] private float dotInterval;
    
    private void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>().RegisterUIRender(UIType.LoadingUI, this);
    }

    private void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.LoadingUI);
    }
    
    public void Render(UIRenderData data)
    {
        if (data is LoadingUIRenderData renderData)
        {
            Debug.Log($"[LoadingUIController] => {renderData.Open}");
            bgImage.sprite = Utils.DayCheck.IsDaytime() ? dayBackground : nightBackground;
            loadingPage.SetActive(renderData.Open);
            AnimationDot().Forget();
        }
    }

    private async UniTask AnimationDot()
    {
        while (loadingPage.activeSelf)
        {
            foreach (GameObject dot in dots)
            {
                dot.SetActive(true);
                await UniTask.WaitForSeconds(dotInterval);
            }

            foreach (GameObject dot in dots)
            {
                dot.SetActive(false);
            }
            await UniTask.WaitForSeconds(dotInterval);
        }
    }
}