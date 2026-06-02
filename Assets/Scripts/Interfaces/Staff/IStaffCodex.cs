using System.Collections.Generic;
using UnityEngine;

// 스태프 전체의 정보에 관한 인터페이스 . 직원 전체의 도감?같은 거
public interface IStaffCodex
{
    public List<StaffViewData> GetAllStaffViewDataList();
}
