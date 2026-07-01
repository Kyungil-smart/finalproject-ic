using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LoadingTitleUIController : MonoBehaviour
{
    [SerializeField] private GameObject loadingPage;
    [SerializeField] private GameObject[] managers;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressBarText;
    [SerializeField] private AudioClip bgmClip;
    
    private List<IReadyStatus> _readyStatuses = new ();
    private float totalProgressCount;

    private void Start()
    {
        foreach (var manager in managers)
            _readyStatuses.Add(manager.GetComponent<IReadyStatus>());
        totalProgressCount = _readyStatuses.Count;
        Debug.Log($"[LoadingTitleUIController] readyStatuses count: {_readyStatuses.Count}");
        CheckReadyStatus();
    }
    
    private async UniTask CheckReadyStatus()
    {
        try
        {
            await Utils.TaskAsync.WaitUntilOrThrowAsync("LoadingTitle", GetReadyStatus, 20f);
            progressBar.fillAmount = 1f;
        }
        finally
        {
            await UniTask.WaitForSeconds(1f);
            loadingPage.SetActive(false);
            ServiceLocater.Get<ISoundManager>().PlayBgm(bgmClip);
        }
    }
    
    private bool GetReadyStatus()
    {
        for (int i = 0; i < totalProgressCount; i++)
        {
            var r = _readyStatuses[i];
            progressBar.fillAmount = i / totalProgressCount;
            foreach (var status in r.ReadyStatus)
            {
                progressBarText.text = $"Loading ... {status.Key}";
                if (!status.Value) return false;
            }    
        }
        return true;
    }
}