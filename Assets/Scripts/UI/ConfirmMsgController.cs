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
        cancelBtn.onClick.AddListener(() => gameObject.SetActive(false));
        confirmBtn.onClick.RemoveAllListeners();
    }
    
    public void Render(int textId, Action okCallback)
    {
        gameObject.SetActive(true);
        msg.TextId = textId;
        confirmBtn.onClick.AddListener(() => okCallback?.Invoke());
        confirmBtn.onClick.AddListener(() => gameObject.SetActive(false));
    }
}