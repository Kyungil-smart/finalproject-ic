using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class T12AwardsUIRender : MonoBehaviour
{
    [SerializeField] private TextLoader congratulationsMsg;
    [SerializeField] private Image awardsImage;
    [SerializeField] private TextLoader awardsValue;
    [SerializeField] private Button awardsConfirmBtn;
    
    public void Render(T12AwardsUIRenderData data)
    {
        var projectManager = ServiceLocater.Get<IProjectManager>();
        congratulationsMsg.TextId = projectManager.Awards.resultId;
        awardsValue.TextId = projectManager.Awards.name.textId;
        // ToDo. Image 변경 코드 추가
    }
    
    
}