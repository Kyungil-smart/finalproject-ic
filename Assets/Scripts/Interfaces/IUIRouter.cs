public interface IUIRouter
{
    public void ConnectCanvasController(ICanvasController canvasController);
    public void RegisterUIRender(UIType uiType, IUIRender uiRender);
    public void NavigateTo(UIType uiType, UIRenderData data);
    public void CloseCurrentCanvas();
}