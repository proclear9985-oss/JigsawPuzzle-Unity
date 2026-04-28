# PuzzlePiece Prefab 설정 가이드

## 개요
Unity Editor에서 PuzzlePiece Prefab을 직접 구성하는 방법을 설명합니다.

---

## 1. PuzzlePiece Prefab 생성

### Step 1 - 빈 GameObject 생성
```
Hierarchy → 우클릭 → Create Empty
이름: PuzzlePiece
```

### Step 2 - 컴포넌트 추가 (Inspector)
| 컴포넌트 | 설명 |
|----------|------|
| `SpriteRenderer` | 조각 이미지 렌더링 |
| `BoxCollider2D` | 클릭/드래그 감지 |
| `PuzzlePiece` (Script) | 핵심 조각 로직 |

### Step 3 - SpriteRenderer 설정
```
Sorting Layer : PuzzlePieces
Order in Layer: 0
Material      : Sprites/Default (또는 커스텀 머티리얼)
```

### Step 4 - BoxCollider2D 설정
```
Is Trigger : false
Size       : (1, 1)  ← PuzzleManager에서 런타임에 자동 설정됨
Offset     : (0, 0)
```

### Step 5 - PuzzlePiece 스크립트 필드 연결
Inspector에서 아래 항목을 드래그로 연결:
```
Sprite Renderer : [SpriteRenderer 컴포넌트]
Col2d           : [BoxCollider2D 컴포넌트]
```

### Step 6 - Prefab 저장
```
Hierarchy의 PuzzlePiece → Assets/Prefabs/ 폴더로 드래그
→ "PuzzlePiece.prefab" 생성 확인
```

---

## 2. PuzzleManager에 Prefab 연결

GameScene에서 PuzzleManager 오브젝트의 Inspector:
```
Piece Prefab  : [PuzzlePiece.prefab 드래그]
Board Root    : [BoardRoot 빈 오브젝트 드래그]
Scatter Root  : [ScatterRoot 빈 오브젝트 드래그]
Piece World Size : 1.0
Snap Distance    : 0.35
```

---

## 3. 레이어 설정 (TagManager 기준)

| 레이어 번호 | 이름 | 용도 |
|------------|------|------|
| 8 | PuzzlePiece | 미배치 조각 |
| 9 | PlacedPiece | 배치 완료 조각 |

PuzzlePiece GameObject의 Layer를 `PuzzlePiece` 로 설정

---

## 4. 머티리얼 (선택사항)

투명도(Alpha)가 필요하므로 아래 셰이더 사용 권장:
```
Shader: Sprites/Default
또는
Shader: Universal Render Pipeline/2D/Sprite-Lit-Default (URP 사용 시)
```

Material 파일: `Assets/Materials/PuzzlePieceMat.mat`
- Rendering Mode: Transparent
- Main Texture: (런타임에 자동 할당)

---

## 5. 완성된 Prefab 구조

```
PuzzlePiece (GameObject)
├── Transform
├── SpriteRenderer
│   ├── Sprite: None (런타임 할당)
│   ├── Color: White (1,1,1,1)
│   └── Sorting Layer: PuzzlePieces
├── BoxCollider2D
│   ├── Size: (1,1)
│   └── Is Trigger: false
└── PuzzlePiece (Script)
    ├── Sprite Renderer: [연결됨]
    └── Col2d: [연결됨]
```
