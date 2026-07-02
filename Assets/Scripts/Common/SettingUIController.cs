using System;
using TMPro;
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
    [SerializeField] private GameObject bgImg;
    
    [Header("Sound Panel")]
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private Button closeSoundBtn;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TextMeshProUGUI masterVolume;
    [SerializeField] private TextMeshProUGUI sfxVolume;
    [SerializeField] private TextMeshProUGUI bgmVolume;

    [Header("Confirm Msg")] 
    [SerializeField] private ConfirmMsgController cmc;
    
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "ProcessScene") firstBtn.interactable = false;
        settingBtn.onClick.AddListener(OpenSettingPanel);
        continueBtn.onClick.AddListener(CloseSettingPanel);
        soundBtn.onClick.AddListener(OpenSoundPanel);
        firstBtn.onClick.AddListener(GoFirst);
        closeSoundBtn.onClick.AddListener(CloseSoundPanel);
        masterSlider.onValueChanged.AddListener(v =>
        {
            ServiceLocater.Get<ISoundManager>().SetMasterVolume(v);
            masterVolume.text = $"{(int)(v * 100)}%";
        });
        bgmSlider.onValueChanged.AddListener(v =>
        {
            ServiceLocater.Get<ISoundManager>().SetBgmVolume(v);
            bgmVolume.text = $"{(int)(v * 100)}%";
        });
        sfxSlider.onValueChanged.AddListener(v =>
        {
            ServiceLocater.Get<ISoundManager>().SetSfxVolume(v);
            sfxVolume.text = $"{(int)(v * 100)}%";
        });
    }

    private void OpenSettingPanel()
    {
        Time.timeScale = 0;
        settingPanel.SetActive(true);
        bgImg.SetActive(true);
        settingBtn.gameObject.SetActive(false);
    }

    private void CloseSettingPanel()
    {
        Time.timeScale = 1;
        settingPanel.SetActive(false);
        bgImg.SetActive(false);
        settingBtn.gameObject.SetActive(true);
    }
    
    private void OpenSoundPanel()
    {
        var sound = ServiceLocater.Get<ISoundManager>();
        masterSlider.value = sound.GetMasterVolume();
        sfxSlider.value = sound.GetSfxVolume();
        bgmSlider.value = sound.GetBgmVolume();
        masterVolume.text = $"{(int)(masterSlider.value * 100)}%";
        bgmVolume.text = $"{(int)(bgmSlider.value * 100)}%";
        sfxVolume.text = $"{(int)(sfxSlider.value * 100)}%";
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
