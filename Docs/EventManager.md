# Event Manager

![](./Imgs/EventManagerConcept.png)

## 컨셉

- Event Task 에 대한 실행 및 관리를 담당

## 구성 요소

- Event Data SO: Event 에서 사용해야 할 데이터에 대한 정의
- Event Context: Event 에서 사용해야 할 Asset 참조에 대한 정의
- Uni-event Pure Class: Event 에서 진행할 내용에 대한 정의

## 주의 사항

- Event 가 실행 완료되면 `게임 개발 프로세스`에 알려줄 필요가 있음.
