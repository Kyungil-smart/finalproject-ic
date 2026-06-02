using UnityEngine;

public interface IStateInformation
{
    [Header("이전 프로세스 상태 ID")]
    public int PreviousStateID { get; }

    [Header("현재 프로세스 상태 ID")]
    public int CurrentStateID { get; }

    [Header("다음 프로세스 상태 ID")]
    public int NextStateID { get; }

    [Header("이전 프로세스 상태 Name")]
    public string PreviousStateName { get; }

    [Header("현재 프로세스 상태 Name")]
    public string CurrentStateName { get; } 

    [Header("다음 프로세스 상태 Name")]
    public string NextStateName { get; }


    public void UpdateStateInformation();
}
