using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmMsgController : MonoBehaviour
{
    [SerializeField] private TextLoader msg;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private Button cancelBtn;

    private void OnEnable()
    {
        cancelBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.RemoveAllListeners();
    }
    
    public void Render(int textId, Action okCallback, Action cancelCallback = null, string subText = null)
    {
        gameObject.SetActive(true);
        msg.TextId = textId;
        if (subText != null) msg.Text = $"{msg.GetString(textId.ToString())}\n\n{subText}";
        cancelBtn.onClick.AddListener(() => cancelCallback?.Invoke());
        cancelBtn.onClick.AddListener(() => gameObject.SetActive(false));
        confirmBtn.onClick.AddListener(() => okCallback?.Invoke());
        confirmBtn.onClick.AddListener(() => gameObject.SetActive(false));
    }
}