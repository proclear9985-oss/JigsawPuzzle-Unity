using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// GameScene UI 전체를 담당합니다.
/// Canvas > GameUICanvas 하위에 배치된 버튼/텍스트와 연결합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public TextMeshProUGUI  timerText;
    public TextMeshProUGUI  progressText;
    public Slider           progressSlider;
    public Button           previewButton;
    public GameObject       previewPanel;
    public RawImage         previewImage;

    [Header("Edge Sort Button")]
    public Button  edgeSortButton;

    [Header("Pause Menu")]
    public GameObject pausePanel;
    public Button     pauseButton;
    public Button     resumeButton;
    public Button     restartButton;
    public Button     mainMenuButton;

    [Header("Clear Screen")]
    public GameObject       clearPanel;
    public TextMeshProUGUI  clearTimeText;
    public TextMeshProUGUI  clearPiecesText;
    public Button           clearMenuButton;
    public Button           clearRestartButton;
    public ParticleSystem   confettiEffect;

    // ── 내부 상태 ─────────────────────────────────────────────────────────
    private bool _isPaused        = false;
    private bool _isPreviewShown  = false;

    // ── 생명주기 ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // 미리보기 이미지 설정
        if (GameManager.Instance?.selectedImage != null && previewImage != null)
            previewImage.texture = GameManager.Instance.selectedImage;

        // 버튼 이벤트 연결 (버튼 클릭 효과음 포함)
        previewButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); TogglePreview(); });
        edgeSortButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); SortEdgePieces(); });
        pauseButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); TogglePause(); });
        resumeButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); TogglePause(); });
        restartButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); RestartGame(); });
        mainMenuButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); GoToMainMenu(); });
        clearMenuButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); GoToMainMenu(); });
        clearRestartButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); RestartGame(); });

        // 게임씬 BGM 재생
        AudioManager.Instance?.PlayGameBGM();

        // 초기 패널 상태
        pausePanel?.SetActive(false);
        clearPanel?.SetActive(false);
        previewPanel?.SetActive(false);
    }

    void Update()
    {
        // 타이머 업데이트
        if (GameManager.Instance != null && GameManager.Instance.isGameActive)
            timerText?.SetText(GameManager.FormatTime(GameManager.Instance.elapsedTime));

        // ESC → 일시정지
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // ── 진행률 ────────────────────────────────────────────────────────────
    public void RefreshProgress(int placed, int total)
    {
        if (progressText != null)
            progressText.text = $"{placed} / {total}";

        if (progressSlider != null)
        {
            progressSlider.maxValue = total;
            progressSlider.value    = placed;
        }
    }

    // ── 미리보기 토글 ─────────────────────────────────────────────────────
    private void TogglePreview()
    {
        _isPreviewShown = !_isPreviewShown;
        previewPanel?.SetActive(_isPreviewShown);
    }

    // ── 모서리 정렬 ───────────────────────────────────────────────────────
    private void SortEdgePieces()
    {
        var pm = PuzzleManager.Instance;
        if (pm == null) return;

        float boardW  = GameManager.Instance.columns * pm.pieceWorldSize;
        float boardH  = GameManager.Instance.rows    * pm.pieceWorldSize;
        float marginX = boardW  * 0.6f + pm.pieceWorldSize;
        float marginY = boardH  * 0.6f + pm.pieceWorldSize;

        int cols = GameManager.Instance.columns;
        int rows = GameManager.Instance.rows;

        foreach (var piece in pm.GetAllPieces())
        {
            if (piece.IsPlaced) continue;
            bool isEdge = piece.Data.col == 0        || piece.Data.col == cols - 1 ||
                          piece.Data.row == 0        || piece.Data.row == rows - 1;
            if (!isEdge) continue;

            // 가장자리 조각을 보드 바깥 특정 영역에 이동
            float angle = Mathf.Atan2(piece.CorrectPos.y, piece.CorrectPos.x);
            float r     = Mathf.Max(marginX, marginY) * 1.1f;
            piece.transform.position = new Vector3(
                Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
        }
    }

    // ── 일시정지 ──────────────────────────────────────────────────────────
    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        pausePanel?.SetActive(_isPaused);
        if (GameManager.Instance != null)
            GameManager.Instance.isGameActive = !_isPaused;
    }

    // ── 완성 화면 ─────────────────────────────────────────────────────────
    public void ShowClearScreen(float elapsed)
    {
        clearPanel?.SetActive(true);

        if (clearTimeText != null)
            clearTimeText.text = $"완성 시간: {GameManager.FormatTime(elapsed)}";

        if (clearPiecesText != null)
        {
            var pm = PuzzleManager.Instance;
            clearPiecesText.text = pm != null ? $"총 조각: {pm.totalPieces}개" : "";
        }

        // 완성 효과음 + 파티클
        AudioManager.Instance?.PlayClear();
        confettiEffect?.Play();
        ConfettiEffect.Instance?.Play();
    }

    // ── 씬 이동 ───────────────────────────────────────────────────────────
    private void RestartGame()
    {
        Time.timeScale = 1f;
        GameSaveLoad.DeleteSave();
        GameManager.Instance?.LoadGameScene();
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.LoadMainMenu();
    }
}
