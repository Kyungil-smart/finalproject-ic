using System;
using System.Collections.Generic;
using R3;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main Scene 에서 상시 뜨고 있어야 한다. 이벤트 성으로 UI 를 뛰워주는 것이 아니기 때문에 MonoBehaviour 로 충분히 대처.
/// </summary>
public class MainUIController : MonoBehaviour
{
    [Header("Top UI")]
    [SerializeField] private TextLoader goldTl;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI dateText;
    
    [Header("Process UI")]
    [SerializeField] private TextLoader previousStepTl;
    [SerializeField] private TextMeshProUGUI previousStepNum;
    [SerializeField] private TextLoader currentStepTl;
    [SerializeField] private TextMeshProUGUI currentStepNum;
    [SerializeField] private TextLoader nextStepTl;
    [SerializeField] private TextMeshProUGUI nextStepNum;
    
    [Header("Bottom UI")]
    [SerializeField] private Button lastProjectsButton;
    [SerializeField] private TextLoader lastProjectsTl;
    [SerializeField] private Button goNextProcessButton;
    [SerializeField] private TextLoader goNextProcessTl;
    [SerializeField] private Button staffListButton;
    [SerializeField] private TextLoader staffListTl;

    // R3 구독 해제를 관리하기 위한 디스포저 컨테이너
    private readonly CompositeDisposable _disposables = new();
    private IGameManager _gameManager;   
    
    private void Awake() => Initialize();

    private void OnEnable()
    {
        lastProjectsButton.onClick.AddListener(OnClickViewLastProject);
        goNextProcessButton.onClick.AddListener(OnClickNextProcess);
        staffListButton.onClick.AddListener(OnClickViewStaffList);
    }

    private void Start()
    {
        _gameManager = ServiceLocater.Get<IGameManager>();        
        UpdateGoldUI();
        UpdateDateUI();
        UpdateProcessData();
    }

    private void OnDisable()
    {
        lastProjectsButton.onClick.RemoveListener(OnClickViewLastProject);
        goNextProcessButton.onClick.RemoveListener(OnClickNextProcess);
        staffListButton.onClick.RemoveListener(OnClickViewStaffList);
    }
    
    private void Initialize()
    {
        lastProjectsTl.TextId = 0;
        goNextProcessTl.TextId = 0;
        staffListTl.TextId = 0;
        previousStepNum.text = StepString(0);  
        currentStepNum.text = StepString(0);
        nextStepNum.text = StepString(0);
    }

    private string StepString(int stepNum) => $"{stepNum:2D}/12";
    
    // ------------ R3 Property Bind 할 것들 -------------

    private void UpdateGoldUI()
    {
        _gameManager.Money
            .Subscribe(gold =>
            {
                goldText.text = $"{gold}";
            }).AddTo(_disposables);
    }

    private void UpdateDateUI()
    {
        _gameManager.Date
            .DistinctUntilChanged()
            .Subscribe(date =>
            {
                dateText.text = date.ToString("yyyy-MM-dd");            
            }).AddTo(_disposables);
    }

    private void UpdateProcessData()
    {
        // ToDo. Main State Machine 에게 State 관련 데이터 정보 요청 진행.
    }
    
    // ------------ 버튼 핸들러들 -------------
    private void OnClickNextProcess()
    {
        // ToDo. MainStateMachine 에게 다음 시작 Trigger 전송
        SceneManager.LoadScene("ProcessScene");
    }

    private void OnClickViewLastProject()
    {
        // ToDo. Project 는 Game Manager 에서 관리 할 것. 따라서 해당 매니저에게 데이터 요청 진행.
        var dataList = _gameManager.Projects;
        LastProjectRenderData data = new();
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.LastProjectUI, data);
    }
    
    private void OnClickViewStaffList()
    {
        // ToDo. Staff 로부터 Staff 데이터를 받을 수 있도록 요청하기.
        StaffListRenderData data = new();
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.StaffUI, data);
    }
}