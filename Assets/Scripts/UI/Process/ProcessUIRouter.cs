
using System;
using System.Collections.Generic;


public class UIRenderData { }


public class ProcessUIRouter : IProcessUIRouter, IDisposable
{
    private Dictionary<ProcessUIType, IProcessUIRender> _renders = new ();
    
    public void RegisterUIRender(ProcessUIType uiType, IProcessUIRender uiRender) 
        => _renders[uiType] = uiRender;
    public void NavigateTo(ProcessUIType uiType, UIRenderData data) 
        => _renders[uiType].Render(data);
    public void Dispose() 
        => _renders.Clear();
    
}