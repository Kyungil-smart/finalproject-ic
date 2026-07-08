# 🎮 <인디 게임 개발 인디 ?!>

> 신생 게임 개발사를 운영하며 직원을 채용하고 게임을 출시해 성장시키는 경영 시뮬레이션

![타이틀](/Docs/Imgs/TitleImg.png)

| 항목 | 내용 |
|------|------|
| **장르** | 경영 시뮬레이션
| **플랫폼** | Android
| **엔진** | Unity 6 (6000.3.9f1) · URP |
| **언어** | C# |
| **개발 기간** | 2026.05.06 ~ 2026.07.16 |
| **인원** | 총 10명(개발 3인 / 기획 8인) |

🔗 **플레이 영상 / 빌드**: (준비중)

---

## 📖 게임 소개

플레이어는 게임 개발사의 대표가 되어 **직원을 채용**하고, **장르·테마를 선정**한 뒤
**기획 → 개발 → QA → 마케팅 → 출시**의 제작 파이프라인을 거쳐 게임을 완성합니다.
출시한 게임은 품질에 따라 유저 리뷰와 수익으로 이어지며, 그 자본으로 회사를 키워 나갑니다.

---

## ✨ 주요 기능

- **게임 제작 파이프라인** — 인사 → 시장조사 → 장르/테마 → 제작 → QA → 마케팅 → 출시까지 12단계 공정을 상태 머신으로 구현
- **직원(스태프) 시스템** — 채용·배치·이동, 능력치 기반 데이터 생성 및 AI 행동
- **품질 / 마케팅 / 리뷰 시스템** — 직원 배치와 공정 결과로 게임 품질을 산출, 마케팅과 유저 리뷰·수익으로 환산
- **랜덤 이벤트 시스템** — 정기·연계·직원 이벤트를 라우팅하여 진행에 변수를 부여
- **미니게임** — 버그 잡기 미니게임 (QA 연계)
- **보상 시스템**
- **세이브 / 로드** — 런타임 데이터 직렬화 저장·복원

---

## 🛠 기술 스택

| 분류 | 사용 기술 |
|------|-----------|
| **엔진 / 렌더링** | Unity 6, Universal Render Pipeline (URP), 2D Feature, Pixel Perfect |
| **비동기 / 반응형** | [UniTask](https://github.com/Cysharp/UniTask), [R3](https://github.com/Cysharp/R3) |
| **애니메이션** | DOTween, Unity Timeline |
| **데이터 로딩** | Addressables, Google Sheets(CSV) 런타임 임포트, ScriptableObject |
| **입력** | Input System |
| **직렬화 / 기타** | Newtonsoft.Json, SerializeReference Extensions, NuGetForUnity |

---

## 🧩 아키텍처 하이라이트

- **ServiceLocator + 인터페이스 결합** — 매니저/순수 클래스를 타입(인터페이스) 기반으로 등록·조회하여 시스템 간 의존성을 느슨하게 결합
- **상태 머신 기반 게임 진행** — `MainProcessStateMachine` + 공정별 `TaskRunner`(T01~T12)로 제작 흐름을 단계별 모듈로 분리
- **데이터 드리븐 설계** — 기획 수치(이벤트·품질·마케팅 등)를 Google Sheets에서 CSV로 받아오고 ScriptableObject로 관리해, **코드 수정 없이 밸런싱 가능**
- **게임 내 이벤트 파이프라인** — `EventManager.OccurEvent(EventType)`로 정기·직원 이벤트를 발생시키고, `EventRandom`이 시너지·중복방지(runIds)로 후보를 선별해 `IEventTaskRunner`가 실행. 선택지 효과는 `EventRouter`가 target 키 → 보상 핸들러로 디스패치
- **UI 렌더링 라우팅 정규화** — 모든 UI가 `IUIRender.Render(UIRenderData)`를 구현하고 `UIType`로 `UIRouter`에 자가 등록, `NavigateTo(UIType, data)` 단일 경로로 캔버스 전환·렌더를 통일
- **비동기 준비상태 게이팅** — 각 매니저가 `IReadyStatus`로 데이터 로딩 완료 여부를 표준화해, 부트스트랩이 준비 완료를 대기한 뒤 다음 단계로 진행
- **세이브 / 로드 Capture–Restore** — 매니저별 `CaptureSaveData / RestoreSaveData(DTO)` 규약으로 런타임 상태를 3슬롯 JSON에 직렬화·복원
- **부트스트랩 기반 초기화** — `InGameBootstrap`에서 세이브 로딩·렌더링·매니저 등록을 UniTask 비동기로 순차 구성
- **R3 + UniTask** — 리액티브 프로퍼티로 데이터 변경을 UI에 전파, 네트워크/로딩은 전부 비동기 처리

---

## 📂 프로젝트 구조
```
Assets/
├── Scripts/
│ ├── Bootstrap/ # 게임 시작 초기화 (InGameBootstrap)
│ ├── Project/ # 프로젝트(게임 제작) 데이터·매니저
│ ├── State/ # 메인/서브 상태 머신, 공정별 TaskRunner(T01~T12)
│ ├── Event/ # 이벤트 매니저·라우터·러너 (정기/연계/직원)
│ ├── Staff/ # 직원 생성·배치·AI·이동
│ ├── Minigame/ # 버그 잡기 미니게임
│ ├── Common/ # ServiceLocator, GSheet, 품질/마케팅/리뷰/세이브 등 공용
│ ├── Data/ # ScriptableObject 정의, Enum
│ ├── UI/ # 메인/이벤트/프로세스 UI
│ └── MainSceneControllers/ 메인씬 내 Staff 연출
├── Scenes/ # Title / Main / Process / Minigame / Slot
└── SOAssets/ # ScriptableObject 데이터 에셋
Docs/ # 시스템 설계 문서 모음
```

### 주요 씬
| 씬 | 역할 |
|----|------|
| `TitleScene` | 타이틀 / 진입 |
| `MainScene` | 사무실 메인 (직원·진행 관리) |
| `ProcessScene` | 게임 제작 공정 진행 |
| `MinigameScene` | 미니게임 |
| `SlotScene` | 세이브된 데이터 로드 |

---

## 📚 설계 문서

세부 시스템 설계는 [`Docs/`](./Docs) 폴더에 정리되어 있습니다.

- [폴더 가이드](./Docs/FolderGuide.md) · [컨벤션 가이드](./Docs/ConventionGuide.md)
- 이벤트 시스템 — [EventManager](./Docs/EventManager.md) · [EventRouter](./Docs/EventRouter.md) · [EventUI](./Docs/EventUI.md)
- [품질 계산](./Docs/QualityCalculate.md) · [보상 이벤트](./Docs/RewardEvent.md) · [스태프 빌더](./Docs/StaffBuilder.md)
- [ServiceLocator](./Docs/ServiceLocater.md) · [SO 상태 머신](./Docs/ScriptableObjectStateMachine.md) · [R3 & UniTask 가이드](./Docs/R3_UniTask_Guide.md)

---

## 🚀 실행 방법

1. **Unity 6 (6000.3.9f1)** 이상으로 프로젝트를 엽니다.
2. 패키지 의존성은 `Packages/manifest.json` 기준으로 자동 복원됩니다.
3. Firebase / Google Sheets 키 등 별도 설정 관련은 별도 문의 바랍니다.
4. `Assets/Scenes/TitleScene` 을 열고 실행합니다.

---

## 👤 제작

🔸 (팀명 / 개인 / 담당 역할 등을 적으세요)