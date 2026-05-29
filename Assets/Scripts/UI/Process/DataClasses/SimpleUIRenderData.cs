using System;

/// <summary>
/// ProcessSimpleUIRenderer 용 데이터 정의
/// </summary>
public class SimpleUIRenderData : UIRenderData
{
    public int mainTextId;
    public int btTextId;
    public Action btCallback;

    public SimpleUIRenderData(int mainTextId, int btTextId, Action btCallback)
    {
        this.mainTextId = mainTextId;
        this.btTextId = btTextId;
        this.btCallback = btCallback;
    }
}