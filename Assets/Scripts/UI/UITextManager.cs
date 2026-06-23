using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DataDispatcher;
using UnityEngine;
using Channel = DataDispatcher.Channel;

[Serializable]
public struct UITextLanguage
{
    public LanguageType language;
    public string gid;
}

public class UITextManager : Manager, IUITextManager, IReadyStatus
{
    [Header("GSheet Information")]
    [SerializeField] string _gSheetId;
    [SerializeField] UITextLanguage[] _textTypes;

    [Header("UI Text Data")]
    [SerializeField] private LanguageType _currentLanguage = LanguageType.Korean;
    [SerializeField] private UITextSOScript[] uiTextSOs;
    
    private List<(LanguageType language, GSheetManager manager)> _gSheetManagers = new();
    private IPostManager _postManager;
    private List<Line> _texts = new();
    public bool IsDataUpdated { get; set; }
    private Dictionary<string, bool> _readyStatus = new ();
    public Dictionary<string, bool> ReadyStatus => _readyStatus;

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    
    protected override void Init()
    {
        InitAsync().Forget();
    }

    private async UniTaskVoid InitAsync()
    {
        _readyStatus.Clear();
        _readyStatus["UIText"] = false;
        foreach (var textType in _textTypes)
        {
            if (Utils.Environment.isDevelopment)
            {
                var gsManager = new GSheetManager(_gSheetId, textType.gid);
                await Utils.TaskAsync.WaitUntilOrThrowAsync(() => gsManager.IsDownload);
                _gSheetManagers.Add((textType.language, gsManager));    
            }
        }
            
        if (IsDataUpdated) return;
        UpdateUITextData();
    }
    
    protected override void Register()
    {
        ServiceLocater.Register<IUITextManager>(this);
        SubscribeChannel().Forget();
    }

    protected override void Unregister()
    {
        ServiceLocater.Unregister<IUITextManager>(this);
        _postManager.Unsubscribe<int, string>(Channel.GetUIText, GetText);
        _postManager = null;
    }

    private async UniTaskVoid SubscribeChannel()
    {
        await Utils.TaskAsync.WaitUntilOrThrowAsync(() => ServiceLocater.Get<IPostManager>() != null);
        ServiceLocater.Get<IPostManager>().Subscribe<int, string>(Channel.GetUIText, GetText);
    }

    private void UpdateUITextData()
    {
        UniTask.Void(async () =>
        {
            if (Utils.Environment.isDevelopment) await GetDataFromGSheet();
            await ConvertSOtoData();
            IsDataUpdated = true;
            _readyStatus["UIText"] = true;
        });
    }

    private async UniTask GetDataFromGSheet()
    {   // SO 에 데이터 담기
        foreach (var (language, gSheetManager) in _gSheetManagers)
        {
            var data = gSheetManager.GetData();
            if (data.Count == 0)
            {
                Debug.LogWarning($"{language}: No GSheet Data.");
                continue;
            }
            foreach (var so in uiTextSOs)
            {
                if (so.language != language) continue;
                so.lines.Clear();
                foreach (var soData in data)
                    so.lines.Add(new Line(soData["Text_ID"], soData["Text_Value"]));
            }
        }
        await UniTask.Yield();
    }

    private UniTask ConvertSOtoData()
    {   // Language 에 따라 runtime 으로 옮기기
        foreach (var so in uiTextSOs)
        {
            if (so.language == _currentLanguage)
            {
                _texts = so.lines;
                break;    
            }
        }
        return UniTask.CompletedTask;
    }

    public void ApplyAllText() => _postManager?.Post(Channel.UpdateAllUITexts, true);
    public void ChangeCurrentLanguage(LanguageType language) => _currentLanguage = language;

    private string GetText(int textId)
    {
        foreach (var text in _texts)
            if (text.id == textId) return text.text;
        Debug.LogWarning($"[UITextManager] Not Found Text ID : {textId}");
        return textId.ToString();
    }

    [ContextMenu("Load Data")]
    private void LoadData()
    {
        InitAsync().Forget();
    }
}