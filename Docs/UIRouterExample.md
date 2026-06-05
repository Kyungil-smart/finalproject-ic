# UI Router Example

> 하위 코드는 UI Router 에 등록되는 Event UI 나 Process UI 에 대한 간단한 사용법 입니다.

```csharp
using UnityEngine;

public class UITest : MonoBehaviour
{
    [ContextMenu("Test/Simple Process")]
    private void SimpleProcessTest()
    {
        var renderData = new SimpleUIRenderData(1400010, 9900003, () => { });
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcessSimpleUI, renderData);
    }

    private void TestCallback(int num)
    {
        Debug.Log($"TestCallBack: {num}");
}
    
    [ContextMenu("Test/Choice Event")]
    private void ChoiceEventUITest()
    {
        var renderData = new NormalEventUIRenderData(EventType.Regular, 1400010, TestCallback);
        renderData.choices.Add((11, 9900003));
        renderData.choices.Add((12, 9900003));
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.EventUI, renderData);
    }
    
    [ContextMenu("Test/Confirm Event")]
    private void ConfirmEventUITest()
    {
        var renderData = new NormalEventUIRenderData(EventType.Regular, 1400010, TestCallback);
        renderData.choices.Add((11, 9900003));
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.EventUI, renderData);
    }
    
    [ContextMenu("Test/Reward Event")]
    private void RewardEventUITest()
    {
        var renderData = new RewardEventUIRenderData(EventType.Reward, 1400010, TestCallback, null);
        renderData.options.Add((11, null, 9900003));
        renderData.options.Add((12, null, 9900003));
        renderData.options.Add((13, null, 9900003));
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.EventUI, renderData);
    }

    [ContextMenu("Test/Close Current Canvas")]
    private void CloseCurrentCanvasTest()
    {
        ServiceLocater.Get<IUIRouter>().CloseCurrentCanvas();
    }

    [ContextMenu("Test/Check Service Locater List")]
    private void PrintServiceLocaterListTest()
    {
        ServiceLocater.PrintServices();
    }
}
```