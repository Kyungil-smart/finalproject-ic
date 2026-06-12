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
    private class RecruitCandidate
    {
        public StaffInitData init;
        public StaffRuntimeData runtime;
    }
    
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

    // 세이브/로드 테스트용 임시 메모리 (테스트용, 나중에 삭제) 
    private StaffInitData _savedInitData; 
    private StaffRuntimeData _savedRuntimeData;
    
    // 세이브/로드용 고용된 리스트
    private List<StaffInitData> _hiredStaffList = new (); // 변수명 변경 생각중.. 
    private Dictionary<int, StaffRuntimeData> _hiredRuntimeDataDict = new (); // 런타임 데이터용. 키값은 StaffInitData의 StaffID
    
    // 빌더로 실제로 완성된 게임 오브젝트들 저장하는 디셔너리. Key 값 = Staff_ID
    // 나중에 _hiredStaffList의 StaffInitData랑 StaffEntity의 new StaffInitData 중복 선언된거 고치기. 
    private Dictionary<int, StaffEntity> _spawnedEntities = new ();
    
    // 가챠 버튼 눌렀을 때 생성되어 UI 후보 리스트 창에 띄워질 실제 후보들 런타임 데이터
    private List<RecruitCandidate> _recruitCandidates = new ();

    private StaffDataFactory _dataFactory = new ();
    
    private void Awake()
    {
        // ServiceLocater.Register<StaffManager>(this);
        
        // 연관 인터페이스로도 등록
        ServiceLocater.Register<IStaffHireService>(this);
        ServiceLocater.Register<IStaffRegister>(this);
        ServiceLocater.Register<IStaffRecruit>(this);
    }

    private void OnDestroy()
    {
        // ServiceLocater.Unregister<StaffManager>(this);
        
        ServiceLocater.Unregister<IStaffHireService>(this);
        ServiceLocater.Unregister<IStaffRegister>(this);
        ServiceLocater.Unregister<IStaffRecruit>(this);
    }

    private void Start() => Init();

    private void Init()
    {
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

        var dataManager = ServiceLocater.Get<StaffDataManager>();
        if (dataManager == null || dataManager.StaffList == null) return;

        // 현재 고용되지 않은 스태프들의 원본 Row 목록만 필터링
        HashSet<int> hiredIDs = new HashSet<int>(_hiredRuntimeDataDict.Keys);
        var unhiredRows = dataManager.StaffList.Where(s => !hiredIDs.Contains(s.Staff_ID)).ToList();

        if (unhiredRows.Count == 0)
        {
            Debug.LogWarning("[StaffManager] 더 이상 가챠로 뽑을 수 있는 미고용 스태프가 원본 데이터에 없습니다.");
            return;
        }

        // 고용 되지 않은 인원들 랜덤 분산 후 원하는 카드 수만큼 선택
        var pickedRows = unhiredRows.OrderBy(x => Random.value).Take(cardCount).ToList();

        // 선택된 Row들의 ID를 기틀 삼아, 팩토리에서 최종 데이터(등급, 스탯 확정본) 생성 및 대기실 저장
        foreach (var row in pickedRows)
        {
            StaffInitData candidateData = await _dataFactory.CreateDataByStaffIDAsync(row.Staff_ID, playerLevel);
            if (candidateData != null)
            {
                _recruitCandidates.Add(new RecruitCandidate()
                {
                    init = candidateData,
                    runtime = _dataFactory.CreateInitialRuntimeData(candidateData)
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
            unhiredViewList.Add(ConvertToViewData(candidateData.init, candidateData.runtime));
        }
    
        return unhiredViewList;
    }

    // 고용 버튼 눌렀을때 ViewList 중 해당 직원 뽑는 것은 UI쪽에서 처리? 
    
    // ## 채용 3단계
    // 최종 계약 확정 (UI 목록에서 버튼을 눌러 채용할 때 내부 데이터 업데이트 및 스태프 생성)
    public async UniTask ConfirmHireAsync(int targetStaffID, bool free = true)
    {
        // 최대 고용 인원 수 확인
        if (_hiredStaffList.Count >= maxHiredStaffCount) return;
        
        // 채용 후보 리스트에 해당 사번이 실제로 대기 중인지 체크.
        var targetData = _recruitCandidates.Find(c => c.init.Staff_ID == targetStaffID);
        if (targetData == null)
        {
            Debug.LogError($"사번 {targetStaffID}번 스태프는 현재 채용 후보 목록에 없습니다.");
            return;
        }
        Debug.Log($"[StaffManager] {targetData.init.Staff_Name} - 고용 절차 시작");
        
        // 후보 리스트에서 제거 후 정식 고용 리스트 및 딕셔너리로 이사 
        _recruitCandidates.Remove(targetData); 
        _hiredStaffList.Add(targetData.init);
        
        // runtimeData 새로만들어 넣어줘서 WithRuntimeData()에 매개변수로 클래스 참조로 대입하게 해서
        // StaffEntity의 대이터와 StaffManager의 데이터 동기화 시킴.
        _hiredRuntimeDataDict[targetData.init.Staff_ID] = targetData.runtime; 
        
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

        // 만든 직원 데이터 세이브 (테스트용, 나중에 삭제할 예정)
        var savable = newStaff as ISavableStaff;
        if (savable != null)
        {
            _savedInitData = savable.GetInitData();
            _savedRuntimeData = savable.GetRuntimeData();
        }

        if (!free)
        {
            var cost = _spawnedEntities[targetData.init.Staff_ID].GetSalary();
            ServiceLocater.Get<IGameManager>().AddMoney(cost * -1);
        }
        
        Debug.Log($"[{targetData.init.Staff_Name}] 정식 채용 및 오브젝트 생성 완료");
    }
    
    
    // 기존 직원 로드 파이프라인 (테스트 용)
    public async UniTaskVoid LoadStaffAsync()
    {
        if (_savedInitData == null)
        {
            Debug.LogWarning("저장된 직원 데이터가 없습니다. 우클릭 메뉴로 먼저 고용해주세요.");
            return;
        }

        Debug.Log("기존 직원 로드 시작");
        
        // 저장해둔 InitData와 RuntimeData를 빌더에 넣어서 스태프 생성. 
        IStaffInfo loadedStaff = await new StaffBuilder()
            .WithInitData(_savedInitData)
            .WithRuntimeData(_savedRuntimeData) // 런타임 데이터 주입
            .WithVisualAsset(tempCbtPrefab)     // 화면에 띄워야 하니 에셋은 로드함
            .BuildAsync(staffContainer);

        loadedStaff.DisplayInfo();
        Debug.Log("기존 직원 로드 및 씬 배치 완료");
    }
    
    // 직원 해고 함수 (UI에서 해고 누를 시 함수 호출)
    public async UniTask FireStaff(int targetStaffID)
    {
        // 고용 리스트에서 삭제.
        int delNum = _hiredStaffList.RemoveAll(x => x.Staff_ID == targetStaffID);
        Debug.Log($"[StaffManager] 총 {delNum} 명의 Staff 가 해고 되었습니다.");
        _hiredRuntimeDataDict.Remove(targetStaffID);
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

        foreach (var initData in _hiredStaffList)
        {
            _hiredRuntimeDataDict.TryGetValue(initData.Staff_ID, out var runtimeData);
        
            // 만들어둔 변환 함수 사용하여 초기데이터 + Runtime데이터 -> View용 데이터(StaffViewData)으로 데이터 전환. 
            viewDataList.Add(ConvertToViewData(initData, runtimeData));
        }

        return viewDataList; 
    }
    
    // 상태변경 전용 함수. 
    // Staff의 Current_State(Idle, Work)를 변경하여 고용된 직원의 상태를 변경.
    // (Staff의 고용 상태변경할때만 사용 다른 데이터들은 바로 아래 함수 ModifyStaffData 를 사용해서 StaffManager의 Data 변경)
    // 불필요시 삭제 예정.
    public void ChangeStaffState(int targetStaffID, StaffState newState)
    {
        if (_hiredRuntimeDataDict.TryGetValue(targetStaffID, out var runtimeData))
        {
            runtimeData.Current_State = newState;
            Debug.Log($"사번 {targetStaffID}번 직원이 {newState} 상태가 되었습니다.");
            
            // 애니메이션 처리? 
        }
        else
        {
            Debug.LogWarning("해당 직원을 찾을 수 없습니다.");
        }
    }
    
    // 특정 스태프의 아무 데이터나 변경시킬때 쓰는 범용 데이터 변경 함수.
    // StaffID를 받아서 해당 스태프의 변경된 StaffRuntimeData 값을 StaffManager에 반영시켜주는 함수. (StaffInitData도 변경은 가능하게 설계) 
    public void ModifyStaffData(int staffID, Action<StaffInitData, StaffRuntimeData> modifier)
    {
        //StaffID에 해당하는 InitData와 RuntimeData를 가져옴. (Init은 리스트, Runtime은 딕셔너리) 
        var initData = _hiredStaffList.Find(x => x.Staff_ID == staffID);
        if (initData != null && _hiredRuntimeDataDict.TryGetValue(staffID, out var runtimeData))
        {
            // 외부에서 넘겨준 수정 로직을 현재 StaffManager의 StaffID에 해당하는 사원의 initData, runtimeData에 반영. 
            modifier?.Invoke(initData, runtimeData);
            
            // 오브젝트의 애니메이션 변경 등 나중에 필요시 추가. 
            
            Debug.Log($"{staffID}번 사원 데이터 통합 업데이트 완료");
        }
        else
        {
            Debug.LogWarning($"{staffID}번 직원을 찾을 수 없어 갱신에 실패했습니다.");
        }
        
        // 해당 함수 사용법: 사용시 아래 코드 복붙 후 staffID에 실제 바꿀 스태프의 ID값을 넣고 람다식 안의 Data 수정할 부분만 바꿔주시면 됩니다.  
        
        // staffManager.ModifyStaffData(staffID, (StaffInitData initData, StaffRuntimeData runtimeData) => 
        // {
        //     runtimeData.Current_State = StaffState.Idle; // 해당 람다식 안에 데이터 수정할 부분만 넣어주시면 됩니다. 
        //     runtimeData.Added_Common_Concentration += 30; // 여러 개 동시 변경 가능. 모든 데이터 수정 가능. 
        //     // (InitData도 수정은 가능함)
        // });
    }
    
    // --------------------------
    
    
    // initData와 Runtime 데이터를 반영한 View용 StaffViewData 생성. 
    public StaffViewData ConvertToViewData(StaffInitData initData, StaffRuntimeData runtimeData)
    {
        if (runtimeData == null) 
        {
            runtimeData = new StaffRuntimeData();
        }

        StaffViewData viewData = new StaffViewData();
        
        // 매핑
        viewData.Staff_ID = initData.Staff_ID;
        viewData.Staff_Name = initData.Staff_Name;
        viewData.Staff_Gender = initData.Staff_Gender;
        viewData.Job_Name = initData.Job.ToString();
        viewData.Avatar_ID = initData.Avatar_ID; // 아직 에셋이 안나와서 팩토리 임시 랜덤값 바인딩
        
        viewData.Grade = initData.Grade.ToString();
        viewData.DISC_Type = initData.DISC_Type.ToString();
        
        viewData.Current_State = runtimeData.Current_State.ToString();
        viewData.Current_Level = runtimeData.Current_Level;
        viewData.Current_Exp = runtimeData.Current_Exp;
        
        viewData.Salary = initData.Salary;
        viewData.Hire_Cost = initData.Hire_Cost;
        
        viewData.Final_Career = initData.Base_Career + runtimeData.Added_Career;
        viewData.Final_Common_Concentration = initData.Base_Common_Concentration + runtimeData.Added_Common_Concentration;
        viewData.Final_Common_Creativity = initData.Base_Common_Creativity + runtimeData.Added_Common_Creativity;
        viewData.Final_Common_Communication = initData.Base_Common_Communication + runtimeData.Added_Common_Communication;
        viewData.Final_Job_Planning = initData.Base_Job_Planning + runtimeData.Added_Job_Planning;
        viewData.Final_Job_Development = initData.Base_Job_Development + runtimeData.Added_Job_Development;
        viewData.Final_Job_Art = initData.Base_Job_Art + runtimeData.Added_Job_Art;
        
        viewData.All_Tags.Add(initData.Fixed_Tag);
        
        if (runtimeData.Added_Tags != null && runtimeData.Added_Tags.Count > 0)
        {
            viewData.All_Tags.AddRange(runtimeData.Added_Tags);
        }

        return viewData;
    }
    
    
    public StaffEntity GetStaffEntity(int staffId)
    {
        _spawnedEntities.TryGetValue(staffId, out var entity);
        return entity;
    }

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

    // ------------------------------------------------------------------------
    
    
    // 컨텍스트 메뉴 테스트 코드
    [ContextMenu("직원 가챠 1회 테스트 (레벨 3고정)")]
    public void TestHireStaff()
    {
        
    }

    [ContextMenu("저장된 직원 로드 테스트")]
    public void TestLoadStaff()
    {
        // 저장된 직원 로드 강제 실행
        LoadStaffAsync().Forget();
    }

    // 채용 단계 테스트 (UI 작성 시 참조) 
    [ContextMenu("채용 1단게: 후보군 3명 생성(게임실행중에만 가능)")]
    public void ContextTest_Step1() => GenerateRecruitCandidatesAsync(playerLevel: 3, cardCount: 3).Forget();
    
    [ContextMenu("채용 2단계: 후보군 데이터 UI 검사")]
    public void ContextTest_Step2()
    {
        var list = GetAvailableStaffList();
        Debug.Log($"==채용 후보군 UI 렌더링 검사 ({list.Count}명)==");
        foreach (var v in list)
        {
            Debug.Log($"[ID: {v.Staff_ID}] 이름: {v.Staff_Name} | 직무: {v.Job_Name} | 등급: {v.Grade} | 연봉: {v.Salary} | 집중력합산: {v.Final_Common_Concentration} | 태그코드수: {v.All_Tags.Count}");
        }
    }

    [ContextMenu("채용 3단계: 첫 번째 후보 최종 고용하기")]
    public void ContextTest_Step3()
    {
        if (_recruitCandidates.Count > 0)
        {
            // 선택 된 후보 중 첫 번째 사람의 사번을 가져와서 고용 함수에 넘김
            ConfirmHireAsync(_recruitCandidates[0].init.Staff_ID).Forget();
        }
        else
        {
            Debug.LogWarning("대기실이 비어있습니다. 1단계를 먼저 실행해서 후보를 뽑아주세요");
        }
    }
    
    
    [ContextMenu("테스트 4: [동기화 검증] 첫 번째 직원 스탯 수정")]
    public void ContextTest_Step4_ModifyData()
    {
        if (_hiredStaffList.Count == 0)
        {
            Debug.LogWarning("고용된 직원이 없습니다 컨텍스트 메뉴에있는 채용 1단계, 3단계를 먼저 실행해주세요.");
            return;
        }

        // 고용된 첫 번째 직원의 사번 가져오기
        int targetID = _hiredStaffList[0].Staff_ID;

        // 씬에 스폰된 엔티티(3D 캐릭터)에서 직접 수정 전 데이터를 읽어옵니다.
        int beforeConcentration = _spawnedEntities[targetID].GetTotalConcentration();
        Debug.Log($"[수정 전] 사번 {targetID}의 집중력: {beforeConcentration}");

        // 만능 수정 함수(ModifyStaffData)를 호출하여 데이터를 조작합니다.
        ModifyStaffData(targetID, (init, runtime) => 
        {
            runtime.Added_Common_Concentration += 500; // 집중력 500 증가
            runtime.Current_State = StaffState.Working; // 상태도 변경
        });

        // 엔티티를 다시 조회하여 값이 바뀌었는지 확인합니다.
        int afterConcentration = _spawnedEntities[targetID].GetTotalConcentration();
        Debug.Log($"[수정 후] 씬에 있는 캐릭터가 인식하는 집중력: {afterConcentration}");
        
        if (afterConcentration == beforeConcentration + 500)
        {
            Debug.Log("[성공] 매니저와 씬 오브젝트 간의 힙 메모리 동기화가 완벽하게 작동합니다");
        }
    }

    [ContextMenu("테스트 5: 해고 검증 첫 번째 직원 해고")]
    public void ContextTest_Step5_FireStaff()
    {
        if (_hiredStaffList.Count == 0)
        {
            Debug.LogWarning("고용된 직원이 없습니다.");
            return;
        }

        int targetID = _hiredStaffList[0].Staff_ID;
        
        // 해고 함수 호출
        FireStaff(targetID);

        // 검증: 리스트에서 잘 지워졌는지, 씬에서 오브젝트가 날아갔는지 체크
        Debug.Log($"남은 고용 인원 수: {_hiredStaffList.Count}명");
        
        if (_spawnedEntities.ContainsKey(targetID) == false)
        {
            Debug.Log("딕셔너리 및 씬의 3D 오브젝트가 깔끔하게 파괴되었습니다");
        }
    }
    
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