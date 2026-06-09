
using System;
using System.Collections.Generic;
using UnityEngine;


public class UIRenderData { }


public class UIRouter : IUIRouter, IDisposable
{
    private Dictionary<UIType, IUIRender> _renders = new ();
    ICanvasController _canvasController;

    public void ConnectCanvasController(ICanvasController canvasController)
    {
        _canvasController = canvasController;
        Debug.Log("[UIRouter] ConnectCanvasController ... Success");
    }
    
    public bool IsCanvasConnected() => _canvasController != null;

    public void DisconnectCanvasController()
    {
        _canvasController?.DisableCurrentCanvas();
        _canvasController = null;
        Debug.Log("[UIRouter] DisconnectCanvasController ... Success");
    }

    public void RegisterUIRender(UIType uiType, IUIRender uiRender) 
        => _renders[uiType] = uiRender;
    
    public void UnregisterUIRender(UIType uiType) 
        => _renders.Remove(uiType);

    public void NavigateTo(UIType uiType, UIRenderData data)
    {
        _canvasController.Enable(uiType);
        if (_renders.ContainsKey(uiType)) return;
        _renders[uiType].Render(data);
    }

    public void CloseCurrentCanvas() => _canvasController?.DisableCurrentCanvas();
    
    public void Dispose() 
        => _renders.Clear();
}