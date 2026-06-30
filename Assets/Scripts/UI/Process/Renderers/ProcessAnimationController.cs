using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcessAnimationController : MonoBehaviour, IUIRender
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Image staticImage;
    [SerializeField] private Image progressImage;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Staff Control")]
    [SerializeField] private Transform leftStaffPos;
    [SerializeField] private Transform rightStaffPos;
    
    [Header("Talk Balloon")]
    [SerializeField] private Image leftTalkBalloon;
    [SerializeField] private Image rightTalkBalloon;
    
    private void OnEnable() => ServiceLocater.Get<IUIRouter>()?.RegisterUIRender(UIType.ProcAnimationUI, this);

    private void OnDisable() => ServiceLocater.Get<IUIRouter>()?.UnregisterUIRender(UIType.ProcAnimationUI);

    public void Render(UIRenderData data)
    {
        if (data is ProgressAnimationRenderData renderData)
        {
            UniTask.Void(async () =>
            {
                mainPanel.SetActive(true);
                if (renderData.staticImage != null) progressImage.sprite = renderData.staticImage;
                for (int i = 1; i <= renderData.progressTexts.Count; i++)
                {
                    progressText.text = renderData.progressTexts[i - 1];
                    progressImage.fillAmount = (1f / renderData.progressTexts.Count) * i;
                    await UniTask.WaitForSeconds(1f);
                }
                progressImage.fillAmount = 1f;
                await UniTask.WaitForSeconds(1f);
                mainPanel.SetActive(false);
                renderData.callback?.Invoke();
            });
        }
    }

    [ContextMenu("Test/Basic")]
    private void BasicTest()
    {
        var data = new ProgressAnimationRenderData()
        {
            staticImage = null,
            progressTexts = new() { "기존 직원 해고", "새 직원 고용", "사무실 자리 셋팅", "월 컴투 헬" }
        };
        Render(data);
    }
    
    [ContextMenu("Test/Animation")]
    private void AnimationTest()
    {
        var data = new ProgressAnimationRenderData()
        {
            staticImage = null,
            progressTexts = new() { "기존 직원 해고", "새 직원 고용", "사무실 자리 셋팅", "월 컴투 헬" }
        };
        Render(data);
    }
}
