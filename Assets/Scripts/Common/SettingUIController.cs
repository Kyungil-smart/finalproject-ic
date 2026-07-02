using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingUIContorller : MonoBehaviour
{
    [Header("Settings Buttons")]
    [SerializeField] private Button settingBtn;
    
    [Header("Setting Panel")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button soundBtn;
    [SerializeField] private Button firstBtn;
    
    [Header("Sound Panel")]
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private Button closeSoundBtn;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    [Header("Confirm Msg")] 
    [SerializeField] private ConfirmMsgController cmc;
    private void Start()
    {
        settingBtn.onClick.AddListener(OpenSettingPanel);
        continueBtn.onClick.AddListener(CloseSettingPanel);
        soundBtn.onClick.AddListener(OpenSoundPanel);
        firstBtn.onClick.AddListener(GoFirst);
        closeSoundBtn.onClick.AddListener(CloseSoundPanel);
        masterSlider.onValueChanged.AddListener(v => ServiceLocater.Get<ISoundManager>().SetMasterVolume(v));
        bgmSlider.onValueChanged.AddListener(v => ServiceLocater.Get<ISoundManager>().SetBgmVolume(v));
        sfxSlider.onValueChanged.AddListener(v => ServiceLocater.Get<ISoundManager>().SetSfxVolume(v));
    }

    private void OpenSettingPanel()
    {
        Time.timeScale = 0;
        settingPanel.SetActive(true);
    }

    private void CloseSettingPanel()
    {
        Time.timeScale = 1;
        settingPanel.SetActive(false);
    }
    
    private void OpenSoundPanel()
    {
        var sound = ServiceLocater.Get<ISoundManager>();
        masterSlider.value = sound.GetMasterVolume();
        sfxSlider.value = sound.GetSfxVolume();
        bgmSlider.value = sound.GetBgmVolume();
        settingPanel.SetActive(false);
        soundPanel.SetActive(true);
    }

    private void CloseSoundPanel()
    {
        soundPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    private void GoFirst()
    {
        cmc.Render(9900054, () =>
        {
            Time.timeScale = 1;
            ServiceLocater.Get<ISceneChanger>().ChangeScene("TitleScene");
        },null);
    }
}
