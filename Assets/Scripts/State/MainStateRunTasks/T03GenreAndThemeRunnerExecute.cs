using Cysharp.Threading.Tasks;
using DataDispatcher;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class T03GenreAndThemeRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    [SerializeField] private GenreThemeTypeDataSO genreThemeTypeDataSO;     // TODO : 추후 Addressable로 변경 예정

    private T03TrendGenreThemeSelectRenderData _t03TrendGenreThemeSelectRenderData;
    private NameTag _selectedGenre;
    private NameTag _selectedTheme;

    private bool _endProcess;
    private bool _conditionGoback;


    // Enter에서 이벤트 -> 장르 / 테마 클릭 시 출력할 데이터 -> 장르 / 테마 선정 진행하기 버튼 클릭 시(가선택) -> 장르 & 테마 선택 완료 순서로
    public async UniTask Execute()
    {
        _endProcess = false;
        await ShowGenreThemeList();
        await UniTask.WaitUntil(() => _endProcess);
    }

    // 장르 / 테마 클릭 시 출력할 데이터 만들기 기능
    private async UniTask ShowGenreThemeList()
    {
        // 시작용 전달
        _waiting = true;    // 단계 끝날 때까지 기다리기
        await UniTask.Yield();  // 다음 프레임까지 대기

        // 본문
        if (_t03TrendGenreThemeSelectRenderData == null)
        {
            // SO에서 장르와 테마 가져오기
            List<GenreThemeRow> genreList = genreThemeTypeDataSO.GetGenreThemeListByType(1);    // 장르(Type = 1)만 모으기
            List<GenreThemeRow> themeList = genreThemeTypeDataSO.GetGenreThemeListByType(2);    // 테마(Type = 2)만 모으기

            _t03TrendGenreThemeSelectRenderData = new T03TrendGenreThemeSelectRenderData();
            _t03TrendGenreThemeSelectRenderData.genres = new List<NameTag>();
            _t03TrendGenreThemeSelectRenderData.themes = new List<NameTag>();

            var postManager = ServiceLocater.Get<IPostManager>();

            // List<NameTag> genre 넣어주기
            foreach (var item in genreList)
            {
                NameTag nameTag;    // 구조체라 new(); 사용 안함
                nameTag.id = item.GT_ID;
                nameTag.name = postManager.Request<int, string>(DataDispatcher.Channel.GetUIText, item.GT_Name_ID);
                nameTag.textId = item.GT_Name_ID;

                _t03TrendGenreThemeSelectRenderData.genres.Add(nameTag);
            }
            // List<NameTag> themes 넣어주기
            foreach (var item in themeList)
            {
                NameTag nameTag;
                nameTag.id = item.GT_ID;
                nameTag.name = postManager.Request<int, string>(DataDispatcher.Channel.GetUIText, item.GT_Name_ID);
                nameTag.textId = item.GT_Name_ID;

                _t03TrendGenreThemeSelectRenderData.themes.Add(nameTag);
            }
            await UniTask.Yield();
            _t03TrendGenreThemeSelectRenderData.onSelectCallback = OnGenreThemeSelectedCallback;
        }
        await UniTask.Yield();

        // 끝용 전달
        Debug.Log($"[T03] _t03TrendGenreThemeSelectRenderData = {_t03TrendGenreThemeSelectRenderData.genres.Count} | {_t03TrendGenreThemeSelectRenderData.themes.Count}");
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.MarketGenreThemeUI, _t03TrendGenreThemeSelectRenderData);
        await WaitProcess();    // _waiting 변수가 false가 될 때까지 코드의 실행을 중단하고 대기
        await CheckGenreTheme();  // 다음 기능 끝날 때까지 대기
    }

    private void OnGenreThemeSelectedCallback((NameTag genre, NameTag theme) selection)
    {
        _waiting = false;
        _selectedGenre = selection.genre;
        _selectedTheme = selection.theme;
    }


    // 장르 / 테마 선정 진행하기 버튼 클릭 시 기능 (가선택)
    public async UniTask CheckGenreTheme()
    {
        _waiting = true;
        T03TrendGenreThemeResultRenderData t03TrendGenreThemeResultRenderData = new T03TrendGenreThemeResultRenderData();
        t03TrendGenreThemeResultRenderData.genre = _selectedGenre;
        t03TrendGenreThemeResultRenderData.theme = _selectedTheme;
        t03TrendGenreThemeResultRenderData.goBackCallback = GoCheckThemeGenreList;
        t03TrendGenreThemeResultRenderData.goNextCallback = GoCheckThemeGenreProcess;

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.MarketGenreThemeUI, t03TrendGenreThemeResultRenderData);
        await WaitProcess();
        if (_conditionGoback) await ShowGenreThemeList();
        else await GenreThemeProcessing();        
    }

    // 게임 장르 & 테마 선정 3: 결과 - 뒤로가기 버튼 클릭 시 체크
    private void GoCheckThemeGenreList()
    {
        _waiting = false;
        _conditionGoback = true;
    }

    // 게임 장르 & 테마 선정 3: 결과 - 확정하기 버튼 클릭 시 체크
    private void GoCheckThemeGenreProcess()
    {
        _waiting = false;
        _conditionGoback = false;
    }

    // 장르 & 테마 선택 완료 기능(실제 확정 이후) -> T01의 RecruitProcessing() 과 CheckHiring() 이 합쳐진 형태
    private async UniTask GenreThemeProcessing()
    {
        _waiting = true;

        // T01 직원 관리와 다르게 확정 된 이후 표시하는 UI없어서 UI 부분은 생략
        // 프로젝트 매니저에 넣어주기
        Debug.Log($"[T03] {_selectedGenre} 장르 선택");
        ServiceLocater.Get<IProjectManager>().Genre = _selectedGenre;
        Debug.Log($"[T03] {_selectedTheme} 테마 선택");
        ServiceLocater.Get<IProjectManager>().Theme = _selectedTheme;

        // 장르 & 테마 선택에 따른 비용 넣어주기 -> T11에서도 사용
        var cal = ServiceLocater.Get<IProjectManager>().CostCalculator; 
        uint cost = cal.CalculateDevCost(_selectedGenre.id, _selectedTheme.id);
        ServiceLocater.Get<IProjectManager>().Cost = cost;
        Debug.Log($"[T03] 개발 비용 : {cost}");

        await UniTask.Yield();
        await GoToNextProcess();    // 선정하기 버튼이 눌리면 CheckGenreTheme() -> GenreThemeProcessing()로 자동 진행되고 프로세스가 끝나도록(선택 확정한 뒤에 다시확인하는 UI는 없음)
        await WaitProcess();
    }

    // 마지막에 다음 프로세스 상태로 가기 위한 기능
    private async UniTask GoToNextProcess()
    {
        ServiceLocater.Get<IGameManager>().UpdateInputProjectNameActive(true);
        _waiting = false;
        _endProcess = true;
    }
}