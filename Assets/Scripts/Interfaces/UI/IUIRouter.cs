public interface IUIRouter
{
    public bool HasCanvas { get; }
    public void ConnectCanvasController(ICanvasController canvasController);
    public bool IsCanvasConnected();
    public void DisconnectCanvasController();
    public void RegisterUIRender(UIType uiType, IUIRender uiRender);
    public void UnregisterUIRender(UIType uiType);
    public void NavigateTo(UIType uiType, UIRenderData data);
    public void CloseCurrentCanvas();
}