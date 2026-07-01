using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundBinder : MonoBehaviour
{
    [SerializeField] private AudioClip _btnClip;

    private void Start()
    {
        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            if (btn.CompareTag("NoAutoSound")) continue;
            btn.onClick.AddListener(() => ServiceLocater.Get<ISoundManager>().PlaySfx(_btnClip));
        }
    }
}
