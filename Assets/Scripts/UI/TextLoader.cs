using R3;
using DataDispatcher;
using UnityEngine;
using TMPro;

public class TextLoader : MonoBehaviour
{
    [SerializeField] private int textId;
    private TextMeshProUGUI _textGui;
    private IPostManager _postManager;

    public int TextId
    {
        get => textId;
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
        UpdateText(true);
    }

    private void OnDisable() => _postManager?.Unsubscribe<bool>(Channel.RequestChangeText, UpdateText);
    private void UpdateText(bool dummy)
    {
        if (textId == 0) return;  // ID 가 0 이면 아무 것도 하지 않음.
        if (_textGui == null) _textGui = GetComponent<TextMeshProUGUI>();
        try
        {
            _textGui.text = _postManager?.Request<int, string>(Channel.UITextRequest, textId);
        }
        catch{
            Debug.Log($"[TextLoader] {textId} has not {Channel.UITextRequest} text.");
        }
    }
}