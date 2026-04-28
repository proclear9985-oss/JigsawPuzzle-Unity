using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 메인 메뉴 씬을 담당합니다.
/// - 이미지 갤러리 (Resources/Images 폴더)
/// - 난이도 선택 (Easy / Normal / Hard / Expert)
/// - 이어하기 버튼
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject galleryPanel;
    public GameObject difficultyPanel;

    [Header("Gallery")]
    public Transform       gridParent;       // ScrollView > Content
    public GameObject      thumbnailPrefab;  // Image + Button 프리팹
    public int             thumbnailSize = 150;

    [Header("Difficulty Buttons")]
    public Button[] difficultyButtons;       // 0=Easy, 1=Normal, 2=Hard, 3=Expert
    public TextMeshProUGUI selectedDiffText;

    [Header("Main Buttons")]
    public Button continueButton;
    public Button newGameButton;
    public Button quitButton;

    [Header("Preview")]
    public RawImage    selectedPreview;
    public TextMeshProUGUI selectedNameText;

    // ── 내부 상태 ─────────────────────────────────────────────────────────
    private List<Texture2D> _images      = new();
    private List<string>    _imageNames  = new();
    private Texture2D       _chosenImage = null;
    private string          _chosenName  = "";
    private int             _difficulty  = 0;   // 0=Easy

    // ── 생명주기 ──────────────────────────────────────────────────────────
    void Start()
    {
        ShowPanel(mainPanel);

        // 이어하기 버튼 활성화 여부
        bool hasSave = GameSaveLoad.HasSaveData();
        continueButton?.gameObject.SetActive(hasSave);

        // 메인메뉴 BGM 재생
        AudioManager.Instance?.PlayMainMenuBGM();

        // 버튼 연결 (클릭 효과음 포함)
        newGameButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); OnNewGame(); });
        continueButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); OnContinue(); });
        quitButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); OnQuit(); });

        // 난이도 버튼 연결
        for (int i = 0; i < difficultyButtons.Length; i++)
        {
            int idx = i;
            difficultyButtons[i]?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); SelectDifficulty(idx); });
        }

        SelectDifficulty(0);   // 기본: Easy
        LoadImages();
    }

    // ── 이미지 로드 ───────────────────────────────────────────────────────
    private void LoadImages()
    {
        _images.Clear();
        _imageNames.Clear();

        // Resources/Images 폴더의 Texture2D 전부 로드
        Texture2D[] textures = Resources.LoadAll<Texture2D>("Images");
        foreach (var tex in textures)
        {
            _images.Add(tex);
            _imageNames.Add(tex.name);
        }

        if (_images.Count > 0)
        {
            _chosenImage = _images[0];
            _chosenName  = _imageNames[0];
            UpdatePreview();
        }

        BuildGallery();
    }

    // ── 갤러리 빌드 ───────────────────────────────────────────────────────
    private void BuildGallery()
    {
        if (gridParent == null || thumbnailPrefab == null) return;

        // 기존 썸네일 제거
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        for (int i = 0; i < _images.Count; i++)
        {
            int    idx  = i;
            var    go   = Instantiate(thumbnailPrefab, gridParent);
            var    img  = go.GetComponentInChildren<RawImage>();
            var    btn  = go.GetComponent<Button>();
            var    lbl  = go.GetComponentInChildren<TextMeshProUGUI>();

            if (img != null) img.texture = _images[i];
            if (lbl != null) lbl.text    = _imageNames[i];
            btn?.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); SelectImage(idx); });
        }
    }

    // ── 선택 ──────────────────────────────────────────────────────────────
    private void SelectImage(int idx)
    {
        _chosenImage = _images[idx];
        _chosenName  = _imageNames[idx];
        UpdatePreview();
        ShowPanel(difficultyPanel);
    }

    private void SelectDifficulty(int idx)
    {
        _difficulty = idx;
        GameManager.Instance?.SetDifficulty(idx);

        string[] labels = { "Easy (25)", "Normal (49)", "Hard (100)", "Expert (196)" };
        if (selectedDiffText != null)
            selectedDiffText.text = "난이도: " + (idx < labels.Length ? labels[idx] : "");

        // 선택된 버튼 강조
        for (int i = 0; i < difficultyButtons.Length; i++)
        {
            if (difficultyButtons[i] == null) continue;
            var colors = difficultyButtons[i].colors;
            colors.normalColor            = i == idx ? Color.yellow : Color.white;
            difficultyButtons[i].colors   = colors;
        }
    }

    private void UpdatePreview()
    {
        if (selectedPreview != null)  selectedPreview.texture = _chosenImage;
        if (selectedNameText != null) selectedNameText.text   = _chosenName;
    }

    // ── 패널 전환 ─────────────────────────────────────────────────────────
    private void ShowPanel(GameObject target)
    {
        mainPanel?.SetActive(target == mainPanel);
        galleryPanel?.SetActive(target == galleryPanel);
        difficultyPanel?.SetActive(target == difficultyPanel);
    }

    // ── 버튼 액션 ─────────────────────────────────────────────────────────
    private void OnNewGame()
    {
        ShowPanel(galleryPanel);
    }

    public void OnStartGame()
    {
        if (_chosenImage == null)
        {
            Debug.LogWarning("[MainMenu] 이미지를 먼저 선택하세요.");
            return;
        }
        GameManager.Instance?.StartGame(_chosenImage, _chosenName);
    }

    private void OnContinue()
    {
        var save = GameSaveLoad.PeekSaveData();
        if (save == null) return;

        GameManager.Instance.columns     = save.columns;
        GameManager.Instance.rows        = save.rows;
        GameManager.Instance.elapsedTime = save.elapsedTime;

        // 저장된 이미지 이름으로 텍스처 검색
        Texture2D savedTex = Resources.Load<Texture2D>("Images/" + save.imageName);
        if (savedTex != null)
        {
            GameManager.Instance.selectedImage     = savedTex;
            GameManager.Instance.selectedImageName = save.imageName;
            GameManager.Instance.ContinueGame();
        }
        else
        {
            Debug.LogWarning("[MainMenu] 저장된 이미지를 찾을 수 없습니다: " + save.imageName);
        }
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── 뒤로가기 ─────────────────────────────────────────────────────────
    public void OnBackToMain()       => ShowPanel(mainPanel);
    public void OnBackToGallery()    => ShowPanel(galleryPanel);
}
