public enum UIType
{
    ProcessSimpleUI,  // 프로세스에서 간단한 팝업
    ProcessListUI,    // 프로세스에서 간단한 리스트 (미구현)
    ProcessListToggleUI,  // 프로세스에서 간단한 토글 포함된 리스트 (미구현)
    EventUI,  // 이벤트용 UI
    LastProjectUI,  // 지난 Project 확인 UI (미구현)
    StaffCandidateUI,  // 직원 고용 UI (고용된 직원 + 신규 채용 보일용) - T01
    MarketGenreThemeUI,  // 시장조사, 장르/테마 선택 UI  - T02, T03
    ProductionUI,  // T04-09
    LoadingUI,  // 로딩 UI 
    SlideUI,  // 다량의 데이터를 Slide 형으로 보기 위한 UI 
    TagSelectUI,  // Main UI 에서 Staff Level Up 씬 발생시
    ReleaseUI,  // 출시(릴리즈) 용 UI - T12
    ProcAnimationUI,   // T01-12 내 애니메이션이 필요한 내용
    MarketingUI,    // 마케팅 용 UI - T11
}