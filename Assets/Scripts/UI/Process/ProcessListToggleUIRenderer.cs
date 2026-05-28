using System;
using UnityEngine;
using UnityEngine.UI;

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

/// <summary>
/// 프로세스에서 보여야할 내용 중 List 형으로 보여주고 선택도 가능하게 할 UI
/// </summary>
public class ProcessListToggleUIRenderer : MonoBehaviour, IProcessUIRender
{
    [SerializeField] private Button _confirmBt;
    [SerializeField] private GameObject _itemPrefab;
    
    public void OnEnable()
    {
        ServiceLocater.Get<IProcessUIRouter>()
            .RegisterUIRender(ProcessUIType.ListToggleUI, this);
    }
    
    public void Render(UIRenderData renderData)
    {
        if (renderData is ListToggleUIRenderData data)
        {
            
        }
        else
        {
            Debug.LogError($"[ProcessListToggleUIRenderer] Not Supported Data Type {renderData.GetType().Name}");
        }
    }
}