using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ProcessSimpleUIRenderer 용 데이터 정의
/// </summary>
public class SimpleUIRenderData : UIRenderData
{
    public int mainTextId;
    public int btTextId;
    public Action btCallback;
}

/// <summary>
/// 프로세스에서 보여야할 내용 중 간단한 애니메이션 출력 + 확인 버튼만 있는 UI 에 대한 매니저
/// </summary>
public class ProcessSimpleUIRenderer : MonoBehaviour, IProcessUIRender
{
    [SerializeField] private TextMeshProUGUI _mainTmp;
    [SerializeField] private Button _confirmBt;
    
    public void OnEnable()
    {
        ServiceLocater.Get<IProcessUIRouter>()
            .RegisterUIRender(ProcessUIType.SimpleUI, this);
    }

    public void Render(UIRenderData renderData)
    {
        if (renderData is SimpleUIRenderData data)
        {
            
        }
        else
        {
            Debug.LogError($"[ProcessListUIRenderer] Not Supported Data Type {renderData.GetType().Name}");
        }
    }
}