# Slide Detail UI 

- Slide 형식으로 Detail 정보가 있는 List 형태의 데이터를 UI 로 띄우는 기능

# 현재 상태

- `Instantiate` 와 `Destory` 로 진행 중

## 문제점

- GC 발생
- 데이터량이 많아질 수록 GC 많이 발생
  - Staff 수는 최대 8명으로 제한이 있지만, Project 수는 제한이 없음.


# 해결 방법

- Object Pooling + Windowing 기법
  - 미리 각각 5개 정도의 Slot 을 선정.
  - Slot 은 각 데이터를 Rendering 할 Panel.
  - 5개보다 적을때는 불필요한 Slot 을 Disable
  - 5개보다 많을 경우는 필요한 만큼의 데이터를 가져오는 형식


## 고려사항

### Windowing 

- 필요한 데이터의 Window 만큼 가져오고, 
  현재 보여야 하는 Slot 변화시 새로운 데이터를 앞뒤로 끼워넣어주는 Logic 에 대한 알고리즘 필요
- 가장 첫 데이터나 가장 마지막 데이터의 경우 Slot 의 처음 혹은 끝까지를 보여주고, 더 이상 움직이지 않는 연출 필요.


### 구현 아이디어 

- 현재 보여야 할 Index 의 값을 R3 Property 로 정의 -> 해당 값 변화에 따라 보여줄 값 변화 함수 실행 -> 반응형이라 딱 좋음
- 쉽게 가기 위해 Core 에 해당하는 것과 Slot 에 해당하는 Panel 을 각각 Prefab 으로 만들고, 씬에는 연결만 잘 해놓어야함.
- Core 에서는 어떤 데이터를 보여야하는지에 대한 분기 처리를 잘 해야 할듯.

