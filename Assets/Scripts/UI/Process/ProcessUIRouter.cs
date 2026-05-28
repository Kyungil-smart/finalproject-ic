
using System;
using System.Collections.Generic;

public class ProcessUIRouter : IProcessUIRouter, IDisposable
{
    private Dictionary<ProcessUIType, IProcessUIRender> _renders = new ();
    
    public void RegisterUIRender(ProcessUIType uiType, IProcessUIRender uiRender)
    {
        _renders.Add(uiType, uiRender);
    }
    
    public void Open(ProcessUIType uiType)
    {
        _renders[uiType].Render();
    }

    public void Dispose()
    {
        _renders.Clear();
    }
}