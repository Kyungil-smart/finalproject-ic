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
public class StaffManager : Manager, IStaffHireService, IStaffRegister, IStaffRecruit
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
    
    private List<StaffEntity> _staffList = new ();
    
    // 가챠 버튼 눌렀을 때 생성되어 UI 후보 리스트 창에 띄워질 실제 후보들 런타임 데이터
    private List<StaffEntity> _recruitCandidates = new ();
    private StaffDataFactory _dataFactory = new ();
    private IStaffDataManager _staffDataManager;

    private void Awake() => Register();

    private void OnDestroy() => Unregister();

    private void Start() => Init();

    private void Init()
    {
        if (Utils.Environment.isDevelopment)
            DownloadSlotData().Forget();
        
        _staffDataManager = ServiceLocater.Get<IStaffDataManager>();
        _slots = slotUnlockData.slots;
        Debug.Log($"[StaffManager:Init] 로딩된 slot 총 개수: {_slots.Count}");
        var data = LoadingSavedData();
        // ToDo: Save file loading 후에 slot 상태에 대한 업데이트 필요.
        if (data == null)
            _currentSlot = _slots[_slotIndex]; 
    }

    protected override void Register()
    {
        // 연관 인터페이스로도 등록
        ServiceLocater.Register<IStaffHireService>(this);
        ServiceLocater.Register<IStaffRegister>(this);
        ServiceLocater.Register<IStaffRecruit>(this);
    }

    protected override void Unregister()
    {
        ServiceLocater.Unregister<IStaffHireService>(this);
        ServiceLocater.Unregister<IStaffRegister>(this);
        ServiceLocater.Unregister<IStaffRecruit>(this);
    }

    private object LoadingSavedData()
    {
        return null;
    }
    
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
                readOnlyStaffData = _staffDataManager.StaffList[Random.Range(0, _staffDataManager.StaffList.Count)];
                if (_staffList.FindAll(x => x.init.Staff_ID == readOnlyStaffData.Staff_ID).Count < 1
                    && _recruitCandidates.FindAll(x => x.init.Staff_ID == readOnlyStaffData.Staff_ID).Count < 1) 
                    break;
                await UniTask.WaitForSeconds(0.1f);
            }
            var candidate = await _dataFactory.CreateDataByStaffIDAsync(readOnlyStaffData.Staff_ID, playerLevel);
            if (candidate != null)
            {
                StaffEntity staff = new ()
                {
                    init = candidate,
                    runtime = _dataFactory.CreateInitialRuntimeData(candidate)
                };
                Debug.Log($"[StaffManager] {staff.init.Staff_ID}:{staff.init.Staff_Name} 대기실 배치");
                _recruitCandidates.Add(staff); // 대기실 리스트업
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
        var staff = _staffList.Find(x => x.init.Staff_ID == targetData.init.Staff_ID);
        var cost = staff.GetHireCost();
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
        foreach (var candidate in _recruitCandidates)
        {
            // 1. 유니티 가짜 Null 검증 (해당 객체 자체가 유니티 상에서 null인지 확인)
            Debug.Log($"[StaffManager] 객체 자체 null 체크: {candidate == null}");
    
            // 2. Equals 메서드를 통한 값 비교 검증 (참조 비교 오류 방지)
            Debug.Log($"[StaffManager] Equals 비교: {candidate.init.Staff_ID.Equals(targetStaffID)}");
        }
        
        var targetData = _recruitCandidates.Find(c => c.init.Staff_ID.Equals(targetStaffID));
        if (targetData == null)
        {
            Debug.LogError($"사번 {targetStaffID}번 스태프는 현재 채용 후보 목록에 없습니다.");
            return StaffHireResult.NoRecruiter;
        }
        Debug.Log($"[StaffManager] {targetData.init.Staff_Name} - 고용 절차 시작");
        
        if (!free)
        {
            var gameManager = ServiceLocater.Get<IGameManager>();
            var staff = _staffList.Find(x => x.init.Staff_ID == targetData.init.Staff_ID);
            var cost = staff.GetHireCost();
            if (gameManager.Money.CurrentValue < cost)
                return StaffHireResult.NotEnoughMoney;
            ServiceLocater.Get<IGameManager>().AddMoney(cost * -1);
        }

        // 후보 리스트에서 제거 후 정식 고용 리스트 및 딕셔너리로 이사 
        _recruitCandidates.Remove(targetData); 
        _staffList.Add(targetData);
        
        // 빌더 파이프라인으로 실제 캐릭터 프리팹 생성 및 배치
        IStaffInfo newStaff = await new StaffBuilder()
            .WithStaffData(targetData)
            .WithVisualAsset(tempCbtPrefab) 
            .BuildAsync(staffContainer);
        
        newStaff.DisplayInfo();
        Debug.Log($"[{targetData.init.Staff_Name}] 정식 채용 및 오브젝트 생성 완료");
        return StaffHireResult.Success;
    }
    
    // 직원 해고 함수 (UI에서 해고 누를 시 함수 호출)
    public async UniTask FireStaff(int targetStaffID)
    {
        // 고용 리스트에서 삭제.
        var staff = _staffList.Find(x => x.init.Staff_ID == targetStaffID);
        _staffList.Remove(staff);
        await UniTask.Yield(); 
        Destroy(staff.GetGameObject());
        await UniTask.Yield();
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
    public StaffViewData ConvertToViewData(StaffEntity data)
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
            Level = data.init.Level,
            Exp = data.init.Exp,
            Salary = data.init.Salary,
            Hire_Cost = data.init.Hire_Cost,
            Final_Career = data.init.Base_Career + data.runtime.Added_Career,
            Final_Common_Concentration = data.init.Base_Common_Concentration + data.runtime.Added_Common_Concentration,
            Final_Common_Creativity = data.init.Base_Common_Creativity + data.runtime.Added_Common_Creativity,
            Final_Common_Communication = data.init.Base_Common_Communication + data.runtime.Added_Common_Communication,
            Final_Job_Planning = data.init.Base_Job_Planning + data.runtime.Added_Job_Design,
            Final_Job_Development = data.init.Base_Job_Development + data.runtime.Added_Job_Development,
            Final_Job_Art = data.init.Base_Job_Art + data.runtime.Added_Job_Art
        };

        foreach (var tag in data.runtime.Added_Tags)
            viewData.All_Tags.Add(tag.Tag_Name);    
        
        return viewData;
    }
    
    
    public StaffEntity GetStaffEntity(int staffId)
    {
        return _staffList.Find(x => x.init.Staff_ID == staffId);
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
    public void GetExpAllStaffs()
    {
        float exp = _staffDataManager.GetExpList.Find(x => x.expType == ExpType.LaunchEXP).expValue;
        foreach (var staffData in _staffList)
        {
            var ratio = _staffDataManager.GradeList
                .Find(x => x.Grade == staffData.init.Grade.ToString())
                .Grade_XP;
            staffData.ApplyExp(exp * ratio);
        }
        // Player 경험치 증가
        ServiceLocater.Get<IGameManager>().AddExp(exp);
    }
    
    public void GetExpInProduction(GameDevProcName procName, List<int> staffIds)
    {
        float exp = 0;
        List<GameDevProcName> pres = new()
        {
            GameDevProcName.ArtPreProduction, 
            GameDevProcName.ConceptPreProduction,
            GameDevProcName.DevelopmentPreProduction
        };
        
        List<GameDevProcName> fulls = new()
        {
            GameDevProcName.ArtFullProduction, 
            GameDevProcName.ConceptFullProduction,
            GameDevProcName.DevelopmentFullProduction
        };
        
        if (pres.Contains(procName))
            exp += _staffDataManager.GetExpList.Find(x => x.expType == ExpType.PreEXP).expValue;
        else if (fulls.Contains(procName))
            exp += _staffDataManager.GetExpList.Find(x => x.expType == ExpType.FullEXP).expValue;
        
        foreach (var staffId in staffIds)
        {
            var staffData = _staffList.Find(x => x.init.Staff_ID == staffId);
            var ratio = _staffDataManager.GradeList
                .Find(x => x.Grade == staffData.init.Grade.ToString())
                .Grade_XP;
            staffData.ApplyExp(exp * ratio);
        }
    }

    public async UniTask LevelUpStaffs()
    {   // Main 씬으로 넘어간 후에 현재 Staff 들의 경험치를 토대로 렙업을 일괄/순차적으로 진행해야함.
        foreach (var staffData in _staffList)
        {
            var levelExpData = _staffDataManager
                .LevelExpList
                .Find(x => x.level == staffData.init.Level + 1);
            
            if (levelExpData.requiredExp >= staffData.init.Exp) 
                await staffData.LevelUp(levelExpData.isTag);
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