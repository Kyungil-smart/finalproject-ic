using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 프로세스에서 보여야할 내용 중 간단한 애니메이션 출력 + 확인 버튼만 있는 UI 에 대한 매니저
/// </summary>
public class ProcessSimpleUIRenderer : MonoBehaviour, IUIRender
{
    [Header("UI Elements - Panel")]
    [SerializeField] private GameObject _panelObject;
    
    [Header("UI Elements - Main Session")]
    [SerializeField] private TextLoader _mainTxtLd;
    
    [Header("UI Elements - Button Session")]
    [SerializeField] private Button _confirmBt;
    [SerializeField] private TextLoader _confirmBtTxtLd;
    
    private void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>()
            .RegisterUIRender(UIType.ProcessSimpleUI, this);
    }

    private void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.ProcessSimpleUI);
    }

    public void Render(UIRenderData renderData)
    {
        if (renderData is SimpleUIRenderData data)
        {
            _mainTxtLd.TextId = data.mainTextId;
            _confirmBtTxtLd.TextId = data.btTextId;
            _confirmBt.onClick.RemoveAllListeners();
            _confirmBt.onClick.AddListener(() => _panelObject.SetActive(false));
            _confirmBt.onClick.AddListener(() => data.btCallback());
            _panelObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"[ProcessSimpleUIRenderer] Not Supported Data Type {renderData.GetType().Name}");
        }
    }
}