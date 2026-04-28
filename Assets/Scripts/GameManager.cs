using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체 흐름을 관리하는 싱글톤 매니저
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int columns = 5;
    public int rows    = 5;
    public Texture2D selectedImage;
    public string    selectedImageName = "";

    [Header("State")]
    public bool  isGameActive  = false;
    public bool  isGameCleared = false;
    public float elapsedTime   = 0f;

    // 난이도 프리셋 (columns x rows)
    public static readonly (int c, int r)[] DifficultyPresets =
    {
        (5,  5),   // Easy   – 25조각
        (7,  7),   // Normal – 49조각
        (10, 10),  // Hard   – 100조각
        (14, 14),  // Expert – 196조각
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (isGameActive && !isGameCleared)
            elapsedTime += Time.deltaTime;
    }

    // ── 씬 이동 ──────────────────────────────────────────────
    public void LoadMainMenu()  => SceneManager.LoadScene("MainMenu");
    public void LoadGameScene() => SceneManager.LoadScene("GameScene");

    // ── 난이도 설정 ───────────────────────────────────────────
    public void SetDifficulty(int index)
    {
        index   = Mathf.Clamp(index, 0, DifficultyPresets.Length - 1);
        columns = DifficultyPresets[index].c;
        rows    = DifficultyPresets[index].r;
    }

    // ── 게임 시작 ─────────────────────────────────────────────
    public void StartGame(Texture2D image, string imageName)
    {
        selectedImage     = image;
        selectedImageName = imageName;
        elapsedTime       = 0f;
        isGameActive      = true;
        isGameCleared     = false;
        LoadGameScene();
    }

    // ── 게임 클리어 ───────────────────────────────────────────
    public void OnPuzzleCleared()
    {
        isGameActive  = false;
        isGameCleared = true;
        Debug.Log($"[GameManager] Puzzle Cleared! Time: {FormatTime(elapsedTime)}");
    }

    // ── 이어하기 ──────────────────────────────────────────────
    public void ContinueGame()
    {
        if (GameSaveLoad.HasSaveData())
        {
            isGameActive  = true;
            isGameCleared = false;
            LoadGameScene();
        }
    }

    // ── 유틸 ──────────────────────────────────────────────────
    public static string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60);
        int s = (int)(seconds % 60);
        return $"{m:00}:{s:00}";
    }
}
