using System;

namespace DataDispatcher
{
    public interface IPostManager
    {
        // Channel 등록
        public void Subscribe<T>(Channel key, Action<T> callback);
        public void Subscribe<TReq, TRes>(Channel key, Func<TReq, TRes> callback);
        // Channel 등록해제
        public void Unsubscribe<T>(Channel key, Action<T> callback);
        public void Unsubscribe<TReq, TRes>(Channel key, Func<TReq, TRes> callback);
        // 데이터 전송
        public void Post<T>(Channel key, T data);
        public TRes Request<TReq, TRes>(Channel key, TReq data);
    }    
}
