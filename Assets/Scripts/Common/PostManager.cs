﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataDispatcher
{
    class VoidMessage<T> : IPostContainer
    {
        public Action<T> actions;
    }

    class RequestMessage<TReq, TRes> : IPostContainer
    {
        public Func<TReq, TRes> function;
    }

    /// <summary>
    /// 객체간 Data Communication 을 위한 중앙 관리 매니저
    /// </summary>
    public class PostManager : Manager, IPostManager
    {
        private Dictionary<Channel, IPostContainer> _subscribes = new ();

        private void OnEnable() => Register();
        private void OnDisable() => Unregister();

        /// <summary>
        /// 함수 구독  
        /// </summary>
        /// <param name="key">데이터 키</param>
        /// <param name="callback">Callback 함수</param>
        /// <typeparam name="T">데이터 타입</typeparam>
        public void Subscribe<T>(Channel key, Action<T> callback)
        {
            if (!_subscribes.TryGetValue(key, out var postMessages))
            {
                postMessages = new VoidMessage<T>();
                _subscribes[key] = postMessages;
            }

            if (postMessages is VoidMessage<T> pm)
            {
                pm.actions += callback;
            }
        }
        public void Subscribe<TReq, TRes>(Channel key, Func<TReq, TRes> callback)
        {
            if (!_subscribes.TryGetValue(key, out var postMessages))
            {
                postMessages = new RequestMessage<TReq, TRes>();
                _subscribes[key] = postMessages;
            }

            if (postMessages is RequestMessage<TReq, TRes> pm)
            {
                pm.function = callback;
            }
        }
        
        /// <summary>
        /// 함수 구취
        /// </summary>
        /// <param name="key">데이터 키</param>
        /// <param name="callback">Callback 함수</param>
        /// <typeparam name="T">데이터 타입</typeparam>
        public void Unsubscribe<T>(Channel key, Action<T> callback)
        {
            if (_subscribes.TryGetValue(key, out var postMessages) && postMessages is VoidMessage<T> pm)
                pm.actions -= callback;
        }
        public void Unsubscribe<TReq, TRes>(Channel key, Func<TReq, TRes> callback)
        {
            if (_subscribes.TryGetValue(key, out var postMessages) && postMessages is RequestMessage<TReq, TRes> pm)
                pm.function = null;
        }

        /// <summary>
        /// 데이터 변경 알림
        /// </summary>
        /// <param name="key">데이터 키</param>
        /// <param name="data">변경된 데이터</param>
        /// <typeparam name="T">데이터 타입</typeparam>
        public void Post<T>(Channel key, T data)
        {
            if (!_subscribes.TryGetValue(key, out var postMessages))
            {
                postMessages = new VoidMessage<T>();
                _subscribes[key] = postMessages;
            }
            (postMessages as VoidMessage<T>)?.actions?.Invoke(data);
        }
        
        /// <summary>
        /// 데이터 변경에 따른 자료 요청. (Only 1:1 만 사용 가능)
        /// </summary>
        /// <param name="key">데이터 키</param>
        /// <param name="data">요청 데이터</param>
        /// <typeparam name="T">데이터 타입</typeparam>
        public TRes Request<TReq, TRes>(Channel key, TReq data)
        {
            if (!_subscribes.TryGetValue(key, out var postMessages))
            {
                postMessages = new RequestMessage<TReq, TRes>();
                _subscribes[key] = postMessages;
            }
            if (postMessages is RequestMessage<TReq, TRes> pm)
            {
                if (pm.function != null)
                {
                    return pm.function.Invoke(data);
                }
            }
            return default;
        }

        protected override void Register() => ServiceLocater.Register<IPostManager>(this);
        protected override void Unregister() => ServiceLocater.Unregister<IPostManager>(this);
    }
}
