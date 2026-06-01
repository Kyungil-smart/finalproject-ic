using UnityEngine;

public class LastProjectRenderer : MonoBehaviour, IUIRender
{
    public void Render(UIRenderData data)
    {
        if (data is LastProjectRenderData renderData)
        {
            
        }; 
        gameObject.SetActive(true);
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}