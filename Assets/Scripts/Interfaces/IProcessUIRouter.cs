public interface IProcessUIRouter
{
    public void RegisterUIRender(ProcessUIType uiType, IProcessUIRender uiRender);
    public void NavigateTo(ProcessUIType uiType, UIRenderData data);
}