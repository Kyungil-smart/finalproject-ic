using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IEventTask
{
    void Init(List<string> ids);
    UniTask Execute();
    void Reset();
}
