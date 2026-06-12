using System;
using Cysharp.Threading.Tasks;

// 직원을 고용하고 불러오는 서비스 인터페이스
public interface IStaffHireService
{ 
    UniTask FireStaff(int targetStaffID); // 해고 함수. 
    void ModifyStaffData(int staffID, Action<StaffInitData, StaffRuntimeData> modifier); // 범용 데이터 수정 함수. 
}