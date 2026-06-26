using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 스태프 데이터 빌딩 (빌드 패턴)
/// StaffInitData, StaffRuntimeData, 에셋 넣어서 게임 오브젝트로 빌드 . 
/// </summary>
public class StaffBuilder
{
    private StaffEntity _staffData;
    private GameObject _fallbackPrefab;   // 기존 _visualPrefab → 폴백 용도
    private string _addressableKey;
    private Vector3 _spawnPosition = new Vector3(9999f, 9999f, 0f);

    public StaffBuilder WithStaffData(StaffEntity data) { _staffData = data; return this; }
    public StaffBuilder WithVisualAsset(GameObject prefab) { _fallbackPrefab = prefab; return this; }
    public StaffBuilder WithAddressableKey(string key) { _addressableKey = key; return this; }
    public StaffBuilder WithSpawnPosition(Vector3 pos) { _spawnPosition = pos; return this; }

    // 빌더에 등록된 데이터를 바탕으로 스태프 오브젝트를 생성.
    // 매개변수 parent는 하이래키창에 스태프를 담는 빈 상위 오브젝트(폴더). 폴더 안에 스태프 오브젝트를 생성.
    
    public async UniTask<(IStaffInfo staffInfo, GameObject go)> BuildAsync(Transform parent)
    {
        if (_staffData == null) return (null, null);

        // 유니티 씬에 껍데기 오브젝트 생성
        GameObject staffObj = new GameObject($"Staff_{_staffData.init.Staff_ID}_{_staffData.init.Job}");
        staffObj.transform.SetParent(parent);
        GameObject go = null;
        // 1) 어드레서블 키로 SPUM 프리팹 인스턴스화
        if (!string.IsNullOrEmpty(_addressableKey))
        {
            try
            {
                go = await Addressables.InstantiateAsync(_addressableKey, staffObj.transform).ToUniTask();
                _staffData.SetVisualInstance(go); // ↓ ③ 해제용으로 보관 (어드레서블 인스턴스만)
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[StaffBuilder] 프리팹 로드 실패({_addressableKey}) → 폴백: {e.Message}");
            }
        }

        // 2) 실패 시 폴백 프리팹 (어드레서블 아님 → 해제 추적 안 함)
        if (go == null && _fallbackPrefab != null)
            go = Object.Instantiate(_fallbackPrefab, staffObj.transform);

        // 3) Staff 컴포넌트 보장 (SPUM 프리팹엔 보통 Staff 스크립트가 없음)
        if (go != null)
        {
            var movement = go.GetComponent<StaffMovement>();   // SPUM 루트에 있음
            if (movement == null)
            {
                Debug.LogError($"[StaffBuilder] {_addressableKey}: StaffMovement 없음(폴백/프리팹 확인). 주입 생략");
            }
            else
            {
                var staffComp = staffObj.AddComponent<Staff>();
                staffComp.SetEntity(_staffData, movement);
            }
        }

        return (_staffData, staffObj);
    }
}