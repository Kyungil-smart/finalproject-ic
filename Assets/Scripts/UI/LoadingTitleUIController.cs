using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class LoadingTitleUIController : MonoBehaviour
{
    [SerializeField] private GameObject loadingPage;
    [SerializeField] private GameObject[] managers;
    
    private List<IReadyStatus> _readyStatuses = new ();


    private void Start()
    {
        foreach (var manager in managers)
            _readyStatuses.Add(manager.GetComponent<IReadyStatus>());
        Debug.Log($"[LoadingTitleUIController] readyStatuses count: {_readyStatuses.Count}");
        CheckReadyStatus();
    }
    
    private async UniTask CheckReadyStatus()
    {
        try
        {
            await Utils.TaskAsync.WaitUntilOrThrowAsync(GetReadyStatus, 20f);
        }
        finally
        {
            loadingPage.SetActive(false);    
        }
    }
    
    private bool GetReadyStatus()
    {
        foreach (var r in _readyStatuses)
            foreach (var status in r.ReadyStatus)
                if (!status.Value) return false;
        return true;
    }
}