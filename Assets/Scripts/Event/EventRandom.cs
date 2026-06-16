using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 랜덤 이벤트 분리
public class EventRandom
{
    // 이벤트 매니저에서 키값(이벤트 타입으로)으로 랜덤이벤트 발생
    public UniTask<EventTaskData> GetRandomly(List<EventTaskData> tasks, List<int> runIds)
    {
        // 아직 실행 안 된 이벤트만 후보로
        var pool = tasks.FindAll(t => !runIds.Contains(t.id));
        if (pool.Count == 0)
            return UniTask.FromResult<EventTaskData>(null);  // 뽑을 게 없음

        var task = pool[Random.Range(0, pool.Count)];
        return UniTask.FromResult(task);
    }

    // 시너지에 따라 필터링된 이벤트분류에서 다시 이벤트 발생시키기
    public async UniTask<EventTaskData> GetStaffRandomly(List<EventTaskData> tasks, List<int> runIds, Synergy synergy)
    {
        // Todo. 기획에서 시너지분류범위가 아직 안나옴
        var filtered = synergy switch
        {
            Synergy.Good   => tasks.FindAll(t => t.id == 310001),
            Synergy.Normal => tasks.FindAll(t => t.id >= 310003 && t.id <= 310004),
            Synergy.Bad    => tasks.FindAll(t => t.id == 310002),
            _              => tasks
        };
        return await GetRandomly(filtered, runIds);
    }
}
