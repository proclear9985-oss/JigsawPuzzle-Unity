using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 퍼즐 조각 전체를 생성/배치/관리하는 핵심 매니저
/// GameScene에 배치된 빈 GameObject에 부착합니다.
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("References")]
    public GameObject  piecePrefab;          // PuzzlePiece 프리팹
    public Transform   boardRoot;            // 조각들의 부모 Transform
    public Transform   scatterRoot;          // 흩뿌려진 조각 부모

    [Header("Board Settings")]
    public float pieceWorldSize = 1.0f;      // 조각 1개의 월드 크기(정사각형 기준)
    public float snapDistance   = 0.35f;     // 스냅 감지 거리

    [Header("Runtime")]
    public int totalPieces     = 0;
    public int placedPieces    = 0;

    // 전체 조각 딕셔너리 (id → PuzzlePiece)
    private Dictionary<int, PuzzlePiece> _pieces = new();

    // 이벤트
    public event System.Action<int, int> OnProgressChanged;   // (placed, total)
    public event System.Action           OnPuzzleCompleted;

    // ── 생명주기 ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.selectedImage == null)
        {
            Debug.LogWarning("[PuzzleManager] GameManager 또는 이미지 없음. 데모 모드 실행.");
            return;
        }

        // 저장 데이터가 있으면 이어하기, 없으면 새 게임
        if (GameSaveLoad.HasSaveData())
            LoadGame();
        else
            GeneratePuzzle(gm.selectedImage, gm.columns, gm.rows);
    }

    // ── 퍼즐 생성 ─────────────────────────────────────────────────────────
    public void GeneratePuzzle(Texture2D image, int cols, int rows)
    {
        ClearBoard();

        List<PieceData> pieceDataList = ImageSlicer.Slice(image, cols, rows);
        totalPieces  = pieceDataList.Count;
        placedPieces = 0;

        // 보드 중심을 원점으로 계산
        float boardW = cols * pieceWorldSize;
        float boardH = rows * pieceWorldSize;
        Vector2 boardOrigin = new Vector2(-boardW * 0.5f, -boardH * 0.5f);

        foreach (var data in pieceDataList)
        {
            // 정답 위치
            Vector2 correctPos = boardOrigin + new Vector2(
                (data.col + 0.5f) * pieceWorldSize,
                (data.row + 0.5f) * pieceWorldSize);

            // 조각 오브젝트 생성
            GameObject go = Instantiate(piecePrefab, scatterRoot);
            go.name = $"Piece_{data.id:000}";

            var piece = go.GetComponent<PuzzlePiece>();
            piece.Initialize(data, correctPos, pieceWorldSize, snapDistance);

            // 이벤트 연결
            piece.OnSnapped += HandlePieceSnapped;

            _pieces[data.id] = piece;
        }

        // 조각 흩뿌리기
        ScatterPieces(boardW, boardH);

        if (GameManager.Instance != null) GameManager.Instance.isGameActive = true;
        UIManager.Instance?.RefreshProgress(placedPieces, totalPieces);
    }

    // ── 흩뿌리기 ─────────────────────────────────────────────────────────
    private void ScatterPieces(float boardW, float boardH)
    {
        float margin  = pieceWorldSize * 1.5f;
        float rangeX  = boardW * 0.5f + margin;
        float rangeY  = boardH * 0.5f + margin;

        foreach (var kv in _pieces)
        {
            if (kv.Value.IsPlaced) continue;

            // 보드 바깥 영역에 랜덤 배치
            float x, y;
            do {
                x = Random.Range(-rangeX, rangeX);
                y = Random.Range(-rangeY, rangeY);
            } while (Mathf.Abs(x) < boardW * 0.5f && Mathf.Abs(y) < boardH * 0.5f);

            kv.Value.transform.localPosition = new Vector3(x, y, 0);
            kv.Value.transform.rotation      = Quaternion.Euler(0, 0, Random.Range(-30f, 30f));
        }
    }

    // ── 스냅 콜백 ─────────────────────────────────────────────────────────
    private void HandlePieceSnapped(PuzzlePiece piece)
    {
        placedPieces++;
        OnProgressChanged?.Invoke(placedPieces, totalPieces);
        UIManager.Instance?.RefreshProgress(placedPieces, totalPieces);

        // 완성 체크
        if (placedPieces >= totalPieces)
        {
            GameManager.Instance?.OnPuzzleCleared();
            OnPuzzleCompleted?.Invoke();
            UIManager.Instance?.ShowClearScreen(GameManager.Instance?.elapsedTime ?? 0f);
            GameSaveLoad.DeleteSave();
        }
        else
        {
            // 자동 저장
            SaveGame();
        }
    }

    // ── 저장 / 불러오기 ───────────────────────────────────────────────────
    public void SaveGame()  => GameSaveLoad.Save(this, _pieces);
    public void LoadGame()
    {
        var gm = GameManager.Instance;
        GeneratePuzzle(gm.selectedImage, gm.columns, gm.rows);   // 먼저 생성
        GameSaveLoad.Load(this, _pieces);                         // 그 다음 상태 복원
    }

    // ── 유틸 ──────────────────────────────────────────────────────────────
    private void ClearBoard()
    {
        foreach (var kv in _pieces) Destroy(kv.Value.gameObject);
        _pieces.Clear();
        placedPieces = 0;
    }

    /// <summary>인접 조각 반환 (그룹핑 체크용)</summary>
    public PuzzlePiece GetPiece(int id)
        => _pieces.TryGetValue(id, out var p) ? p : null;

    /// <summary>모든 조각 반환</summary>
    public IEnumerable<PuzzlePiece> GetAllPieces() => _pieces.Values;
}
