# Scene 구성 가이드

## MainMenu 씬 구성

### Hierarchy 구조
```
MainMenu (Scene)
├── [GameManager]              ← 빈 GameObject (DontDestroyOnLoad)
│   └── GameManager.cs
│   └── ImageLoader.cs
│
├── [MainMenuManager]
│   └── MainMenuManager.cs
│
├── Main Camera
│   ├── Camera (Background Color: #1a1a2e)
│   └── AudioListener
│
└── Canvas (MainMenuCanvas)
    ├── Canvas
    │   ├── Render Mode: Screen Space - Overlay
    │   └── UI Scale Mode: Scale With Screen Size (1280×720)
    ├── CanvasScaler
    ├── GraphicRaycaster
    │
    ├── [MainPanel]                ← 기본 메인 화면
    │   ├── Background (Image, 전체화면 어두운 오버레이)
    │   ├── TitleText (TextMeshPro) "🧩 Jigsaw Puzzle"
    │   ├── NewGameButton (Button + TextMeshPro) "새 게임"
    │   ├── ContinueButton (Button + TextMeshPro) "이어하기"
    │   └── QuitButton (Button + TextMeshPro) "종료"
    │
    ├── [GalleryPanel]             ← 이미지 선택 화면
    │   ├── PanelBackground
    │   ├── TitleText "이미지 선택"
    │   ├── ScrollView
    │   │   └── Viewport
    │   │       └── Content (Grid Layout Group)
    │   │           └── ThumbnailItem × N (프리팹)
    │   └── BackButton "뒤로"
    │
    └── [DifficultyPanel]          ← 난이도 선택 화면
        ├── PanelBackground
        ├── TitleText "난이도 선택"
        ├── PreviewImage (RawImage, 512×384)
        ├── SelectedNameText (TextMeshPro)
        ├── DifficultyButtons
        │   ├── EasyButton "Easy (25조각)"
        │   ├── NormalButton "Normal (49조각)"
        │   ├── HardButton "Hard (100조각)"
        │   └── ExpertButton "Expert (196조각)"
        ├── SelectedDiffText (TextMeshPro)
        ├── StartButton "게임 시작" → OnStartGame()
        └── BackButton "뒤로"
```

### MainMenuManager Inspector 연결
| 필드 | 연결 대상 |
|------|-----------|
| Main Panel | [MainPanel] |
| Gallery Panel | [GalleryPanel] |
| Difficulty Panel | [DifficultyPanel] |
| Grid Parent | Content (GridLayoutGroup) |
| Thumbnail Prefab | ThumbnailItem.prefab |
| Difficulty Buttons[0~3] | Easy/Normal/Hard/Expert 버튼 |
| Selected Diff Text | SelectedDiffText |
| Continue Button | ContinueButton |
| New Game Button | NewGameButton |
| Quit Button | QuitButton |
| Selected Preview | PreviewImage |
| Selected Name Text | SelectedNameText |

---

## GameScene 씬 구성

### Hierarchy 구조
```
GameScene (Scene)
├── Main Camera
│   ├── Camera
│   │   ├── Projection: Orthographic
│   │   ├── Size: 6
│   │   └── Background Color: #2d2d2d (어두운 회색)
│   ├── AudioListener
│   └── CameraController.cs
│       ├── Zoom Speed: 2
│       ├── Min Zoom: 2 / Max Zoom: 20
│       └── Use Bounds: true
│
├── [PuzzleManager]             ← 빈 GameObject
│   ├── PuzzleManager.cs
│   │   ├── Piece Prefab: PuzzlePiece.prefab
│   │   ├── Board Root: [BoardRoot]
│   │   ├── Scatter Root: [ScatterRoot]
│   │   ├── Piece World Size: 1.0
│   │   └── Snap Distance: 0.35
│   └── ConfettiEffect.cs
│
├── [BoardRoot]                 ← 배치 완료 조각 부모
├── [ScatterRoot]               ← 흩어진 조각 부모
│
├── [BoardOutline]              ← (선택) 퍼즐 보드 경계선 표시
│   └── SpriteRenderer (단색 테두리)
│
└── Canvas (GameUICanvas)
    ├── Canvas (Screen Space - Overlay)
    ├── CanvasScaler (1280×720)
    ├── GraphicRaycaster
    │
    ├── [UIManager]
    │   └── UIManager.cs (모든 UI 레퍼런스 연결)
    │
    ├── [HUD]                   ← 항상 표시되는 상단 HUD
    │   ├── TimerText (TextMeshPro) "00:00"
    │   ├── ProgressText (TextMeshPro) "0 / 25"
    │   ├── ProgressSlider (Slider)
    │   ├── PreviewButton (Button) "미리보기"
    │   └── EdgeSortButton (Button) "모서리 정렬"
    │
    ├── [PreviewPanel]          ← 미리보기 패널 (기본 비활성)
    │   ├── Background (반투명 검정)
    │   ├── PreviewImage (RawImage, 전체 이미지)
    │   └── CloseButton "닫기"
    │
    ├── [PausePanel]            ← 일시정지 (기본 비활성)
    │   ├── PauseBackground
    │   ├── PauseTitle "일시정지"
    │   ├── ResumeButton "계속하기"
    │   ├── RestartButton "다시 시작"
    │   └── MainMenuButton "메인 메뉴"
    │
    └── [ClearPanel]            ← 완성 화면 (기본 비활성)
        ├── ClearBackground
        ├── ClearTitle "🎉 완성!"
        ├── ClearTimeText "완성 시간: 00:00"
        ├── ClearPiecesText "총 조각: 25개"
        ├── ClearMenuButton "메인 메뉴"
        └── ClearRestartButton "다시 도전"
```

### UIManager Inspector 연결
| 필드 | 연결 대상 |
|------|-----------|
| Timer Text | TimerText |
| Progress Text | ProgressText |
| Progress Slider | ProgressSlider |
| Preview Button | PreviewButton |
| Preview Panel | [PreviewPanel] |
| Preview Image | PreviewImage |
| Edge Sort Button | EdgeSortButton |
| Pause Panel | [PausePanel] |
| Pause Button | PauseButton (HUD 상단) |
| Resume Button | ResumeButton |
| Restart Button | RestartButton |
| Main Menu Button | MainMenuButton |
| Clear Panel | [ClearPanel] |
| Clear Time Text | ClearTimeText |
| Clear Pieces Text | ClearPiecesText |
| Clear Menu Button | ClearMenuButton |
| Clear Restart Button | ClearRestartButton |
| Confetti Effect | [PuzzleManager]의 ConfettiEffect |

---

## ThumbnailItem Prefab 구성

```
ThumbnailItem (GameObject)
├── RectTransform (150×150)
├── Button
├── ThumbnailItem.cs
│
├── ThumbnailImage (RawImage, 150×120)
├── NameLabel (TextMeshPro, 하단 30px)
└── SelectedBorder (Image, 테두리, 기본 비활성)
    └── Outline 색상: 노란색 (#FFD700)
```

---

## Build Settings 씬 순서

```
File → Build Settings → Scenes In Build:
  0: Scenes/MainMenu   ← 첫 번째 (인덱스 0)
  1: Scenes/GameScene  ← 두 번째 (인덱스 1)
```

---

## 카메라 설정 요약

| 씬 | Projection | Size | Position |
|----|-----------|------|----------|
| MainMenu | Perspective | - | (0,0,-10) |
| GameScene | Orthographic | 6 | (0,0,-10) |
