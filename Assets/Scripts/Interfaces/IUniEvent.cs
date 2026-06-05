using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IUniEvent
{
    UniTask Execute(CancellationToken cancellationToken);
}
