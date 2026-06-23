using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;


// 해당 클래스의 역할이 많은데 나중에 분리할 예정입니다 .
// 채용 관리 역할
// 고용된 스태프의 StaffInitData, StaffRunTimeData, StaffEntity 저장. 
// 고용된 스태프의 데이터 읽기(GetAllHiredStaffList), 쓰기(ModifyStaffData) 가능. 
public class StaffManager : Manager, IStaffHireService, IStaffRegister, IStaffRecruit, IReadyStatus
{
    // Slot Data
    [Header("슬롯 데이터")]
    [SerializeField] private SlotUnlockDataSO slotUnlockData;
    [SerializeField] private string slotGSheetId;
    [SerializeField] private string slotGId;
    private SlotState _currentSlot;
    public SlotState CurrentSlot => _currentSlot;
    private List<SlotState> _slots = new();
    private int _slotIndex = 2;
    public int maxHiredStaffCount => GetMaxHiredStaffCount();
    
    [Header("스태프 생성 설정")]
    public Transform staffContainer;      // 직원들 모아둘 부모 폴더 (하이러키 창에서)
    public GameObject tempCbtPrefab;      // 임시 캐릭터 프리팹 ( 나중에 아바타 시트, 어드레서블 적용) 
    
    private List<StaffEntity> _staffList = new ();
    
    // 가챠 버튼 눌렀을 때 생성되어 UI 후보 리스트 창에 띄워질 실제 후보들 런타임 데이터
    private List<StaffEntity> _recruitCandidates = new ();
    private StaffDataFactory _dataFactory = new ();
    private IStaffDataManager _staffDataManager;

    private Dictionary<string, bool> _readyStatus = new();
    public Dictionary<string, bool> ReadyStatus => _readyStatus;
    
    private const string ThumbnailLabel  = "Staff_Thumnail"; // 라벨 규칙
    private const string ThumbnailPrefix = "sfth_";          // 어드레서블 이름 규칙: sfth_XXXX
    private const string PrefabPrefix = "sfpf_";
    // 어드레서블 ID → 키 문자열 ("sfth_0012")
    private static string ToThumbnailKey(int staffId) => $"{ThumbnailPrefix}{staffId:D4}";
    private static string ToPrefabKey(int staffId) => $"{PrefabPrefix}{staffId:D4}";
    
    private void Awake() => Register();

    private void OnDestroy() => Unregister();

    private void Start() => InitData();

    private async UniTask InitData()
    {
        if (Utils.Environment.isDevelopment)
            await DownloadSlotData();
        
        _staffDataManager = ServiceLocater.Get<IStaffDataManager>();
        await InitSlot();
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
        foreach (var c in _recruitCandidates) c.ReleaseThumbnail();
        _recruitCandidates.Clear(); // 이전 후보 데이터 초기화
        Debug.Log($"[StaffManager] Staff 데이터 확인: {_staffDataManager.StaffList.Count}");
        
        // (A) Full = 고용 + 현재 후보. 의도를 코드에 명시해 어떤 호출 흐름에서도 중복 차단
        var excludedIds = new HashSet<int>(_staffList.Select(x => x.init.Staff_ID));
        excludedIds.UnionWith(_recruitCandidates.Select(x => x.init.Staff_ID)); // Clear 직후라 비어있지만 안전망

        // (B) 어드레서블 라벨로 "썸네일이 실제 존재하는 ID"만 추출
        var avatarPoolIds  = await LoadAvailableThumbnailIdsAsync();
        
        // (C) 스태프 풀 = 전체 - Full(Staff_ID 중복). 썸네일 필터 없음!
        var pool = _staffDataManager.StaffList
            .Where(row => !excludedIds.Contains(row.Staff_ID))
            .ToList();

        // 풀이 부족하면 가능한 만큼만 (무한 루프 대신 안전하게 축소)
        int targetCount = Mathf.Min(cardCount, pool.Count);
        if (targetCount < cardCount)
            Debug.LogWarning($"[StaffManager] 뽑을 수 있는 후보가 부족: 요청 {cardCount} / 가능 {targetCount}");
        
        // Full(고용 스태프)이 이미 점유한 아바타 XXXX → 중복 배정 방지용
        var usedAvatarKeys = new HashSet<int>(_staffList.Select(s => s.init.AvatarKey));
        
        for (int i = 0; i < targetCount; i++)
        {
            // 비복원 추출: 뽑은 건 풀에서 제거 → 다음 루프에서 자동으로 중복 제외
            int idx = Random.Range(0, pool.Count);
            StaffRow readOnlyStaffData = pool[idx];
            pool.RemoveAt(idx);
            
            var candidate = await _dataFactory.CreateDataByStaffIDAsync(readOnlyStaffData.Staff_ID, playerLevel);
            if (candidate == null) continue;
            
            // (D) 아바타 XXXX 배정: 가용 풀에서 Full 미사용분만 랜덤 (동시 중복 없음)
            var freeAvatars = avatarPoolIds.Where(id => !usedAvatarKeys.Contains(id)).ToList();
            Sprite thumbnail = null;
            AsyncOperationHandle<Sprite> spriteHandle = default;
            if (freeAvatars.Count > 0)
            {
                int avatarKey = freeAvatars[Random.Range(0, freeAvatars.Count)];
                usedAvatarKeys.Add(avatarKey);          // 다음 후보와도 중복 방지
                candidate.AvatarKey = avatarKey;
                candidate.AssetId = ToThumbnailKey(avatarKey);   // "sfth_XXXX"
                spriteHandle = Addressables.LoadAssetAsync<Sprite>(candidate.AssetId);
                thumbnail = await spriteHandle.ToUniTask();
            }
            else
            {
                Debug.LogWarning($"[StaffManager] 가용 아바타 부족(동시 {usedAvatarKeys.Count}개 점유). 썸네일 없이 생성");
            }
        
            StaffEntity staff = new ()
            {
                init = candidate,
                runtime = _dataFactory.CreateInitialRuntimeData(candidate)
            };
            
            staff.SetThumbnail(thumbnail, spriteHandle); // 핸들 없으면 default 전달
            // 다음 루프부터 이 후보도 Full로 취급 (안전망)
            excludedIds.Add(candidate.Staff_ID);
            Debug.Log($"[StaffManager] {staff.init.Staff_ID}:{staff.init.Staff_Name} 대기실 배치");
            _recruitCandidates.Add(staff); // 대기실 리스트업
            
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
            var cost = targetData.GetHireCost();
            if (gameManager.Money.CurrentValue < cost)
                return StaffHireResult.NotEnoughMoney;
            ServiceLocater.Get<IGameManager>().AddMoney(cost * -1);
        }

        // 후보 리스트에서 제거 후 정식 고용 리스트 및 딕셔너리로 이사 
        _recruitCandidates.Remove(targetData); 
        _staffList.Add(targetData);
        
        // 빌더 파이프라인으로 실제 캐릭터 프리팹 생성 및 배치
        (IStaffInfo newStaff, GameObject go) = await new StaffBuilder()
            .WithStaffData(targetData)
            .WithAddressableKey(ToPrefabKey(targetData.init.AvatarKey))
            .WithVisualAsset(tempCbtPrefab) 
            .BuildAsync(staffContainer);
        
        newStaff.DisplayInfo();
        targetData.SetGameObject(go);
        Debug.Log($"[{targetData.init.Staff_Name}] 정식 채용 및 오브젝트 생성 완료");
        return StaffHireResult.Success;
    }
    
    // 직원 해고 함수 (UI에서 해고 누를 시 함수 호출)
    public async UniTask FireStaff(int targetStaffID)
    {
        // 고용 리스트에서 삭제.
        var staff = _staffList.Find(x => x.init.Staff_ID == targetStaffID);
        if (staff == null) return;
        _staffList.Remove(staff);
        staff.ReleaseThumbnail(); 
        staff.ReleaseVisualInstance();
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
            Job_Type = data.init.Job,
            Thumbnail = data.Thumbnail,
            Grade = data.init.Grade.ToString(),
            DISC_Type = data.init.DISC_Type,
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

    private int GetMaxHiredStaffCount()
    {
        int cnt = 0;
        foreach (var slot in _slots)
            if (slot.unlocked && slot.id % 10 != 0) cnt++;
        return cnt;
    }
    
    private async UniTask InitSlot()
    {
        Debug.Log($"[StaffManager:Init] 로딩된 slot 총 개수: {slotUnlockData.slots.Count}");
        _slots = slotUnlockData.slots.Select(def => new SlotState(def)).ToList();
        for (int i = 0; i < _slotIndex; i++)
            _slots[i].unlocked = true;
    }

    public void SetSlotPos(Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
            _slots[i].pos = transforms[i];
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
    
    // 라벨에 묶인 썸네일들의 "존재하는 Staff_ID 집합"을 가져온다 (실제 이미지는 아직 로드 안 함)
    private async UniTask<HashSet<int>> LoadAvailableThumbnailIdsAsync()
    {
        var ids = new HashSet<int>();

        var locHandle = Addressables.LoadResourceLocationsAsync(ThumbnailLabel, typeof(Sprite));
        var locations = await locHandle.ToUniTask();

        foreach (var loc in locations)
        {
            // PrimaryKey 예: "sfth_0012"
            string key = loc.PrimaryKey;
            if (!key.StartsWith(ThumbnailPrefix)) continue;
            if (int.TryParse(key.Substring(ThumbnailPrefix.Length), out int id))
                ids.Add(id);
        }

        Addressables.Release(locHandle); // 위치 핸들은 바로 해제
        Debug.Log($"[StaffManager] 사용 가능한 썸네일 ID {ids.Count}개 확인");
        return ids;
    }

    // ------------------------------------------------------------------------
    
    [ContextMenu("Download Slot Data")]
    private async UniTask DownloadSlotData()
    {
        _readyStatus["StaffSlot"] = false;
        GSheetManager gSheetManager = new(slotGSheetId, slotGId);
        await UniTask.WaitUntil(() => gSheetManager.IsDownload);
        var dataList = gSheetManager.GetData();
        if (slotUnlockData.slots.Count > 0)
        {
            _readyStatus["StaffSlot"] = true;
            return;
        }
        slotUnlockData.slots.Clear();
        foreach (var data in dataList)
        {
            slotUnlockData.slots.Add(new SlotDef()
            {
                id = int.Parse(data["Slot_ID"]),
                cost = int.Parse(data["Slot_Cost"]),
            });
        }
        _readyStatus["StaffSlot"] = true;
    }

    [ContextMenu("Level Up Staff")]
    private void LevelUpStaff()
    {
        foreach (var staff in _staffList)
        {
            staff.LevelUp(true);
        }
    }
}