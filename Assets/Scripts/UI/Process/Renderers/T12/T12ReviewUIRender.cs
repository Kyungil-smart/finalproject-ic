using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class T12ReviewUIRender : MonoBehaviour
{
    [SerializeField] private List<ReviewPanelRender> reviewPanels;
    [SerializeField] private Button reviewConfirmBtn;

    public void Render(T12ReviewUIRenderData data)
    {
        for (int i = 0; i < reviewPanels.Count; i++)
        {
            if (i < data.reviews.Count)
            {
                reviewPanels[i].Render(data.reviews[i]);
                reviewPanels[i].gameObject.SetActive(true);
            }
            else
            {
                reviewPanels[i].gameObject.SetActive(false);
            }
        }
        reviewConfirmBtn.onClick.RemoveAllListeners();
        reviewConfirmBtn.onClick.AddListener(() => data.btCallback?.Invoke());
        reviewConfirmBtn.onClick.AddListener(() => gameObject.SetActive(false));
    }
}
