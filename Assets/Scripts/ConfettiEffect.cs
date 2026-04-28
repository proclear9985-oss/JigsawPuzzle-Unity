using UnityEngine;

/// <summary>
/// 퍼즐 완성 시 화면에 색종이 파티클 효과를 연출합니다.
/// ParticleSystem이 없으면 코드로 직접 생성합니다.
/// </summary>
public class ConfettiEffect : MonoBehaviour
{
    public static ConfettiEffect Instance { get; private set; }

    [Header("Particle Settings")]
    public int   burstCount   = 150;
    public float speed        = 5f;
    public float lifetime     = 3f;
    public float spread       = 6f;

    private ParticleSystem _ps;

    // 색종이 색상 팔레트
    private static readonly Color[] Colors =
    {
        new Color(1f,   0.2f, 0.2f),
        new Color(0.2f, 0.8f, 0.2f),
        new Color(0.2f, 0.5f, 1f),
        new Color(1f,   0.9f, 0.1f),
        new Color(1f,   0.4f, 0.9f),
        new Color(0.2f, 0.9f, 0.9f),
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildParticleSystem();
    }

    // ── ParticleSystem 코드 생성 ─────────────────────────────────────────
    private void BuildParticleSystem()
    {
        _ps = GetComponent<ParticleSystem>();
        if (_ps == null) _ps = gameObject.AddComponent<ParticleSystem>();

        // 자동 재생 끄기
        var main = _ps.main;
        main.playOnAwake       = false;
        main.loop              = false;
        main.duration          = 0.3f;
        main.startLifetime     = lifetime;
        main.startSpeed        = speed;
        main.startSize         = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
        main.maxParticles      = burstCount;
        main.simulationSpace   = ParticleSystemSimulationSpace.World;
        main.gravityModifier   = 0.4f;

        // 색상 무작위
        var col = _ps.colorOverLifetime;
        col.enabled = false;

        // 방사 설정
        var emission = _ps.emission;
        emission.enabled = false;  // Burst 방식 사용

        // 모양 – 원뿔형 분사
        var shape = _ps.shape;
        shape.enabled     = true;
        shape.shapeType   = ParticleSystemShapeType.Cone;
        shape.angle       = 60f;
        shape.radius      = 0.1f;

        // 렌더러
        var renderer = _ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        // 기본 머티리얼 사용 (Unity Default-Particle)
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
    }

    // ── 공개 API ─────────────────────────────────────────────────────────
    /// <summary>화면 중앙 상단에서 색종이 폭죽 발사</summary>
    public void Play()
    {
        // 화면 상단 중앙 월드 좌표
        Camera cam = Camera.main;
        Vector3 topCenter = cam != null
            ? cam.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 10f))
            : Vector3.up * 5f;

        transform.position = topCenter;

        // 버스트 방식으로 한 번에 발사
        _ps.Emit(burstCount);
        _ps.Play();

        // 여러 위치에서 동시 발사 (화면 양쪽)
        if (cam != null)
        {
            EmitAt(cam.ViewportToWorldPoint(new Vector3(0.1f, 0.9f, 10f)), burstCount / 3);
            EmitAt(cam.ViewportToWorldPoint(new Vector3(0.9f, 0.9f, 10f)), burstCount / 3);
        }
    }

    private void EmitAt(Vector3 pos, int count)
    {
        var emitParams      = new ParticleSystem.EmitParams();
        emitParams.position = pos;
        _ps.Emit(emitParams, count);
    }

    public void Stop() => _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
}
