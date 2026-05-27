using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DataDispatcher;
using UnityEngine;
using Channel = DataDispatcher.Channel;


public class UITextManager : Manager, IUITextManager
{
    [Header("GSheet Information")]
    [SerializeField] string _gSheetId;
    [SerializeField] string _gid;

    [Header("UI Text Data")]
    [SerializeField] private UITextSOScript uiTextSoScript;
    
    
    private IPostManager _postManager;
    private List<(int id, string data)> _texts = new();
    public bool IsDataUpdated { get; set; }

    private void Awake() => Init();

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Init()
    {
        _postManager = ServiceLocater.Get<IPostManager>();
        UpdateUITextData();
    }
    
    protected override void Register()
    {
        ServiceLocater.Register<IUITextManager>(this);
        _postManager?.Subscribe<int, string>(Channel.GetUIText, GetText);
    }

    protected override void Unregister()
    {
        ServiceLocater.Unregister<IUITextManager>(this);
        _postManager?.Unsubscribe<int, string>(Channel.GetUIText, GetText);
    }

    private void UpdateUITextData()
    {
        if (Utils.Environment.isDevelopment)
        {
            UniTask.Void(async () =>
            {
                await GetDataFromGSheet();
                await ConvertSOtoData();
            });
        }
        IsDataUpdated = true;
    }
    
    private async UniTask GetDataFromGSheet()
    {   // SO 에 데이터 담기
        
    }

    private async UniTask ConvertSOtoData()
    {   // Language 에 따라 runtime 으로 옮기기
        
    }

    public void ApplyAllText() => _postManager?.Post(Channel.UpdateAllUITexts, true);
    
    private string GetText(int textId)
    {
        foreach (var text in _texts)
            if (text.id == textId) return text.data;
        return $"Not Found Text ID : {textId}";
    }
}