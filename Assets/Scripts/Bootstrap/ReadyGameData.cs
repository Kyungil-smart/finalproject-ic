using Cysharp.Threading.Tasks;
using UnityEngine;
using DataDispatcher;
using Channel = DataDispatcher.Channel;


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
            var save = ServiceLocater.Get<ISaveManager>();
            int slot = save.CurrentSlot;
            
            if (slot < 0 || save.IsEmpty(slot))
            {   // 게임 데이터가 없다.
                await InitStaff();
                await UniTask.Yield();
                ServiceLocater.Get<IMainStateMachine>().SetCurrentMainState(GameDevProcName.HumanResources);
            }
            else
            {
                await save.Load(slot);    // RestoreGame → _procName 복원(ChangeState 우회라 Save 안 일어남)
                await UniTask.Yield();
                var procName = ServiceLocater.Get<IGameManager>().ProcName.CurrentValue;
                ServiceLocater.Get<IMainStateMachine>().SetCurrentMainState(procName);   // 복원된 공정으로 진입
            }
            await UniTask.Yield();
            ServiceLocater.Get<IPostManager>().Post(Channel.CloseLoading, true);
        });
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
            var result = await ServiceLocater.Get<IStaffRecruit>().ConfirmHireAsync(staff.Staff_ID, free: true);
            if (result == StaffHireResult.Success)
                Debug.Log($"[ReadyGameData] Hire staff: {staff.Staff_ID} {staff.Staff_Name}");
            else
                Debug.LogError($"[ReadyGameData] Hire Process; {result}");
        }
        
        var hiredStaffs = ServiceLocater.Get<IStaffRegister>().GetAllHiredStaffList();
        foreach (var staff in hiredStaffs)
            Debug.Log($"[ReadyGameData:InitStaff] Hired staff ID: {staff.Staff_ID}");
    }
}
