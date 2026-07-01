using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

/// <summary>
/// 빌드할 때 생성.
/// 스태프가 가지는 인터페이스 기능 구현, MonoBehaviour를 상속받아 유니티에서 오브젝트로 존재할 수 있게 구현.
/// </summary>
public class StaffEntity : IStaffInfo, ISavableStaff
{
    // 빌더를 통해서 빌드할 때 StaffManager의 데이터를 참조로 데이터를 받아 StaffManager의 데이터와 동기화 되어있음.
    // 해당 InitData, RuntimeData 수정할 시 StaffManager에도 반영됨.  
    public StaffInitData init;
    public StaffRuntimeData runtime;
    public Action<bool, StaffEntity> OnLevelUp;
    private GameObject _gameObject;
    private GameObject _visualInstance;
    private IStaffDataManager _staffDataManager = ServiceLocater.Get<IStaffDataManager>();
    private StaffDataFactory _staffDataFactory = new ();
    private bool isSelectingTag;
    private int _seatId;

    public Sprite Thumbnail { get; private set; }
    private AsyncOperationHandle<Sprite> _thumbnailHandle;
    private bool _hasThumbnailHandle;
    
    // IStaffInfo 구현 (읽기 전용)
    public StaffMovement GetMovement() => _visualInstance.GetComponent<StaffMovement>();
    public GameObject GetGameObject() => _gameObject;
    public GameObject GetAvatar() => _visualInstance;
    public int GetStaffID() => init.Staff_ID;
    public string GetFullName() => init.Staff_Name;
    public JobType GetJob() => init.Job;
    public StaffGrade GetGrade() => init.Grade;
    public int GetSalary() => init.Salary;
    public int GetHireCost() => init.Hire_Cost;
    public DiscType GetDiscType() => init.DISC_Type;
    public void DisplayInfo() => Debug.Log($"[{init.Grade}급 {init.Job}] 사번:{init.Staff_ID} / 연봉:{init.Salary}");

    public int GetTotalCareer() => init.Base_Career + runtime.Added_Career;
    
    public int GetConcentration()
    {
        int state = init.Base_Common_Concentration;
        state += runtime.Added_Common_Concentration;
        return state;
    }

    public int GetCreativity()
    {
        int state = init.Base_Common_Creativity;
        state += runtime.Added_Common_Creativity;
        return state;
    }

    public int GetCommunication()
    {
        int state = init.Base_Job_Art;
        state += runtime.Added_Job_Art;
        return state;
    }

    public int GetDesign()
    {
        int state = init.Base_Job_Planning;
        state += runtime.Added_Job_Design;
        return state;
    }

    public int GetDevelopment()
    {
        int state = init.Base_Job_Development;
        state += runtime.Added_Job_Development;
        return state;
    }

    public int GetArt()
    {
        int state = init.Base_Job_Art;
        state += runtime.Added_Job_Art;
        return state;
    }
    public int GetSeatId() => _seatId;

    // Set 인터페이스 추가.
    public int SetSeatId(int seatId) => _seatId = seatId;
    public void SetGameObject(GameObject gameObject) => _gameObject = gameObject;
    
    public UniTask LevelUp(bool isAttachTag)
    {
        init.Level++;
        
        // Apply Common Status
        init.Base_Common_Communication = ApplyState(init.Base_Common_Communication, isCommon: true);
        init.Base_Common_Creativity = ApplyState(init.Base_Common_Creativity, isCommon: true);
        init.Base_Common_Concentration = ApplyState(init.Base_Common_Concentration, isCommon: true);

        // Apply Job Status
        init.Base_Job_Art = ApplyState(init.Base_Job_Art, isCommon: false);
        init.Base_Job_Development = ApplyState(init.Base_Job_Development, isCommon: false);
        init.Base_Job_Planning = ApplyState(init.Base_Job_Planning, isCommon: false);
        
        _staffDataFactory.CalculateCosts(init);
        // LevelUP UI 발생
        
        // Tag UI 발생
        if (isAttachTag)
        {
            var entity = new TagUIRenderData()
            {
                OnConfirmCallback = AddSelectedTag,
                StaffEntity = this
            };
            isSelectingTag = true;
            ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.TagSelectUI, entity);
        }
        UniTask.WaitUntil(() => !isSelectingTag);
        
        // 만렙 고정치
        if (init.Level >= 15)
            init.Exp = _staffDataManager.LevelExpList.Find(x => x.level == init.Level).cumulativeExp;
        return UniTask.CompletedTask;
    }

    private int ApplyState(int baseValue, bool isCommon)
    {
        int min = 0;
        int max = 0;
        if (isCommon)
        {
            min += _staffDataManager.LevelStatsDict[init.Level].Common_Min;
            max += _staffDataManager.LevelStatsDict[init.Level].Common_Max;
        }
        else
        {
            min += _staffDataManager.LevelStatsDict[init.Level].Job_Min;
            max += _staffDataManager.LevelStatsDict[init.Level].Job_Max;
        }
        if (baseValue < min) baseValue = min;
        return Random.Range(++baseValue, max);
    }
    
    // ISavableStaff 구현 (데이터 세이브, 로드용도)
    public StaffInitData GetInitData() => init;
    public StaffRuntimeData GetRuntimeData() => runtime;

    public void ApplyExp(float exp)
    {
        // 만렙이면 더 경험치 얻지 않음
        if (init.Level >= 15) return;
        
        init.Exp += exp;
    }

    public void AddSelectedTag(int tagId)
    {
        var tag = _staffDataManager.TagList.Find(x => x.Tag_Id == tagId);
        runtime.Added_Tags.Add(tag);
        _staffDataFactory.ApplyTagEffect(init, runtime, tag.Tag_A_Effect_Name, tag.Tag_A_Effect_Value, tag.Tag_A_Effect_Ratio);
        _staffDataFactory.ApplyTagEffect(init, runtime, tag.Tag_B_Effect_Name, tag.Tag_B_Effect_Value, tag.Tag_B_Effect_Ratio);
        isSelectingTag = false;
    }
    
    public void SetThumbnail(Sprite sprite, AsyncOperationHandle<Sprite> handle)
    {
        ReleaseThumbnail();
        Thumbnail = sprite;
        _thumbnailHandle = handle;
        _hasThumbnailHandle = handle.IsValid();
    }

    public void ReleaseThumbnail()
    {
        if (!_hasThumbnailHandle) return;
        Addressables.Release(_thumbnailHandle);
        _hasThumbnailHandle = false;
        Thumbnail = null;
    }

    public void SetVisualInstance(GameObject go) => _visualInstance = go;

    public void ReleaseVisualInstance()
    {
        if (_visualInstance == null) return;
        Addressables.ReleaseInstance(_visualInstance); // 어드레서블 인스턴스 해제 + 파괴
        _visualInstance = null; 
    }
}
