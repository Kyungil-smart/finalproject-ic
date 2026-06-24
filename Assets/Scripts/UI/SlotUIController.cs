using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUIController : MonoBehaviour
{
    [SerializeField] private SlotUIButtonRender[] slotButtons;

    [Header("Player Name Input Panel Control")] 
    [SerializeField] private GameObject inputPlayerNamePanel;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button cancelButton; // input name panel 에서 사용
    [SerializeField] private Button confirmButton; // input name panel 에서 사용
    
    [Header("Player Name Confirm Control")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private Button goBackButton; // confirm panel -> input name panel 용
    [SerializeField] private Button goMainSceneButton; // confirm panel -> main scene 전환용

    private ISaveManager _saveManager;
    private ReactiveProperty<string> _playerName = new();
    private readonly CompositeDisposable _disposables = new();

    private void Start()
    {
        _saveManager = ServiceLocater.Get<ISaveManager>();
        for (int i = 0; i < slotButtons.Length; i++)
            slotButtons[i].Render(i, _saveManager.GetMeta(i));   // 빈 슬롯이면 GetMeta=null → Render가 slotOff

        inputPlayerNamePanel.SetActive(false);
        confirmPanel.SetActive(false);
        _playerName.Subscribe(playername =>
        {
            confirmText.text = playername;
        }).AddTo(_disposables);
    }

    private void OnEnable()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int idx = i;   // ★ 클로저 캡처: 루프 변수 직접 쓰면 전부 마지막 값(3)이 됨
            slotButtons[i].GetComponent<Button>().onClick.AddListener(() => OnSlotClicked(idx));
        }

        cancelButton.onClick.AddListener(() => inputPlayerNamePanel.SetActive(false));
        confirmButton.onClick.AddListener(OpenConfirmButton);
        goBackButton.onClick.AddListener(GoBackButton);
        goMainSceneButton.onClick.AddListener(UpdateInputPlayerName);
        CloseLoadingScreen().Forget();
    }

    private void OnDisable()
    {
        foreach (var render in slotButtons)
            render.GetComponent<Button>().onClick.RemoveAllListeners();

        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveListener(OpenConfirmButton);
        goBackButton.onClick.RemoveListener(GoBackButton);
        goMainSceneButton.onClick.RemoveListener(UpdateInputPlayerName);
    }
    
    private void OnSlotClicked(int slot)
    {
        _saveManager.SelectSlot(slot);          // _currentSlot = slot (빈/찬 공통)
        if (_saveManager.IsEmpty(slot))
            OpenInputPlayerName();               // 빈 슬롯 → 새 게임 이름 입력 (D에서 패널 내부 완성)
        else
            GoToNextScene();                     // 찬 슬롯 → MainScene (로드는 ReadyGameData가)
    }

    private void GoToNextScene()
    {
        ServiceLocater.Get<ISceneChanger>().ChangeScene("MainScene");
    }

    private async UniTaskVoid CloseLoadingScreen()
    {
        // ToDo. 임시 코드
        await UniTask.WaitForSeconds(1f);
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.LoadingUI, new LoadingUIRenderData(false));
    }

    private void OpenInputPlayerName()
    {
        _playerName.Value = "";
        inputPlayerNamePanel.SetActive(true);
    }

    private void OpenConfirmButton()
    {
        _playerName.Value = playerNameInputField.text;
        confirmPanel.SetActive(true);
    }

    private void GoBackButton()
    {
        confirmPanel.SetActive(false);
    }

    private void UpdateInputPlayerName()
    {
        ServiceLocater.Get<IGameManager>().SetPlayerName(_playerName.Value);
        GoToNextScene();
    }
}