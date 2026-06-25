using UnityEngine;

public class SeatController : MonoBehaviour, ISeatSitable
{
    [SerializeField] private int slotNumber;
    [SerializeField] private Transform sitPos;

    public int SlotNumber => slotNumber;
    
    public void SetStaffToSeat(Staff staff)
    {
        staff.transform.position = sitPos.position;
    }
}