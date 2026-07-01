using UnityEngine;
using UnityEngine.EventSystems;

public class SoundTrigger : MonoBehaviour, IPointerClickHandler
{
    private AudioClip _audioClip;
    
    public void Init(AudioClip audioClip) => _audioClip = audioClip;

    public void OnPointerClick(PointerEventData eventData)
    {
        ServiceLocater.Get<ISoundManager>().PlaySfx(_audioClip);
    }
}
