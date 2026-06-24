using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUIController : MonoBehaviour
{
    [SerializeField] private SlotUIButtonRender[] slotButtons;

    [Header("Player Name Input Panel Control")] [SerializeField]
    private GameObject inputPlayerNamePanel;

    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private Button confirmButton; // input name panel 에서 사용
    [SerializeField] private Button goBackButton; // confirm panel -> input name panel 용
    [SerializeField] private Button goMainSceneButton; // confirm panel -> main scene 전환용

    private ISaveManager _saveManager;
    private ReactiveProperty<string> _playerName;
    private readonly CompositeDisposable _disposables = new();

    private void Start()
    {
        _saveManager = ServiceLocater.Get<ISaveManager>();
        for (int i = 0; i < slotButtons.Length; i++)
        {
            // SaveManager 와 slotButtons 를 Mapping 하기.
            // slotButtons 의 render 에 slot 데이터 입히기.
        }

        inputPlayerNamePanel.SetActive(false);
        confirmPanel.SetActive(false);
        _playerName.Subscribe(playername =>
        {
            confirmText.text = playername;
        }).AddTo(_disposables);
    }

    private void OnEnable()
    {
        foreach (SlotUIButtonRender render in slotButtons)
        {
            // 빈 슬롯이냐 아니냐에 따라 Binding 할 함수를 변경하거나 함수에서 분기 처리가 필요할듯.
            // 일단 아래 Binding 은 변경 필요함.
            var button = render.GetComponent<Button>();
            button.onClick.AddListener(GoToNextScene);
        }

        goMainSceneButton.onClick.AddListener(GoToNextScene);
        CloseLoadingScreen().Forget();
    }

    private void OnDisable()
    {
        foreach (SlotUIButtonRender render in slotButtons)
        {
            // 빈 슬롯이냐 아니냐에 따라 Binding 할 함수를 변경하거나 함수에서 분기 처리가 필요할듯.
            // 일단 아래 Binding 은 변경 필요함.
            var button = render.GetComponent<Button>();
            button.onClick.RemoveListener(GoToNextScene);
        }

        goMainSceneButton.onClick.RemoveListener(GoToNextScene);
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

    private void OpenInputPlayerName() => inputPlayerNamePanel.SetActive(true);

    private void OpenConfirmButton()
    {
        _playerName.Value = playerName.text;
        confirmPanel.SetActive(true);
    }

    private void GoBackButton()
    {
        confirmPanel.SetActive(false);
    }

    private void UpdateInputPlayerName()
    {
        ServiceLocater.Get<IGameManager>().SetPlayerName(playerName.text);
        confirmPanel.SetActive(true);
    }
}