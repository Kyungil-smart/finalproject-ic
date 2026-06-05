using System;
using System.Collections.Generic;

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
    public int titleTextId;
    public int confirmBtTextId;
    public Action confirmBtCallback;
    // public List<ListItemUIRenderData> items = new();
    public List<string> items = new();
    
    public ListUIRenderData(int confirmBtTextId, Action confirmBtCallback)
    {
        this.confirmBtTextId = confirmBtTextId;
        this.confirmBtCallback = confirmBtCallback;
    }
}