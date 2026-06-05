using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Cysharp.Threading.Tasks;


public interface IProcessTaskRunnerExecute
{
    public UniTask Execute();
}

public interface IProcessTaskRunnerEnterExit
{
    public UniTask Enter(ProcessStateSO so);
    public UniTask Exit();
}