using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ISoundManager
{
    public void SetSfxVolume(float volume);
    public void SetBgmVolume(float volume);
    public float GetSfxVolume();
    public float GetBgmVolume();
    public UniTaskVoid PlayBgm(AudioClip bgm, float duration = 1f);
    public void PlaySfx(AudioClip sfx);
}
