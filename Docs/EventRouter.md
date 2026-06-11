# EventResultRouter

![alt text](EventRouterConcept.png)

## 구성요소
- Dictionary<string, IEventTarget>(예시) : target과 구현체를 매핑해두는 딕셔너리. 새로운 보상 타겟이 생기면 추가로 구현하고 여기에 추가하면 됨.
- IEventTarget : 보상 적용 구현체들이 따르는 인터페이스, Apply메소드 하나만 있으면 됨.
- 타겟 구현체 : 보상을 실질적으로 적용시키는 부분.

## 흐름
1. 버튼 선택
2. 이벤트 라우터 호출
3. 버튼의 타겟을 키값으로 딕셔너리 조회
4. 인터페이스 구현체 실행
5. 데이터 반영

## 주의사항
- 새로운 보상 추가시 딕셔너리 등록을 해야함