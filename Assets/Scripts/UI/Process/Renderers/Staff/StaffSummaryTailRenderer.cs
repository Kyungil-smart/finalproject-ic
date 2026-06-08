using System;
using UnityEngine;
using UnityEngine.UI;

public class StaffSummaryTailOnClickController : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject mainPanel;

    public void SetOnClickCallBack(Action[] callback)
    {
        button.onClick.RemoveAllListeners();
        foreach (var callbackItem in callback)
        {
            button.onClick.AddListener(() => callbackItem());    
        }
        button.onClick.AddListener(() => mainPanel.SetActive(false));
    }
}