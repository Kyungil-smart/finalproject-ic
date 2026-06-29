using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoundManager : Manager, ISoundManager
{
    protected override void Register() => ServiceLocater.Register(this);

    protected override void Unregister() => ServiceLocater.Unregister(this);
    
    public void SetSfxVolume(float volume)
    {
        
    }

    public void SetBgmVolume(float volume)
    {
        
    }

    public float GetSfxVolume()
    {
        throw new System.NotImplementedException();
    }

    public float GetBgmVolume()
    {
        throw new System.NotImplementedException();
    }

    public UniTaskVoid PlayBgm(AudioClip bgm, float duration = 1)
    {
        throw new System.NotImplementedException();
    }

    public void PlaySfx(AudioClip sfx)
    {
        throw new System.NotImplementedException();
    }
}
