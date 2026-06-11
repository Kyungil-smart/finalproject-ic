using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시장 조사
/// </summary>
public class T02MarketResearchRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    [SerializeField] private GenreThemeTypeDataSO genreThemeTypeDataSO;     // TODO: 추후 Addressable로 변경 예정

    // 시장 조사 시작(Enter, 이벤트 호출) -> 트렌드 탐색 -> 트렌드 장르와 테마 확인 -> 시장 조사 종료(Exit)
    public async UniTask Execute()
    {
        SelectMarketResearch(); // 애니메이션 나타내기 이전에 탐색은 완료하기
        await StartAnimation(); // 애니메이션 실행하고 끝날때까지 기다리기
    }


    private void SelectMarketResearch()
    {
        // TODO: 결과 데이터 받아서, 정리가 필요하면 정리하고 미리 출력하는 곳에 데이터 보내주기(출력하는 곳은 비활성화)
        // TODO: 년도, 이미지 데이터는 어떻게 넣지?

        List<GenreThemeRow> genreList = genreThemeTypeDataSO.GetGenreThemeListByType(1);    // 장르(Type = 1)만 모으기
        List<GenreThemeRow> themeList = genreThemeTypeDataSO.GetGenreThemeListByType(2);    // 테마(Type = 2)만 모으기

        int ranGenreIndex = Random.Range(0, genreList.Count);
        int ranThemeIndex = Random.Range(0, themeList.Count);

        var projectManager = ServiceLocater.Get<IProjectManager>();
        // TODO: 기획 리뷰 듣고 진행하기
        // TODO: 트렌드 장르 및 테마 IProjectManager의 해당 함수에 넣어주기


    }


    private async UniTask StartAnimation()
    {
        // TODO: T01HumanResourceRunnerExecute 에서 애니메이션 부분 만들어지면 활용하여 넣기 + 애니메이션 끝나면 출력하는 곳 활성화하여 결과 보여주기
    }
}