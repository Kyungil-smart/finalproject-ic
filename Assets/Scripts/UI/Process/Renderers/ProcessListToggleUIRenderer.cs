using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 프로세스에서 보여야할 내용 중 List 형으로 보여주고 선택도 가능하게 할 UI
/// </summary>
public class ProcessListToggleUIRenderer : MonoBehaviour, IUIRender
{
    [SerializeField] private Button _confirmBt;
    [SerializeField] private GameObject _itemPrefab;
    
    public void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>()
            .RegisterUIRender(UIType.ProcessListToggleUI, this);
    }
    
    public void Render(UIRenderData renderData)
    {
        if (renderData is ListToggleUIRenderData data)
        {
            
        }
        else
        {
            Debug.LogError($"[ProcessListToggleUIRenderer] Not Supported Data Type {renderData.GetType().Name}");
        }
    }
}