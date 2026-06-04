using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class CanvasList
{
    public UIType uiType;
    public Canvas uiCanvas;
}

public class CanvasController : MonoBehaviour, ICanvasController
{
    [SerializeField] private CanvasList[] _canvasList;
    private CanvasList _currentEnableCanvas;

    private void OnEnable() => Initialize().Forget();
        
    private async UniTaskVoid Initialize()
    {
        await UniTask.WaitUntil(() => ServiceLocater.Get<IBootStrap>() != null);
        await UniTask.WaitUntil(() => ServiceLocater.Get<IUIRouter>() != null);
        ServiceLocater.Get<IUIRouter>().ConnectCanvasController(this);
    }

    private void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>()?.DisconnectCanvasController();
    }
    
    public void Enable(UIType uiType)
    {
        if (_currentEnableCanvas != null && _currentEnableCanvas.uiType == uiType) return;
        foreach (var canvas in _canvasList)
        {
            if (canvas.uiType != uiType) continue;
            canvas.uiCanvas.gameObject.SetActive(true);
            _currentEnableCanvas = canvas;
        }
    }

    public void DisableCurrentCanvas()
    {
        _currentEnableCanvas?.uiCanvas.gameObject.SetActive(false);
        _currentEnableCanvas = null;
    }
}