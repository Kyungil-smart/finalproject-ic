using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 게임 씬 시작시, 게임 실제 시작 전에 해야 할 것들에 대해 정의 한다.
/// - Save Data 에서 데이터 로딩
/// - 로딩된 데이터 기준으로 화면 랜더링 및 런타임 데이터 구성
/// - 그 밖에 씬에 등록해야 할 각종 Manager 나 Pure Class 등
/// Unity 생명주기에 따라 필요한 내용 추가해도 상관 없음.
/// 되도록 Unitask 를 이용하여 비동기로 진행할 것.
/// </summary>
public class InGameBootstrap : MonoBehaviour, IBootStrap
{
    private bool _isCompleted;
    public bool IsCompleted { get => _isCompleted; }
    private bool initializable;
    
    private void Awake()
    {
        SetDonDestroyOnLoad();
        if (initializable) Init();
    }

    private void OnDestroy() => Destory();
    
    private void Init()
    {   // 게임 씬에서 필요한 것들
        _isCompleted = false;
        Debug.Log("[InGameBootstrap] Start Init ... ");
        ServiceLocater.Register<IBootStrap>(this);
        Debug.Log("[InGameBootstrap] Start Init ... Register IBootStrap");
        UniTask.Void(async () =>
        {
            // Async 함수들은 여기에 적어주세요.
            await ServiceLocaterRegistration();
            _isCompleted = true;
        });
    }

    private void SetDonDestroyOnLoad()
    {
        transform.SetParent(null);
        var candidates = GameObject.FindGameObjectsWithTag("ddolObject");
        if (candidates.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        initializable = true;
        DontDestroyOnLoad(gameObject);
    }

    // private void Dispose()
    // {   // 게임 씬이 아닌 곳에서는 제거할 것들 (파괴가 아닙니다)
    //     UniTask.Void(async () =>
    //     {
    //         // Async 함수들은 여기에 적어주세요.
    //         await ServiceLocatorUnregistration();
    //         ServiceLocater.Unregister<IBootStrap>(this);
    //     });
    // }
    
    private void Destory()
    {   // 게임 씬이 아닌 곳에서 파괴가 필요한 것들.
        UniTask.Void(async () =>
        {
            // Async 함수들은 여기에 적어주세요.
        });
    }

    private UniTask ServiceLocaterRegistration()
    {
        ServiceLocater.Register<IUIRouter>(new UIRouter());
        Debug.Log("[InGameBootstrap] Start Init ... Register IUIRouter");
        // ToDo. 메모리 누수 발생 위험 있음. 추후 변경 필요.
        ServiceLocater.Register(new ManagementData());
        return UniTask.CompletedTask;
    }

    private UniTask ServiceLocatorUnregistration()
    {
        ServiceLocater.Unregister<IUIRouter>();
        Debug.Log("[InGameBootstrap] Start Init ... Unregister IUIRouter");
        return UniTask.CompletedTask;
    }
}