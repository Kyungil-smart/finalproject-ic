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
        bool bootstrapComplete = false;
        for (int i = 0; i < 10; i++)
        {
            var sl = ServiceLocater.Get<IBootStrap>();
            if (sl == null)
                await UniTask.WaitForSeconds(0.2f);
            else if (sl.IsCompleted)
            {
                bootstrapComplete = true;
                break;
            }
        }
        if (bootstrapComplete)
            ServiceLocater.Get<IUIRouter>().ConnectCanvasController(this);
        else
            Debug.LogError("[UIRouter] Could not connect CanvasController to UIRouter");
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