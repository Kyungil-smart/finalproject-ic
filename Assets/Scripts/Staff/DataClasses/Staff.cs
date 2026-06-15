using UnityEngine;

public class Staff : MonoBehaviour
{
    private StaffEntity _entity;
    
    public void SetEntity(StaffEntity entity)
    {
        _entity = entity;
        entity.SetGameObject(gameObject);
    }
}