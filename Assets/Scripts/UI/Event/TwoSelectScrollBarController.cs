using System;
using System.Collections.Generic;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class EventScrollTransform
{
    public RectTransform panelTf;  // msgPanel. 실제로는 Image 로 되어 있음.
    public RectTransform goalTf;   // 빈 object.
}


public class TwoSelectScrollBarController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IEndDragHandler
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField][Range(0f, 0.5f)] private float initScrollValue = 0.5f;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private EventScrollTransform rightView;
    [SerializeField] private EventScrollTransform leftView;
    [SerializeField] private TextLoader[] msgTls;  // 0 왼, 1 오른.
    [SerializeField] private float holdDuration   = 1f;
    [SerializeField] private float returnTweenTime = 0.3f;
    [SerializeField] private ConfirmMsgController confirmMsgController;
    [SerializeField] private TextLoader infoTl;
    
    [Header("ProgressBar Controller")]
    [SerializeField] private GameObject progressBarObject;
    [SerializeField] private Image progressBar;
    [SerializeField] private Sprite progressBarRightImg;
    [SerializeField] private Sprite progressBarLeftImg;
    
    public ReactiveProperty<float> scrollValue = new ();
    private Vector2 _leftStartPos;
    private Vector2 _rightStartPos;
    
    private bool   _isHolding;
    private float  _holdTimer;
    private bool   _confirmed;
    private Tween  _returnTween;
    private bool   _holdScreen;
    private List<(int id, Action<int> callback)> _choices = new();

    private const float Eps = 0.001f;   // 0/1 도달 판정 여유
    
    private void Awake()
    {
        _leftStartPos  = leftView.panelTf.anchoredPosition;
        _rightStartPos = rightView.panelTf.anchoredPosition;
    }

    private void OnEnable()
    {
        RestoreInfoText();
        scrollbar.value = initScrollValue;
        scrollbar.interactable = true;
        progressBarObject.SetActive(false);
        progressBar.fillAmount = 0f;
        _choices.Clear();
        confirmMsgController.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_holdScreen) return;
        float value = scrollbar.value;
        scrollValue.Value = value;
        
        // 0.5 -> 1 : rightView 가 오른쪽에서 들어와 왼쪽 변이 goalTf에서 멈춤
        MovePanel(rightView, _rightStartPos, Mathf.InverseLerp(0.5f, 1f, value), true);

        // 0.5 -> 0 : leftView 가 왼쪽에서 들어와 오른쪽 변이 goalTf에서 멈춤
        MovePanel(leftView,  _leftStartPos,  Mathf.InverseLerp(0.5f, 0f, value), false);
        
        bool atEnd = value <= Eps || value >= 1f - Eps;

        if (_isHolding && atEnd)
        {
            HoldInfoText();
            _holdTimer += Time.deltaTime;
            if (_holdTimer > 0)
            {
                progressBar.sprite = scrollbar.value >= 0.5 ? progressBarRightImg : progressBarLeftImg;
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
                _holdScreen = true;
                scrollbar.interactable = false;
                int confirmedIndex = (int)Math.Round(scrollbar.value);
                Debug.Log($"[ScrollBar] 확정! value = {value}");
                if (!confirmMsgController.gameObject.activeSelf)
                {
                    confirmMsgController.Render(
                        9900042, 
                        () =>
                        {
                            var choice = _choices[confirmedIndex];
                            choice.callback?.Invoke(choice.id);
                            _holdScreen = false;
                            mainPanel.SetActive(false);
                            gameObject.SetActive(false);
                        },
                        () =>
                        {
                            progressBarObject.SetActive(false);
                            progressBar.fillAmount = 0f;
                            _holdScreen = false;
                            scrollbar.interactable = true;
                            scrollbar.value = 0.5f;
                            scrollValue.Value = 0.5f; 
                        });
                }
            }
        }
        else
        {
            RestoreInfoText();
            _holdTimer = 0f;   // 끝에서 벗어나거나 손 떼면 리셋
        }
    }
    
    private void HoldInfoText() => infoTl.TextId = 9900057;
    private void RestoreInfoText() => infoTl.TextId = 9900055;

    public void SetData(int index, int id, int textId, Action<int> callback)
    {
        msgTls[index].TextId = textId;
        _choices.Add((id, callback));
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
        progressBar.fillAmount = 0f;
        _returnTween?.Kill();   // 복귀 트윈 도중 다시 잡으면 취소
    }

    public void OnPointerUp(PointerEventData e) => EndHold();
    public void OnEndDrag(PointerEventData e)   => EndHold();   // 영역 밖 종료 안전망

    private void EndHold()
    {
        if (!_isHolding || _holdScreen) return;   // PointerUp+EndDrag 동시 호출 중복 방지
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
                0.5f, returnTweenTime)
            .SetEase(Ease.OutQuad);
    }
}
