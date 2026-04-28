using UnityEngine;

/// <summary>
/// 카메라 패닝(중간 버튼 드래그), 줌(스크롤 휠), 경계 제한을 처리합니다.
/// Main Camera GameObject에 부착합니다.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float zoomSpeed      = 2f;
    public float minZoom        = 2f;
    public float maxZoom        = 20f;

    [Header("Pan Settings")]
    public float panSpeed       = 1f;
    public bool  enableEdgePan  = false;   // 화면 가장자리 패닝 (옵션)
    public float edgePanMargin  = 20f;     // 픽셀 단위

    [Header("Bounds")]
    public bool  useBounds      = true;
    public float boundsPadding  = 3f;      // 보드보다 얼마나 더 벗어날 수 있는지

    // ── 내부 상태 ─────────────────────────────────────────────────────────
    private Camera   _cam;
    private Vector3  _lastMousePos;
    private bool     _isPanning = false;
    private float    _boardHalfW, _boardHalfH;

    // ── 생명주기 ──────────────────────────────────────────────────────────
    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void Start()
    {
        var gm = GameManager.Instance;
        if (gm != null && PuzzleManager.Instance != null)
        {
            float pw    = PuzzleManager.Instance.pieceWorldSize;
            _boardHalfW = gm.columns * pw * 0.5f + boundsPadding;
            _boardHalfH = gm.rows    * pw * 0.5f + boundsPadding;
        }
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
        if (enableEdgePan) HandleEdgePan();
        if (useBounds) ClampCamera();
    }

    // ── 줌 (스크롤 휠) ────────────────────────────────────────────────────
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        _cam.orthographicSize = Mathf.Clamp(
            _cam.orthographicSize - scroll * zoomSpeed * 10f,
            minZoom, maxZoom);
    }

    // ── 패닝 (중간 버튼 또는 Alt + 왼쪽 버튼) ────────────────────────────
    private void HandlePan()
    {
        // 중간 버튼 드래그
        bool middleDown = Input.GetMouseButtonDown(2);
        bool middleHeld = Input.GetMouseButton(2);
        bool altLeft    = Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0);

        if (middleDown)
        {
            _isPanning    = true;
            _lastMousePos = Input.mousePosition;
        }

        if (!middleHeld && !altLeft) _isPanning = false;

        if (_isPanning || altLeft)
        {
            Vector3 delta = Input.mousePosition - _lastMousePos;
            delta        *= -panSpeed * _cam.orthographicSize / Screen.height;
            transform.Translate(new Vector3(delta.x, delta.y, 0f), Space.World);
            _lastMousePos = Input.mousePosition;
        }
    }

    // ── 가장자리 패닝 ─────────────────────────────────────────────────────
    private void HandleEdgePan()
    {
        Vector2 mp    = Input.mousePosition;
        Vector3 dir   = Vector3.zero;
        float   speed = panSpeed * _cam.orthographicSize * 0.5f * Time.deltaTime;

        if (mp.x < edgePanMargin)                   dir.x = -1f;
        else if (mp.x > Screen.width - edgePanMargin)  dir.x =  1f;
        if (mp.y < edgePanMargin)                   dir.y = -1f;
        else if (mp.y > Screen.height - edgePanMargin) dir.y =  1f;

        transform.Translate(dir * speed, Space.World);
    }

    // ── 카메라 경계 제한 ──────────────────────────────────────────────────
    private void ClampCamera()
    {
        float camH = _cam.orthographicSize;
        float camW = camH * _cam.aspect;

        float clampX = Mathf.Max(0f, _boardHalfW - camW);
        float clampY = Mathf.Max(0f, _boardHalfH - camH);

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -clampX - camW * 0.5f, clampX + camW * 0.5f);
        pos.y = Mathf.Clamp(pos.y, -clampY - camH * 0.5f, clampY + camH * 0.5f);
        transform.position = pos;
    }

    // ── 카메라 초기화 (퍼즐 보드 전체가 보이도록) ────────────────────────
    public void FitToBoard(int cols, int rows, float pieceSize)
    {
        float boardW = cols * pieceSize;
        float boardH = rows * pieceSize;

        float requiredH = boardH * 0.5f + boardH * 0.3f;   // 약간 여유
        float requiredW = (boardW * 0.5f + boardW * 0.3f) / _cam.aspect;

        _cam.orthographicSize = Mathf.Max(requiredH, requiredW);
        _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, minZoom, maxZoom);
        transform.position    = new Vector3(0f, 0f, -10f);
    }
}
