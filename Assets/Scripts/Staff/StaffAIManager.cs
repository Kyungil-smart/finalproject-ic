using System;
using System.Collections.Generic;
using UnityEngine;

public class StaffAIManager : Manager, IStaffAIManager
{
    [Serializable]
    public class FloorPatrolArea
    {
        public int floor;            // 층 번호 (= seatId / 10)
        public Transform leftEnd;
        public Transform rightEnd;
    }

    [Header("층별 배회 구간 (양 끝단)")]
    [SerializeField] private List<FloorPatrolArea> floorAreas = new();

    [Header("타이밍(초): x=최소, y=최대")]
    [SerializeField] private Vector2 sitDuration  = new Vector2(3f, 7f);
    [SerializeField] private Vector2 paceDuration = new Vector2(4f, 8f);
    [SerializeField] private float minPaceStep = 1f;

    private enum AIState { Sitting, Pacing, ReturningToSeat }

    private class Ctx
    {
        public StaffMovement movement;
        public Vector2 seatPos;
        public float leftX, rightX;
        public AIState state;
        public bool toRight;
        public float timer;
        public Action arrivedHandler;
    }

    private readonly List<Ctx> _ctxs = new();
    private readonly Dictionary<int, FloorPatrolArea> _floorMap = new();
    private bool _running;

    private void OnEnable()  => Register();
    private void OnDisable() { Stop(); Unregister(); }

    protected override void Register()   => ServiceLocater.Register<IStaffAIManager>(this);
    protected override void Unregister() => ServiceLocater.Unregister<IStaffAIManager>(this);

    // ===== 씬 진입 후 호출 (MainUIPostProcessing 끝에서) =====
    public void Begin()
    {
        Stop();                         // 재진입 안전: 기존 컨텍스트/구독/플래그 정리
        BuildFloorMap();

        var register = ServiceLocater.Get<IStaffRegister>();
        if (register == null) { Debug.LogError("[StaffAIManager] IStaffRegister 없음"); return; }

        foreach (var view in register.GetAllHiredStaffList())
        {
            var entity = register.GetStaffEntity(view.Staff_ID);
            if (entity == null) continue;

            var go = entity.GetGameObject();
            var movement = go != null ? go.GetComponentInChildren<StaffMovement>() : null;
            if (movement == null)
            { Debug.LogWarning($"[StaffAIManager] {view.Staff_ID}: StaffMovement 없음 → 스킵"); continue; }

            int floor = entity.GetSeatId() / 10;
            if (!_floorMap.TryGetValue(floor, out var area))
            { Debug.LogWarning($"[StaffAIManager] {view.Staff_ID}: 층 {floor} 구간 미설정 → 스킵"); continue; }

            var seatTr = register.GetSeatTransform(view.Staff_ID);
            if (seatTr == null)
            { Debug.LogWarning($"[StaffAIManager] {view.Staff_ID}: seat 좌표 없음 → 스킵"); continue; }

            var ctx = new Ctx
            {
                movement = movement,
                seatPos  = seatTr.position,
                leftX    = area.leftEnd.position.x,
                rightX   = area.rightEnd.position.x,
                state    = AIState.Sitting,
                timer    = Rand(sitDuration),     // stagger: 제각각 시작
            };

            movement.Warp(ctx.seatPos);           // ★ 무조건 자리에서 시작 + 플래그 초기화

            ctx.arrivedHandler = () => OnArrived(ctx);
            movement.OnArrived += ctx.arrivedHandler;

            _ctxs.Add(ctx);
        }

        _running = true;
        Debug.Log($"[StaffAIManager] Begin: {_ctxs.Count}명 착석 초기화");
    }

    public void Stop()
    {
        foreach (var ctx in _ctxs)
            if (ctx.movement != null && ctx.arrivedHandler != null)
                ctx.movement.OnArrived -= ctx.arrivedHandler;   // 구독 해제 (누수 방지)
        _ctxs.Clear();
        _running = false;
    }

    private void Update()
    {
        if (!_running) return;
        float dt = Time.deltaTime;

        foreach (var ctx in _ctxs)
        {
            if (ctx.movement == null) continue;
            switch (ctx.state)
            {
                case AIState.Sitting:
                    ctx.timer -= dt;
                    if (ctx.timer <= 0f) EnterPacing(ctx);
                    break;
                case AIState.Pacing:
                    ctx.timer -= dt;
                    if (ctx.timer <= 0f) EnterReturning(ctx);
                    break;
                case AIState.ReturningToSeat:
                    break;   // OnArrived가 처리
            }
        }
    }

    private void EnterPacing(Ctx ctx)
    {
        ctx.state = AIState.Pacing;
        ctx.timer = Rand(paceDuration);
        ctx.movement.StandUp();
        ctx.movement.MoveTo(RandomPaceX(ctx));
    }
    
    private float RandomPaceX(Ctx ctx)
    {
        float min = Mathf.Min(ctx.leftX, ctx.rightX);
        float max = Mathf.Max(ctx.leftX, ctx.rightX);
        float cur = ctx.movement.transform.position.x;
        float target;
        int guard = 0;
        do { target = UnityEngine.Random.Range(min, max); }
        while (Mathf.Abs(target - cur) < minPaceStep && ++guard < 5);   // 현재서 일정 거리 떨어진 곳
        return target;
    }

    private void EnterReturning(Ctx ctx)
    {
        ctx.state = AIState.ReturningToSeat;
        ctx.movement.MoveTo(ctx.seatPos.x);   // 자리 X까지 걸어옴
    }

    private void OnArrived(Ctx ctx)
    {
        switch (ctx.state)
        {
            case AIState.Pacing:                       // 끝단 도착 → 반대편 (왔다갔다)
                ctx.movement.MoveTo(RandomPaceX(ctx));
                break;
            case AIState.ReturningToSeat:              // 자리 X 도착 → 뿅 앉기
                ctx.movement.Sit(ctx.seatPos);
                ctx.state = AIState.Sitting;
                ctx.timer = Rand(sitDuration);
                break;
        }
    }

    private void BuildFloorMap()
    {
        _floorMap.Clear();
        foreach (var a in floorAreas)
            if (a != null && a.leftEnd != null && a.rightEnd != null)
                _floorMap[a.floor] = a;
    }

    private static float Rand(Vector2 r) => UnityEngine.Random.Range(r.x, r.y);
}