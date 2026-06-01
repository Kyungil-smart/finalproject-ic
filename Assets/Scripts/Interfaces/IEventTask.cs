using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IEventTask
{
    void Init(List<int> ids);
    UniTask Execute();
    void Reset();
}
