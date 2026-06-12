using Cysharp.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 스태프 데이터 빌딩 (빌드 패턴)
/// StaffInitData, StaffRuntimeData, 에셋 넣어서 게임 오브젝트로 빌드 . 
/// </summary>
public class StaffBuilder
{
    private StaffEntity _staffData;
    private StaffRuntimeData _runtimeData;
    private GameObject _visualPrefab;

    // StaffDataFactory가 만든 초기화 데이터 등록
    public StaffBuilder WithStaffData(StaffEntity data)
    {
        _staffData = data;
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
        if (_staffData == null) return null;

        // 유니티 씬에 껍데기 오브젝트 생성
        GameObject staffObj = new GameObject($"Staff_{_staffData.init.Staff_ID}_{_staffData.init.Job}");
        staffObj.transform.SetParent(parent);

        // 직군에 따라 IJobAction를 상속받는 직군 컴포넌트 부착
        switch (_staffData.init.Job)
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
        return _staffData;
    }
}