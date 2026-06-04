using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class StaffManager : MonoBehaviour, IStaffHireService, IStaffRegister, IStaffRecruit
{
    [Header("스태프 생성 설정")]
    public Transform staffContainer;      // 직원들 모아둘 부모 폴더 (하이러키 창에서)
    public GameObject tempCbtPrefab;      // 임시 캐릭터 프리팹 ( 나중에 아바타 시트, 어드레서블 적용) 

    // 세이브/로드 테스트용 임시 메모리 (테스트용, 나중에 삭제) 
    private StaffInitData _savedInitData; 
    private StaffRuntimeData _savedRuntimeData;
    
    // 세이브/로드용 고용된 리스트
    private List<StaffInitData> _hiredStaffList = new List<StaffInitData>();
    private Dictionary<int, StaffRuntimeData> _hiredRuntimeDataDict = new Dictionary<int, StaffRuntimeData>(); // 런타임 데이터용. 키값은 StaffID
    
    // 가챠 버튼 눌렀을 때 생성되어 UI 후보 리스트 창에 띄워질 실제 후보들 런타임 데이터
    private List<StaffInitData> _recruitCandidates = new List<StaffInitData>();

    private StaffDataFactory _dataFactory = new StaffDataFactory();
    
    // 채용 프로세스 ----------------
    
    // ## 채용 1단계
    // 카드 수에 채용 후보 리스트 생성 (가챠 UI 창 열거나 새로고침 시 호출)
    public async UniTask GenerateRecruitCandidatesAsync(int playerLevel, int cardCount) 
    {
        Debug.Log($"신규 채용 후보 {cardCount}명 생성 시작...");
        _recruitCandidates.Clear(); // 이전 후보 데이터 초기화

        var dataManager = ServiceLocater.Get<StaffDataManager>();
        if (dataManager == null || dataManager.StaffList == null) return;

        // 현재 고용되지 않은 스태프들의 원본 Row 목록만 필터링
        HashSet<int> hiredIDs = new HashSet<int>(_hiredRuntimeDataDict.Keys);
        var unhiredRows = dataManager.StaffList.Where(s => !hiredIDs.Contains(s.Staff_ID)).ToList();

        if (unhiredRows.Count == 0)
        {
            Debug.LogWarning("더 이상 가챠로 뽑을 수 있는 미고용 스태프가 원본 데이터에 없습니다.");
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
                _recruitCandidates.Add(candidateData); // 대기실 리스트업
            }
        }

        Debug.Log($"채용 후보 데이터 {_recruitCandidates.Count}명 확정 셋팅 완료.");
    }
    
    // ## 채용 2단계 
    // IStaffRecruit 구현: UI용. 전체 스태프 중 고용되지 않은 스태프 리스트를 StaffViewData 형태로 반환. 
    public List<StaffViewData> GetAvailableStaffList()
    {
        List<StaffViewData> unhiredViewList = new List<StaffViewData>();
    
        // 팩토리가 스탯 연산을 마친 진짜 후보 리스트를 순회하며 뷰용 데이터로 변환.
        foreach (var candidateData in _recruitCandidates) //_recruitCandidates: 팩토리로 InitData 생성한 데이터
        {
            // 아직 미고용 상태이므로 런타임 데이터는 빈 객체 대입 
            StaffRuntimeData emptyRuntimeData = new StaffRuntimeData();
            
            // 이름, 직군같은 기본 StatsRow 정보뿐 아니라, 팩토리로 계산된 InitData도 포함해서 ViewData로 저장.
            unhiredViewList.Add(ConvertToViewData(candidateData, emptyRuntimeData));
        }
    
        return unhiredViewList;
    }
    
    // 고용 버튼 눌렀을때 ViewList 중 해당 직원 뽑는 것은 UI쪽에서 처리? 
    
    // ## 채용 3단계
    // 최종 계약 확정 (UI 목록에서 버튼을 눌러 채용할 때 사용)
    public async UniTask ConfirmHireAsync(int targetStaffID)
    {
        // 채용 후보 리스트에 해당 사번이 실제로 대기 중인지 체크.
        var targetData = _recruitCandidates.Find(c => c.Staff_ID == targetStaffID);
        if (targetData == null)
        {
            Debug.LogError($"사번 {targetStaffID}번 스태프는 현재 채용 후보 목록에 없습니다.");
            return;
        }
        

        // 후보 리스트에서 제거 후 정식 고용 리스트 및 딕셔너리로 이사
        _recruitCandidates.Remove(targetData);
        _hiredStaffList.Add(targetData);
        _hiredRuntimeDataDict[targetData.Staff_ID] = new StaffRuntimeData();

        // 빌더 파이프라인으로 실제 캐릭터 프리팹 생성 및 배치
        IStaffInfo newStaff = await new StaffBuilder()
            .WithInitData(targetData)
            .WithVisualAsset(tempCbtPrefab) 
            .BuildAsync(staffContainer);

        newStaff.DisplayInfo(); 
        if (((Component)newStaff).TryGetComponent(out IJobAction job))
        {
            job.DoWork(); 
        }

        // 만든 직원 데이터 세이브 (테스트용, 나중에 삭제할 예정)
        var savable = newStaff as ISavableStaff;
        if (savable != null)
        {
            _savedInitData = savable.GetInitData();
            _savedRuntimeData = savable.GetRuntimeData();
        }
        
        Debug.Log($"[{targetData.Staff_Name}] 정식 채용 및 오브젝트 생성 완료");
    }
    
    // --------
    
    // 무작위 신규 직원 영입 파이프라인 (팩토리 가챠 -> 빌더 조립) (현재 사용안하는 중 테스트 용)
    public async UniTask HireStaffAsync(int playerLevel)
    {
        Debug.Log("직원 채용 시작"); 

        HashSet<int> currentHiredIDs = new HashSet<int>(_hiredRuntimeDataDict.Keys);
        
        // 팩토리: 랜덤 가챠 데이터 생성 (비동기 대기)
        StaffInitData newData = await _dataFactory.CreateRandomDataAsync(playerLevel, currentHiredIDs);
        if (newData == null) return;
        
        Debug.Log($"가챠 완료 에셋 로딩 중... (설정된 연봉: {newData.Salary})");

        // 빌더: InitData와 에셋을 주입하여 조립 (비동기 대기)
        // 신규 생성 시에는 RuntimeData 주입을 생략(RuntimeData의 기본값 적용)
        IStaffInfo newStaff = await new StaffBuilder()
            .WithInitData(newData)
            .WithVisualAsset(tempCbtPrefab) 
            .BuildAsync(staffContainer);

        // 완료 후 테스트 출력
        newStaff.DisplayInfo(); 
        if (((Component)newStaff).TryGetComponent(out IJobAction job))
        {
            job.DoWork(); 
        }

        // 만든 직원 데이터 세이브
        var savable = newStaff as ISavableStaff;
        if (savable != null)
        {
            _hiredStaffList.Add(savable.GetInitData());
            _hiredRuntimeDataDict[newData.Staff_ID] = savable.GetRuntimeData();
            
            // 로드 테스트를 위한 데이터 스냅샷 임시 저장
            _savedInitData = savable.GetInitData();
            _savedRuntimeData = savable.GetRuntimeData();
        }
        
        Debug.Log("직원 채용 및 씬 배치 완료");
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
    
    
    
    // IStaffRegister 구현: 고용된 직원 확인 기능
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

    // ------------------------------------------------------------------------
    // 컨텍스트 메뉴 테스트 코드
    [ContextMenu("직원 가챠 1회 테스트 (레벨 3고정)")]
    public void TestHireStaff()
    {
        HireStaffAsync(3).Forget(); 
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
            ConfirmHireAsync(_recruitCandidates[0].Staff_ID).Forget();
        }
        else
        {
            Debug.LogWarning("대기실이 비어있습니다. 1단계를 먼저 실행해서 후보를 뽑아주세요");
        }
    }
    
    
}