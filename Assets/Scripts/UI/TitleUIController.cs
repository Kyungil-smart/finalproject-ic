using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    [SerializeField] private Button loginButton;

    private void OnEnable() => loginButton.onClick.AddListener(GoToNextScene);
    private void OnDisable() => loginButton.onClick.RemoveListener(GoToNextScene);

    private void GoToNextScene()
    {
        ServiceLocater.Get<ISceneChanger>().ChangeScene("SlotScene");
    }
}