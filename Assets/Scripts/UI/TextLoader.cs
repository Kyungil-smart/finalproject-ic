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
        _postManager?.Subscribe<bool>(Channel.UpdateAllUITexts, UpdateText);
        UpdateText(true);
    }

    private void OnDisable() => _postManager?.Unsubscribe<bool>(Channel.UpdateAllUITexts, UpdateText);
    private void UpdateText(bool dummy)
    {
        if (textId == 0) return;  // ID 가 0 이면 아무 것도 하지 않음.
        if (_textGui == null) _textGui = GetComponent<TextMeshProUGUI>();
        _textGui.text = _postManager?.Request<int, string>(Channel.GetUIText, textId);
    }
}