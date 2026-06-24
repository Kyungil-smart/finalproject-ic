using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUIButtonRender : MonoBehaviour
{
    [SerializeField] private GameObject slotOn;
    [SerializeField] private GameObject slotOff;
    
    [Header("Slot On Info")]
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI projectCount;
    [SerializeField] private TextMeshProUGUI yearCount;
    [SerializeField] private TextMeshProUGUI saveAt;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private Image icon;  // ToDo. 추후 추가
    
    public void Render(SaveMeta meta)
    {
        if (meta == null)
        {
            slotOff.SetActive(true);
            slotOn.SetActive(false);
        }
        else
        {
            slotOff.SetActive(false);
            slotOn.SetActive(true);
            
            
        }
    }    
}
