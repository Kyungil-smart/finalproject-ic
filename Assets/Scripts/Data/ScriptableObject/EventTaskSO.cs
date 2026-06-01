using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MackySoft.SerializeReferenceExtensions;

[CreateAssetMenu(fileName = "EventTaskSO", menuName = "Scriptable Objects/EventTaskSO")]
public class EventTaskSO : ScriptableObject
{
    [SerializeField]
    [SerializeReference]
    [SubclassSelector]
    private List<IUniEvent> _uniEvents = new();

    private List<int> _remainingIndexes = new();

    public int Count => _uniEvents.Count;

    public void Reset()
    {
        _remainingIndexes.Clear();
        for (int i = 0; i < _uniEvents.Count; i++) _remainingIndexes.Add(i);
    }

    public async UniTask Execute(CancellationToken token)
    {
        if (_remainingIndexes.Count == 0) return;

        var randomIndex = Random.Range(0, _remainingIndexes.Count);
        var index = _remainingIndexes[randomIndex];
        _remainingIndexes.RemoveAt(randomIndex);

        if (_uniEvents[index] == null) return;
        token.ThrowIfCancellationRequested();
        await _uniEvents[index].Execute(token);
    }
}