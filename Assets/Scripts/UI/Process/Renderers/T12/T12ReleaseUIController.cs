using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class T12ReleaseUIController : MonoBehaviour, IUIRender
{
    [SerializeField] private T12IncomeUIRender incomeUIRender;
    [SerializeField] private T12AwardsUIRender awardsUIRender;
    [SerializeField] private T12ProjectDetailUIRender projectDetailUIRender;
    
    public void Render(UIRenderData data)
    {
        if (data is T12AwardsUIRenderData aRenderData)
        {
            awardsUIRender?.Render(aRenderData);
            awardsUIRender?.gameObject.SetActive(true);
        }
        else if (data is T12IncomeUIRenderData iRenderData)
        {
            incomeUIRender?.Render(iRenderData);
            incomeUIRender?.gameObject.SetActive(true);
        }
        else if (data is T12ProjectDetailUIRenderData pRenderData)
        {
            projectDetailUIRender?.Render(pRenderData);
            projectDetailUIRender?.gameObject.SetActive(true);
        }
    }
}
