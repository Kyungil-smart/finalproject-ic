using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ScrollRect 기반 무한 루프 데이터 피커.
/// - 7개 고정 슬롯에 데이터 리스트를 Windowing 으로 매핑
/// - 양방향 무한 루프, 한 제스처당 최대 한 칸 이동
/// - 가운데 아이템 강조(bold/black), 외부에서 중앙 데이터 조회/초기 지정 가능
/// </summary>
public class DataRolePicker : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private List<RectTransform> uiItems = new(); // 위→아래 순서로 7개

    [Header("Layout")]
    [SerializeField] private float itemHeight = 50f;
    [SerializeField] private float spacing = 1f;

    [Header("Scroll Behaviour")]
    [SerializeField] private float snapSpeed = 15f;
    [SerializeField] private float dragThreshold = 25f; // 이 거리 이상 끌면 한 칸 확정

    [Header("Text Style")]
    [SerializeField] private Color centerColor = Color.black;
    [SerializeField] private Color normalColor = Color.gray;

    [Header("Initial State")]
    [SerializeField] private string initialCenterValue = "a";

    /// <summary>스냅 확정으로 중앙 데이터가 바뀔 때 통지</summary>
    public event Action<NameTag> OnCenterDataChanged;

    private List<NameTag> _dataList = new();

    private readonly List<TMP_Text> _texts = new();
    private RectTransform _content;
    private int _centerSlot;          // 7개 중 중앙 슬롯 인덱스(=3)
    private int _centerDataIndex;     // 현재 중앙에 있는 데이터 인덱스
    private bool _isDragging;
    private bool _isSnapping;
    private float _snapY;             // 스냅 중 우리가 소유하는 Y값
    private float _snapTarget;

    private float Step => itemHeight + spacing;

    public NameTag CenterData =>
        _dataList.Count > 0 ? _dataList[_centerDataIndex] : new NameTag();

    private void Awake()
    {
        _content = scrollRect.content;
        _centerSlot = uiItems.Count / 2;

        foreach (RectTransform item in uiItems)
            _texts.Add(item.GetComponentInChildren<TMP_Text>());

        LayoutSlots();
        SetInitialCenterValue(initialCenterValue);
    }

    // ScrollRect 가 이벤트 단계에서 Content 를 움직인 뒤, LateUpdate 에서 우리가 최종 보정한다.
    private void LateUpdate()
    {
        if (_isDragging)
        {
            // 한 제스처에 최대 한 칸: ScrollRect 이동량을 ±1칸으로 제한
            SetContentY(Mathf.Clamp(_content.anchoredPosition.y, -Step, Step));
            UpdateTextStyles();
        }
        else if (_isSnapping)
        {
            _snapY = Mathf.Lerp(_snapY, _snapTarget, Time.deltaTime * snapSpeed);

            if (Mathf.Abs(_snapY - _snapTarget) < 0.5f)
            {
                FinalizeStep();
            }
            else
            {
                SetContentY(_snapY);
                UpdateTextStyles();
            }
        }
        else if (_content.anchoredPosition.y != 0f)
        {
            // 대기 상태에선 어떤 외부 이동(휠, 무시된 드래그)도 허용하지 않음
            SetContentY(0f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isSnapping) return; // 스냅 중 시작된 제스처는 무시
        _isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

        float y = Mathf.Clamp(_content.anchoredPosition.y, -Step, Step);
        SetContentY(y);

        _snapY = y;
        _snapTarget = Mathf.Abs(y) >= dragThreshold ? Mathf.Sign(y) * Step : 0f;
        _isSnapping = true;
    }

    // ───────────────────────── 외부 API ─────────────────────────

    /// <summary>데이터 리스트 교체. 중앙은 첫 데이터로 리셋.</summary>
    public void SetDataList(IList<NameTag> newDataList)
    {
        if (newDataList == null || newDataList.Count == 0) return;
        _dataList = new List<NameTag>(newDataList);
        SetInitialCenterIndex(0);
    }

    /// <summary>초기 중앙 데이터를 값으로 지정. 리스트에 없으면 첫 데이터.</summary>
    public void SetInitialCenterValue(string value)
    {
        for (int index = 0; index < _dataList.Count; index++)
        {
            if (_dataList[index].name == value)
            {
                SetInitialCenterIndex(Mathf.Max(0, index));
                break;
            }
        }
    }

    /// <summary>초기 중앙 데이터를 인덱스로 지정.</summary>
    public void SetInitialCenterIndex(int dataIndex)
    {
        if (_dataList.Count == 0) return;
        _isDragging = false;
        _isSnapping = false;
        SetContentY(0f);
        _centerDataIndex = WrapIndex(dataIndex, _dataList.Count);
        RefreshTexts();
        UpdateTextStyles();
    }

    /// <summary>현재 중앙 데이터 값.</summary>
    public NameTag GetCenterData() => CenterData;

    // ───────────────────────── 내부 구현 ─────────────────────────

    private void LayoutSlots()
    {
        for (int i = 0; i < uiItems.Count; i++)
        {
            RectTransform rt = uiItems[i];
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(0f, itemHeight);
            rt.anchoredPosition = new Vector2(0f, (_centerSlot - i) * Step);
        }
    }

    // Windowing: 중앙 데이터 기준으로 7개 슬롯에 앞뒤 3개씩 매핑 (인덱스는 순환)
    private void RefreshTexts()
    {
        for (int i = 0; i < uiItems.Count; i++)
        {
            int dataIdx = WrapIndex(_centerDataIndex + (i - _centerSlot), _dataList.Count);
            _texts[i].text = _dataList[dataIdx].name;
        }
    }

    private void FinalizeStep()
    {
        int step = Mathf.RoundToInt(_snapTarget / Step); // 위로 밀면 +1(다음 데이터), 아래로 -1

        _isSnapping = false;
        SetContentY(0f);

        if (step != 0)
        {
            _centerDataIndex = WrapIndex(_centerDataIndex + step, _dataList.Count);
            RefreshTexts();
            OnCenterDataChanged?.Invoke(CenterData);
        }

        UpdateTextStyles();
    }

    private void UpdateTextStyles()
    {
        float contentY = _content.anchoredPosition.y;
        float minDistance = float.MaxValue;
        int closest = _centerSlot;

        for (int i = 0; i < uiItems.Count; i++)
        {
            // 슬롯 기준 위치 + Content 오프셋 = 화면상 중앙(0)으로부터의 거리
            float dist = Mathf.Abs((_centerSlot - i) * Step + contentY);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = i;
            }
        }

        for (int i = 0; i < uiItems.Count; i++)
        {
            bool isCenter = i == closest;
            _texts[i].color = isCenter ? centerColor : normalColor;
            _texts[i].fontStyle = isCenter ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    private void SetContentY(float y)
    {
        _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, y);
    }

    private static int WrapIndex(int index, int count)
    {
        return count == 0 ? 0 : (index % count + count) % count;
    }
}