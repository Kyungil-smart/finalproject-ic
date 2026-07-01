using System;
using Unity.VisualScripting;
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
            Debug.Log("버튼 효과음 재생");
            var trigger = btn.gameObject.AddComponent<SoundTrigger>();
            trigger.Init(_btnClip);
        }
    }
}
