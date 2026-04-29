# 2026-04-29 — 모듈 신설: Mound.UI + Boxy.App.Gameplay/Levels (Phase 3 + Phase 4 합본)

## Context
Phase 3에서 Mound.UI 모듈 신설, Phase 4에서 Boxy.App.Gameplay/Levels 도메인 추가.
CLAUDE.md §17-1 트리거 "구조 변경" 발동 → 결정 기록 필수.

## Changes

### Mound.UI (Phase 3)
- `Assets/Mound/UI/Mound.UI.asmdef` — Mound.Core 참조
- `Assets/Mound/UI/Styles/tokens.uss` — Foundation 토큰 (mound-design-system.md §2,3,5,6 매핑)
- `Assets/Mound/UI/Styles/components.uss` — Button/Popup/Toast/Progress/Toggle/Slider
- C# 코드 아직 없음. Phase 6 이후 PopupManager, ToastManager 등 추가 예정

### Boxy.App.Gameplay (Phase 4)
- `Assets/Boxy.App/Gameplay/Domain/ItemShape.cs` — 셀 모양 readonly struct + Rotate 메서드 (정규화 포함)
- `Assets/Boxy.App/Gameplay/BoxyGrid.cs` — 런타임 그리드 상태, string itemKey 점유 추적
- `Assets/Boxy.App/Gameplay/PlacementValidator.cs` — 정적 검증, PlacementResult enum

### Boxy.App.Levels (Phase 4)
- `Assets/Boxy.App/Levels/LevelData.cs` — LevelId(struct) + LevelItemTemplate + LevelData(SO)
- `Assets/Boxy.App/Levels/LevelRepository.cs` — LevelId-keyed Dictionary wrap (CLAUDE.md §6-3)

## 구조

```
Boxy.App/
├── Gameplay/
│   ├── Domain/   ← 순수 도메인 (Unity 의존 최소: Vector2Int만)
│   ├── BoxyGrid.cs / PlacementValidator.cs
│   └── (예정) GameplayController.cs / DraggableItem.cs / RotationController.cs
├── Levels/       ← 데이터 레이어 (SO + Repository)
└── (예정) Themes / Tutorial / UI
```

## 의존 방향
- Boxy.App.Gameplay → Boxy.App.Gameplay.Domain (같은 asmdef, 다른 namespace)
- Boxy.App.Levels → Boxy.App.Gameplay.Domain (LevelItemTemplate.ToShape이 ItemShape 반환)
- Boxy.App.* → Mound.Core (asmdef 참조)
- Mound.* → Boxy.App ❌ (asmdef references 검증)

## 디자인 결정

### 1. Vector2Int 사용 (자체 GridCoord 안 만듦)
- Unity 직렬화 즉시 지원 (`List<Vector2Int>` SO 안에 인스펙터 노출)
- 연산자 (`+`, `==`) 빌트인
- 도메인 의미는 변수명/문맥으로 표현

### 2. ItemShape 불변
- readonly struct + readonly array
- Rotate는 새 ItemShape 반환
- 회전 후 정규화 → 같은 모양 비교 가능 (예: (1,0)(2,0) ≡ (0,0)(1,0))

### 3. string itemKey (BoxyGrid 점유 표시)
- 단순. 테스트/디버그 편함
- 위험: 런타임 타이포 → v1.1에서 ItemId struct로 wrap 권장

### 4. PlacementValidator static class
- 상태 없음, 순수 함수
- 반환: PlacementResult enum (Valid / OutOfBounds / Collision)
- BoxyGrid.Place는 검증 안 함 — 호출자 계약 (CQS)

## Consequences

**좋은 것**:
- Domain 분리로 테스트 용이 (ItemShape 회전은 Unity Editor 없이 EditMode 테스트 가능)
- Dictionary 노출 X (CLAUDE.md §6 준수) — LevelRepository wrap
- ItemShape 불변 → race condition 회피, 디버깅 쉬움

**나쁜 것 / 위험**:
- string itemKey 타이포 위험 (위 §3 참조)
- BoxyGrid.Place 사전 검증 의존 — 사용처마다 PlacementValidator 호출 강제
- 컴파일 검증 미실시 (Unity 라이선스 미활성) — Hub OAuth 후 일괄 검증 필요

## Revisit when
- Hub OAuth → 라이선스 활성 → 첫 컴파일 결과 확인 시
- Day 13 50레벨 직접 플레이에서 모양 비교/회전 버그 발견 시
- Phase 4 Turn C 자동 생성 알고리즘 작성 중 ItemShape API 부족 발견 시
- Boxy Sort/Cafe 신규 게임 시작 시 — Mound.UI / Mound.Core 재사용성 검증
