// 런타임 전용 — 가변 상태. SO를 건드리지 않음.

using UnityEngine;

public class SlotState
{
    public int id;
    public int cost;
    public bool unlocked;       // 런타임에서만 변경
    public Transform pos;       // Main 씬에서 주입
    public int staffId;

    public bool IsRoom => id % 10 == 0;   // 방 슬롯 판별을 여기로 캡슐화

    public SlotState(SlotDef def)   // 정의로부터 생성
    {
        id = def.id;
        cost = def.cost;
    }
}