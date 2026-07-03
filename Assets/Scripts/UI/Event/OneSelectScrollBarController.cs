using System;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class OneSelectScrollBarController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IEndDragHandler
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField][Range(0f, 0.5f)] private float initScrollValue = 0.5f;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private EventScrollTransform rightView;
    [SerializeField] private TextLoader msgTl;
    [SerializeField] private float holdDuration   = 1f;
    [SerializeField] private float returnTweenTime = 0.3f;
    [SerializeField] private ConfirmMsgController confirmMsgController;
    [SerializeField] private TextLoader infoTl;
    
    [Header("ProgressBar Controller")]
    [SerializeField] private GameObject progressBarObject;
    [SerializeField] private Image progressBar;
    [SerializeField] private Sprite progressBarRightImg;
    
    public ReactiveProperty<float> scrollValue = new ();
    private Vector2 _rightStartPos;
    
    private bool   _isHolding;
    private float  _holdTimer;
    private bool   _confirmed;
    private Tween  _returnTween;
    
    private Action<int> _callback;
    private int _btnId;

    private const float Eps = 0.001f;   // 0/1 도달 판정 여유
    
    private void Awake()
    {
        _rightStartPos = rightView.panelTf.anchoredPosition;
    }

    private void OnEnable()
    {
        scrollbar.value = initScrollValue;
        RestoreInfoText();
        confirmMsgController.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        float value = scrollbar.value;
        scrollValue.Value = value;

        MovePanel(rightView, _rightStartPos, Mathf.InverseLerp(0f, 1f, value), true);
        
        bool atEnd = value <= Eps || value >= 1f - Eps;

        if (_isHolding && atEnd)
        {
            HoldInfoText();
            _holdTimer += Time.deltaTime;
            if (_holdTimer > 0)
            {
                progressBar.sprite = progressBarRightImg;
                progressBarObject.SetActive(true);
                progressBar.fillAmount = _holdTimer / holdDuration;
            }
            else
            {
                progressBarObject.SetActive(false);
                progressBar.fillAmount = 0f;
            }
            if (!_confirmed && _holdTimer >= holdDuration)
            {
                _confirmed = true;
                Debug.Log($"[ScrollBar] 확정! value = {value}");
                if (!confirmMsgController.gameObject.activeSelf)
                {
                    confirmMsgController.Render(
                        9900042, 
                        () =>
                        {
                            _callback?.Invoke(_btnId);
                            mainPanel.SetActive(false);
                            gameObject.SetActive(false);
                        },
                        () => { 
                            scrollbar.value = 0f;
                            scrollValue.Value = 0f; 
                        });
                }
            }
        }
        else
        {
            _holdTimer = 0f;   // 끝에서 벗어나거나 손 떼면 리셋
        }
    }

    private void HoldInfoText() => infoTl.TextId = 9900057;
    private void RestoreInfoText() => infoTl.TextId = 9900056;

    public void SetData(int id, int textId, Action<int> callback)
    {
        _btnId = id;
        _callback = callback;
        msgTl.TextId = textId;
        RestoreInfoText();
    }
    
    private void MovePanel(EventScrollTransform view, Vector2 startPos, float t, bool innerIsLeftEdge)
    {
        RectTransform p = view.panelTf;
        float width  = p.rect.width;
        float pivotX = p.pivot.x;
        float goalX  = view.goalTf.anchoredPosition.x;

        // panel의 '가장자리'가 goalX에 닿도록, pivot이 도달해야 할 목표 x를 보정
        float targetPivotX = innerIsLeftEdge
            ? goalX + pivotX * width          // 왼쪽 변 = goalX
            : goalX - (1f - pivotX) * width;  // 오른쪽 변 = goalX

        float x = Mathf.Lerp(startPos.x, targetPivotX, t);
        p.anchoredPosition = new Vector2(x, startPos.y);
    }
    
    public void OnPointerDown(PointerEventData e)
    {
        _isHolding  = true;
        _confirmed  = false;
        _holdTimer  = 0f;
        _returnTween?.Kill();   // 복귀 트윈 도중 다시 잡으면 취소
    }

    public void OnPointerUp(PointerEventData e) => EndHold();
    public void OnEndDrag(PointerEventData e)   => EndHold();   // 영역 밖 종료 안전망

    private void EndHold()
    {
        if (!_isHolding) return;   // PointerUp+EndDrag 동시 호출 중복 방지
        _isHolding = false;
        progressBar.fillAmount = 0f;
        progressBarObject.SetActive(false);
        RestoreInfoText();
        if (!_confirmed) ReturnToHalf();
    }
    
    private void ReturnToHalf()
    {
        _returnTween?.Kill();
        _returnTween = DOTween.To(() => scrollbar.value,
                v => scrollbar.value = v,
                initScrollValue, returnTweenTime)
            .SetEase(Ease.OutQuad);
    }
}
