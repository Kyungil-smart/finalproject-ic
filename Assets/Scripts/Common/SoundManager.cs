using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoundManager : Manager, ISoundManager
{
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    
    private float _bgmVolume = 1f;
    private float _sfxVolume = 1f;
    private float _masterVolume = 1f;
    
    private const string BGMKEY = "BGM";
    private const string SFXKEY = "SFX";
    private const string MASTERKEY = "MASTER";

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
    public float GetMasterVolume() => _masterVolume;
    
    public void LoadVolume()
    {
        _sfxVolume = PlayerPrefs.GetFloat(SFXKEY, 0.5f);
        _bgmVolume = PlayerPrefs.GetFloat(BGMKEY, 0.5f);
        _masterVolume = PlayerPrefs.GetFloat(MASTERKEY, 0.5f);
        _bgmSource.volume = _bgmVolume * _masterVolume;
        _sfxSource.volume = _sfxVolume * _masterVolume;
    }
    
    public void SetMasterVolume(float volume)
    {
        _masterVolume = volume;
        _bgmSource.volume = _bgmVolume * volume;
        _sfxSource.volume = _sfxVolume * volume;
        PlayerPrefs.SetFloat(MASTERKEY, volume);
    }
    
    public void SetSfxVolume(float volume)
    {
        _sfxVolume = volume;
        _sfxSource.volume = _masterVolume * volume;
        PlayerPrefs.SetFloat(SFXKEY, volume);
    }

    public void SetBgmVolume(float volume)
    {
        _bgmVolume = volume;
        _bgmSource.volume = _masterVolume * volume;
        PlayerPrefs.SetFloat(BGMKEY, volume);
    }
    
    public UniTaskVoid PlayBgm(AudioClip bgm, float duration = 1)
    {
        Debug.Log("[SoundManager] PlayBgm");
        _bgmSource.Stop();
        _bgmSource.clip = bgm;
        _bgmSource.volume = _masterVolume * _bgmVolume;
        _bgmSource.Play();
        return default;
    }

    public void PlaySfx(AudioClip sfx)
    {
        Debug.Log("[SoundManager] PlaySfx");
        _sfxSource.PlayOneShot(sfx, _masterVolume * _sfxVolume);
    }

    public void PauseBgm() => _bgmSource.Pause();
    public void ResumeBgm() => _bgmSource.UnPause();
}
