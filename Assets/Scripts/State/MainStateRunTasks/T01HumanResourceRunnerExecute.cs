using System;
using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

/// <summary>
/// 직원 관리
/// </summary>
public class T01HumanResourceRunnerExecute : ProcessTaskRunner, IProcessTaskRunnerExecute
{
    public async UniTask Execute()
    {
        CheckStaffList();
        HireStaff();
    }

    private void CheckStaffList()
    {
        Debug.Log($"[StaffHireSubState] : 직원 리스트 확인");
        // TODO : 보유 직원 리스트 확인
    }

    private void HireStaff()
    {
        Debug.Log($"[StaffHireSubState] : 직원 채용 완료");
        // TODO : 빈 직원 슬롯 확인 및 채용
    }
}