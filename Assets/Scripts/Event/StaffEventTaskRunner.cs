using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StaffEventTaskRunner : IEventTaskRunner
{
    private EventTaskData _taskData;

    public void SetEventData(EventTaskData taskData) => _taskData = taskData;

    public async UniTask Execute()
    {
        throw new System.NotImplementedException();
    }
}

public enum Synergy {Good, Normal, Bad}