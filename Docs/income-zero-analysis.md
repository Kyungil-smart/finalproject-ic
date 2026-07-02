# 매출(Income)이 0으로만 나오는 원인 분석

## 결론

매출을 계산하는 유일한 함수 `CalculateIncome()`이 **어디에서도 호출되지 않는다.**
따라서 `ProjectManager.Income`은 항상 초기값 `0`(uint)으로 남고, 출시 결과 UI에도 0원이 표시된다.

---

## 1. 매출 계산 로직 위치

매출을 산출하는 코드는 아래 한 곳뿐이다.

**`Assets/Scripts/Common/ProjectCostCalculate.cs:32`**

```csharp
public void CalculateIncome()
{
    float achieve = ServiceLocater.Get<IQualityManager>().Calculator.CalculateFullAchieve();
    var pm = ServiceLocater.Get<IProjectManager>();

    var income = _incomeRatioDataSO.ratioList.Find(i => achieve >= i.achieveMin && achieve < i.achieveMax);
    if (income == null) return;

    // Todo. 마케팅 미반영 상태의 단순 계산
    pm.Income = (uint)(pm.Cost * income.moneyRatio);   // ← Income은 여기서만 세팅됨
}
```

- 전체 코드베이스 검색 결과 **`CalculateIncome()`의 호출부가 0개** (정의만 존재).
- `pm.Income`에 값을 대입하는 코드도 **오직 이 함수 41행 하나뿐** — 다른 경로의 세팅 없음.

→ 호출이 없으므로 `Income`은 계속 기본값 `0`.

---

## 2. 끊긴 지점 — T12 출시 프로세스

매출을 계산해 결과 화면으로 넘겨야 할 자리는 출시(T12) 절차다.
그러나 매출 계산 단계가 애니메이션만 재생하고 **계산 호출을 하지 않는다.**

**`Assets/Scripts/State/MainStateRunTasks/T12ReleaseRunnerExecute.cs:75`**

```csharp
private async UniTask CalculateRevenue()
{
    _waiting = true;
    // ToDO. Animation 추가 작업 필요.
    var data = new ProgressAnimationRenderData() { ... };   // 진행 애니메이션만
    ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcAnimationUI, data);

    // TODO: 매출 건내주는 기능이 나오면 수정이 필요할 수도 있음   ← 여기가 미완성 (끊긴 고리)

    await WaitProcess();
    await CheckRevenue();   // 계산 없이 곧바로 결과 UI로
}
```

바로 다음 `CheckRevenue()`가 결과 UI를 띄우고, UI는 `Income`을 그대로 읽어 표시한다.

**`Assets/Scripts/UI/Process/Renderers/T12/T12IncomeUIRender.cs:15`**

```csharp
public void Render(T12IncomeUIRenderData data)
{
    var projectManager = ServiceLocater.Get<IProjectManager>();
    incomeValue.text   = $"{projectManager.Income} 원";     // 계산 안 됐으니 0
    projectCostValue.text = $"{projectManager.Cost} 원";
    staffsCostValue.text  = $"{projectManager.StaffsCost} 원";
    earningsValue.text    = $"{projectManager.Earnings} 원";
    ...
}
```

---

## 3. 데이터 흐름 요약

| 단계 | 처리 | 상태 |
|---|---|---|
| T03 | `pm.Cost = CalculateDevCost(genre, theme)` | ✅ Cost 세팅됨 |
| T11 | `MarketingCost` / `MarketingBonus` 저장 | ✅ 저장됨 |
| **T12 `CalculateRevenue()`** | **`CalculateIncome()` 호출** | ❌ **누락 (끊긴 고리)** |
| T12 결과 UI | `Income` 표시 | ⚠️ 항상 0원 |

---

## 4. 조치 방안

`CalculateRevenue()`에서 결과 UI로 넘어가기 전에 매출 계산을 호출한다.

```csharp
// T12ReleaseRunnerExecute.CalculateRevenue() 내부, CheckRevenue() 진입 전
ServiceLocater.Get<IProjectManager>().CostCalculator.CalculateIncome();
```

이렇게 하면 결과 UI 표시 전에 `Income`이 채워진다.

---

## 5. 함께 확인해야 할 연관 문제

매출이 흐르기 시작한 뒤에도 값의 정확도에 영향을 주는 항목들.

### 5-1. `Earnings` uint 언더플로우
**`Assets/Scripts/Project/ProjectManager.cs:115`**

```csharp
public uint Earnings => _projectData.income - _projectData.cost;   // 둘 다 uint
```

- `Income = 0`, `Cost > 0`이면 `0 - cost`가 음수가 아니라 **약 42억(uint 언더플로우)** 으로 표시됨.
- 매출을 고쳐도 `income < cost`인 프로젝트에서는 여전히 발생 → `int` 계산 또는 하한(0) 처리 검토 필요.

### 5-2. `CalculateCost()` 미호출 → 인건비 누락
- 스태프 인건비(`StaffsCost`)를 Cost에 더하는 `ProjectCostCalculate.CalculateCost()`(22행)도 호출되지 않는다.
- T03은 `CalculateDevCost()`만 사용 → **Cost에 인건비가 빠져 있음.**
- 매출식이 `Cost * ratio`이므로 매출 규모에도 영향.

### 5-3. 마케팅 매출 미반영
- `CalculateIncome`은 `MarketingBonus`를 사용하지 않는다(40행 TODO).
- 결과 UI의 `marketBonusValue` / `marketCostValue`도 `Render()`에서 세팅되지 않음("마케팅은 추후에").

---

## 우선순위

1. **(필수) 끊긴 고리 복구** — T12 `CalculateRevenue()`에서 `CalculateIncome()` 호출.
2. 5-1 `Earnings` 언더플로우 방지.
3. 5-2 / 5-3 매출식 정확도(인건비·마케팅 반영)는 이후 설계에서 정리.
