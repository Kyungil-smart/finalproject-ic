using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EventRandom
{
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

    public async UniTask<EventTaskData> GetStaffRandomly(List<EventTaskData> tasks, List<int> runIds, Synergy synergy)
    {
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
