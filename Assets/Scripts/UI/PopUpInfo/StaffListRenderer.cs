using UnityEngine;

public class StaffListRenderer : MonoBehaviour, IUIRender
{
    public void Render(UIRenderData data)
    {
        if (data is StaffListRenderData renderData)
        {
            
        }; 
        gameObject.SetActive(true);
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}