# Service Locater - Managers

싱글톤 매니저들은 모두 해당 모듈을 이용하여 사용하는 것을 원칙으로 합니다.

## Interface 정의

```csharp
public Interface ISomeManager
{
    public void SomeMethod();
}

```

## 매니저 정의 / Service Locater 에 등록 및 등록 해제

```csharp
public class SomeManager : Manager, ISomeManager
{
    protected override Register() => ServiceLocater.Register<ISomeManager>(this);
    protected override Unregister() => ServiceLocater.Unregister<ISomeManager>();

    // 구현
    private void OnEnable() => Register();
    private void OnDiable() => Unregister();
}
```

## 실제 사용하기

```csharp
ServiceLocater.Get<ISomeManager>().SomeMethodd();
```
