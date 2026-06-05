using UnityEngine;

public class StaffDetailRenderer: MonoBehaviour, IUIRender
{
    public void Render(UIRenderData data)
    {
        if (data is StaffDetailRenderData renderData)
        {
            
        }; 
        gameObject.SetActive(true);
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}