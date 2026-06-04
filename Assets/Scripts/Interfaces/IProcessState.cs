using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Cysharp.Threading.Tasks;


public interface IProcessState
{
    [Header("이 상태의 SO")]
    public ProcessStateSO CurrentStateDataSO { get; }   // 현재 상태

    // [Header("상태 종료 여부")]
    // private bool _isFinished;    // 상태가 끝났는지 여부를 나타내는 프로퍼티

    public event Action<IProcessState> OnStateFinished;


    public void Enter();
    public UniTask Execute();
    public void Exit();

}
