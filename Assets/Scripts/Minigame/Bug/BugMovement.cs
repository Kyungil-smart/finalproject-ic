using UnityEngine;
using DG.Tweening;


public class BugMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 4f;

    [Header("Map Boundary")]
    // 기획서 왼쪽 이미지의 회색 사각형 영역 크기에 맞게 인스펙터에서 설정
    [SerializeField] private Vector2 minBoundary = new Vector2(-1.9f, -4.6f);
    [SerializeField] private Vector2 maxBoundary = new Vector2(1.9f, 3.6f);
    [SerializeField] private float angleOffset;

    private Tween _moveTween;

    private void OnEnable()
    {
        // 오브젝트 풀에서 꺼내져 활성화될 때마다 움직임 시작
        StartRandomMove();
    }

    private void StartRandomMove()
    {
        // 1. 랜덤 방향 및 거리 계산으로 목적지 설정
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minDistance, maxDistance);
        Vector3 targetPosition = transform.position + (Vector3)(randomDirection * randomDistance);

        // 2. 맵 바깥으로 나가지 않도록 목적지 제한 
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBoundary.x, maxBoundary.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBoundary.y, maxBoundary.y);

        Vector3 moveDir = targetPosition - transform.position;
        if (moveDir.sqrMagnitude > 0.0001f)   // 제자리면 회전 스킵(Atan2(0,0) 방지)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + angleOffset);
        }
        
        // 3. 랜덤 속도 계산 및 이동 시간(Duration) 산출 (거리 / 속도 = 시간)
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        float duration = Vector3.Distance(transform.position, targetPosition) / randomSpeed;

        // 4. DoTween을 이용한 이동 실행
        // 기존 실행 중인 트윈이 있다면 안전하게 제거 후 생성
        _moveTween?.Kill();
        
        _moveTween = transform.DOMove(targetPosition, duration)
            .SetEase(Ease.Linear) // 일정한 속도로 이동
            .OnComplete(() =>
            {
                // 목적지에 도착하면 다시 새로운 랜덤 이동 시작 (재귀 루프)
                StartRandomMove();
            });
    }

    private void OnDisable()
    {
        // 오브젝트 풀로 반환될 때 실행 중인 트윈을 반드시 종료 (메모리 누수 및 오작동 방지)
        _moveTween?.Kill();
    }
}
