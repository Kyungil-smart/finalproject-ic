using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SlideUIController : MonoBehaviour, IUIRender
{
    [Header("Staff 관련")]
    [SerializeField] private GameObject staffMainPanel;
    [SerializeField] private List<StaffDatailUIRenderer> staffDatailUIRenderers;
    
    [Header("Project 관련")]
    [SerializeField] private GameObject projectMainPanel;
    [SerializeField] private ProjectDetailUIRenderer projectDetailUI;
    [SerializeField] private RectTransform projectContentRt;
    
    [Header("Scroll View 제어")]
    [SerializeField] private RectTransform viewPort;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private float swipeTime = 0.2f;
    [SerializeField] private float swipeDistance = 50.0f;
    
    private float[] _scrollPageValues;
    private float _valueDistance = 0;
    private int _currentPage = 0;
    private int _maxPage = 0;
    private float _startTouchX;
    private float _endTouchX;
    private bool _isSwapeMode = false;
    private int projectCnt = 0;
    
    public void Render(UIRenderData data)
    {
        staffMainPanel.SetActive(true);
        if (data is StaffDetailRenderData renderData)
        {
            for (int i = 0; i < renderData.staffDataList.Count; i++)
            {   
                var staffData = renderData.staffDataList[i];
                var staffUI = staffDatailUIRenderers[i];
                staffUI.gameObject.SetActive(true);
                var rt = staffUI.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(viewPort.rect.width, rt.sizeDelta.y);
                staffUI.Render(staffData, Close);
            }
            Init();
        }   
        else
        {
            projectMainPanel.SetActive(true);
            // ToDo.
            // 1. Project List 가져오기
            // 2. Project 개수 확인 
            // 2-1. Project 개수 변화 없으면 리턴
            // 2-2. Project 개수 변화 있으면 AddProject 함수 실행
            // 2-2-1. 추가된 Project 에 대해서만 진행.
            // Project Panel 자체는 영구 보존
            UniTask.Void(async () =>
            {
                await AddProject(new ProjectDetailRenderData());    
            });
            // 여기는 뭐 실행되는거 없어야함. (안그럼 꼬입니다)
        }
    }

    private void Close()
    {
        staffMainPanel.SetActive(false);
        foreach (var staffUI in staffDatailUIRenderers)
            staffUI.gameObject.SetActive(false);
    }
    
    private UniTask AddProject(ProjectDetailRenderData data)  
    {  
        // Project 상세 Panel 은 게임 시작 및 Project 진행 완료시 하나씩 생성.
        // Project 상세 Panel 은 게임이 종료되기 전까지 삭제되지 않음.
        var go = Instantiate(projectDetailUI, projectContentRt);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(viewPort.rect.width, rt.sizeDelta.y);
        go.Render(data, () => projectMainPanel.SetActive(false));
        return UniTask.CompletedTask;
    }

    private void Init()
    {
        int enabledPanelCount = staffDatailUIRenderers.FindAll(x => x.gameObject.activeSelf).Count; 
        _scrollPageValues = new float[enabledPanelCount];
        _maxPage = enabledPanelCount;
        
        if (_maxPage <= 1)
        {
            _scrollPageValues[0] = 0;
            _valueDistance = 0;
        }
        else
        {
            _valueDistance = 1f / (_scrollPageValues.Length - 1);
            for (int i = 0; i < _scrollPageValues.Length; i++)
                _scrollPageValues[i] = _valueDistance * i;    
        }
        
        SetScrollBarValue(0);
        Debug.Log($"_maxPage = {_maxPage}");
    }
    
    private void Awake()
    {
        ServiceLocater.Get<IUIRouter>().RegisterUIRender(UIType.SlideUI, this);
    }
    
    private void OnDestroy()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.SlideUI);
    }
    
    private void Update()
    {
        UpdateInput();
    }

    private void SetScrollBarValue(int index)
    {
        _currentPage = index;
        scrollbar.value = _scrollPageValues[index];
    }

    private void UpdateInput()
    {
        if (!staffMainPanel.activeSelf) return;
        if (_isSwapeMode) return;
        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            _startTouchX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _endTouchX = Input.mousePosition.x;
            UpdateSwipe();
        }
        #endif
        
        #if UNITY_ANDROID
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                _startTouchX = touch.position.x;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _endTouchX = touch.position.x;
                UpdateSwipe();
            }
        }
        #endif
    }

    private void UpdateSwipe()
    {
        if (Mathf.Abs(_endTouchX - _startTouchX) < swipeDistance)
        {
            StartCoroutine(OnSwipeOneStep(_currentPage));
            return;
        }

        if (_startTouchX < _endTouchX)
        {
            if (_currentPage <= 0) return;
            _currentPage--;
        }
        else
        {
            if (_currentPage == _maxPage - 1) return;
            _currentPage++;
        }
    }

    private IEnumerator OnSwipeOneStep(int index)
    {
        float start = scrollbar.value;
        float current = 0;
        float percent = 0;
        _isSwapeMode = true;
        while (percent < 1f)
        {
            current += Time.deltaTime;
            percent = current / swipeTime;
            scrollbar.value = Mathf.Lerp(start, _scrollPageValues[index], percent);
            yield return null;
        }
        _isSwapeMode = false;
    }
}
