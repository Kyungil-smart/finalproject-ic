using UnityEngine;

public class SlotController : MonoBehaviour
{
    [SerializeField] private Transform[] slotTransforms;

    private void Start()
    {
        ServiceLocater.Get<IStaffRegister>().SetSlotPos(slotTransforms);
    }
    
    public void SetSlot(int staffId, int slotId)
    {
        
    }
}