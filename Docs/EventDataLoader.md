## EventDataLoader
구글시트에서 이벤트 데이터를 로드하고 관리

### 주요 기능
- `LoadEvent(GSheetManager)` : 시트에서 데이터 로드 후 타입별 분류, EventManager에 등록
- `GetEventData(int)` : ID로 EventData 조회

### ID 범위
| 범위 | 타입 |
|------|------|
| 31001~32000 | Staff |
| 32001~33000 | Linkage |
| 33001~34000 | Regular |
| 34001~35000 | Reward |