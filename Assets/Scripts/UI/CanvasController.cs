using System;
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
    
    private void OnEnable()
    {
        Debug.Log("[CanvasController] OnEnable");
        ServiceLocater.Get<IUIRouter>().ConnectCanvasController(this);
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
        _currentEnableCanvas.uiCanvas.gameObject.SetActive(false);
        _currentEnableCanvas = null;
    }
}