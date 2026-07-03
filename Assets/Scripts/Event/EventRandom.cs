using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 랜덤 이벤트 분리
public class EventRandom
{
    private static readonly Dictionary<GameDevProcName, int> StageCode = new()
    {
        { GameDevProcName.MarketResearch,            20 },
        { GameDevProcName.GenreAndTheme,             30 },
        { GameDevProcName.ConceptPreProduction,      41 },
        { GameDevProcName.ArtPreProduction,          42 },
        { GameDevProcName.DevelopmentPreProduction,  43 },
        { GameDevProcName.ConceptFullProduction,     51 },
        { GameDevProcName.ArtFullProduction,         52 },
        { GameDevProcName.DevelopmentFullProduction, 53 },
        { GameDevProcName.Marketing,                 60 },
    };
    
    // 이벤트 매니저에서 키값(이벤트 타입으로)으로 랜덤이벤트 발생
    public UniTask<EventTaskData> GetRandomly(List<EventTaskData> tasks, List<int> runIds)
    {
        var proc = ServiceLocater.Get<IGameManager>().ProcName.CurrentValue;
        
        // 매핑에 없는 단계(HR/QA/Release)는 뽑을 이벤트가 없음
        if (!StageCode.TryGetValue(proc, out var code))
            return UniTask.FromResult<EventTaskData>(null);

        // 미실행 + 현재 단계(categoryId 앞 2자리 == 단계코드) 이벤트만 후보로
        var pool = tasks.FindAll(t => !runIds.Contains(t.id) && t.categoryId / 10 == code);
        if (pool.Count == 0)
            return UniTask.FromResult<EventTaskData>(null);

        var task = pool[Random.Range(0, pool.Count)];
        return UniTask.FromResult(task);
    }

    // 시너지에 따라 필터링된 이벤트분류에서 다시 이벤트 발생시키기
    public async UniTask<EventTaskData> GetStaffRandomly(List<EventTaskData> tasks, List<int> runIds, Synergy synergy)
    {
        var filtered = synergy switch
        {
            Synergy.Good   => tasks.FindAll(t => t.categoryId % 10 == 1),
            Synergy.Normal => tasks.FindAll(t => t.categoryId % 10 == 3),
            Synergy.Bad    => tasks.FindAll(t => t.categoryId % 10 == 2),
            _              => tasks
        };
        return await GetRandomly(filtered, runIds);
    }
}
