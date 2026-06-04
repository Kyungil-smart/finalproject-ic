using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Utils;

public class SceneChanger : Manager, ISceneChanger
{
    private string _currentSceneName;
    
    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    
    public string GetCurrentSceneName() => _currentSceneName;
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        UniTask.Void(async () =>
        {
            await UniTask.WaitUntil(() => SceneManager.GetSceneByName(sceneName).isLoaded);
            await TaskAsync.WaitUntilOrThrowAsync(() => ServiceLocater.Get<IUIRouter>() != null);
            await TaskAsync.WaitUntilOrThrowAsync(() => ServiceLocater.Get<IUIRouter>().IsCanvasConnected());
            await OpenLoadingPage();
        });
    }

    private UniTask OpenLoadingPage()
    {
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.LoadingUI, new LoadingUIRenderData(true));
        return UniTask.CompletedTask;
    }

    protected override void Register() => ServiceLocater.Register<ISceneChanger>(this);
    protected override void Unregister() => ServiceLocater.Unregister<ISceneChanger>(this);
}