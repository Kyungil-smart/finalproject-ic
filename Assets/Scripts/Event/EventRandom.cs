using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 랜덤 이벤트 분리
public class EventRandom
{
    // 이벤트 매니저에서 키값(이벤트 타입으로)으로 랜덤이벤트 발생
    public async UniTask<EventTaskData> GetRandomly(List<EventTaskData> tasks, List<int> runIds)
    {
        int totalEventCount = tasks.Count;
        while (true)
        {
            var index = UnityEngine.Random.Range(0, totalEventCount);
            var task = tasks[index];
            if (runIds.Count < totalEventCount)
            {
                if (!runIds.Contains(task.id))
                    return task;
                await UniTask.Yield();
            }
            return null;
        }
    }

    // 시너지에 따라 필터링된 이벤트분류에서 다시 이벤트 발생시키기
    public async UniTask<EventTaskData> GetStaffRandomly(List<EventTaskData> tasks, List<int> runIds, Synergy synergy)
    {
        // Todo. 기획에서 시너지분류범위가 아직 안나옴
        var filtered = synergy switch
        {
            // Synergy.Good   => tasks.FindAll(),
            // Synergy.Normal => tasks.FindAll(),
            // Synergy.Bad    => tasks.FindAll(),
            _              => tasks
        };
        return await GetRandomly(filtered, runIds);
    }
}
