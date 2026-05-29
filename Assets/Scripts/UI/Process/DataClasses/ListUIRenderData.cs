using System;

/// <summary>
/// ProcessListUIRenderer 용 데이터 정의
/// </summary>
public class ListItemUIRenderData
{
    public ProcessListUIItemType itemType;
    public int[] itemId;

    public ListItemUIRenderData(ProcessListUIItemType itemType, int[] itemId)
    {
        this.itemType = itemType;
        this.itemId = itemId;
    }
}

public class ListUIRenderData : UIRenderData
{
    public ListItemUIRenderData[] items;
    public int confirmBtTextId;
    public Action confirmBtCallback;

    public ListUIRenderData(ListItemUIRenderData[] items, int confirmBtTextId, Action confirmBtCallback)
    {
        this.items = items;
        this.confirmBtTextId = confirmBtTextId;
        this.confirmBtCallback = confirmBtCallback;
    }
}