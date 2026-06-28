using UnityEngine;
using UnityEngine.EventSystems;

public class BugBody : MonoBehaviour, IPointerClickHandler
{
    [Header("Size Settings")]
    [SerializeField] private float minSize = 0.1f;
    [SerializeField] private float maxSize = 1.0f;
    [SerializeField] private Sprite[] bugImgs;
    
    private IMinigameManager _minigameManager;
    
    private SpriteRenderer _spriteRenderer;
    
    private void Awake() => _spriteRenderer = GetComponent<SpriteRenderer>();
    
    private void Start() => _minigameManager = ServiceLocater.Get<IMinigameManager>();
    
    private void OnEnable()
    {
        // 오브젝트 풀에서 활성화될 때마다 무작위 색상으로 초기화
        _spriteRenderer.sprite = bugImgs[Random.Range(0, bugImgs.Length)];
        // 오브젝트 풀에서 활성화될 때마다 무작위 크기로 초기화
        SetRandomSize();
    }

    /// <summary>
    /// 버그의 외형 크기와 Collider 크기를 무작위로 조절하는 메서드
    /// </summary>
    private void SetRandomSize()
    {
        float randomScale = Random.Range(minSize, maxSize);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);
    }

    /// <summary>
    /// 모바일 터치 및 마우스 클릭을 감지하는 이벤트 인터페이스 구현
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 게임오버 상태이거나 이미 비활성화 중이라면 터치 무시
        if (_minigameManager == null || _minigameManager.IsGameOver.Value) return;
        // GameManager에 잡힌 버그 알림 및 오브젝트 자신을 넘겨주어 풀링 반환 처리
        _minigameManager.OnBugCaught(gameObject);
    }
}
