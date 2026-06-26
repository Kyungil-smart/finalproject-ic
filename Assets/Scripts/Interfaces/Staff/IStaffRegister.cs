using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IStaffRegister
{
    public List<StaffViewData> GetAllHiredStaffList(); // 고용된 직원들 확인. 현재는 StaffManager에서 구현
    public SlotState CurrentSlot { get; }
    public SlotState GetSlotData(int index);
    public (bool result, int nextSlotIndex) UpgradeSlot();
    public int maxHiredStaffCount { get; }
    public StaffEntity GetStaffEntity(int staffId);
    public void GetExpInProduction(GameDevProcName name, List<int> staffIds);
    public void GetExpAllStaffs();
    public UniTask LevelUpStaffs();
    public void SetSlotPos(Transform[] transforms);
    public UniTask RestoreSaveData(StaffManagerSaveData dto);
    public StaffManagerSaveData CaptureSaveData();
    public Transform GetSeatTransform(int staffId); 
}