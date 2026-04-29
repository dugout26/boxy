# 2026-04-29 — 그리드 반응형 + 아이템 색상 매핑

## Context

§17 트리거 #1 (구조 변경) — UI 구성 변경.

iPhone 17 + iPad mini 시뮬 검증 후:
1. `GridCellView.CellSizePx = const 36f` 하드코딩 → 4×4 grid가 144×144만 차지. bag-frame 320×320 안에 작게 박힘. 디바이스마다 셀 사이즈가 달라야 함.
2. 모든 아이템이 같은 노란색 → §B-3-1 "거의 다 됐는데" 트릭이 시각적으로 안 보임. 아이템 구분 시각 단서 필요.

## Decision

### 1. GridCellView 동적 cellSize
- `CellSizePx` const → `CurrentCellSizePx` static mutable
- `Create(coord, cellSize)` 시그니처로 사이즈 주입
- `GameplayUI.BuildGrid`에서 `gridContainer.contentRect`로 동적 계산
- `GeometryChangedEvent` 콜백으로 layout 변경 시 자동 resize

### 2. 9개 itemKey 색상 매핑
| itemKey | 색상 | 역할 |
|---|---|---|
| book_0 | 빨강 #F26B6B | 책 |
| notebook_1 | 파랑 #6BA8F2 | 노트 |
| pencil_case_2 | 보라 #9E6BF2 | 필통 |
| lunchbox_3 | 주황 #F29E6B | 도시락 |
| water_bottle_4 | 청록 #6BD9D9 | 물병 |
| apple_5 | 분홍 #F28C8C | 사과 |
| snack_6 | 황토 #F2C84D | 과자 |
| ruler_7 | 초록 #80D980 | 자 |
| calculator_8 | 회보 #8C8CA6 | 계산기 |

마스코트는 노란 #FFD60A로 정체성 유지, 아이템은 다른 색으로 대비 (boxy-plan §A-6).

## Consequences

### 좋은 것
- 4×4 그리드도 320×320 bag-frame 가득 채움 (cellSize ≈ 76px)
- iPad mini도 600×600 bag-frame에 cellSize ≈ 145px로 적절
- 아이템 9개 색상 구분 → "지금 옮기는 게 어느 책인지" 시각적 학습
- §B-3-1 L3 "거의 다 됐는데" 트릭이 시각적으로 살아남 (위치 잘못 갔을 때 색이 안 맞음)

### 나쁜 것 / 위험
- Day 11~12 실제 아이템 스프라이트 (책·노트·과자 일러스트) 들어오면 base color 위에 그릴지, 완전 교체할지 결정 필요 — 일러스트 우선이 정답
- 색맹 사용자 — 빨/초/보라 구분 어려움. v1.1에서 패턴/모양도 추가 검토

## Revisit when
- 아이템 스프라이트 (Day 11~12) 들어오면 색상 매핑 → 스프라이트 매핑으로 교체
- 색맹 접근성 이슈 보고 시 패턴 오버레이 추가
