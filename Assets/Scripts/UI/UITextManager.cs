using System.Collections.Generic;
using DataDispatcher;


public class UITextManager : Manager, IUITextManager
{
    private IPostManager _postManager;
    private List<(int id, string data)> _texts = new();
    
    private void Awake() => _postManager = ServiceLocater.Get<IPostManager>();

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    
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
    
    private void GetDataFromGSheet()
    {   // SO 에 데이터 담기
        
    }

    private void ConvertSOtoData()
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