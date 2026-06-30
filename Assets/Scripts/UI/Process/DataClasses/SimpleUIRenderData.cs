using System;

/// <summary>
/// ProcessSimpleUIRenderer 용 데이터 정의
/// </summary>
public class SimpleUIRenderData : UIRenderData
{
    public int titleTextId;
    public string mainText;
    public string imageId;
    public int btTextId;
    public Action btCallback;
    public string text;
}