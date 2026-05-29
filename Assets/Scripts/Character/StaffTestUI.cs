using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
// 버튼으로 스태프 생성 테스트 
public class StaffTestUI : MonoBehaviour
{
    [Header("연동 컴포넌트")]
    public StaffManager staffManager;
    public Button spawnButton; // 스태프 뽑기 버튼

    private void Start()
    {
        spawnButton.onClick.AddListener(() => HireStaffWithLockAsync().Forget());
    }

    // 비동기 가챠 실행
    private async UniTask HireStaffWithLockAsync()
    {
        // 가챠 도중 버튼 연타 방지
        spawnButton.interactable = false;
        
        await staffManager.HireStaffAsync();

        // 고용 완료 후 버튼 다시 활성화
        spawnButton.interactable = true;
    }
}