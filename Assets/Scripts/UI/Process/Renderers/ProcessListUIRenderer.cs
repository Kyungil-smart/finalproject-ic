using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 프로세스에서 보여야할 내용 중 List 형으로 보여주고 선택도 가능하게 할 UI
/// </summary>
public class ProcessListUIRenderer : MonoBehaviour, IUIRender
{
    [Header("UI Elements - Main")]
    [SerializeField] private GameObject _panelObject;
    [SerializeField] private TextLoader _titleTl;
    
    [Header("UI Elements - Content Session")]
    [SerializeField] private TextMeshProUGUI _bodyTmp;
    
    [Header("UI Elements - Button Session")]
    [SerializeField] private Button _confirmBt;
    [SerializeField] private TextLoader _confirmTl;
    
    [Header("UI Elements - Item Prefab")]
    [SerializeField] private GameObject _itemPrefab;
    
    public void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>()
            .RegisterUIRender(UIType.ProcessListUI, this);
    }
    
    public void Render(UIRenderData renderData)
    {
        if (renderData is ListUIRenderData data)
        {
            _titleTl.TextId = data.titleTextId;
            _confirmTl.TextId = data.confirmBtTextId;
            _confirmBt.onClick.RemoveAllListeners();
            _confirmBt.onClick.AddListener(() => _panelObject.SetActive(false));
            _confirmBt.onClick.AddListener(() => data.confirmBtCallback());
            _panelObject.SetActive(true);
            // 출력해야 하는 타입에 맞춰 상세 내용 변경 예정
            RenderSimpleList(data.items);
        }
        else
        {
            Debug.LogError($"[ProcessListUIRenderer] Not Supported Data Type {renderData.GetType().Name}");
        }
    }

    private void RenderSimpleList(List<string> items)
    {
        // 임시 내용. 추후 제대로된 Panel 구현시 변경 예정
        _bodyTmp.text = String.Join("\n", items);
    }
}