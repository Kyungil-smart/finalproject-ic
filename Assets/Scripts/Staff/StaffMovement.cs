using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;   

[RequireComponent(typeof(Rigidbody2D))]
public class StaffMovement : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float arriveThreshold = 0.05f;

    [Header("앉기(뿅)")]
    [SerializeField] private float jumpPower = 0.5f;
    [SerializeField] private float jumpDuration = 0.25f;
    
    [Header("Sorting (SPUM UnitRoot의 SortingGroup)")]
    [SerializeField] private string moveSortingLayer = "ChrMoveLayer";
    [SerializeField] private string sitSortingLayer  = "ChrShitLayer";
    [SerializeField] private int moveSortingOrder = 0;
    [SerializeField] private int sitSortingOrder  = 5;
    
    [Header("시각/애니")]
    [SerializeField] private bool defaultFacingRight = true;

    private StaffEntity _entity;
    private Rigidbody2D _rb;
    private SortingGroup _sortingGroup;
    private float _speed;

    private bool _moving;
    private float _targetX;
    
    private SPUM_Prefabs _spum;
    private Transform _visual;          // UnitRoot (= SortingGroup.transform)
    private Vector3 _visualBaseScale;

    public bool IsMoving => _moving;
    public bool IsSeated { get; private set; }
    public event Action OnArrived;   // 목표 X 도달 통지 → AIManager가 다음 행동 결정

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sortingGroup = GetComponentInChildren<SortingGroup>(true);
        if (_sortingGroup != null)
        {
            _visual = _sortingGroup.transform;          // UnitRoot
            _visualBaseScale = _visual.localScale;
        }
        _spum = GetComponent<SPUM_Prefabs>();
        if (_spum != null) _spum.OverrideControllerInit(); 
    }

    public void SetEntity(StaffEntity entity) => _entity = entity;

    // 초기 착석: 연출 없이 즉시 자리에 배치 (중력 OFF + 정지)
    public void Warp(Vector2 pos)
    {
        _moving = false;
        _rb.linearVelocity = Vector2.zero;
        _rb.gravityScale = 0f;
        _rb.position = pos;
        transform.position = pos;   // 즉시 시각 반영
        IsSeated = true;
        ApplySorting(sitSortingLayer, sitSortingOrder); 
        PlayIdle();
    }

    // 배회: 목표 X까지 걷기 (중력 ON → 바닥에 서서 좌우 이동, 벽에 막힘)
    public void MoveTo(float targetX)
    {
        IsSeated = false;
        _rb.gravityScale = 1f;
        _targetX = targetX;
        _moving = true;
        _speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        ApplySorting(moveSortingLayer, moveSortingOrder);
        PlayMove();
    }

    public void Stop()
    {
        _moving = false;
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);   // 수평만 정지, 낙하는 유지
    }

    // 자리 도착 후: 뿅 점프해서 앉기 (중력 OFF + 고정)
    public void Sit(Vector2 seatPos)
    {
        _moving = false;
        _rb.linearVelocity = Vector2.zero;
        _rb.gravityScale = 0f;
        transform.DOJump(seatPos, jumpPower, 1, jumpDuration);
        IsSeated = true;
        ApplySorting(sitSortingLayer, sitSortingOrder);
        PlayIdle();
    }

    // 일어서기: 중력 ON → 바닥으로 내려옴 (이후 AIManager가 MoveTo 호출)
    public void StandUp()
    {
        IsSeated = false;
        _rb.gravityScale = 1f;
    }

    private void FixedUpdate()
    {
        if (!_moving) return;

        float dx = _targetX - _rb.position.x;
        if (Mathf.Abs(dx) <= arriveThreshold)
        {
            Stop();
            OnArrived?.Invoke();
            return;
        }

        float dir = Mathf.Sign(dx);
        Face(dir);
        _rb.linearVelocity = new Vector2(dir * _speed, _rb.linearVelocity.y);   // X 제어, Y는 중력+바닥
    }
    
    private void ApplySorting(string layer, int order)
    {
        if (_sortingGroup == null) return;
        _sortingGroup.sortingLayerName = layer;
        _sortingGroup.sortingOrder = order;
        
        if (_sortingGroup.sortingLayerID == 0 && layer != "Default")
            Debug.LogWarning($"[StaffMovement] Sorting Layer '{layer}' 없음(이름 확인). Default로 처리됨");
    }

    private void PlayMove()
    {
        if (_spum != null) 
            _spum.PlayAnimation(PlayerState.MOVE, 0);
    }

    private void PlayIdle()
    {
        if (_spum != null) 
            _spum.PlayAnimation(PlayerState.IDLE, 0);
    }
    
    private void Face(float dirX)
    {
        if (_visual == null || dirX == 0f) return;
        bool right = dirX > 0f;
        float sign = (right == defaultFacingRight) ? 1f : -1f;
        var s = _visualBaseScale;
        s.x = Mathf.Abs(_visualBaseScale.x) * sign;
        _visual.localScale = s;     // UnitRoot만 반전 → 콜라이더/물리 영향 없음
    }
    
    public void ResetMotion()
    {
        _moving   = false;
        IsSeated  = false;
        transform.DOKill();                 // 진행 중 DOJump 등 트윈 강제 종료
        _rb.linearVelocity  = Vector2.zero; // 관성 제거
        _rb.angularVelocity = 0f;
        _rb.gravityScale    = 0f;           // 낙하 방지(애니메이션 중 고정)
        if (_spum != null) _spum.PlayAnimation(PlayerState.IDLE, 0);
    }
}