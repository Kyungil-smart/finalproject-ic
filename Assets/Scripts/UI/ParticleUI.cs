using UnityEngine;
using UnityEngine.UI;


// 설명 : 유니티 내부 파티클 시스템이 물리 수학 계산을 담당하고, 실제 화면에 그리는 렌더링은 기본 파티클 시스템 대신 OnPopulateMesh가 담당하는 구조를 활용

// ParticleSystem, CanvasRenderer 가 반드시 붙어있어야 작동함
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(CanvasRenderer))]
public class ParticleUI : MaskableGraphic
{
    [Header("콘페티 이미지")]
    [SerializeField] private Texture _particleTexture; // 2x2 콘페티 텍스처 이미지(4개로 나뉘어 랜덤 출력됨)

    // 내부 캐싱 변수
    private ParticleSystem _ps; // 물리 연산을 담당할 파티클 시스템 컴포넌트
    private ParticleSystem.Particle[] _particles;   // 현재 살아있는 개별 파티클들의 데이터를 담아둘 메모리 배열
    private ParticleSystemRenderer _3dRenderer; // 기존 3D 공간에 파티클을 그리던 렌더러 컴포넌트

    // 유니티 UI 시스템이 메쉬를 그릴 때 기본 UI 텍스처 대신 우리가 등록한 콘페티 텍스처를 사용하도록 주입
    public override Texture mainTexture => _particleTexture != null ? _particleTexture : base.mainTexture;

    protected override void Awake()
    {
        base.Awake();
        _ps = GetComponent<ParticleSystem>();
        _3dRenderer = GetComponent<ParticleSystemRenderer>();

        // 3D 전용 렌더러는 끄고 데이터만 활용
        if (_3dRenderer != null) _3dRenderer.enabled = false;

        // 파티클 시스템 설정
        var main = _ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local; // UI 좌표계 매핑
        main.playOnAwake = false;   // 자동재생 안 하도록 설정

        // 파티클의 최대 개수만큼 배열을 한 번만 할당
        _particles = new ParticleSystem.Particle[main.maxParticles];
    }


    protected override void OnEnable()
    {
        base.OnEnable();

        // Awake 시점에 캐싱 누락시 대비한 안전장치
        if (_ps == null) _ps = GetComponent<ParticleSystem>();

        if (_ps != null)
        {
            _ps.Clear(); // 이전 연출이 있다면 생길 수 있는 잔상 제거
            _ps.Play();  // 패널이 켜지는 순간 파티클 시뮬레이션 시작
        }
    }


    private void Update()
    {
        // 파티클 시스템이 활성화되어 있고 현재 조각들이 살아서 움직이는 중에만 메쉬 갱신을 요청
        if (_ps != null && _ps.isPlaying)
        {
            SetVerticesDirty();
        }
    }


    // 세팅된 파티클의 원시 데이터를 받아 화면에 보일 실물 2D 사각형 조각들로 재조립
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); // 이전 프레임에 그려 두었던 메쉬 데이터 초기화
        if (_ps == null) return;

        // 현재 화면에 살아있는 실제 파티클의 총 개수
        int activeCount = _ps.GetParticles(_particles);

        for (int i = 0; i < activeCount; i++)
        {
            ParticleSystem.Particle p = _particles[i];

            // 파티클 시스템이 계산해준 실시간 중심 위치, 크기, 컬러 값을 추출
            Vector3 center = p.position;
            float halfSize = p.GetCurrentSize(_ps) * 0.5f;  // 중심점 기준 사방 크기
            Color32 color = p.GetCurrentColor(_ps); // 현재 색상

            // 2x2 이미지 슬라이스 UV 계산 (각 파티클 고유 시드로 4개 중 1개만 매핑) -> 이미지를 4등분하여 1개만 사용하기 위해 필요
            int frame = (int)(p.randomSeed % 4); // 0, 1, 2, 3 결정
            int col = frame % 2;    // 가로 칸
            int row = frame / 2;    // 세로 칸

            // 텍스처 좌표계(0.0 ~ 1.0) 가로세로를 반(0.5)으로 쪼개어 해당 파티클이 가질 사각형 UV 영역을 지정
            float uMin = col * 0.5f;
            float uMax = uMin + 0.5f;
            float vMin = row * 0.5f;
            float vMax = vMin + 0.5f;

            // [팔랑임 효과] 파티클 시스템 자체의 롤링 회전값(p.rotation) 반영
            float rotationRad = p.rotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rotationRad);
            float sin = Mathf.Sin(rotationRad);

            // 회전 전 기본 UI 사각형 모서리 좌표
            Vector2[] localCorners = new Vector2[4]
            {
                new Vector2(-halfSize, -halfSize), // 좌하
                new Vector2(-halfSize, halfSize),  // 좌상
                new Vector2(halfSize, halfSize),   // 우상
                new Vector2(halfSize, -halfSize)   // 우하
            };

            // 현재 VertexHelper에 추가될 정점의 시작 인덱스
            int startIndex = vh.currentVertCount;

            // 회전 행렬을 계산하여 4개의 정점 생성 및 UV 매핑
            for (int j = 0; j < 4; j++)
            {
                // 회전 수학 공식 적용
                float rotX = (localCorners[j].x * cos) - (localCorners[j].y * sin);
                float rotY = (localCorners[j].x * sin) + (localCorners[j].y * cos);

                // 최종 회전된 상대 좌표에 파티클의 실제 중심 위치(center)를 더해 최종 월드 UI 좌표를 완성
                Vector3 finalPos = new Vector3(center.x + rotX, center.y + rotY, 0);

                // 사각형 꼭짓점 순서에 맞춰 아까 쪼개둔 2x2 전용 UV 좌표를 1:1 매핑
                Vector2 uv = Vector2.zero;
                if (j == 0) uv = new Vector2(uMin, vMin);
                else if (j == 1) uv = new Vector2(uMin, vMax);
                else if (j == 2) uv = new Vector2(uMax, vMax);
                else if (j == 3) uv = new Vector2(uMax, vMin);

                vh.AddVert(finalPos, color, uv);
            }

            // 삼각형 2개 조립하여 사각형 생성 -> 이 과정이 끝나면 실제 렌더링이 일어남
            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }
    }
}

