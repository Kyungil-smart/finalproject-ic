using UnityEngine;
using UnityEngine.SceneManagement;

public class Staff : MonoBehaviour
{
    private StaffEntity _entity;
    private StaffMovement _movement;
    
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        
    private void OnDisable()=> SceneManager.sceneLoaded -= OnSceneLoaded;   // 누수/중복 방지
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => UpdateStaffVisibility(scene.name);
    
    private void UpdateStaffVisibility(string sceneName) => gameObject.SetActive(sceneName == "MainScene");
    
    public void SetEntity(StaffEntity entity, StaffMovement movement)
    {
        _entity = entity;
        _movement = movement;
        _movement.SetEntity(entity);
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