using Cysharp.Threading.Tasks;
using DataDispatcher;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// T02 시장 조사
/// </summary>
public class T02MarketResearchRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    [SerializeField] private GenreThemeTypeDataSO genreThemeTypeDataSO;     // TODO: 추후 Addressable로 변경 예정

    private T02TrendGenreThemeRenderData _t02TrendGenreThemeRenderData;
    NameTag trendGenre;
    NameTag trendTheme;

    private bool _endProcess;
    private bool _conditionGoback;

    // 시장 조사 시작(Enter, 이벤트 호출) -> 트렌드 탐색 -> 트렌드 장르와 테마 확인 -> 시장 조사 종료(Exit)
    public async UniTask Execute()
    {
        _endProcess = false;

        await MarketResearchProcessing(); // Enter로 들어가면 자동으로 애니메이션 재생

        await UniTask.WaitUntil(() => _endProcess);
    }

    private async UniTask MarketResearchProcessing()
    {
        _waiting = true;

        // ToDO. Animation 추가 작업 필요.

        var data = new ProgressAnimationRenderData()
        {
            staticImage = null,
            progressTexts = new() { "스토어 인가 차트 분석", "키워드 검색량 추적", "수명 주기 평가", "매니아/대중성 조율" },
            callback = GoProcess,
        };
        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.ProcAnimationUI, data);

        DoMarketResearch();

        await WaitProcess();
        await CheckTrendGenreTheme();
    }

    // 자동으로 탐색하기
    private void DoMarketResearch()
    {
        List<GenreThemeRow> genreList = genreThemeTypeDataSO.GetGenreThemeListByType(1);    // 장르(Type = 1)만 모으기
        List<GenreThemeRow> themeList = genreThemeTypeDataSO.GetGenreThemeListByType(2);    // 테마(Type = 2)만 모으기

        int ranGenreIndex = Random.Range(0, genreList.Count);
        int ranThemeIndex = Random.Range(0, themeList.Count);

        var postManager = ServiceLocater.Get<IPostManager>();

        // 장르랑 테마 네임테그로 넣어주기
        trendGenre.id = genreList[ranGenreIndex].GT_ID;
        trendGenre.name = postManager.Request<int, string>(DataDispatcher.Channel.GetUIText, genreList[ranGenreIndex].GT_Name_ID);
        trendGenre.textId = genreList[ranGenreIndex].GT_Name_ID;

        trendTheme.id = themeList[ranGenreIndex].GT_ID;
        trendTheme.name = postManager.Request<int, string>(DataDispatcher.Channel.GetUIText, themeList[ranGenreIndex].GT_Name_ID);
        trendTheme.textId = themeList[ranGenreIndex].GT_Name_ID;

        // 트렌드 장르 및 테마 IProjectManager의 해당 함수에 넣어주기
        ServiceLocater.Get<IProjectManager>().TrendGenre = trendGenre;
        ServiceLocater.Get<IProjectManager>().TrendTheme = trendTheme;

        Debug.Log($"[T02MarketResearchRunnerExecute] 트랜드 장르 : {ServiceLocater.Get<IProjectManager>().TrendGenre} | 트랜드 테마 : {ServiceLocater.Get<IProjectManager>().TrendTheme}");
    }

    private void GoProcess()
    {
        _waiting = false;
    }

    // 결과 확인
    private async UniTask CheckTrendGenreTheme()
    {
        _waiting = true;

        T02TrendGenreThemeRenderData _t02TrendGenreThemeRenderData = new T02TrendGenreThemeRenderData() 
        {
            genre = trendGenre,
            theme = trendTheme,
            confirmCallback = GoToNextProcess,
        };

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.MarketGenreThemeUI, _t02TrendGenreThemeRenderData);

        await UniTask.Yield();
        await WaitProcess();
    }

    private void GoToNextProcess()
    {
        _waiting = false;
        _endProcess = true;
    }
}