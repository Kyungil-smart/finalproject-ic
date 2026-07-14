# 미니게임 시스템 (Minigame System)

> QA 공정에서 진입하는 "버그 잡기" 미니게임. 제한 시간 안에 화면에 출몰하는
> 버그(Bug)를 터치해 잡고, 그 성과를 **QA 결과값으로 환산해 프로젝트에 반영**한다.
> **R3(리액티브) + UniTask(비동기 흐름) + Object Pool(생성 비용 절감)** 을 축으로 설계했다.
>
> 관련 문서: [`ServiceLocater.md`](./ServiceLocater.md) · [`ProjectSystem.md`](./ProjectSystem.md) · [`R3_UniTask_Guide.md`](./R3_UniTask_Guide.md) · [`Setting.md`](./Setting.md)

---

## 1. 개요

미니게임은 "게임 개발 시뮬레이션" 안에서 **QA 단계의 성과를 플레이어의 손맛으로 결정**하는 서브 게임이다.
카운트다운 후 시작하면 버그가 맵 위를 돌아다니고, 플레이어는 제한 시간(기본 60초) 안에
목표 수(기본 20마리)를 클릭/터치로 잡는다. 결과는 잡은 비율에 보정치를 곱해 `ProjectManager.QAResult`로
전달되고, 씬은 다시 `ProcessScene`으로 복귀한다.

핵심 설계 원칙은 세 가지다.
- **상태는 데이터로, UI는 구독으로** — 게임 상태(시간·잡은 수·종료 여부 등)를 R3 `ReactiveProperty`로 두고, UI는 그 값을 구독만 한다. 매니저는 UI를 직접 모른다.
- **생성/파괴 대신 재활용** — 버그 오브젝트는 `ObjectPool`로 돌려쓴다.
- **시간 흐름은 UniTask로** — 카운트다운, 시작 대기, 배경 애니메이션 등 순차 흐름은 async 루프로 표현한다.

---

## 2. 구성 요소

| 스크립트 | 역할 |
|----------|------|
| `IMinigameManager` | 미니게임의 공개 계약(인터페이스). R3 프로퍼티들과 `OnBugCaught`, `ApplyResult` 노출 |
| `MinigameManager` | 코어. 오브젝트 풀 관리, 스폰 제어, 시간/점수 상태 관리, 결과 산출·씬 전환 |
| `MinigameUIManager` | R3 프로퍼티 구독 → 텍스트/팝업/카운트다운 UI 갱신. 매니저를 역참조하지 않고 인터페이스로만 접근 |
| `MapController` | 배경 스프라이트를 `interval` 주기로 순환시키는 루프 애니메이션 |
| `BugBody` | 버그 1마리의 외형(랜덤 스프라이트·크기) + 터치 입력 감지(`IPointerClickHandler`) |
| `BugMovement` | 버그의 랜덤 이동(DOTween). 도착 시 재귀적으로 다음 이동 |

---

## 3. 적용 기술 & 디자인 패턴

### 3.1 R3 — 리액티브 상태 관리 (핵심)

게임 상태 전체를 `ReactiveProperty`로 정의하고, 인터페이스로 노출한다.

```csharp
public interface IMinigameManager
{
    public ReactiveProperty<int>   TotalBugs   { get; }
    public ReactiveProperty<int>   CatchBugs   { get; }
    public ReactiveProperty<float> CurrentTime { get; }
    public ReactiveProperty<bool>  IsGameOver  { get; }
    public ReactiveProperty<int>   CountDown   { get; }
    ...
}
```

**의도**: 매니저는 값만 바꾸고(`CatchBugs.Value++`), UI는 그 변화를 구독해 스스로 갱신한다.
매니저 → UI 방향의 직접 참조가 사라져 **단방향 데이터 흐름**이 성립한다.

UI 측 구독 예시 — 게임 종료는 `Where`로 "true가 된 순간"만 통과시킨다:

```csharp
_minigameManager.IsGameOver
    .Where(isOver => isOver)      // false는 무시, true일 때만
    .Subscribe(_ => ShowGameOverPopup())
    .AddTo(_disposables);
```

시간 값 하나(`CurrentTime`)를 구독해 초/센티초 두 텍스트로 분리 렌더하는 것도 리액티브 방식의 이점이다.

> **메모리 관리**: 모든 구독은 `CompositeDisposable`(`_disposables`)에 `AddTo`로 묶고, `OnDestroy`에서 일괄 `Dispose`한다. 매니저 쪽 `ReactiveProperty`들도 `OnDestroy`에서 `Dispose`하여 누수를 차단한다.

### 3.2 Object Pool — 버그 재활용

Unity 내장 `UnityEngine.Pool.ObjectPool<GameObject>`을 사용한다. 잦은 `Instantiate/Destroy`로 인한
GC·프레임 튐을 막는 것이 목적이다.

```csharp
_bugPool = new ObjectPool<GameObject>(
    createFunc:       CreateBugInstance,   // 없을 때만 실제 생성 (비활성으로)
    actionOnGet:      OnGetBug,            // 꺼낼 때: 랜덤 위치 배치 + 활성화 + 카운트 증가
    actionOnRelease:  OnReleaseBug,        // 반납: 비활성화 + 카운트 감소
    actionOnDestroy:  DestroyBugInstance,
    collectionCheck:  true,                // 중복 반납 방지
    defaultCapacity:  maxConcurrentBugs,
    maxSize:          maxConcurrentBugs
);
```

- **동시 출현 제한**: `maxConcurrentBugs`(기본 5)를 풀 크기로 삼아, 화면에 동시에 존재하는 버그 수를 물리적으로 제한한다.
- **총량 제한**: `_spawnedTotalCount < totalBugCount` 조건으로 게임당 총 출현 수를 통제한다.
- **자기 반납 구조**: 버그가 잡히면(`OnBugCaught`) 매니저가 `_bugPool.Release(bugInstance)`로 반납하고 곧바로 `TrySpawnBug()`로 다음 마리를 꺼낸다 → 항상 일정 밀도를 유지.
- 게임 종료 시 `AllBugsGoToPool()`로 남은 활성 버그를 역순 순회하며 전부 반납한다.

> **연계 포인트**: 버그의 랜덤성(스프라이트·크기·이동)은 풀에서 **꺼내질 때마다** `OnEnable`에서 재설정된다(`BugBody`, `BugMovement`). 재활용 오브젝트를 "새 것처럼" 보이게 하는 표준 풀링 패턴이다.

### 3.3 UniTask — 비동기 흐름 제어

시간 축으로 진행되는 절차를 코루틴 대신 async로 표현한다.

- **카운트다운**: `for` 루프 + `UniTask.Delay(1000)` + 카운트다운 SFX
- **시작 게이트**: `await UniTask.WaitUntil(() => _gameStart)` — UI의 "시작!" 연출이 끝나 `GameStart=true`가 될 때까지 스폰을 보류
- **배경 애니메이션**: `MapController`가 `while(_isLoop)` + `WaitForSeconds(interval)`로 스프라이트를 순환. 게임 시작 전에는 `continue`로 대기
- **의존성 대기**: `await UniTask.WaitUntil(() => ServiceLocater.Get<IMinigameManager>() != null)` — 매니저 등록 순서에 의존하지 않도록 방어

### 3.4 ServiceLocator — 의존성 접근

`MinigameManager`는 `OnEnable/OnDisable`에서 `IMinigameManager`로 자가 등록/해제한다.
UI·버그·맵 등 나머지는 **구체 클래스가 아니라 인터페이스로** 매니저에 접근한다.

```csharp
protected override void Register()   => ServiceLocater.Register<IMinigameManager>(this);
protected override void Unregister() => ServiceLocater.Unregister<IMinigameManager>();
```

사운드(`ISoundManager`), 결과 반영(`IProjectManager`)도 모두 ServiceLocator로 가져온다 → 씬 간 결합도 최소화.

### 3.5 DOTween — 연출

- **버그 이동(`BugMovement`)**: 랜덤 방향·거리로 목적지를 잡고 `Mathf.Clamp`로 맵 경계에 가둔 뒤 `DOMove`. `OnComplete`에서 다시 `StartRandomMove()`를 호출하는 **재귀 트윈 루프**로 끊김 없이 배회.
- **카운트다운 펀치(`MinigameUIManager`)**: `DOScale`로 커졌다 작아지는 시퀀스. `SetUpdate(true)`로 `timeScale=0`에서도 동작, `SetLink`로 오브젝트 파괴 시 자동 정리.

> **트윈 안전 처리**: 이동/펀치 모두 실행 전 `Kill()`(또는 `DOKill()`)로 기존 트윈을 정리하고 스케일을 초기화한다. 풀 재활용·연타 상황에서 트윈이 누적/중복되는 것을 막는 방어 코드다. `OnDisable`(풀 반납 시점)에서도 `_moveTween?.Kill()`을 반드시 호출.

### 3.6 입력 — 인터페이스 기반 터치/클릭

`BugBody`가 `IPointerClickHandler`를 구현해 **마우스 클릭과 모바일 터치를 동일 경로**로 처리한다.
게임오버 상태면 입력을 무시하고, 유효하면 자기 자신(`gameObject`)을 매니저에 넘겨 풀 반납까지 위임한다.

```csharp
public void OnPointerClick(PointerEventData eventData)
{
    if (_minigameManager == null || _minigameManager.IsGameOver.Value) return;
    _minigameManager.OnBugCaught(gameObject);
}
```

---

## 4. 전체 흐름

```
씬 진입
  └ MinigameManager.Awake        → 오브젝트 풀 초기화
  └ MinigameManager.Start(Init)  → 상태 초기화 → 카운트다운(3→0, SFX) 
                                   → WaitUntil(GameStart) → BGM 재생 → 초기 버그 스폰
  └ MinigameUIManager.Start      → R3 프로퍼티 구독, 카운트다운 팝업 표시
        │
   [플레이 루프]
  └ Update: CurrentTime -= dt     → (R3) UI 시간 자동 갱신
  └ 버그 터치(BugBody)            → OnBugCaught → SFX + CatchBugs++ + 풀 반납 + 다음 스폰
        │
   [종료 조건]  CurrentTime ≤ 0  또는  CatchBugs ≥ totalBugCount
  └ EndGame: IsGameOver=true → (R3) 게임오버 팝업 / BGM 정지 / 남은 버그 반납 / timeScale=0
        │
   [결과 반영]  확인 버튼
  └ ApplyResult: QAResult = (잡은수 / 총수) × correctionValue
                → ProjectManager에 전달 → timeScale=1 → ProcessScene 로드
```

---

## 5. 튜닝 파라미터 (인스펙터 노출)

| 변수 | 기본값 | 의미 |
|------|:---:|------|
| `totalBugCount` | 20 | 게임당 총 출현 버그 수(=목표) |
| `maxConcurrentBugs` | 5 | 동시 출현 상한(=풀 크기) |
| `gamePlayTime` | 60 | 제한 시간(초) |
| `correctionValue` | 0.1~100 | QA 결과 환산 보정치 |
| `spawnPos` | — | 스폰 랜덤 좌표 범위(min/max X·Y) |
| `minSize`/`maxSize` (BugBody) | 0.1 / 1.0 | 버그 랜덤 크기 범위 |
| `minSpeed`/`maxSpeed` (BugMovement) | 2 / 5 | 이동 속도 범위 |
| `min/maxBoundary` (BugMovement) | — | 이동 가능 맵 경계 |

---

## 6. 결과의 게임 내 의미

미니게임 성과는 독립 점수가 아니라 **QA 공정의 산출물**이다.

```csharp
float result = (CatchBugs.Value / (float)totalBugCount) * correctionValue;
pm.QAResult = result;   // ProjectManager로 전달 → 이후 품질/등급 계산에 반영
```

즉, 잘 잡을수록 QA 결과가 좋아지고, 이는 [`QualityCalculate`](./QualityCalculate.md)·[`ProjectSystem`](./ProjectSystem.md)의 품질/등급 파이프라인으로 이어진다.