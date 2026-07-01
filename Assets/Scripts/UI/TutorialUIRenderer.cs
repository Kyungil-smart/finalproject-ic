using UnityEngine;

public class TutorialUIRenderer : MonoBehaviour
{
    // 활성화 여부
    public void SetActivePanel(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
