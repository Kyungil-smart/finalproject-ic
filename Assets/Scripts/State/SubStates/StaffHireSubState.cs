using System;
using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;


public class StaffHireSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;


    // 이벤트 데이터 자체가 아닌 어떤 이벤트 발생해야할지만 가지고 있어야함 -> 실제 발동은 이벤트 매니저에서
    public void Enter()
    {
        Debug.Log($"[StaffHireSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
        // TODO : 1. 직원 관리 단계 시작
    }

    public async UniTask Execute()
    {
        CheckStaffList();
        HireStaff();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
        // TODO : 1. 직원 관리 단계 종료
    }


    public void CheckStaffList()
    {
        Debug.Log($"[StaffHireSubState] : 직원 리스트 확인");
        // TODO : 보유 직원 리스트 확인
    }

    public void HireStaff()
    {
        Debug.Log($"[StaffHireSubState] : 직원 채용 완료");
        // TODO : 빈 직원 슬롯 확인 및 채용
    }
}