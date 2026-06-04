using Cysharp.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 스태프 데이터 빌딩 (빌드 패턴)
/// StaffInitData, StaffRuntimeData, 에셋 넣어서 게임 오브젝트로 빌드 . 
/// </summary>
public class StaffBuilder
{
    private StaffInitData _initData;
    private StaffRuntimeData _runtimeData;
    private GameObject _visualPrefab;

    // StaffDataFactory가 만든 초기화 데이터 등록
    public StaffBuilder WithInitData(StaffInitData data)
    {
        _initData = data;
        return this;
    }

    // 런타임 데이터 등록 (로드할 때만 기존의 런타임 데이터를 등록. 그냥 스태프 생성할때는 등록과정 생략)
    public StaffBuilder WithRuntimeData(StaffRuntimeData data)
    {
        _runtimeData = data;
        return this;
    }

    // 에셋 등록 (향후 어드레서블 + Unitask 적용 예정)
    public StaffBuilder WithVisualAsset(GameObject prefab)
    {
        _visualPrefab = prefab;
        return this;
    }

    // 빌더에 등록된 데이터를 바탕으로 스태프 오브젝트를 생성.
    // 매개변수 parent는 하이래키창에 스태프를 담는 빈 상위 오브젝트(폴더). 폴더 안에 스태프 오브젝트를 생성.
    
    public async UniTask<IStaffInfo> BuildAsync(Transform parent)
    {
        if (_initData == null) return null;

        // 유니티 씬에 껍데기 오브젝트 생성
        GameObject staffObj = new GameObject($"Staff_{_initData.Staff_ID}_{_initData.Job}");
        staffObj.transform.SetParent(parent);

        // Monobehaviour를 상속받고 인터페이스 기능들을 구현한 StaffEntity 부착 및 데이터 주입.
        StaffEntity entity = staffObj.AddComponent<StaffEntity>();
        entity.Initialize(_initData, _runtimeData ?? new StaffRuntimeData());

        // 직군에 따라 IJobAction를 상속받는 직군 컴포넌트 부착
        switch (_initData.Job)
        {
            case JobType.Planner:   staffObj.AddComponent<PlannerAction>();   break;
            case JobType.Developer: staffObj.AddComponent<DeveloperAction>(); break;
            case JobType.Artist:    staffObj.AddComponent<ArtistAction>();    break;
        }

        // 캐릭터 에셋을 엔티티의 자식으로 부착 (향후 어드레서블 적용할 수 있게 변경)
        if (_visualPrefab != null)
        {
            // await Addressables.InstantiateAsync(...)
            await UniTask.Delay(500); // 나중에 고정된 시간이 아닌 위의 어드레서블 적용 코드로 변경
            GameObject.Instantiate(_visualPrefab, staffObj.transform);
        }

        // 외부에는 읽기 전용 인터페이스만 리턴 (캡슐화)
        return entity;
    }
}