using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IEventTaskRunner
{
    public void SetEventData(EventTaskData taskData);
    public UniTask Execute();
}
