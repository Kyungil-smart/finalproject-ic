using DataDispatcher;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TutorialUIController : MonoBehaviour, IUIRender
{
    [Header("[UI Panels]")]
    [SerializeField] private GameObject mainPanel;  // 전체 부모 패널
    [SerializeField] private List<TutorialUIRenderer> tutorialPanels;   // 각 페이지

    [Header("[Navigation Buttons]")]
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button completeBtn;

    [Header("[Variable Text]")]
    [SerializeField] private TextMeshProUGUI lastTutorialText;

    // 현재 UI에서 보여주고 있는 로컬 페이지 인덱스 (0부터 시작)
    private int _currentLocalIndex = 0;

    private TutorialUIRenderData _cachedRenderData;


    private void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>().RegisterUIRender(UIType.TutorialUI, this);

        if (nextBtn != null) nextBtn.onClick.AddListener(HandleNextButtonClick);
        if (previousBtn != null) previousBtn.onClick.AddListener(HandlePreviousButtonClick);
        if (completeBtn != null) completeBtn.onClick.AddListener(HandleCompleteButtonClick);
    }

    private void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.TutorialUI);

        if (nextBtn != null) nextBtn.onClick.RemoveListener(HandleNextButtonClick);
        if (previousBtn != null) previousBtn.onClick.RemoveListener(HandlePreviousButtonClick);
        if (completeBtn != null) completeBtn.onClick.RemoveListener(HandleCompleteButtonClick);
    }

    private void Start()
    {
        var postManager = ServiceLocater.Get<IPostManager>();

        lastTutorialText.text = ServiceLocater.Get<IGameManager>().PlayerName + " " + postManager.Request<int, string>(DataDispatcher.Channel.GetUIText, 9910005);
    }


    public void Render(UIRenderData data)
    {
        _cachedRenderData = data as TutorialUIRenderData;
        if (_cachedRenderData == null) return;

        // 메인 패널 활성화, 다른 UI 입력 막기
        if (mainPanel != null) mainPanel.SetActive(true);

        _currentLocalIndex = 0;

        UpdatePageVisibility();
    }

    // 다음 버튼 클릭 시 처리되는 뷰 로직
    private void HandleNextButtonClick()
    {
        if (_currentLocalIndex < tutorialPanels.Count - 1)
        {
            _currentLocalIndex++;
            UpdatePageVisibility();
            _cachedRenderData?.onGoNextCallback?.Invoke();
        }
    }

    // 이전 버튼 클릭 시 처리되는 뷰 로직
    private void HandlePreviousButtonClick()
    {
        if (_currentLocalIndex > 0)
        {
            _currentLocalIndex--;
            UpdatePageVisibility();
            _cachedRenderData?.onGoBackCallback?.Invoke();
        }
    }

    // 마지막 버튼 클릭 시 처리되는 뷰 로직
    private void HandleCompleteButtonClick()
    {
        CloseTutorialUI();
        _cachedRenderData?.onTutorialCompleteCallback?.Invoke();
    }

    // 현재 인덱스에 맞춰 패널들을 On/Off 하고 버튼의 예외 상태 연출
    private void UpdatePageVisibility()
    {
        if (tutorialPanels == null || tutorialPanels.Count == 0) return;

        int totalCount = tutorialPanels.Count;

        // 현재 인덱스 키고 나머지 끄기
        for (int i = 0; i < tutorialPanels.Count; i++)
        {
            if (tutorialPanels[i] != null)
            {
                tutorialPanels[i].SetActivePanel(i == _currentLocalIndex);
            }
        }

        // 0번 인덱스에선 에서는 이전 버튼을 비활성화
        if (previousBtn != null)
        {
            previousBtn.gameObject.SetActive(_currentLocalIndex > 0);
        }

        bool isLastPage = (_currentLocalIndex == totalCount - 1);

        // 마지막 페이지 아닐 때만 다음 버튼을 활성화하고, 마지막 페이지에서는 숨김
        if (nextBtn != null)
        {
            nextBtn.gameObject.SetActive(!isLastPage);
        }

        // 마지막 페이지에서만 완료 버튼을 활성화
        if (completeBtn != null)
        {
            completeBtn.gameObject.SetActive(isLastPage);
        }
    }

    // 튜토리얼 UI 자체를 끄고 닫기
    private void CloseTutorialUI()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
    }
}
