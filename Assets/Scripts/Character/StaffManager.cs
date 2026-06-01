using UnityEngine;
using Cysharp.Threading.Tasks;

public class StaffManager : MonoBehaviour, IStaffHireService
{
    [Header("스태프 생성 설정")]
    public Transform staffContainer;      // 직원들 모아둘 부모 폴더
    public GameObject tempCbtPrefab;      // 임시 캐릭터 프리팹

    // 세이브/로드 테스트용 임시 메모리
    private StaffInitData _savedInitData; 
    private StaffRuntimeData _savedRuntimeData;
    
    private StaffDataFactory _dataFactory = new StaffDataFactory();
    
    // 신규 직원 영입 파이프라인 (팩토리 가챠 -> 빌더 조립)
    public async UniTask HireStaffAsync(int playerLevel)
    {
        Debug.Log("직원 채용 시작"); 

        // 팩토리: 랜덤 가챠 데이터 생성 (비동기 대기)
        StaffInitData newData = await _dataFactory.CreateRandomDataAsync(playerLevel);
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

        // 로드 테스트를 위한 데이터 스냅샷 임시 저장 (나중에는 직원들 모아둘 부모 폴더(staffContainer)에 있는 직원들 불러와서 하면 될것 같음)
        var savable = newStaff as ISavableStaff;
        if (savable != null)
        {
            _savedInitData = savable.GetInitData();
            _savedRuntimeData = savable.GetRuntimeData();
        }
        
        Debug.Log("직원 채용 및 씬 배치 완료");
    }

    
    // 기존 직원 로드 파이프라인 (다이렉트 빌더 조립)
    // 나중에 다수 로드할 수 있게 수정할 예정. 
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
}