# Docs Index

프로젝트 문서를 성격별로 분류한 인덱스입니다.

## 📘 기술문서
> 구현된 시스템·기능의 구조와 동작을 상세히 기술한 문서

| 파일명 | 내용 |
|--------|------|
| [BootPipeline.md](./BootPipeline.md) | 씬 진입~플레이 시작까지 준비 계층. 인프라 등록→매니저 자가등록→데이터 로딩을 비동기 조립 |
| [MainProcessStateMachine.md](./MainProcessStateMachine.md) | 게임개발 12단계 공정을 SO 링크드리스트로 정의·순환시키는 코어 상태머신 |
| [MainUIOrchestration.md](./MainUIOrchestration.md) | 모든 UI를 단일 라우팅 규약으로 통일하고 상시 UI를 중앙 오케스트레이션 |
| [ProjectSystem.md](./ProjectSystem.md) | 게임 한 편(ProjectData) 도메인. 12공정 거치며 장르·퀄리티·등급·매출 데이터 누적 |
| [StaffSystem.md](./StaffSystem.md) | 직원 채용(가챠)→고용→성장→배치→저장/복원→해고 전 생애주기 관리 |
| [SaveLoadSystem.md](./SaveLoadSystem.md) | 여러 매니저 상태를 DTO로 추출해 3슬롯 JSON 저장/복원하는 영속성 계층 |
| [QualityCalculate.md](./QualityCalculate.md) | 품질 산출 시스템. 직원 유효스탯·파트별/합산 퀄리티·트렌드·시너지 계산 |
| [Setting.md](./Setting.md) | BGM/SFX 볼륨·환경설정 UI. SoundManager 등 구성, PlayerPrefs 저장 |
| [EventDataLoader.md](./EventDataLoader.md) | 구글시트에서 이벤트 데이터 로드·타입별 분류·ID 범위 관리 |
| [이벤트시스템_기술문서.md](./이벤트시스템_기술문서.md) | 이벤트 시스템 기술 아키텍처(데이터 로딩→실행→보상) 전체 흐름 |

## 📐 설계문서
> 시스템 컨셉·아이디어·구조 설계와 계획을 담은 문서

| 파일명 | 내용 |
|--------|------|
| [ServiceLocater.md](./ServiceLocater.md) | 싱글톤 매니저를 인터페이스 기반으로 등록·조회하는 DI 허브 |
| [DataDisaptcher.md](./DataDisaptcher.md) | 오브젝트 간 결합 제거용 단방향 데이터 송수신(Sender/Receiver) 모듈 |
| [ScriptableObjectStateMachine.md](./ScriptableObjectStateMachine.md) | State를 SO로 정의하고 Enum 매핑으로 실행 내용을 연결하는 상태머신 컨셉 |
| [StaffBuilder.md](./StaffBuilder.md) | 공정처럼 Step by Step으로 직원 데이터·클래스를 결합하는 빌더 컨셉 |
| [EventManager.md](./EventManager.md) | 이벤트 Task 실행 및 관리. Event Data SO / Context 구성 |
| [EventRouter.md](./EventRouter.md) | 이벤트 보상 라우터. IEventTarget 매핑 딕셔너리로 타겟별 보상 처리 |
| [EventUI.md](./EventUI.md) | 이벤트 UI 컨셉. 고정 포맷에 데이터만 바꿔 띄우는 형태 |
| [RewardEvent.md](./RewardEvent.md) | 프리프로덕션 종료 시 달성 지표값으로 보상 이벤트 발생 |
| [StaffEvent.md](./StaffEvent.md) | 투입 직원 DISC 값 합산으로 조합 등급 판단→이벤트 발생 |
| [UIControlConcept.md](./UIControlConcept.md) | UI 자체 제어(Canvas 켜고 끄기 등) 컨셉과 기능별 Canvas 분리 배경 |
| [ProcessUIPlan.md](./ProcessUIPlan.md) | 공정별 UI 목록과 데모/CBT 마일스톤·우선순위 계획 |
| [R3_UniTask_Guide.md](./R3_UniTask_Guide.md) | UniTask & R3 도입 기준과 프로젝트 시스템 구조 명세 |
| [SlideDetailUI.md](./SlideDetailUI.md) | 슬라이드형 리스트 상세 UI 컨셉과 GC 문제·개선 방향 |
| [MermaidDoc.md](./MermaidDoc.md) | 주요 클래스 구조를 Mermaid 다이어그램으로 표현 |

## 🔍 리뷰
> 코드/구조 분석 및 리뷰 문서

| 파일명 | 내용 |
|--------|------|
| [EventManagerReview.md](./EventManagerReview.md) | develop 브랜치 기준 EventManager 구조 파악·리뷰 노트 |
| [income-zero-analysis.md](./income-zero-analysis.md) | 매출이 항상 0으로 나오는 원인 분석(CalculateIncome 미호출) |

## 🗂 기타
> 가이드·컨벤션·회고·예제

| 파일명 | 내용 |
|--------|------|
| [ConventionGuide.md](./ConventionGuide.md) | 브랜치 생성·커밋 등 팀 코딩 컨벤션 규칙 |
| [FolderGuide.md](./FolderGuide.md) | Docs 및 Assets 내 폴더 구조·사용 규칙 |
| [FirebaseAnalytics.md](./FirebaseAnalytics.md) | Firebase Analytics 개념과 지표 정의, 기획/개발 역할 분담 |
| [UIRouterExample.md](./UIRouterExample.md) | UI Router에 등록되는 Event/Process UI 사용 예제 코드 |
| [WithAI.md](./WithAI.md) | 개발 과정에서 AI(Claude)를 설계·문제분석 협업 파트너로 활용한 회고 |