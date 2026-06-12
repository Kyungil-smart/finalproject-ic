using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;


// 해당 클래스의 역할이 많은데 나중에 분리할 예정입니다 .
// 채용 관리 역할
// 고용된 스태프의 StaffInitData, StaffRunTimeData, StaffEntity 저장. 
// 고용된 스태프의 데이터 읽기(GetAllHiredStaffList), 쓰기(ModifyStaffData) 가능. 
public class StaffManager : MonoBehaviour, IStaffHireService, IStaffRegister, IStaffRecruit
{
    // Slot Data
    [Header("슬롯 데이터")]
    [SerializeField] private SlotUnlockDataSO slotUnlockData;
    [SerializeField] private string slotGSheetId;
    [SerializeField] private string slotGId;
    private SlotData _currentSlot;
    public SlotData CurrentSlot => _currentSlot;
    private List<SlotData> _slots = new();
    private int _slotIndex;
    public int maxHiredStaffCount => _slotIndex + 2;
    
    [Header("스태프 생성 설정")]
    public Transform staffContainer;      // 직원들 모아둘 부모 폴더 (하이러키 창에서)
    public GameObject tempCbtPrefab;      // 임시 캐릭터 프리팹 ( 나중에 아바타 시트, 어드레서블 적용) 
    
    private List<StaffData> _staffList = new (); 
    // 빌더로 실제로 완성된 게임 오브젝트들 저장하는 디셔너리. Key 값 = Staff_ID
    // 나중에 _hiredStaffList의 StaffInitData랑 StaffEntity의 new StaffInitData 중복 선언된거 고치기. 
    private Dictionary<int, StaffEntity> _spawnedEntities = new ();
    
    // 가챠 버튼 눌렀을 때 생성되어 UI 후보 리스트 창에 띄워질 실제 후보들 런타임 데이터
    private List<StaffData> _recruitCandidates = new ();
    private StaffDataFactory _dataFactory = new ();
    private IStaffDataManager _staffManager;
    
    private void Awake()
    {
        // 연관 인터페이스로도 등록
        ServiceLocater.Register<IStaffHireService>(this);
        ServiceLocater.Register<IStaffRegister>(this);
        ServiceLocater.Register<IStaffRecruit>(this);
    }

    private void OnDestroy()
    {
        ServiceLocater.Unregister<IStaffHireService>(this);
        ServiceLocater.Unregister<IStaffRegister>(this);
        ServiceLocater.Unregister<IStaffRecruit>(this);
    }

    private void Start() => Init();

    private void Init()
    {
        _staffManager = ServiceLocater.Get<IStaffDataManager>();
        _slots = slotUnlockData.slots;
        Debug.Log($"[StaffManager:Init] 로딩된 slot 총 개수: {_slots.Count}");
        var data = LoadingSavedData();
        // ToDo: Save file loading 후에 slot 상태에 대한 업데이트 필요.
        if (data == null)
            _currentSlot = _slots[_slotIndex]; 
    }

    private object LoadingSavedData()
    {
        return null;
    }
    
    // 채용 프로세스 ----------------
    
    
    
    // ## 채용 1단계
    // 카드 수만큼 채용 후보 리스트 생성 (가챠 UI 창 열거나 새로고침 시 호출)
    public async UniTask GenerateRecruitCandidatesAsync(int playerLevel, int cardCount) 
    {
        Debug.Log($"[StaffManager] 신규 채용 후보 {cardCount}명 생성 시작...");
        _recruitCandidates.Clear(); // 이전 후보 데이터 초기화

        for (int i = 0; i < cardCount; i++)
        {
            StaffRow readOnlyStaffData;
            while (true)
            {
                readOnlyStaffData = _staffManager.StaffList[Random.Range(0, _staffManager.StaffList.Count)];
                if (_staffList.FindAll(x => x.init.Staff_ID == readOnlyStaffData.Staff_ID).Count < 1) break;
                await UniTask.WaitForSeconds(0.1f);
            }
            var candidate = await _dataFactory.CreateDataByStaffIDAsync(readOnlyStaffData.Staff_ID, playerLevel);
            if (candidate != null)
            {
                _recruitCandidates.Add(new StaffData()
                {
                    init = candidate,
                    runtime = _dataFactory.CreateInitialRuntimeData(candidate)
                }); // 대기실 리스트업
            }
        }
        Debug.Log($"[StaffManager] 채용 후보 데이터 {_recruitCandidates.Count}명 확정 셋팅 완료.");
    }
    
    // ## 채용 2단계 
    // IStaffRecruit 구현: UI용. 뽑힌 후보 리스트를 StaffViewData 형태로 반환. 
    public List<StaffViewData> GetAvailableStaffList()
    {
        List<StaffViewData> unhiredViewList = new List<StaffViewData>();
    
        // 팩토리가 스탯 연산을 마친 진짜 후보 리스트를 순회하며 뷰용 데이터로 변환.
        foreach (var candidateData in _recruitCandidates) //_recruitCandidates: 팩토리로 InitData 생성한 데이터
        {
            // 이름, 직군같은 기본 StatsRow 정보뿐 아니라, 팩토리로 계산된 InitData도 포함해서 ViewData로 저장.
            unhiredViewList.Add(ConvertToViewData(candidateData));
        }
    
        return unhiredViewList;
    }

    // 고용 버튼 눌렀을때 ViewList 중 해당 직원 뽑는 것은 UI쪽에서 처리? 
    public StaffHireResult VerifyHirable(int targetStaffID)
    {
        var targetData = _recruitCandidates.Find(c => c.init.Staff_ID == targetStaffID);
        if (targetData == null)
            return StaffHireResult.NoRecruiter;
        
        var gameManager = ServiceLocater.Get<IGameManager>();
        var cost = _spawnedEntities[targetData.init.Staff_ID].GetHireCost();
        if (gameManager.Money.CurrentValue < cost)
            return StaffHireResult.NotEnoughMoney;
        return StaffHireResult.Available;
    }
    
    // ## 채용 3단계
    // 최종 계약 확정 (UI 목록에서 버튼을 눌러 채용할 때 내부 데이터 업데이트 및 스태프 생성)
    public async UniTask<StaffHireResult> ConfirmHireAsync(int targetStaffID, bool free = true)
    {
        // 최대 고용 인원 수 확인
        if (_staffList.Count >= maxHiredStaffCount) return StaffHireResult.Full;
        
        // 채용 후보 리스트에 해당 사번이 실제로 대기 중인지 체크.
        var targetData = _recruitCandidates.Find(c => c.init.Staff_ID == targetStaffID);
        if (targetData == null)
        {
            Debug.LogError($"사번 {targetStaffID}번 스태프는 현재 채용 후보 목록에 없습니다.");
            return StaffHireResult.NoRecruiter;
        }
        Debug.Log($"[StaffManager] {targetData.init.Staff_Name} - 고용 절차 시작");
        
        if (!free)
        {
            var gameManager = ServiceLocater.Get<IGameManager>();
            var cost = _spawnedEntities[targetData.init.Staff_ID].GetHireCost();
            if (gameManager.Money.CurrentValue < cost)
                return StaffHireResult.NotEnoughMoney;
            ServiceLocater.Get<IGameManager>().AddMoney(cost * -1);
        }

        // 후보 리스트에서 제거 후 정식 고용 리스트 및 딕셔너리로 이사 
        _recruitCandidates.Remove(targetData); 
        _staffList.Add(targetData);
        
        // 빌더 파이프라인으로 실제 캐릭터 프리팹 생성 및 배치
        IStaffInfo newStaff = await new StaffBuilder()
            .WithInitData(targetData.init)
            .WithRuntimeData(targetData.runtime)
            .WithVisualAsset(tempCbtPrefab) 
            .BuildAsync(staffContainer);
        
        // 생성된 오브젝트 딕셔너리에 담기. 
        if (newStaff is StaffEntity entity) 
            _spawnedEntities[targetData.init.Staff_ID] = entity;
        newStaff.DisplayInfo(); 
        
        if (((Component)newStaff).TryGetComponent(out IJobAction job))
            job.DoWork();
        
        Debug.Log($"[{targetData.init.Staff_Name}] 정식 채용 및 오브젝트 생성 완료");
        return StaffHireResult.Success;
    }
    
    // 직원 해고 함수 (UI에서 해고 누를 시 함수 호출)
    public async UniTask FireStaff(int targetStaffID)
    {
        // 고용 리스트에서 삭제.
        int delNum = _staffList.RemoveAll(x => x.init.Staff_ID == targetStaffID);
        Debug.Log($"[StaffManager] 총 {delNum} 명의 Staff 가 해고 되었습니다.");
        await UniTask.Yield();
        // 오브젝트 삭제 및 오브젝트 딕셔너리에서 삭제.
        if (_spawnedEntities.TryGetValue(targetStaffID, out StaffEntity targetEntity))
        {
            if (targetEntity != null)
            {
                Destroy(targetEntity.gameObject);
                await UniTask.Yield();
            }
            _spawnedEntities.Remove(targetStaffID);
        }
        
        // 재고용 불가 같은 것은 아직 고려하지 않음.(나중에 기획에 있으면 추가할 예정)
        Debug.Log($"사번 ID: {targetStaffID}번 직원의 데이터와 3D 오브젝트가 제거완료.");
    }
    
    // -----ProjectManager 관련 함수 ---------
    
    // IStaffRegister 구현: 고용된 직원 확인 기능 
    // ProjectManager에서는 GetAllHiredStaffList로 모든 스태프의 StaffViewData를 받은 뒤에 작업 후(정보 볼때만 사용)
    // StaffManager에 특정 사원의 Data를 업데이트 하는 경우 ServiceLocater에 등록된
    // StaffManager.ModifyStaffData로 ID와 변경값을 넘겨줘서 변경.(값 변경 시 사용) 
    public List<StaffViewData> GetAllHiredStaffList()
    {
        List<StaffViewData> viewDataList = new List<StaffViewData>();

        foreach (var initData in _staffList)
        {
            // 만들어둔 변환 함수 사용하여 초기데이터 + Runtime데이터 -> View용 데이터(StaffViewData)으로 데이터 전환. 
            viewDataList.Add(ConvertToViewData(initData));
        }

        return viewDataList; 
    }
    
    // initData와 Runtime 데이터를 반영한 View용 StaffViewData 생성. 
    public StaffViewData ConvertToViewData(StaffData data)
    {
        StaffViewData viewData = new StaffViewData
        {
            // 매핑
            Staff_ID = data.init.Staff_ID,
            Staff_Name = data.init.Staff_Name,
            Staff_Gender = data.init.Staff_Gender,
            Job_Name = data.init.Job.ToString(),
            Avatar_ID = data.init.Avatar_ID, // 아직 에셋이 안나와서 팩토리 임시 랜덤값 바인딩
            Grade = data.init.Grade.ToString(),
            DISC_Type = data.init.DISC_Type.ToString(),
            Current_State = data.runtime.Current_State.ToString(),
            Current_Level = data.runtime.Current_Level,
            Current_Exp = data.runtime.Current_Exp,
            Salary = data.init.Salary,
            Hire_Cost = data.init.Hire_Cost,
            Final_Career = data.init.Base_Career + data.runtime.Added_Career,
            Final_Common_Concentration = data.init.Base_Common_Concentration + data.runtime.Added_Common_Concentration,
            Final_Common_Creativity = data.init.Base_Common_Creativity + data.runtime.Added_Common_Creativity,
            Final_Common_Communication = data.init.Base_Common_Communication + data.runtime.Added_Common_Communication,
            Final_Job_Planning = data.init.Base_Job_Planning + data.runtime.Added_Job_Planning,
            Final_Job_Development = data.init.Base_Job_Development + data.runtime.Added_Job_Development,
            Final_Job_Art = data.init.Base_Job_Art + data.runtime.Added_Job_Art
        };

        // viewData.All_Tags.Add(data.init.Fixed_Tag);
        //
        // if (data.runtime.Added_Tags != null && data.runtime.Added_Tags.Count > 0)
        // {
        //     viewData.All_Tags.AddRange(data.runtime.Added_Tags);
        // }

        return viewData;
    }
    
    
    public StaffEntity GetStaffEntity(int staffId)
    {
        _spawnedEntities.TryGetValue(staffId, out var entity);
        return entity;
    }

    // --- Slot 제어
    
    public (bool result, int nextSlotIndex) UpgradeSlot()
    {
        var money = ServiceLocater.Get<IGameManager>().Money.CurrentValue;
        if (money < _currentSlot.cost || _slotIndex >= _slots.Count - 1)
        {
            Debug.Log("[StaffManager] Slot 해금 실패");
            return (false, 0);
        }
        
        _currentSlot.unlocked = true;
        ServiceLocater.Get<IGameManager>().AddMoney(_currentSlot.cost * -1);
        _currentSlot = _slots[++_slotIndex];
        Debug.Log($"[StaffManager] Slot 해금 성공 : 최대 고용 가능 인원수: {maxHiredStaffCount}");
        return (true, _slotIndex);
    }
    
    // --- Leveling 관련
    
    public void GetStaffExperience(int exp, List<string> staffIds)
    {
        if (staffIds.Count == 0)
        {   // 전체 공통
            foreach (var staffData in _staffList)
            {
                staffData.init.
            }
        }
        else
        {   // 리더만
            
        }
    }

    // ------------------------------------------------------------------------
    
    [ContextMenu("Download Slot Data")]
    private async UniTask DownloadSlotData()
    {
        GSheetManager gSheetManager = new(slotGSheetId, slotGId);
        await UniTask.WaitUntil(() => gSheetManager.IsDownload);
        var dataList = gSheetManager.GetData();
        slotUnlockData.slots.Clear();
        foreach (var data in dataList)
        {
            slotUnlockData.slots.Add(new SlotData()
            {
                id = int.Parse(data["Slot_ID"]),
                cost = int.Parse(data["Slot_Cost"]),
            });
        }
    }
}