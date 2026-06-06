using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SlideUIController : MonoBehaviour, IUIRender
{
    [Header("Object 관련")]
    [SerializeField] private Transform contentTf;
    [SerializeField] private StaffDatailUIRenderer staffDatailUIRenderer;
    
    [Header("Scroll View 제어")]
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private float swipeTime = 0.2f;
    [SerializeField] private float swipeDistance = 50.0f;
    
    private List<GameObject> _scrollPages = new();
    private float[] _scrollPageValues;
    private float _valueDistance = 0;
    private int _currentPage = 0;
    private int _maxPage = 0;
    private float _startTouchX;
    private float _endTouchX;
    private bool _isSwapeMode = false;
    
    
    public void Render(UIRenderData data)
    {
        var viewPortTf = contentTf.parent.GetComponent<RectTransform>();
        if (data is StaffDetailRenderData renderData)
        {
            foreach (var staff in renderData.staffDataList)
            {   
                var sd = Instantiate(staffDatailUIRenderer, contentTf);
                _scrollPages.Add(sd.gameObject);
                var sdTf = sd.GetComponent<RectTransform>();
                sdTf.sizeDelta = new Vector2(viewPortTf.rect.width, sdTf.sizeDelta.y);
                sd.Render(staff, renderData.btnCallback);
            }
            Init();
        }   // ToDo. Last Project Data
    }
    
    private void AddDataList()  // ToDo. Last Project Data
    {  
        
    }

    private void Init()
    {
        _scrollPageValues = new float[contentTf.childCount];
        _maxPage = contentTf.childCount;
        
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

    private void OnDisable()
    {
        foreach (var go in _scrollPages)
        {
            Destroy(go);  // ToDo. gc 해결해야 할텐데 어떻게 하면 좋을지 잘 모르겠음
        }
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
            if (_currentPage > 0) return;
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
