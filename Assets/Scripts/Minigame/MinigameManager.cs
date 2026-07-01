using UnityEngine;
using UnityEngine.Pool;
using R3;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

[Serializable]
public struct SpawnPosition
{
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
}

public class MinigameManager : Manager, IMinigameManager
{
    [Header("Data Setting")] 
    [SerializeField] [Range(0.1f, 100f)] private float correctionValue;
    
    [Header("Pool Settings")]
    [SerializeField] private GameObject bugPrefab;
    [SerializeField] private Transform spawnArea; // 버그가 생성될 맵 영역

    [Header("Game Design Variables (Inspector)")]
    [SerializeField] private int totalBugCount = 20;       // 총 출현 Bug 개수
    [SerializeField] private int maxConcurrentBugs = 5;    // 동시 출현 Bug 개수
    [SerializeField] private float gamePlayTime = 60f;     // 게임 플레이 시간 지정
    [SerializeField] private SpawnPosition spawnPos;
    [SerializeField] private AudioClip startBgmClip;
    [SerializeField] private AudioClip bugClip;
    [SerializeField] private AudioClip countdownClip;

    // R3 리액티브 프로퍼티 (UI 및 시스템이 구독)
    public ReactiveProperty<int> TotalBugs { get; private set; } = new();
    public ReactiveProperty<int> CatchBugs { get; private set; } = new();
    public ReactiveProperty<float> CurrentTime { get; private set; } = new();
    public ReactiveProperty<bool> IsGameOver { get; private set; } = new();
    public ReactiveProperty<int> CountDown { get; private set; } = new();
    
    // 유니티 내장 오브젝트 풀
    private IObjectPool<GameObject> _bugPool;
    private int _spawnedTotalCount = 0; // 지금까지 생성된 총 버그 수
    private int _currentActiveBugs = 0; // 현재 화면에 활성화된 버그 수
    private List<GameObject> _activeBugs = new();
    private bool _gameStart;
    public bool GameStart
    {
        get => _gameStart;
        set => _gameStart = value;
    }

    private void Awake() => InitObjectPool();

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Register() => ServiceLocater.Register<IMinigameManager>(this);

    protected override void Unregister() => ServiceLocater.Unregister<IMinigameManager>();

    private void Start() => UniTask.Void(async () => { await Init(); });

    private void Update()
    {
        if (IsGameOver.Value) return;

        // R3 프로퍼티 값 갱신 (UI 자동 반영)
        if (_gameStart) CurrentTime.Value -= Time.deltaTime;

        if (CurrentTime.Value <= 0f)
        {
            CurrentTime.Value = 0f;
            EndGame();
        } 
    }
    
    private async UniTask Init()
    {
        // 데이터 초기화
        Debug.Log("Init MinigameManager");
        TotalBugs.Value = totalBugCount;
        CatchBugs.Value = 0;
        CurrentTime.Value = gamePlayTime;
        IsGameOver.Value = false;

        _spawnedTotalCount = 0;
        _currentActiveBugs = 0;
        
        for (int i = 3; i >= 0; i--)
        {
            CountDown.Value = i;
            ServiceLocater.Get<ISoundManager>().PlaySfx(countdownClip);
            await UniTask.Delay(1000);
        }
        
        await UniTask.WaitUntil(() => _gameStart);
        ServiceLocater.Get<ISoundManager>().PlayBgm(startBgmClip);
        // 초기에 동시 출현 개수만큼 버그 스폰 시도
        for (int i = 0; i < maxConcurrentBugs; i++)
        {
            TrySpawnBug();
        }
    }

    private void InitObjectPool()
    {
        _bugPool = new ObjectPool<GameObject>(
            createFunc: CreateBugInstance,
            actionOnGet: OnGetBug,
            actionOnRelease: OnReleaseBug,
            actionOnDestroy: DestroyBugInstance,
            collectionCheck: true,
            defaultCapacity: maxConcurrentBugs,
            maxSize: maxConcurrentBugs
        );
        
    }

    private GameObject CreateBugInstance()
    {
        GameObject bug = Instantiate(bugPrefab, spawnArea);
        // 생성 시 버그 스크립트에 풀 참조를 넘겨주어 스스로 반환할 수 있도록 설계 가능
        bug.SetActive(false);
        return bug;
    }

    private void OnGetBug(GameObject bug)
    {
        bug.transform.position = GetRandomSpawnPosition();
        bug.SetActive(true);
        _activeBugs.Add(bug);
        _currentActiveBugs++;
    }

    private void OnReleaseBug(GameObject bug)
    {
        bug.SetActive(false);
        _activeBugs.Remove(bug);
        _currentActiveBugs--;
    }

    private void AllBugsGoToPool()
    {
        for (int i = _activeBugs.Count - 1; i >= 0; i--)
            _bugPool.Release(_activeBugs[i]);
    }

    private void DestroyBugInstance(GameObject bug)
    {
        Destroy(bug);
    }

    private void TrySpawnBug()
    {
        // 총 출현 개수에 도달하지 않았고, 현재 화면에 동시 출현 제한보다 적을 때만 생성
        if (_spawnedTotalCount < totalBugCount && _currentActiveBugs < maxConcurrentBugs)
        {
            _spawnedTotalCount++;
            _bugPool.Get();
        }
    }

    // Bug(Body 클래스)에서 터치되었을 때 호출할 메서드
    public void OnBugCaught(GameObject bugInstance)
    {
        if (IsGameOver.Value) return;
        ServiceLocater.Get<ISoundManager>().PlaySfx(bugClip);
        // 데이터 업데이트 (R3 전송)
        CatchBugs.Value++;

        // 잡힌 버그는 풀로 반환
        _bugPool.Release(bugInstance);

        // 승리 조건 체크 또는 다음 버그 스폰
        if (CatchBugs.Value >= totalBugCount)
        {
            EndGame();
        }
        else
        {
            TrySpawnBug();
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        // 맵 경계 내의 랜덤 좌표 반환 (가상 좌표 설정)
        float randomX = UnityEngine.Random.Range(spawnPos.minX, spawnPos.maxX);
        float randomY = UnityEngine.Random.Range(spawnPos.minY, spawnPos.maxY);
        return new Vector2(randomX, randomY);
    }

    private void EndGame()
    {
        IsGameOver.Value = true;
        ServiceLocater.Get<ISoundManager>().PauseBgm();
        // 추후 UIManager가 IsGameOver를 Subscribe하여 팝업을 띄우게 됨
        AllBugsGoToPool();
        Time.timeScale = 0;
    }

    public void ApplyResult()
    {
        float result = (CatchBugs.Value / (float)totalBugCount) * correctionValue;
        
        var pm = ServiceLocater.Get<IProjectManager>();
        
        if (pm == null)
        {
            Debug.Log($"[MinigameManager] result = {result}");
            return;
        }
        pm.QAResult = result;
        Time.timeScale = 1;
        SceneManager.LoadScene("ProcessScene");
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 R3 프로퍼티 Dispose
        TotalBugs.Dispose();
        CatchBugs.Dispose();
        CurrentTime.Dispose();
        IsGameOver.Dispose();
    }
}