using Cysharp.Threading.Tasks;
using R3;
using DataDispatcher;
using UnityEngine;
using TMPro;
using Utils;
using Channel = DataDispatcher.Channel;

public class TextLoader : MonoBehaviour
{
    [SerializeField] private int textId = -1;
    [Tooltip("데이터 확인용")]
    [SerializeField] private string inputText;
    private TextMeshProUGUI _textGui;
    private IPostManager _postManager;
    // text ID 가 없는 경우를 대비한 임시 string 입력
    public string Text
    {
        get => _textGui.text;
        set => OverrideText(value);
    }

    public int TextId
    {
        set
        {
            textId = value;
            UpdateText(true);
        }
    }

    private void Awake()
    {
        _textGui = GetComponent<TextMeshProUGUI>();
        _postManager = ServiceLocater.Get<IPostManager>();
    }
    
    private void OnEnable()
    {
        if (_postManager == null)
        {
            Debug.LogWarning("[TextLoader] Could not load the Post Manager.");
            return;
        }
        _postManager.Subscribe<bool>(Channel.UpdateAllUITexts, UpdateText);
        UpdateText(true);
    }

    private void OnDisable() => _postManager?.Unsubscribe<bool>(Channel.UpdateAllUITexts, UpdateText);
    private void UpdateText(bool dummy)
    {
        UniTask.Void(async() => { await ChangeText(); });
    }

    private void OverrideText(string text)
    {
        Debug.Log($"[TextLoader] input text = {text}");
        if (_textGui == null) _textGui = GetComponent<TextMeshProUGUI>();
        _textGui.text = text;
        inputText = text;
    }
    
    private UniTask ChangeText()
    {
        if (textId < 0) return UniTask.CompletedTask;  // ID 가 -1 이면 아무 것도 하지 않음.
        if (_textGui == null) _textGui = GetComponent<TextMeshProUGUI>();
        if (_postManager == null) _postManager = ServiceLocater.Get<IPostManager>();
        if (textId != 0)
        {
            var text = _postManager?.Request<int, string>(Channel.GetUIText, textId);
            _textGui.text = text;
            inputText = text;
        }
        else
        {
            _textGui.text = "";
        }
        return UniTask.CompletedTask;
    }
    
    public string GetString(string textId)
    {
        var postManager = ServiceLocater.Get<IPostManager>();
        if (postManager == null)
        {
            Debug.LogError("유니티 게임 실행을 한 후 진행 바랍니다.");
            return null;
        }
        return postManager.Request<int, string>(Channel.GetUIText, int.Parse(textId));
    }
}