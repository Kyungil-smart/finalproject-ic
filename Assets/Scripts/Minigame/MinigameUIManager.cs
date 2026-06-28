using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using TMPro; // UI 텍스트 컴포넌트 (TextMeshPro 사용 권장)
using R3;
using UnityEngine.UI;

public class MinigameUIManager : MonoBehaviour
{
    [Header("UI Text Elements")]
    [SerializeField] private TextMeshProUGUI secText;
    [SerializeField] private TextMeshProUGUI msecText;
    [SerializeField] private TextMeshProUGUI totalBugsText;
    [SerializeField] private TextMeshProUGUI catchBugsText;

    [Header("UI Popup Elements")]
    [SerializeField] private GameObject gameOverPopup;
    [SerializeField] private TextMeshProUGUI catchBugsPopupText;
    [SerializeField] private Button confirmBtn;
    
    [Header("CountDown Elements")]
    [SerializeField] private GameObject countDownPopup;
    [SerializeField] private TextMeshProUGUI countDownText;
    
    // R3 구독 해제를 관리하기 위한 디스포저 컨테이너
    private readonly CompositeDisposable _disposables = new();

    private IMinigameManager _minigameManager;
    
    private void Start()
    {
        _minigameManager =  ServiceLocater.Get<IMinigameManager>();
        // 게임 시작 시 팝업 비활성화
        if (gameOverPopup != null)
        {
            gameOverPopup.SetActive(false);
        }

        UniTask.Void(async () =>
        {
            countDownPopup.SetActive(true);
            await UniTask.Yield();
            BindReactiveProperties();
        });
        
    }

    private void OnEnable() => confirmBtn.onClick.AddListener(OnClickConfirmBtn);
    private void OnDisable() => confirmBtn.onClick.RemoveListener(OnClickConfirmBtn);

    private void BindReactiveProperties()
    {
        // 1. 게임 플레이 시간 변화 적용
        _minigameManager.CurrentTime
            .Subscribe(t =>
            {
                int wholeSec = (int)t;                   // 초
                int centi    = (int)((t - wholeSec) * 100); // 센티초 0~99
                secText.text = $"{wholeSec:D2}.";
                msecText.text = $"{centi:D2}";
            })
            .AddTo(_disposables);

        // 2. 남은 버그 개수 변화 적용
        _minigameManager.TotalBugs
            .Subscribe(count =>
            {
                totalBugsText.text = $"{count:D2}";
            })
            .AddTo(_disposables);

        // 3. 잡은 버그 개수 변화 적용
        _minigameManager.CatchBugs
            .Subscribe(count =>
            {
                catchBugsText.text = $"{count:D2}";
                catchBugsPopupText.text = $"{count:D2}";
            })
            .AddTo(_disposables);

        // 4. 게임 종료 PopUp 제어
        _minigameManager.IsGameOver
            .Where(isOver => isOver == true) // true가 되었을 때만 통과
            .Subscribe(_ =>
            {
                ShowGameOverPopup();
            })
            .AddTo(_disposables);
        
        // 5. 카운트 다운 
        _minigameManager.CountDown
            .Subscribe(countDown =>
            {
                if (countDown <= 0)
                {
                    UniTask.Void(async () =>
                    {
                        countDownText.text = "시작!";
                        PlayCountPunch();
                        await UniTask.Delay(1000);
                        countDownPopup.SetActive(false);
                        ServiceLocater.Get<IMinigameManager>().GameStart = true;
                    });
                }
                else
                {
                    countDownText.text = countDown.ToString(); 
                    PlayCountPunch();
                }
            })
            .AddTo(_disposables);
    }

    private void ShowGameOverPopup()
    {
        if (gameOverPopup != null)
        {
            gameOverPopup.SetActive(true);
        }
    }

    private void PlayCountPunch()
    {
        UniTask.Void(async () =>
        {
            await UniTask.Yield();
            
            Transform t = countDownText.transform;

            t.DOKill(); // 진행 중인 트윈 정리(연타/중복 방지)
            t.localScale = Vector3.one; // 시작 크기 초기화(중단됐을 때 누적 방지)

            DOTween.Sequence()
                .Append(t.DOScale(1.2f, 0.25f).SetEase(Ease.OutQuad)) // 0.25초 커짐
                .Append(t.DOScale(1.0f, 0.25f).SetEase(Ease.InQuad)) // 0.25초 작아짐
                .SetUpdate(true) // timeScale=0 이어도 동작(아래 설명)
                .SetLink(countDownText.gameObject); // 오브젝트 파괴 시 트윈 자동 정리
        });
    }

    private void OnClickConfirmBtn() => _minigameManager.ApplyResult();
    
    private void OnDestroy()
    {
        // 씬 전환이나 오브젝트 파괴 시 구독을 모두 해제하여 메모리 누수 차단
        _disposables.Dispose();
    }
}
