using Cysharp.Threading.Tasks;
using UnityEngine;

public class ProcessUIController : MonoBehaviour
{
    private void Start()
    {
        CloseLoadingScreen().Forget();
    }
    
    private async UniTaskVoid CloseLoadingScreen()
    {   // ToDo. 임시 코드
        await UniTask.WaitForSeconds(1f);
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.LoadingUI, new LoadingUIRenderData(false));
    }
}