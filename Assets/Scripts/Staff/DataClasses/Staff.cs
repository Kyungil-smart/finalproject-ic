using UnityEngine;

public class Staff : MonoBehaviour
{
    private StaffEntity _entity;
    
    public void SetEntity(StaffEntity entity)
    {
        _entity = entity;
        entity.SetGameObject(gameObject);
    }
    private void OnDestroy() => _entity = null;

    [ContextMenu("Show Data")]
    private void ShowData()
    {
        Debug.Log("------ Staff Status -----");
        Debug.Log($"Staff_ID: {_entity.init.Staff_ID}");
        Debug.Log($"Staff_Name: {_entity.init.Staff_Name}");
        Debug.Log($"Staff_Gender: {_entity.init.Staff_Gender}");
        Debug.Log($"Job: {_entity.init.Job}");
        Debug.Log($"Grade: {_entity.init.Grade}");
        Debug.Log($"DISC_Type: {_entity.init.DISC_Type}");
        Debug.Log("-------------------------");
    }
}