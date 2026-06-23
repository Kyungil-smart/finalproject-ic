using System;

/// <summary>
/// ProcessSimpleUIRenderer 용 데이터 정의
/// </summary>
public class SimpleUIRenderData : UIRenderData
{
    public int mainTextId;
    public int btTextId;
    public Action btCallback;
    public string text;

    public SimpleUIRenderData(int mainTextId, int btTextId, Action btCallback, string text = "")
    {
        this.mainTextId = mainTextId;
        this.btTextId = btTextId;
        this.btCallback = btCallback;
        this.text = text;
    }
}