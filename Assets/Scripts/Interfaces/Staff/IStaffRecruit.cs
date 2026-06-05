using System.Collections.Generic;
using UnityEngine;

public interface IStaffRecruit
{
    List<StaffViewData> GetAvailableStaffList(); // 채용할 후보들 확인. 현재는 StaffManager에서 구현
}
