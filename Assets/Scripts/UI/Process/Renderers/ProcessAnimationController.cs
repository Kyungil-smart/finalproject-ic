using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = System.Random;

[Serializable]
public class TalkBalloonData
{
    public SpriteRenderer talkBalloon;
    public Sprite[] talkBalloonSprites;
}

public class ProcessAnimationController : MonoBehaviour, IUIRender
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Image staticImage;
    
    [Header("Progress Bar Control")]
    [SerializeField] private Image progressImage;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField][Range(1f, 2f)] private float progressInterval = 1.5f;

    [Header("Animation Panel")]
    [SerializeField] private GameObject animationPanel;
    
    [Header("Animation - Staff Control")]
    [SerializeField] private Transform leftStaffPos;
    [SerializeField] private Transform rightStaffPos;
    
    [Header("Animation - Talk Balloon")]
    [SerializeField] private TalkBalloonData leftTalkBalloon;
    [SerializeField] private TalkBalloonData rightTalkBalloon;
    
    [Header("Animation - Test 용")]
    [SerializeField] private GameObject leftSpum;
    [SerializeField] private GameObject rightSpum;
    
    private bool _runTalking;
    
    private void OnEnable() => ServiceLocater.Get<IUIRouter>()?.RegisterUIRender(UIType.ProcAnimationUI, this);

    private void OnDisable() => ServiceLocater.Get<IUIRouter>()?.UnregisterUIRender(UIType.ProcAnimationUI);

    public void Render(UIRenderData data)
    {
        if (data is ProgressAnimationRenderData renderData)
        {
            UniTask.Void(async () =>
            {
                mainPanel.SetActive(true);
                if (renderData.staticImage != null)
                {
                    staticImage.gameObject.SetActive(true);
                    progressImage.sprite = renderData.staticImage;
                }
                if ((int)renderData.gameDevProcName <= 9 || (int)renderData.gameDevProcName >= 4)
                {
                    animationPanel.SetActive(true);
                    _runTalking = true;
                    StaffController(renderData.staffIds).Forget();
                    TalkBalloonController(leftTalkBalloon).Forget();
                    TalkBalloonController(rightTalkBalloon).Forget();
                }
                await ProgressBarController(renderData);
                _runTalking = false;
                await UniTask.WaitForSeconds(1f);
                animationPanel.SetActive(false);
                mainPanel.SetActive(false);
                renderData.callback?.Invoke();
            });
        }
    }

    private UniTask StaffController(List<GameObject> spums)
    {
        StaffAnimController(spums[0], leftStaffPos, true).Forget();
        StaffAnimController(spums[1], rightStaffPos, false).Forget();
        return UniTask.CompletedTask;
    }

    private async UniTask StaffAnimController(GameObject spumObj, Transform pos, bool isLeft)
    {
        SPUM_Prefabs spum = spumObj.GetComponent<SPUM_Prefabs>();
        spumObj.transform.position = pos.position;

        var sortingGroup = spumObj.GetComponentInChildren<SortingGroup>(true);
        SetFacing(sortingGroup, isLeft);
        ApplySorting(sortingGroup, "FrontLayer", 50);
        
        while (_runTalking)
        {
            await UniTask.WaitForSeconds(UnityEngine.Random.Range(0.8f, 1.5f));
            spum.PlayAnimation(PlayerState.OTHER, 0);
            await UniTask.WaitForSeconds(UnityEngine.Random.Range(0.8f, 1.5f));
            spum.PlayAnimation(PlayerState.IDLE, 0);
        }
    }

    private void SetFacing(SortingGroup sortingGroup, bool faceLeft)
    {
        if (sortingGroup == null) return;
        Vector3 s = sortingGroup.transform.localScale;
        float baseX = Mathf.Abs(s.x);          // 항상 양수 기준에서 시작
        s.x = faceLeft ? -baseX : baseX;       // 상태 무관하게 무조건 세팅
        sortingGroup.transform.localScale = s;
    }
    
    private void ApplySorting(SortingGroup sortingGroup, string layer, int order)
    {
        if (sortingGroup == null) return;
        sortingGroup.sortingLayerName = layer;
        sortingGroup.sortingOrder = order;
        
        if (sortingGroup.sortingLayerID == 0 && layer != "Default")
            Debug.LogWarning($"[StaffMovement] Sorting Layer '{layer}' 없음(이름 확인). Default로 처리됨");
    }

    private async UniTask TalkBalloonController(TalkBalloonData balloonImage)
    {
        balloonImage.talkBalloon.gameObject.SetActive(true);
        while (_runTalking)
        {
            await UniTask.WaitForSeconds(UnityEngine.Random.Range(0.5f, 0.8f));
            var spriteCount = balloonImage.talkBalloonSprites.Length;
            balloonImage.talkBalloon.sprite = balloonImage.talkBalloonSprites[UnityEngine.Random.Range(0, spriteCount)];
        }
        balloonImage.talkBalloon.gameObject.SetActive(false);
    }

    private async UniTask ProgressBarController(ProgressAnimationRenderData renderData)
    {
        for (int i = 1; i <= renderData.progressTexts.Count; i++)
        {
            progressText.text = renderData.progressTexts[i - 1];
            progressImage.fillAmount = (1f / renderData.progressTexts.Count) * i;
            await UniTask.WaitForSeconds(progressInterval);
        }
        progressImage.fillAmount = 1f;
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
            gameDevProcName = GameDevProcName.ArtFullProduction,
            staticImage = null,
            progressTexts = new() { "뭥미", "살려줘", "갈아만든 작품", "죽여줘..." },
            staffIds = new () { leftSpum, rightSpum }
        };
        Render(data);
    }
}
