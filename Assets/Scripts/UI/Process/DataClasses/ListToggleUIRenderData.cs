using System;

/// <summary>
/// ProcessListToggleUIRenderer 용 데이터 정의
/// </summary>
public class ToggleUIRenderData
{
    public ProcessListUIItemType itemType;
    public int[] itemId;
    
    public ToggleUIRenderData(ProcessListUIItemType itemType, int[] itemId)
    {
        this.itemType = itemType;
        this.itemId = itemId;
    }
}

public class ListToggleUIRenderData : UIRenderData
{
    public ToggleUIRenderData[] items;
    public int confirmBtTextId;
    public Action confirmBtCallback;
    
    public ListToggleUIRenderData(ToggleUIRenderData[] items, int confirmBtTextId, Action confirmBtCallback)
    {
        this.items = items;
        this.confirmBtTextId = confirmBtTextId;
        this.confirmBtCallback = confirmBtCallback;
    }
}