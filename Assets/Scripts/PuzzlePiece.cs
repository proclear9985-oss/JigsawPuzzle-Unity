using UnityEngine;

/// <summary>
/// 개별 퍼즐 조각 – 드래그, 스냅, 회전, 그룹 이동을 담당합니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PuzzlePiece : MonoBehaviour
{
    // ── 이벤트 ────────────────────────────────────────────────────────────
    public event System.Action<PuzzlePiece> OnSnapped;

    // ── 공개 속성 ─────────────────────────────────────────────────────────
    public PieceData  Data         { get; private set; }
    public Vector2    CorrectPos   { get; private set; }
    public bool       IsPlaced     { get; private set; }
    public PieceGroup Group        { get; set; }        // 속한 그룹 (null 가능)

    // ── 직렬화 필드 ───────────────────────────────────────────────────────
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D  col2d;

    [Header("Settings")]
    [SerializeField] private float snapDist    = 0.35f;
    [SerializeField] private float snapScale   = 1.0f;   // 월드 크기

    // ── 내부 상태 ─────────────────────────────────────────────────────────
    private bool      _isDragging  = false;
    private Vector3   _dragOffset  = Vector3.zero;
    private Camera    _cam;
    private int       _defaultOrder;
    private const int DRAG_ORDER    = 100;
    private const int PLACED_ORDER  = -1;

    // ── 초기화 ────────────────────────────────────────────────────────────
    public void Initialize(PieceData data, Vector2 correctPos, float worldSize, float snapDistance)
    {
        Data       = data;
        CorrectPos = correctPos;
        snapDist   = snapDistance;
        snapScale  = worldSize;

        spriteRenderer      = GetComponent<SpriteRenderer>();
        col2d               = GetComponent<BoxCollider2D>();
        _cam                = Camera.main;

        spriteRenderer.sprite        = data.sprite;
        spriteRenderer.sortingOrder  = 0;
        _defaultOrder                = 0;

        // 콜라이더를 월드 크기에 맞춤
        col2d.size   = Vector2.one * worldSize;
        col2d.offset = Vector2.zero;

        // 스프라이트 크기를 worldSize 에 맞게 스케일 조정
        if (data.sprite != null)
        {
            float sprW = data.sprite.bounds.size.x;
            float scale = worldSize / Mathf.Max(sprW, 0.001f);
            transform.localScale = Vector3.one * scale;
        }
    }

    // ── Unity 이벤트 ──────────────────────────────────────────────────────
    void OnMouseDown()
    {
        if (IsPlaced) return;
        BeginDrag();
    }

    void OnMouseDrag()
    {
        if (!_isDragging) return;
        DoDrag();
    }

    void OnMouseUp()
    {
        if (!_isDragging) return;
        EndDrag();
    }

    // ── 마우스 우클릭 회전 ────────────────────────────────────────────────
    void Update()
    {
        if (IsPlaced) return;

        // 우클릭 또는 R 키 → 90° 회전
        if (Input.GetMouseButtonDown(1) && IsMouseOver())
            RotatePiece(90f);

        if (Input.GetKeyDown(KeyCode.R) && _isDragging)
            RotatePiece(90f);
    }

    // ── 드래그 ────────────────────────────────────────────────────────────
    private void BeginDrag()
    {
        _isDragging = true;

        Vector3 worldMouse = GetMouseWorldPos();
        _dragOffset        = transform.position - worldMouse;

        // 집어들기 효과음
        AudioManager.Instance?.PlayPickup();

        // 그룹이면 그룹 전체 끌어올리기
        if (Group != null)
            Group.BringToFront(DRAG_ORDER);
        else
            spriteRenderer.sortingOrder = DRAG_ORDER;
    }

    private void DoDrag()
    {
        Vector3 target = GetMouseWorldPos() + _dragOffset;
        target.z = 0f;

        if (Group != null)
        {
            Vector3 delta = target - transform.position;
            Group.Translate(delta);
        }
        else
        {
            transform.position = target;
        }
    }

    private void EndDrag()
    {
        _isDragging = false;

        // 그룹 소속이면 그룹 내 각 조각의 스냅을 검사
        if (Group != null)
        {
            Group.TrySnapAll();
            Group.ResetOrder(_defaultOrder);
        }
        else
        {
            TrySnap();
            spriteRenderer.sortingOrder = _defaultOrder;
        }
    }

    // ── 스냅 ──────────────────────────────────────────────────────────────
    public void TrySnap()
    {
        if (IsPlaced) return;

        float dist = Vector2.Distance(transform.position, CorrectPos);
        if (dist <= snapDist)
        {
            PlacePiece();
        }
    }

    public void PlacePiece()
    {
        if (IsPlaced) return;

        IsPlaced = true;
        transform.position = new Vector3(CorrectPos.x, CorrectPos.y, 0f);
        transform.rotation = Quaternion.identity;
        spriteRenderer.sortingOrder = PLACED_ORDER;
        col2d.enabled               = false;

        // 그룹 해제
        if (Group != null) { Group.RemovePiece(this); Group = null; }

        // 인접 조각과 연결 시도 (그룹핑)
        TryGroupWithNeighbors();

        OnSnapped?.Invoke(this);

        // 스냅 효과음
        AudioManager.Instance?.PlaySnap();

        // 스냅 이펙트 (간단한 스케일 펄스)
        StartCoroutine(SnapEffect());
    }

    // ── 그룹핑 ────────────────────────────────────────────────────────────
    private void TryGroupWithNeighbors()
    {
        // 이미 배치된 조각과는 그룹 불필요
        // 미배치 인접 조각과의 위치 근접 여부로 그룹 형성
        int[] neighborIds = GetNeighborIds();
        foreach (int nid in neighborIds)
        {
            var neighbor = PuzzleManager.Instance?.GetPiece(nid);
            if (neighbor == null || neighbor.IsPlaced) continue;

            float d = Vector2.Distance(
                (Vector2)transform.position  - CorrectPos,
                (Vector2)neighbor.transform.position - neighbor.CorrectPos);

            if (d < (PuzzleManager.Instance?.snapDistance ?? 0.35f) * 2f)
            {
                PieceGroup.MergeOrCreate(this, neighbor);
            }
        }
    }

    private int[] GetNeighborIds()
    {
        var pm   = PuzzleManager.Instance;
        var gm = GameManager.Instance;
        if (pm == null || gm == null) return new int[0];

        int cols = gm.columns;
        int rows = gm.rows;
        int col  = Data.col;
        int row  = Data.row;

        var ids = new System.Collections.Generic.List<int>();
        if (col > 0)        ids.Add(row * cols + (col - 1));
        if (col < cols - 1) ids.Add(row * cols + (col + 1));
        if (row > 0)        ids.Add((row - 1) * cols + col);
        if (row < rows - 1) ids.Add((row + 1) * cols + col);
        return ids.ToArray();
    }

    // ── 회전 ──────────────────────────────────────────────────────────────
    private void RotatePiece(float angle)
    {
        if (Group != null)
            Group.Rotate(angle);
        else
            transform.Rotate(0, 0, angle);
    }

    // ── 유틸 ──────────────────────────────────────────────────────────────
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = Mathf.Abs(_cam.transform.position.z);
        return _cam.ScreenToWorldPoint(mp);
    }

    private bool IsMouseOver()
    {
        Vector2 mp = _cam.ScreenToWorldPoint(Input.mousePosition);
        return col2d.OverlapPoint(mp);
    }

    // ── 스냅 이펙트 ───────────────────────────────────────────────────────
    private System.Collections.IEnumerator SnapEffect()
    {
        Vector3 original = transform.localScale;
        Vector3 big      = original * 1.15f;
        float   t        = 0f;

        while (t < 1f) { t += Time.deltaTime * 8f; transform.localScale = Vector3.Lerp(original, big, t); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.deltaTime * 8f; transform.localScale = Vector3.Lerp(big, original, t); yield return null; }
        transform.localScale = original;
    }

    // ── 저장용 상태 직렬화 ────────────────────────────────────────────────
    public PieceState GetState() => new PieceState
    {
        id       = Data.id,
        posX     = transform.position.x,
        posY     = transform.position.y,
        rotation = transform.eulerAngles.z,
        isPlaced = IsPlaced,
    };

    public void ApplyState(PieceState state)
    {
        transform.position = new Vector3(state.posX, state.posY, 0f);
        transform.rotation = Quaternion.Euler(0, 0, state.rotation);
        if (state.isPlaced) PlacePiece();
    }
}

// ── 저장용 데이터 클래스 ──────────────────────────────────────────────────────
[System.Serializable]
public class PieceState
{
    public int   id;
    public float posX, posY, rotation;
    public bool  isPlaced;
}
