using UnityEngine;


public class ListUIRenderData : UIRenderData
{
    
}

/// <summary>
/// 프로세스에서 보여야할 내용 중 List 형으로 보여주고 선택도 가능하게 할 UI
/// </summary>
public class ProcessListUIRenderer : MonoBehaviour, IProcessUIRender
{
    public void OnEnable()
    {
        ServiceLocater.Get<IProcessUIRouter>()
            .RegisterUIRender(ProcessUIType.ListUI, this);
    }
    
    public void Render(UIRenderData renderData)
    {
        if (renderData is ListUIRenderData data)
        {
            
        }
        else
        {
            Debug.LogError($"[ProcessListUIRenderer] Not Supported Data Type {renderData.GetType().Name}");
        }
    }
}