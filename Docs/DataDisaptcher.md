# Data Dispatcher

## 계기

- Object 간 데이터 전송에 대한 강한 연결을 없애고자 제작.
- Sender 와 Receiver 가 존재하며 단 방향 통신.
- 한번에 여러 데이터를 보내야 할 경우 `struct` 로 정의해서 보낼 것.

## 사용법

> 참고! Service Locater 로 등록되어 있음

### 채널 정의 (Enum)
```scharp
namespace DataDispatcher
{
    public enum Channel
    {
        // 채널명 기입
    }    
}

```
### 채널 등록 및 등록 해제 (Receiver)
```csharp
using DataDispatcher

// ...skip...
    ServiceLocater.Get<IPostManager>.Subscripbe<SOME_STRUCT>(Channel.Something, Foo)
    ServiceLocater.Get<IPostManager>.Unsubscripbe<SOME_STRUCT_FOR_REQUEST, SOME_STRUCT_FOR_RESPONSE>(Channel.Something, Foo)
//
```

### 데이터 전송하기 (Sender)
```csharp
using DataDispatcher

// ...skip...
    ServiceLocater.Get<IPostManager>.Post(Channel.Something, SOME_DATA)  // return 불가
    var someValue = ServiceLocater.Get<IPostManager>.Request(Channel.Something, SOME_DATA) // return 가능.
//
```