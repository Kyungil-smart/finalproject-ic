using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundBinder : MonoBehaviour
{
    [SerializeField] private AudioClip btnClip;

    private void Start()
    {
        foreach (var btn in GetComponentsInChildren<Button>(true))
            btn.onClick.AddListener(() => ServiceLocater.Get<ISoundManager>().PlaySfx(btnClip));
    }
}
