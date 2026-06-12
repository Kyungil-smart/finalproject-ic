using System.Collections.Generic;

public interface IStaffRegister
{
    public List<StaffViewData> GetAllHiredStaffList(); // 고용된 직원들 확인. 현재는 StaffManager에서 구현
    public SlotData CurrentSlot { get; }
    public (bool result, int nextSlotIndex) UpgradeSlot();
    public int maxHiredStaffCount { get; }
    public void GetStaffExperience(GameDevProcName name, List<int> staffIds);
}