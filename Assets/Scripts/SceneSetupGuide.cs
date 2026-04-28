// ================================================================
// JigsawPuzzle – Scene Setup Guide (SceneSetup.cs)
// 이 파일은 실제 MonoBehaviour가 아닌 씬 구성 가이드입니다.
// Unity 에디터에서 아래 계층 구조를 직접 만들어주세요.
// ================================================================

/*
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  [MainMenu Scene]  Hierarchy
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Main Camera
  EventSystem
  Managers
    ├── GameManager          (Script: GameManager)
    ├── AudioManager         (Script: AudioManager)
    ├── ImageLoader          (Script: ImageLoader)
    └── SettingsManager      (Script: SettingsManager)
  Canvas (MainMenuCanvas)
    ├── MainPanel
    │   ├── TitleText        (TextMeshPro)
    │   ├── NewGameButton    (Button)
    │   ├── ContinueButton   (Button)
    │   ├── SettingsButton   (Button)
    │   └── QuitButton       (Button)
    ├── GalleryPanel
    │   ├── TitleText
    │   ├── ScrollView
    │   │   └── Content      (GridLayoutGroup)
    │   │       └── [ThumbnailPrefab × N]
    │   ├── SelectedPreview  (RawImage)
    │   ├── SelectedNameText (TextMeshPro)
    │   └── BackButton
    ├── DifficultyPanel
    │   ├── TitleText
    │   ├── EasyButton
    │   ├── NormalButton
    │   ├── HardButton
    │   ├── ExpertButton
    │   ├── SelectedDiffText (TextMeshPro)
    │   ├── StartButton      → calls MainMenuManager.OnStartGame()
    │   └── BackButton       → calls MainMenuManager.OnBackToGallery()
    └── SettingsPanel        (SettingsManager 연결)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  [GameScene] Hierarchy
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Main Camera              (Script: CameraController)
  EventSystem
  Managers
    ├── PuzzleManager       (Script: PuzzleManager)
    │     boardRoot    → PuzzleBoard
    │     scatterRoot  → ScatterArea
    │     piecePrefab  → Prefabs/PuzzlePiece.prefab
    └── UIManager           (Script: UIManager)
  PuzzleBoard               (빈 Transform – 완성 조각들의 부모)
  ScatterArea               (빈 Transform – 흩뿌린 조각들의 부모)
  Canvas (GameUICanvas)
    ├── HUDPanel
    │   ├── TimerText        (TextMeshPro)
    │   ├── ProgressText     (TextMeshPro)
    │   ├── ProgressSlider   (Slider)
    │   ├── PreviewButton    (Button)
    │   ├── EdgeSortButton   (Button)
    │   └── PauseButton      (Button)
    ├── PreviewPanel
    │   ├── Background
    │   └── PreviewImage     (RawImage)
    ├── PausePanel
    │   ├── ResumeButton
    │   ├── RestartButton
    │   ├── SettingsButton
    │   └── MainMenuButton
    ├── ClearPanel           (Script: ClearEffect)
    │   ├── Background
    │   ├── ResultPanel
    │   │   ├── TitleText    "PUZZLE COMPLETE!"
    │   │   ├── ClearTimeText
    │   │   ├── ClearPiecesText
    │   │   ├── MainMenuButton
    │   │   └── RestartButton
    │   └── ConfettiParticle (ParticleSystem)
    └── SettingsPanel

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  [PuzzlePiece Prefab] 구조
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  PuzzlePiece (GameObject)
    ├── SpriteRenderer
    ├── BoxCollider2D
    └── Scripts
        └── PuzzlePiece.cs

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  [Thumbnail Prefab] 구조
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ThumbnailItem (GameObject)
    ├── Button
    ├── RawImage   (이미지 미리보기)
    └── Text       (이미지 이름)
*/

// ── 씬 빌드 세팅 (File > Build Settings > Scenes In Build) ──────────────
// 0: Assets/Scenes/MainMenu.unity
// 1: Assets/Scenes/GameScene.unity
