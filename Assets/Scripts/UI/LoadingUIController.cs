using UnityEngine;

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
            loadingPage.SetActive(renderData.Open);
        }
    }
}