using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 채용 시스템 관련 인터페이스 
public interface IStaffRecruit
{
    // 채용할 수(cardCount)만큼 전체 고용되지 않은 직원들중에 무작위로 채용할 수 있는 후보들 _recruitCandidates에 등록. 
    UniTask GenerateRecruitCandidatesAsync(int playerLevel, int cardCount);  
    List<StaffViewData> GetAvailableStaffList(); // 채용할 수 있는 후보들을 StaffViewData 형식으로 반환. 
    UniTask ConfirmHireAsync(int targetStaffID, bool free); // 고용하기로 선택한 직원을 빌드한 후 스태프 매니저의 직원 관리 정보를 업데이트 하고, 직원을 오브젝트화.
}
