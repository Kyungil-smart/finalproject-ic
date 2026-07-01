using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmMsgController : MonoBehaviour
{
    [SerializeField] private TextLoader msg;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private Button cancelBtn;
    [SerializeField] private AudioClip btnClip;

    private void OnEnable()
    {
        cancelBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.RemoveAllListeners();
    }
    
    public void Render(int textId, Action okCallback, Action cancelCallback = null)
    {
        gameObject.SetActive(true);
        msg.TextId = textId;
        cancelBtn.onClick.AddListener(() => cancelCallback?.Invoke());
        cancelBtn.onClick.AddListener(() =>
        {
            ServiceLocater.Get<ISoundManager>().PlaySfx(btnClip);
            gameObject.SetActive(false);
        });
        confirmBtn.onClick.AddListener(() => okCallback?.Invoke());
        confirmBtn.onClick.AddListener(() => gameObject.SetActive(false));
    }
}