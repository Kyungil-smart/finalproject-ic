using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DataDispatcher;
using R3;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;
using Channel = DataDispatcher.Channel;

/// <summary>
/// Main Scene 에서 상시 뜨고 있어야 한다. 이벤트 성으로 UI 를 뛰워주는 것이 아니기 때문에 MonoBehaviour 로 충분히 대처.
/// </summary>
public class MainUIController : MonoBehaviour, IMainUIReadyable
{
    [Header("Top UI")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI dateText;
    
    [Header("Process UI")]
    [SerializeField] private GameObject previousStepSession;
    [SerializeField] private TextLoader previousStepTl;
    [SerializeField] private TextLoader currentStepTl;
    [SerializeField] private GameObject nextStepSession;
    [SerializeField] private TextLoader nextStepTl;
    
    [Header("Bottom UI")]
    [SerializeField] private Button lastProjectsButton;
    [SerializeField] private TextLoader lastProjectsTl;
    [SerializeField] private Button goNextProcessButton;
    [SerializeField] private TextLoader goNextProcessTl;
    [SerializeField] private Button staffListButton;
    [SerializeField] private TextLoader staffListTl;

    [Header("Staff Slot UI")] 
    [SerializeField] private Button[] staffSlots;
    
    [Header("Input Project Name UI")]
    [SerializeField] private GameObject inputProjectPanel;
    [SerializeField] private TMP_InputField projectNameInputField;
    [SerializeField] private Button inputProjectConfirmBtn;
    [SerializeField] private GameObject warningMessagePanel;
    [SerializeField] private TextMeshProUGUI warningMessageText;
    [SerializeField][Range(1f, 3f)] private float popUpInterval; 

    private bool _isReady = false;
    public bool IsReady { get => _isReady; }
    
    // R3 구독 해제를 관리하기 위한 디스포저 컨테이너
    private readonly CompositeDisposable _disposables = new();
    private IGameManager _gameManager;   
    private IMainStateMachine _stateMachine;
    private IPostManager _postManager;
    
    private void Awake() => Initialize();

    private void OnEnable()
    {
        lastProjectsButton.onClick.AddListener(OnClickViewLastProject);
        goNextProcessButton.onClick.AddListener(OnClickNextProcess);
        staffListButton.onClick.AddListener(OnClickViewStaffList);
        inputProjectConfirmBtn.onClick.AddListener(() => ConfirmProjectName());
        ServiceLocater.Register<IMainUIReadyable>(this);
        foreach (var slotBtn in staffSlots)
            slotBtn.onClick.AddListener(UnlockSlot);
        staffSlots[0].gameObject.SetActive(false);
        staffSlots[1].gameObject.SetActive(false);
    }

    private void Start()
    {
        _gameManager = ServiceLocater.Get<IGameManager>();      
        _stateMachine = ServiceLocater.Get<IMainStateMachine>();
        _postManager = ServiceLocater.Get<IPostManager>();
        var data = _postManager.Request<bool, StateViewData>(Channel.ProcessUIUpdate, true);
        UpdateProcessData(data);
        UpdateGoldUI();
        UpdateDateUI();
        // ServiceLocater.Get<IStaffRegister>().SetSlotPos(staffSlots);
        if (ServiceLocater.Get<IGameManager>().InputProjectNameActive) 
            OpenInputProjectNamePanel();
        _isReady = true;
        CloseLoadingScreen().Forget();
    }

    private void OnDisable()
    {
        ServiceLocater.Unregister<IMainUIReadyable>(this);
        lastProjectsButton.onClick.RemoveListener(OnClickViewLastProject);
        goNextProcessButton.onClick.RemoveListener(OnClickNextProcess);
        staffListButton.onClick.RemoveListener(OnClickViewStaffList);
        inputProjectConfirmBtn.onClick.RemoveAllListeners();
        foreach (var slotBtn in staffSlots)
            slotBtn.onClick.RemoveListener(UnlockSlot);
    }
    
    private void Initialize()
    {
        _isReady = false;
        lastProjectsTl.TextId = -1;
        goNextProcessTl.TextId = -1;
        staffListTl.TextId = -1;
        LoadingSavedData();
    }
    
    private string StepString(int stepNum) => $"{stepNum:D2}/12";

    /// <summary>
    /// Save Data 를 Loading 한 후에 UI 에서 초기화 작업 해줘야 할 것들 진행하기
    /// </summary>
    private void LoadingSavedData()
    {
        
    }
    
    // ------------ R3 Property Bind 할 것들 -------------

    private void UpdateGoldUI()
    {
        _gameManager.Money
            .Subscribe(gold =>
            {
                goldText.text = gold.ToString("N0");;
            }).AddTo(_disposables);
    }

    private void UpdateDateUI()
    {
        _gameManager.Date
            .DistinctUntilChanged()
            .Subscribe(date =>
            {
                dateText.text = date.ToString("yyyy") + "년";            
            }).AddTo(_disposables);
    }

    private void UpdateProcessData(StateViewData stateView)
    {
        Debug.Log("[MainUIController:UpdateProcessData] Get Data");
        if (stateView.prev.id <= 0)
        {
            previousStepTl.TextId = 0;
            previousStepSession.SetActive(false);
        }
        else
        {
            previousStepTl.TextId = stateView.prev.textId;
            previousStepSession.SetActive(true);
        }
        currentStepTl.TextId = stateView.current.textId;
        if (stateView.next.id <= 0)
        {
            nextStepTl.TextId = 0;
            nextStepSession.SetActive(false);
        }
        else
        {
            nextStepTl.TextId = stateView.next.textId;
            nextStepSession.SetActive(true);
        }  
    }
    
    // ------------ 버튼 핸들러들 -------------
    private void OnClickNextProcess()
    {
        _stateMachine.SetCurrentMainState(_gameManager.ProcName.CurrentValue);
        _stateMachine.Run();
        ServiceLocater.Get<ISceneChanger>().ChangeScene("ProcessScene"); 
    }

    private void OnClickViewLastProject()
    {
        LastProjectRenderData data = new();
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.SlideUI, data);
    }
    
    private void OnClickViewStaffList()
    {
        StaffDetailRenderData data = new()
        {
            staffDataList = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList()
        };
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.SlideUI, data);
    }
    
    private async UniTaskVoid CloseLoadingScreen()
    {   // ToDo. 임시 코드
        await UniTask.WaitForSeconds(1f);
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.LoadingUI, new LoadingUIRenderData(false));
        // ServiceLocater.Get<IUIRouter>().CloseCurrentCanvas();
    }

    private void UnlockSlot()
    {
        (bool result, int nextSlotIndex) = ServiceLocater.Get<IStaffRegister>().UpgradeSlot();
        if (result)
        {
            staffSlots[nextSlotIndex - 1].gameObject.SetActive(false);
            if (nextSlotIndex < staffSlots.Length)
                staffSlots[nextSlotIndex].interactable = true;
        }
    }

    private void OpenInputProjectNamePanel()
    {
        inputProjectPanel.SetActive(true);
    }

    private async UniTask ConfirmProjectName()
    {   // ToDO. Text 데이터 외부에서 받도록 준비하기.
        var emptyWarning = "최소 1글자 이상의 이름을 입력해주세요.";
        var lengthWarning = "20자 이내로 입력해 주시기 바랍니다.";
        var specificWordWarning = "띄어쓰기는 불가하며, 특수문자는 `-_` 만 사용 가능합니다. \n특수문자로 시작할 수는 없습니다.";
        string pattern = @"^[a-zA-Z0-9가-힣]([a-zA-Z0-9가-힣_-])*$";
        
        var name = projectNameInputField.text;
        if (name.Length <= 0)
        {
            await OpenWarningMessagePanel(emptyWarning);
        }
        else if (name.Length > 20)
        {
            await OpenWarningMessagePanel(lengthWarning);
        }
        else if (!Regex.IsMatch(name, pattern))
        {
            await OpenWarningMessagePanel(specificWordWarning);
        }
        else
        {
            ServiceLocater.Get<IProjectManager>().SetProjectName(name);
            ServiceLocater.Get<IGameManager>().UpdateInputProjectNameActive(false);
            inputProjectPanel.SetActive(false);    
        }
    }
    
    private async UniTask OpenWarningMessagePanel(string warningMessage)
    {
        warningMessageText.text = warningMessage;
        warningMessagePanel.SetActive(true);
        await UniTask.WaitForSeconds(popUpInterval);
        warningMessagePanel.SetActive(false);
    }
}