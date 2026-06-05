using System;
using UnityEngine;

public abstract class Manager : MonoBehaviour
{
    private void Awake() => Init();
    
    protected virtual void Init()
    {
    }

    protected abstract void Register();
    protected abstract void Unregister();
}