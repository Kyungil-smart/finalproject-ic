using System;
using System.Collections.Generic;
using R3;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

// ToDO. UI 개발을 위한 Dummy 내용. 실물 구현시 삭제 예정
public interface IDummyGameData
{
    public ReactiveProperty<int> Gold { get; }
    public ReactiveProperty<DateTime> Date { get; }
    public List<string> GetLastProjects();
}

// ToDO. UI 개발을 위한 Dummy 내용. 실물 구현시 삭제 예정
public class DummyGameData : Manager, IDummyGameData
{
    public ReactiveProperty<int> Gold { get; } = new (0);
    public ReactiveProperty<DateTime> Date { get; } = new ();
    
    protected override void Register()
    {
        ServiceLocater.Register<IDummyGameData>(this);
    }

    protected override void Unregister()
    {
        ServiceLocater.Unregister<IDummyGameData>(this);
    }

    public List<string> GetLastProjects()
    {
        var data = new List<string>();
        data.Add("JSON_STRING?");
        return data;
    }
}


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
    private IDummyGameData _dummyGameData; // 추후 진짜 게임 데이터로 변경

    
    private void Awake() => Initialize();

    private void OnEnable()
    {
        lastProjectsButton.onClick.AddListener(OnClickViewLastProject);
        goNextProcessButton.onClick.AddListener(OnClickNextProcess);
        staffListButton.onClick.AddListener(OnClickViewStaffList);
    }

    private void Start()
    {
        _dummyGameData = ServiceLocater.Get<IDummyGameData>();
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
        goldTl.TextId = 0;
        lastProjectsTl.TextId = 0;
        goNextProcessTl.TextId = 0;
        staffListTl.TextId = 0;
    }
    
    // ------------ R3 Property Bind 할 것들 -------------

    private void UpdateGoldUI()
    {
        _dummyGameData.Gold
            .Subscribe(gold =>
            {
                goldText.text = $"{gold}";
            }).AddTo(_disposables);
    }

    private void UpdateDateUI()
    {
        _dummyGameData.Date
            .DistinctUntilChanged()
            .Subscribe(date =>
            {
                dateText.text = date.ToString("yyyy-MM-dd");            
            }).AddTo(_disposables);
    }

    private void UpdateProcessData()
    {
        // ToDo. Main State Machine 에게 State 관련 데이터 정보 요청 진행.
        // ToDo. 해당 데이터는 R3-ReactProperty 로 관리 요청 하기?
    }
    
    // ------------ 버튼 핸들러들 -------------
    private void OnClickNextProcess()
    {
        
    }

    private void OnClickViewLastProject()
    {
        // ToDo. Project 는 Game Manager 에서 관리 할 것. 따라서 해당 매니저에게 데이터 요청 진행.
        var dataList = _dummyGameData.GetLastProjects();
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