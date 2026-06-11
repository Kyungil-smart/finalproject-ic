using Cysharp.Threading.Tasks;
using UnityEngine;


public class DummyData 
// 추후 SaveData Format 나오면 거기에 맞는 Data 구조로 변경 예정
{
    
}

/// <summary>
/// 게임 시작시
/// </summary>
public class ReadyGameData : MonoBehaviour
{
    private void Start()
    {
        UniTask.Void(async () =>
        {
            await UniTask.Yield();
            var data = await LoadSavedData();
            if (data == null)
            {   // 게임 데이터가 없다.
                await InitStaff();    
            }
            await UniTask.Yield();
        });
    }

    private async UniTask<DummyData> LoadSavedData()
    {   
        // Slot 을 기반으로 Save Game Data 를 Loading.
        // 없으면 null return => 완전 처음 시작 하는 케이스
        // 없고 있고는, File 내 컨텐츠가 없거나 File 자체가 없는 케이스로 생각하기.
        // 혹은 Save Data 를 SO 로 관리할 수 있음 -> 오..?
        return null;
    }
    
    private async UniTask InitStaff()
    {
        var curStaffs = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        if (curStaffs.Count != 0) return; 
        
        // 초기 직원 수 2, level 1 ; 고정값
        await ServiceLocater.Get<IStaffRecruit>().GenerateRecruitCandidatesAsync(1, 2);
        // 직원 확인
        var availableStaffs = ServiceLocater.Get<IStaffRecruit>().GetAvailableStaffList();
        Debug.Log($"[ReadyGameData] Available staffs: {availableStaffs.Count}");
        // 확인한 직원으로 채용 진행
        foreach (var staff in availableStaffs)
        {
            await ServiceLocater.Get<IStaffRecruit>().ConfirmHireAsync(staff.Staff_ID, free: true);
            Debug.Log($"[ReadyGameData] Hire staff: {staff.Staff_ID} {staff.Staff_Name}");
        }
        
        var hiredStaffs = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        foreach (var staff in hiredStaffs)
            Debug.Log($"[ReadyGameData:InitStaff] Hired staff ID: {staff.Staff_ID}");
    }
}
