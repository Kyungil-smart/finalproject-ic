using Cysharp.Threading.Tasks;
using UnityEngine;
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
        Debug.Log($"[SceneChanger] Change scene {_currentSceneName} => {sceneName}");
        UniTask.Void(async () =>
        {
            await UniTask.WaitUntil(() => SceneManager.GetSceneByName(sceneName).isLoaded);
            _currentSceneName = sceneName;
            await TaskAsync.WaitUntilOrThrowAsync("SceneChanger.IUIRouter", () => ServiceLocater.Get<IUIRouter>() != null);
            await TaskAsync.WaitUntilOrThrowAsync("SceneChanger.Canvas", () => ServiceLocater.Get<IUIRouter>().IsCanvasConnected());
            await OpenLoadingPage();
        });
    }

    private UniTask OpenLoadingPage()
    {
        Debug.Log("[SceneChanger] Opening loading page");
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.LoadingUI, new LoadingUIRenderData(true));
        return UniTask.CompletedTask;
    }

    protected override void Register() => ServiceLocater.Register<ISceneChanger>(this);
    protected override void Unregister() => ServiceLocater.Unregister<ISceneChanger>(this);
}