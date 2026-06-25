using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUIButtonRender : MonoBehaviour
{
    [SerializeField] private GameObject slotOn;
    [SerializeField] private GameObject slotOff;
    [SerializeField] private Button deleteBtn;
    
    [Header("Slot On Info")]
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI floor;
    [SerializeField] private TextMeshProUGUI projectCount;
    [SerializeField] private TextMeshProUGUI yearCount;
    [SerializeField] private TextMeshProUGUI saveAt;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private Image icon;  // ToDo. 추후 추가
    [SerializeField] private ConfirmMsgController confirmMsgController;
    
    private int _slotIndex;
    private void OnEnable() => deleteBtn.onClick.AddListener(ConfirmDelete);
    private void OnDisable() => deleteBtn.onClick.RemoveListener(ConfirmDelete);
    
    public void Render(int slotIndex, SaveMeta meta)
    {
        _slotIndex = slotIndex;
        if (meta == null)
        {
            slotOff.SetActive(true);
            slotOn.SetActive(false);
            deleteBtn.gameObject.SetActive(false);
        }
        else
        {
            slotOff.SetActive(false);
            slotOn.SetActive(true);
            deleteBtn.gameObject.SetActive(true);
            
            playerName.text   = meta.playerName;
            floor.text        = $"{meta.floor} F";    // "1F"/"2F" 라벨은 포맷/UI로 -> 데이터 잘못 매핑. 변경필요.
            projectCount.text = $"Projects: {meta.completedProjectCount}개";
            yearCount.text    = $"{meta.year} 년차";
            saveAt.text       = meta.savedAt;
            money.text        = meta.money.ToString("N0");
        }
    }

    private void ConfirmDelete()=> confirmMsgController.Render(9900038, DeleteSlotData);
    
    private void DeleteSlotData()
    {
        ServiceLocater.Get<ISaveManager>().DeleteSlot(_slotIndex);
        Render(_slotIndex, null);
    }
}
