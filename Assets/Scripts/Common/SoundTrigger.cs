using UnityEngine;
using UnityEngine.EventSystems;

public class SoundTrigger : MonoBehaviour, IPointerClickHandler
{
    private AudioClip _audioClip;
    
    public void Init(AudioClip audioClip) => _audioClip = audioClip;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("버튼 효과음 재생");
        ServiceLocater.Get<ISoundManager>().PlaySfx(_audioClip);
    }
}
