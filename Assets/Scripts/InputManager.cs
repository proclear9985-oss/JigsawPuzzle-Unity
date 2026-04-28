using UnityEngine;

/// <summary>
/// 마우스(PC) 및 터치(모바일) 입력을 통합 처리합니다.
/// PuzzlePiece.cs 의 OnMouseDown/Drag/Up 와 병행하여
/// 모바일 환경에서도 동일하게 동작하도록 보완합니다.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Settings")]
    public float pinchZoomSpeed    = 0.02f;   // 핀치 줌 감도
    public float tapTimeThreshold  = 0.2f;    // 탭 판정 시간 (초)
    public float tapMoveThreshold  = 10f;     // 탭 판정 최대 이동 거리 (픽셀)

    // ── 이벤트 ────────────────────────────────────────────────────────────
    public event System.Action<Vector2>         OnTap;          // 탭 (터치 1개)
    public event System.Action<Vector2>         OnDragBegin;
    public event System.Action<Vector2>         OnDragMove;
    public event System.Action<Vector2>         OnDragEnd;
    public event System.Action<float>           OnPinchZoom;    // 핀치 줌 델타
    public event System.Action<Vector2>         OnTwoPanMove;   // 두 손가락 패닝

    // ── 내부 상태 ─────────────────────────────────────────────────────────
    private Camera  _cam;
    private bool    _isDragging       = false;
    private Vector2 _touchStartPos;
    private float   _touchStartTime;
    private float   _prevPinchDist    = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _cam = Camera.main;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    // ── 마우스 입력 (PC / Editor) ─────────────────────────────────────────
    private void HandleMouseInput()
    {
        Vector2 mousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            _isDragging    = false;
            _touchStartPos  = mousePos;
            _touchStartTime = Time.time;
            OnDragBegin?.Invoke(mousePos);
        }

        if (Input.GetMouseButton(0))
        {
            if (Vector2.Distance(mousePos, _touchStartPos) > tapMoveThreshold)
                _isDragging = true;

            if (_isDragging)
                OnDragMove?.Invoke(mousePos);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!_isDragging && Time.time - _touchStartTime < tapTimeThreshold)
                OnTap?.Invoke(mousePos);

            OnDragEnd?.Invoke(mousePos);
            _isDragging = false;
        }

        // 스크롤 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
            OnPinchZoom?.Invoke(-scroll * 10f);
    }

    // ── 터치 입력 (모바일) ────────────────────────────────────────────────
    private void HandleTouchInput()
    {
        int touchCount = Input.touchCount;

        // ── 단일 터치: 드래그 / 탭 ─────────────────────────────────────
        if (touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    _isDragging     = false;
                    _touchStartPos  = t.position;
                    _touchStartTime = Time.time;
                    OnDragBegin?.Invoke(t.position);
                    break;

                case TouchPhase.Moved:
                    if (t.deltaPosition.magnitude > tapMoveThreshold)
                        _isDragging = true;
                    if (_isDragging)
                        OnDragMove?.Invoke(t.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (!_isDragging && Time.time - _touchStartTime < tapTimeThreshold)
                        OnTap?.Invoke(t.position);
                    OnDragEnd?.Invoke(t.position);
                    _isDragging = false;
                    break;
            }
        }

        // ── 두 손가락: 핀치 줌 + 패닝 ────────────────────────────────
        else if (touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float currentDist = Vector2.Distance(t0.position, t1.position);

            if (t1.phase == TouchPhase.Began)
            {
                _prevPinchDist = currentDist;
            }
            else
            {
                // 핀치 줌
                float delta = currentDist - _prevPinchDist;
                if (Mathf.Abs(delta) > 0.5f)
                    OnPinchZoom?.Invoke(-delta * pinchZoomSpeed);

                // 두 손가락 평균 이동 → 패닝
                Vector2 avgDelta = (t0.deltaPosition + t1.deltaPosition) * 0.5f;
                if (avgDelta.magnitude > 0.5f)
                    OnTwoPanMove?.Invoke(avgDelta);

                _prevPinchDist = currentDist;
            }
        }
    }

    // ── 스크린 → 월드 좌표 변환 유틸 ─────────────────────────────────────
    public Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 p = screenPos;
        p.z = Mathf.Abs(_cam.transform.position.z);
        return _cam.ScreenToWorldPoint(p);
    }
}
