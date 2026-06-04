using System;
using UnityEngine;
using System.Collections;



public class MainState : MonoBehaviour
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태


    public void Enter(ProcessStateSO currentStateSO)
    {
        CurrentStateDataSO = currentStateSO;
        Debug.Log($"[MainStateNew] : {CurrentStateDataSO.StateName} 상태 진입");
    }


    public void Exit()
    {
        CurrentStateDataSO = null;
    }
}
