using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SlotUIController : MonoBehaviour
{
    [SerializeField] private Button[] slotButtons;
    
    private void OnEnable()
    {
        foreach (Button button in slotButtons) 
            button.onClick.AddListener(GoToNextScene);
        CloseLoadingScreen().Forget();
    }

    private void OnDisable()
    {
        foreach (Button button in slotButtons) 
            button.onClick.RemoveListener(GoToNextScene);
    }

    private void GoToNextScene()
    {
        ServiceLocater.Get<ISceneChanger>().ChangeScene("MainScene");
    }

    private async UniTaskVoid CloseLoadingScreen()
    {   // ToDo. 임시 코드
        await UniTask.WaitForSeconds(1f);
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.LoadingUI, new LoadingUIRenderData(false));
    }
}