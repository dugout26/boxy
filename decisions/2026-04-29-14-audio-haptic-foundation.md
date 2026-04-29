# 2026-04-29 — 오디오 + 햅틱 기반 (Day 13)

## Context

기획서 §B-5 UX 핵심: "딱 맞을 때 '탁' / 꽉 채울 때 팡파레 / 클리어 컨페티". Day 13 폴리싱 작업.

빈 상태로 출시하면 시각만 좋고 무미건조 — 모바일 게임은 햅틱+사운드가 즉각적 만족감의 80%.

## Decision

### 5종 SFX placeholder 생성 (Python)
- `sfx_ui_tap.wav` 800Hz 60ms — 일반 탭
- `sfx_drag_start.wav` 440Hz 80ms — 드래그 시작
- `sfx_item_placed.wav` C5+G5 dual tone 120ms — 아이템 정확 배치
- `sfx_invalid.wav` 180Hz square 120ms — 잘못된 배치 (낮은 buzz)
- `sfx_level_cleared.wav` C-E-G-C 아르페지오 400ms — 레벨 클리어

Day 11~12에 외주/AI 사운드 디자인 들어오면 같은 파일명으로 교체.

### 햅틱 — 단계적 도입
- Mound.Core.Haptic.IHapticService + HapticIntensity (Light/Medium/Heavy)
- StandardHapticService — Android `Handheld.Vibrate()` (단순 진동), iOS는 silent
- Day 13 후반: Lofelt Nice Vibrations 또는 native UIImpactFeedbackGenerator 래핑 추가

### 이벤트 매핑
| Event | SFX | Haptic |
|---|---|---|
| ItemPlacedEvent | sfx_item_placed | Medium |
| LevelClearedEvent | sfx_level_cleared | Heavy |
| ItemRemovedEvent | sfx_ui_tap | Light |
| 드래그 시작 (UI) | sfx_drag_start | Light |
| 잘못된 배치 (UI) | sfx_invalid | Medium |

### 셋업 자동화
`Boxy.Editor.AudioSetupHelper.SetupAll`:
1. SfxLibrary.asset 생성 + 5 clips 매핑
2. MainMenu 씬에 BoxyBootstrap GameObject + AudioService 자식 + AudioSource 2개
3. SerializedObject로 Inspector wiring

### Bootstrap 통합
`BoxyBootstrap`:
- `[SerializeField] AudioService audioService`
- `[SerializeField] SfxLibrary sfxLibrary`
- `Audio` / `Haptic` 프로퍼티 노출
- `GameAudioBindings`로 EventBus 자동 구독 (Awake)

## Consequences

### 좋은 것
- 게임플레이 이벤트 → 즉시 사운드+햅틱 (피드백 사이클 ~10ms)
- placeholder 사운드라도 무음보다 훨씬 게임 느낌
- iOS 햅틱은 v1.0 출시 후라도 native 플러그인 추가 1줄로 활성화 가능
- AudioService.SetVolume(Bgm/Sfx) 이미 구현 — Settings 화면 슬라이더 연결만 하면 끝

### 나쁜 것 / 위험
- iOS 햅틱 v1.0 출시 시 무진동 (Android만 동작) — 출시 후 일주일 내 native 추가 권장
- placeholder SFX는 "경고음 같다"는 부정적 피드백 가능 — Day 11~12 진짜 사운드로 빠르게 교체 필수
- BGM 미구현 (조용한 게임 컨셉 OK인지 §B-5 재검토 필요)

## Revisit when

- iOS 햅틱 native 플러그인 통합 시점 (Day 13 후반 또는 출시 후 1주차)
- 사용자 리뷰에서 "사운드가 별로다" 피드백 5+ 건
- BGM 추가 결정 — boxy-plan §B-5 "조용한 코지" 컨셉 vs 일반적 게임 BGM
