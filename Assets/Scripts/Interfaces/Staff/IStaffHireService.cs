using System;
using Cysharp.Threading.Tasks;

// 직원을 고용하고 불러오는 서비스 인터페이스
public interface IStaffHireService
{ 
    public UniTask FireStaff(int targetStaffID); // 해고 함수.
}