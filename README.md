# 🧩 Jigsaw Puzzle Game

> Unity 2022 LTS 기반 싱글 플레이 지그소 퍼즐 게임  
> Puzzle Together Multiplayer Jigsaw Puzzles 참고 제작

---

## 📋 목차
1. [프로젝트 구조](#프로젝트-구조)
2. [요구사항](#요구사항)
3. [Unity 프로젝트 열기](#unity-프로젝트-열기)
4. [씬 설정](#씬-설정)
5. [Prefab 설정](#prefab-설정)
6. [이미지 추가 방법](#이미지-추가-방법)
7. [게임 실행](#게임-실행)
8. [게임 조작법](#게임-조작법)
9. [스크립트 설명](#스크립트-설명)
10. [빌드 방법](#빌드-방법)

---

## 📁 프로젝트 구조

```
JigsawPuzzle/
├── Assets/
│   ├── Scripts/                  ← 모든 C# 스크립트
│   │   ├── GameManager.cs        ← 게임 전체 흐름 (싱글톤)
│   │   ├── PuzzleManager.cs      ← 퍼즐 생성/관리 핵심
│   │   ├── PuzzlePiece.cs        ← 개별 조각 (드래그/스냅/회전)
│   │   ├── PieceGroup.cs         ← 조각 그룹핑 시스템
│   │   ├── ImageSlicer.cs        ← 이미지 N×M 슬라이싱
│   │   ├── PieceMeshBuilder.cs   ← 베지어 곡선 조각 Mesh 생성
│   │   ├── ImageLoader.cs        ← 런타임 이미지 로드 (파일/URL)
│   │   ├── UIManager.cs          ← 게임씬 UI 전체 제어
│   │   ├── MainMenuManager.cs    ← 메인메뉴/갤러리/난이도 선택
│   │   ├── CameraController.cs   ← 카메라 패닝/줌
│   │   ├── GameSaveLoad.cs       ← JSON 저장/불러오기
│   │   ├── ConfettiEffect.cs     ← 완성 파티클 이펙트
│   │   └── ThumbnailItem.cs      ← 갤러리 썸네일 아이템
│   │
│   ├── Resources/
│   │   └── Images/               ← ⭐ 퍼즐 이미지 폴더
│   │       └── alpine_village.png
│   │
│   ├── Prefabs/
│   │   ├── PuzzlePiece.prefab    ← (Unity에서 직접 생성 필요)
│   │   ├── ThumbnailItem.prefab  ← (Unity에서 직접 생성 필요)
│   │   └── PREFAB_SETUP_GUIDE.md
│   │
│   ├── Scenes/
│   │   ├── MainMenu.unity        ← (Unity에서 직접 생성 필요)
│   │   ├── GameScene.unity       ← (Unity에서 직접 생성 필요)
│   │   └── SCENE_SETUP_GUIDE.md  ← ⭐ 씬 구성 상세 가이드
│   │
│   ├── Materials/                ← 머티리얼 파일
│   ├── Audio/                    ← 사운드 파일
│   └── UI/                       ← UI 에셋
│
├── Packages/
│   └── manifest.json             ← 패키지 의존성
│
└── ProjectSettings/              ← 프로젝트 설정
    ├── ProjectSettings.asset
    ├── TagManager.asset
    ├── EditorSettings.asset
    └── QualitySettings.asset
```

---

## ⚙️ 요구사항

| 항목 | 버전 |
|------|------|
| Unity | **2022.3 LTS** 이상 권장 |
| Render Pipeline | Built-in (URP도 가능, 셰이더 수정 필요) |
| TextMeshPro | 3.0.6 (자동 설치) |
| 플랫폼 | Windows / macOS / Linux (PC 기준) |

---

## 🚀 Unity 프로젝트 열기

### Step 1 - Unity Hub에서 프로젝트 추가
```
Unity Hub → Projects → Add → 폴더 선택
→ JigsawPuzzle/ 폴더 선택 (Assets 폴더가 있는 최상위 폴더)
```

### Step 2 - Unity 버전 선택
```
Unity 2022.3.x LTS 선택 → Open
```

### Step 3 - 패키지 자동 설치 확인
```
Package Manager가 manifest.json을 읽어
TextMeshPro 등 패키지를 자동 설치합니다.
(최초 실행 시 수 분 소요)
```

### Step 4 - TMP Essentials 임포트
```
Window → TextMeshPro → Import TMP Essential Resources
→ Import 클릭
```

---

## 🎬 씬 설정

> 상세 내용은 `Assets/Scenes/SCENE_SETUP_GUIDE.md` 참고

### MainMenu 씬 생성
```
1. File → New Scene → Basic (Built-in)
2. File → Save As → Assets/Scenes/MainMenu.unity
3. SCENE_SETUP_GUIDE.md 의 [MainMenu 씬 구성] 섹션대로 오브젝트 배치
```

### GameScene 씬 생성
```
1. File → New Scene → Basic (Built-in)
2. File → Save As → Assets/Scenes/GameScene.unity
3. SCENE_SETUP_GUIDE.md 의 [GameScene 씬 구성] 섹션대로 오브젝트 배치
```

### Build Settings에 씬 등록
```
File → Build Settings → Add Open Scenes
순서: 0 = MainMenu, 1 = GameScene
```

---

## 🔧 Prefab 설정

> 상세 내용은 `Assets/Prefabs/PREFAB_SETUP_GUIDE.md` 참고

### PuzzlePiece.prefab
```
1. Hierarchy → Create Empty → 이름: "PuzzlePiece"
2. 컴포넌트 추가:
   - SpriteRenderer
   - BoxCollider2D
   - PuzzlePiece (Script)
3. Assets/Prefabs/ 폴더로 드래그하여 Prefab 저장
```

### ThumbnailItem.prefab
```
1. Hierarchy → UI → Button 생성
2. 하위에 RawImage, TextMeshPro 추가
3. ThumbnailItem (Script) 추가
4. Assets/Prefabs/ 폴더로 드래그하여 Prefab 저장
```

---

## 🖼️ 이미지 추가 방법

### 기본 제공 이미지
```
Assets/Resources/Images/alpine_village.png  (1024×765, 알프스 마을)
```

### 새 이미지 추가
```
1. PNG 또는 JPG 파일을 Assets/Resources/Images/ 폴더에 복사
2. Unity Inspector에서 이미지 선택 → Texture Type: Sprite (2D and UI)
3. Read/Write Enabled: ✅ 체크 (필수!)
4. Apply 클릭
```

### 이미지 Import 설정 (필수)
```
Inspector → Texture Type : Sprite (2D and UI)
           Read/Write    : ✅ Enabled   ← 반드시 체크!
           Max Size      : 2048 이상
           Compression   : None (품질 우선)
           Apply
```

> ⚠️ **Read/Write Enabled를 체크하지 않으면 런타임에 이미지 슬라이싱이 실패합니다!**

---

## ▶️ 게임 실행

```
1. Project 창에서 Assets/Scenes/MainMenu.unity 더블클릭
2. Unity 상단 ▶ Play 버튼 클릭
3. 이미지 선택 → 난이도 선택 → 게임 시작
```

---

## 🎮 게임 조작법

| 입력 | 동작 |
|------|------|
| **마우스 왼쪽 드래그** | 조각 이동 |
| **마우스 우클릭** | 조각 90° 회전 |
| **R 키** | 드래그 중 조각 회전 |
| **스크롤 휠** | 카메라 줌 인/아웃 |
| **마우스 중간 버튼 드래그** | 카메라 패닝 |
| **Alt + 왼쪽 드래그** | 카메라 패닝 (대안) |
| **ESC** | 일시정지 |

### 게임 UI 버튼
| 버튼 | 기능 |
|------|------|
| 👁 미리보기 | 완성 이미지 토글 |
| ⬜ 모서리 정렬 | 테두리 조각을 보드 바깥으로 이동 |
| ⏸ 일시정지 | 게임 일시정지/재개 |

---

## 📜 스크립트 설명

| 스크립트 | 역할 |
|----------|------|
| `GameManager` | 싱글톤. 씬 전환, 난이도, 타이머, 클리어 처리 |
| `PuzzleManager` | 조각 생성, 배치, 스냅 콜백, 저장/불러오기 호출 |
| `PuzzlePiece` | 드래그·스냅·회전·그룹핑. OnMouseDown/Drag/Up 이벤트 |
| `PieceGroup` | 여러 조각을 묶어 함께 이동·회전 |
| `ImageSlicer` | Texture2D를 N×M으로 잘라 PieceData 리스트 반환 |
| `PieceMeshBuilder` | 베지어 곡선 탭/소켓 형태의 커스텀 Mesh 생성 |
| `ImageLoader` | 파일 경로/URL/StreamingAssets에서 텍스처 로드 |
| `UIManager` | HUD, 미리보기, 일시정지, 완성 화면 제어 |
| `MainMenuManager` | 메인화면, 갤러리, 난이도 선택 패널 전환 |
| `CameraController` | 스크롤 줌, 중간버튼 패닝, 카메라 경계 제한 |
| `GameSaveLoad` | JSON으로 퍼즐 진행 상태 저장/불러오기 |
| `ConfettiEffect` | 완성 시 색종이 파티클 연출 |
| `ThumbnailItem` | 갤러리 썸네일 아이템 (선택 시 MainMenu에 콜백) |

---

## 🔨 빌드 방법

### Windows 빌드
```
File → Build Settings
  Platform: PC, Mac & Linux Standalone
  Target Platform: Windows
  Architecture: x86_64
→ Build And Run
```

### WebGL 빌드
```
File → Build Settings
  Platform: WebGL
→ Switch Platform → Build
```

> WebGL 빌드 시 `Application.persistentDataPath` 저장 경로가 IndexedDB로 변경됩니다.  
> 파일 저장/불러오기는 정상 동작하나 브라우저 초기화 시 삭제될 수 있습니다.

---

## 🐛 자주 발생하는 문제

### 조각 이미지가 흰색/분홍색으로 나옴
```
원인: 이미지 Import 설정에서 Read/Write Enabled 미체크
해결: Inspector → Texture → Read/Write Enabled 체크 → Apply
```

### TextMeshPro 오류 (CS0246)
```
원인: TMP Essentials 미설치
해결: Window → TextMeshPro → Import TMP Essential Resources
```

### 씬을 찾을 수 없음 오류
```
원인: Build Settings에 씬이 등록되지 않음
해결: File → Build Settings → 두 씬 모두 Add Open Scenes
```

### 조각이 스냅되지 않음
```
원인: Snap Distance 값이 너무 작음
해결: PuzzleManager → Snap Distance 값을 0.4~0.5로 증가
```

### 카메라가 움직이지 않음
```
원인: Main Camera에 CameraController 스크립트가 없음
해결: Main Camera → Add Component → CameraController
```

---

## 📌 향후 추가 예정 기능

- [ ] 배경 음악 / 효과음
- [ ] 조각 섞기 애니메이션
- [ ] 힌트 기능 (조각 위치 표시)
- [ ] 최고 기록 (Best Time) 저장
- [ ] 커스텀 이미지 불러오기 (파일 탐색기)
- [ ] 반투명 보드 가이드라인

---

## 📄 라이선스

이미지: `alpine_village.png` - AI 생성 이미지 (저작권 없음)  
코드: MIT License

---

*Made with Unity 2022.3 LTS*
