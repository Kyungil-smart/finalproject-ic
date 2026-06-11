using System.Collections.Generic;

public interface IStaffRegister
{
    public List<StaffViewData> GetAllHiredStaffList(); // 고용된 직원들 확인. 현재는 StaffManager에서 구현
    public SlotData CurrentSlot { get; }
    public (bool result, int slotId) UpgradeSlot(int money);
}