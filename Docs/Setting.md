# 사운드 및 환경설정 시스템

## 개요
- 게임 내 BGM/SFX 볼륨 조절 및 환경설정 UI 구현
- 볼륨 설정은 PlayerPrefs에 저장하여 앱 재시작 후에도 유지

## 구성 요소

- **SoundManager** : BGM/SFX 재생 및 볼륨 관리. ServiceLocater에 등록하여 전역 접근.
- **SettingUIController** : 설정 패널 열기/닫기 및 슬라이더 연동. 설정 패널 열 때 `Time.timeScale = 0`으로 게임 일시정지.
- **ButtonSoundBinder** : 하위 Button 컴포넌트에 자동으로 SoundTrigger를 붙여 버튼 효과음 자동 연결.
- **SoundTrigger** : 버튼 클릭 시 SFX 재생. `NoAutoSound` 태그가 붙은 버튼은 제외.

## 동작 방식

1. SoundManager가 초기화 시 PlayerPrefs에서 볼륨값 로드
2. 설정 버튼 클릭 → 설정 패널 오픈 + 게임 일시정지
3. 사운드 버튼 클릭 → 현재 볼륨값을 슬라이더에 반영 후 사운드 패널 오픈
4. 슬라이더 조작 → SoundManager에 즉시 반영 + PlayerPrefs 저장
5. 계속하기 버튼 → 설정 패널 닫기 + 게임 재개

## 볼륨 계산

- BGM 실제 볼륨 = `BGM볼륨 × 마스터볼륨`
- SFX 실제 볼륨 = `SFX볼륨 × 마스터볼륨`

## 주의 사항

- ProcessScene에서는 처음으로 돌아가기 버튼 비활성화
- `NoAutoSound` 태그 버튼은 ButtonSoundBinder 자동 연결 제외