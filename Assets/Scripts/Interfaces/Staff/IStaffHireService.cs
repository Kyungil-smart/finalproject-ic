using System;
using Cysharp.Threading.Tasks;

// 직원을 고용하고 불러오는 서비스 인터페이스
public interface IStaffHireService
{
    UniTask HireStaffAsync(int count, int playerLevel); // 무작위 N명을 채용 프로세스 없이 다이렉트 영입 (맨처음 랜덤 2명 뽑을때와 같은 상황에서 사용) 
    void FireStaff(int targetStaffID); // 해고 함수. 
    void ModifyStaffData(int staffID, Action<StaffInitData, StaffRuntimeData> modifier); // 범용 데이터 수정 함수. 
}