using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoundManager : Manager, ISoundManager
{
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    
    private float _bgmVolume = 1f;
    private float _sfxVolume = 1f;
    
    private const string BGMKEY = "BGM";
    private const string SFXKEY = "SFX";

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Register()
    {
        ServiceLocater.Register<ISoundManager>(this);
        LoadVolume();
    }

    protected override void Unregister() => ServiceLocater.Unregister<ISoundManager>(this);

    public float GetSfxVolume() => _sfxVolume;
    public float GetBgmVolume() => _bgmVolume;
    
    public void LoadVolume()
    {
        _sfxVolume = PlayerPrefs.GetFloat(SFXKEY, 0.5f);
        _bgmVolume = PlayerPrefs.GetFloat(BGMKEY, 0.5f);
        _bgmSource.volume = _bgmVolume;
        _sfxSource.volume = _sfxVolume;
    }
    
    public void SetSfxVolume(float volume)
    {
        _sfxVolume = volume;
        _sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFXKEY, volume);
    }

    public void SetBgmVolume(float volume)
    {
        _bgmVolume = volume;
        _bgmSource.volume = volume;
        PlayerPrefs.SetFloat(BGMKEY, volume);
    }
    
    public UniTaskVoid PlayBgm(AudioClip bgm, float duration = 1)
    {
        Debug.Log("[SoundManager] PlayBgm");
        _bgmSource.Stop();
        _bgmSource.clip = bgm;
        _bgmSource.volume = _bgmVolume;
        _bgmSource.Play();
        return default;
    }

    public void PlaySfx(AudioClip sfx)
    {
        Debug.Log("[SoundManager] PlaySfx");
        _sfxSource.PlayOneShot(sfx, _sfxVolume);
    }

    public void PauseBgm() => _bgmSource.Pause();

    public void ResumeBgm() => _bgmSource.UnPause();
}
