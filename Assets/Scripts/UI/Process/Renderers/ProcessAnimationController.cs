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
    [SerializeField] private ImageLoader staticImage;
    
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
    
    private bool _runTalking;
    
    private void OnEnable() => ServiceLocater.Get<IUIRouter>()?.RegisterUIRender(UIType.ProcAnimationUI, this);

    private void OnDisable() => ServiceLocater.Get<IUIRouter>()?.UnregisterUIRender(UIType.ProcAnimationUI);

    public void Render(UIRenderData data)
    {
        if (data is ProgressAnimationRenderData renderData)
        {
            UniTask.Void(async () =>
            {
                staticImage.gameObject.SetActive(true);
                animationPanel.SetActive(false);
                if (renderData.staticImageId != null)
                {
                    staticImage.ImageId = renderData.staticImageId;
                }
                if ((int)renderData.gameDevProcName <= 9 && (int)renderData.gameDevProcName >= 4)
                {
                    staticImage.gameObject.SetActive(false);
                    _runTalking = true;
                    StaffController(renderData.staffIds).Forget();
                    TalkBalloonController(leftTalkBalloon).Forget();
                    TalkBalloonController(rightTalkBalloon).Forget();
                    animationPanel.SetActive(true);
                }
                mainPanel.SetActive(true);
                await ProgressBarController(renderData);
                _runTalking = false;
                await UniTask.WaitForSeconds(1f);
                animationPanel.SetActive(false);
                mainPanel.SetActive(false);
                renderData.callback?.Invoke();
            });
        }
    }

    private UniTask StaffController(List<StaffEntity> staffs)
    {
        StaffAnimController(staffs[0], leftStaffPos, true).Forget();
        StaffAnimController(staffs[1], rightStaffPos, false).Forget();
        return UniTask.CompletedTask;
    }

    private async UniTask StaffAnimController(StaffEntity staffEntity, Transform pos, bool isLeft)
    {
        var spumObj = staffEntity.GetAvatar();
        staffEntity.GetGameObject().SetActive(true);
        staffEntity.GetMovement().ResetMotion();
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
        staffEntity.GetGameObject().SetActive(false);
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
}
