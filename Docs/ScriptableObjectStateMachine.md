# Scriptable Object + State Machine

## 기본 컨셉

![ConceptImg](./Imgs/ScriptableObjectStateMachine.png)

- State 를 Scripatble Object 로 정의 한다.
- State Machine 은 State 를 관리한다.
- State 에서 실행할 내용에 대해서는 별도의 Class 로 관리한다
- 해당 Class 와 Enum 을 이용한 Mapping 관계를 이용하여 해당 State 에서 실행할 내용에 대해 실행하게 된다.

## 주의 사항

- State 종료에 대한 Handling
  - 해당 State 에서 발생해야 할 모든 이벤트가 완료가 되어야 다음 이벤트로 이동 할 수 있다.
  - State Machine 에서는 이러한 State 의 State (?!)를 파악한 후 State 를 변경해야 한다.
- Event 종료 후 State 변화에 대한 Handling
  - 위와 비슷한 내용이다. 단, Trigger 가 Event 에 있는 만큼 다른 내용 파악을 더 잘해야 한다.

## 사용 예시
### SO 목록
- MainStateSO 12종
  - `StaffManagingStateSO`: 1번 `직원 관리` 상태 데이터를 담은 SO
  - `MarketResearchStateSO`: 2번 `시장 조사` 상태 데이터를 담은 SO
  - `ConceptStateSO`: 3번 `장르 및 테마 선정` 상태 데이터를 담은 SO
  - `DesignPreProductionStateSO`: 4번 `기획 프리 프로덕션` 상태 데이터를 담은 SO
  - `ArtPreProductionStateSO`: 5번 `아트 프리 프로덕션` 상태 데이터를 담은 SO
  - `DevPreProductionStateSO`: 6번 `개발 프리 프로덕션` 상태 데이터를 담은 SO
  - `DesignFullProductionStateSO`: 7번 `기획 풀 프로덕션` 상태 데이터를 담은 SO
  - `ArtFullProductionStateSO`: 8번 `아트 풀 프로덕션` 상태 데이터를 담은 SO
  - `DevFullProductionStateSO`: 9번 `개발 풀 프로덕션` 상태 데이터를 담은 SO
  - `QAStateSO`: 10번 `QA` 상태 데이터를 담은 SO
  - `MarketingStateSO`: 11번 `마케팅` 상태 데이터를 담은 SO
  - `ReleaseStateSO`: 12번 `출시` 상태 데이터를 담은 SO
- SubStateSO 9종
  - `StaffHireSubStateSO`: 1번 `직원 관리` 상태의 서브 상태의 데이터를 담은 SO
  - `MarketSearchSubStateSO`: 2번 `시장 조사` 상태의 서브 상태의 데이터를 담은 SO
  - `ConceptConfirmSubStateSO`: 3번 `장르 및 테마 선정` 상태의 서브 상태의 데이터를 담은 SO
  - `StaffAssignmentSubStateSO`: 4번 `기획 프리 프로덕션` ~ 9번 `개발 풀 프로덕션` 상태의 서브 상태의 데이터를 담은 SO
  - `QAMiniGameSubStateSO`: 10번 `QA` 상태의 서브 상태의 데이터를 담은 SO
  - `MarketingSelectionSubStateSO`: 11번 `마케팅` 상태의 서브 상태의 데이터를 담은 SO
  - `ReleaseReviewGamersSubStateSO`: 12번 `출시` 상태의 `게이머 / 평론가 반응 확인` 서브 상태의 데이터를 담은 SO
  - `ReleaseRevenureSubStateSO`: 12번 `출시` 상태의 `매출 발생` 서브 상태의 데이터를 담은 SO 
  - `ReleaseAwardsSubStateSO`: 12번 `출시` 상태의 `어워즈 선정` 서브 상태의 데이터를 담은 SO


### SO를 받는 클래스 목록
- `MainState.cs` 1종 : MainStateSO 12종을 계속 교체해가며 받는 클래스
- SubStates 9종
  - `StaffHireSubState`: `StaffHireSubStateSO` 를 받는 클래스
  - `MarketSearchSubState`: `MarketSearchSubStateSO`를 받는 클래스
  - `ConceptConfirmSubState`: `ConceptConfirmSubStateSO`를 받는 클래스
  - `StaffAssignmentSubState`: `StaffAssignmentSubStateSO`를 받는 클래스
  - `QAMiniGameSubState`: `QAMiniGameSubStateSO`를 받는 클래스
  - `MarketingSelectionSubState`: `MarketingSelectionSubStateSO`를 받는 클래스
  - `ReleaseReviewGamersSubState`: `ReleaseReviewGamersSubStateSO`를 받는 클래스
  - `ReleaseRevenureSubState`: `ReleaseRevenureSubStateSO`를 받는 클래스
  - `ReleaseAwardsSubState`: `ReleaseAwardsSubStateSO`를 받는 클래스

### 관련 상태 머신
- `MainProcessStateMachine.cs`: 메인 프로세스 상태 머신을 관리하는 클래스
- `SubProcessStateMachine.cs`: 서브 프로세스 상태 머신을 관리하는 클래스