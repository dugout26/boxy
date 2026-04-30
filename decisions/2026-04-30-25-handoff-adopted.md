# 2026-04-30 — 핸드오프 (App Onboarding v2) 채택, UI 전면 재작성

## Context

v1 UI는 웹 사이트 룩 + 클릭 안 되는 버튼(`display: none` 잔재) 문제로 정수가 폐기 결정. 4월 30일 정수가 Drive `Boxy/App Onboarding (2).zip` 업로드 — 8 화면(Splash/Onboarding/MainMenu/LevelSelect/Gameplay/Result/Ad/Settings) React+SVG 프로토타입 (Pretendard, cozy orange `#F59E4B`, 마스코트 5종 포즈). Drive API로 다운로드 + 분석 결과:

- 좌표계 = **390×800pt** (iPhone 12+ base, iOS frame 켜면 402×874pt)
- 사이즈가 iOS HIG (44pt 최소 터치) 정확히 충족
- 컬러/타이포/그림자 토큰이 v3 design-brief의 의도와 일치 (단지 좌표만 1080×1920 가정으로 잘못 짰던 것)

내가 어제 짠 v2 brief는 좌표 가정이 틀려서 모든 컴포넌트 사이즈가 비례 안 맞음. 핸드오프 채택이 가장 빠른 정상화 경로.

## Options

- **A. 핸드오프 좌표 그대로 (390×844pt) PanelSettings refRes로 직결** ← 채택
- B. Unity 1080×1920 유지 + 핸드오프 수치 모두 ×2.77 변환해서 USS 작성
- C. 핸드오프 무시하고 v2 brief를 살짝 보정만

A가 압도적: (1) 핸드오프 React 코드의 px 수치를 USS에 그대로 1:1 옮기면 됨. (2) Unity가 실기 해상도(iPhone Pro 1170×2532, iPad 744×1133 등)에 자동 스케일. (3) v1.1에서 디자이너 외주 받을 때 "Figma는 390pt에 그려달라" 요청만 하면 끝 — 변환 매트릭스 X.

## Decision

핸드오프 채택. 다음 변경:

1. **PanelSettings refRes**: 1080×1920 → **390×844** (`Assets/Boxy.App/UI/BoxyPanelSettings.asset` + `BoxyBuilder.cs` 강제 설정 둘 다)
2. **tokens.uss 재작성**: 핸드오프 `T` 객체의 모든 컬러를 그대로 — `--bg-cozy: #FFF6EC`, `--accent: #F59E4B`, `--accent-deep: #D97A2B`, `--text-primary: #3D2B1F`, 등
3. **components.uss 재작성**: 핸드오프 PrimaryBtn(56pt)/SecondaryBtn(48pt)/GhostBtn(44pt)/IconBtn(44pt round)/Card(16r)/Pill(30h)/MascotFrame(28r) 명세 1:1
4. **7개 UXML 전면 재작성** (`Boxy.App/UI/*.uxml`):
   - 컨트롤러가 `root.Q<>("name")`으로 query하는 element name은 보존 — 기존 `OnboardingController`는 `start-tutorial-button`/`skip-onboarding-button`을 찾고, `ResultPopupController`는 `result-title`/`result-mascot`/`result-star-1..3`/`result-{next,retry,menu}-button`, `SettingsController`는 `restore-button`/`language-button`/`version-value` 등.
   - 레이아웃은 핸드오프 그대로 (topbar / hero / CTA / dots / item tray 등)
   - 컨트롤러가 `AddToClassList("popup-overlay" / "popup-card" / "popup-title" / "popup-body" / "btn-primary" / "btn-secondary" / "level-button" / "level-button-locked" / "level-button-current" / "level-button-unlocked" / "theme-tab-active")`하므로 USS 클래스명 보존.
5. **`accent-boxy.uss` 삭제**: 모든 UXML이 import 안 함 (참조 0건). meta까지 삭제.
6. **design/design-brief.md v2 → v3 갱신**: 좌표계, 사이즈 표 핸드오프 기준으로 다시 씀.
7. **마스코트 5종 SVG**: `mascot.jsx`의 default/happy/sad/thinking/sleeping 그대로 보존. v1.0에서는 `Assets/Boxy.App/Resources/Mascot/Mascot_default.png` 한 장만 사용 (외주 일러스트 도착 시 5종 PNG 교체 또는 SVG → Unity Vector Graphics 패키지 변환).

### 컨트롤러 변경 범위 (이번 결정 X)

이번에는 controllers .cs 파일 전부 그대로 둠. 그 결과:

- **Onboarding 3-step swipe 미구현**: handoff에 있지만 controller에 step 진행 로직 없음. v1.0은 single-step로 시작. v1.1 백로그.
- **MainMenu의 sound-button**: handoff에는 없는 요소. controller가 `if (soundButton != null)`로 null-safe → UXML에서 hidden VisualElement 1개 placeholder.
- **Result의 stat row 텍스트 (소요시간/힌트/되돌리기)**: 컨트롤러는 query 안 함. UXML에 정적 표시 (controller가 채울 때 element name `stat-time/stat-hints/stat-undos`로 바인딩 추가하면 동작 — v1.1).

## Consequences

좋은 것:
- 디자이너 핸드오프 기준 그대로 1:1 — Figma → UXML 변환 매트릭스 X
- 모든 터치 타겟이 iOS HIG 44pt 충족 (web/desktop 사이즈 박힘 문제 해결)
- 컬러/타이포 토큰이 명확한 출처(handoff)에 박혀있어 v1.1 외주 시 바로 인계 가능
- 컨트롤러 변경 0 → 회귀 위험 최소

나쁜 것 / 위험:
- 핸드오프의 3-step Onboarding/마스코트 5종/그림자 2-layer/그라디언트 등 일부는 USS에서 표현 한계 → 단순화. 시각적 충실도 ~80%.
- Unity UI Toolkit USS는 React inline style보다 표현력 약함 (그림자 단일 레이어, gradient native 없음 등) — `--shadow-warm` 토큰은 정의했지만 실제 사용은 단색 `background-color`로 대체.
- iPhone SE 1세대(320pt) 미지원 — 390pt ref라 그 이하 디바이스에선 UI 잘림. v1.0 의도된 결정 (정수 결정).

## Revisit when

- 디자이너 외주 도착 (마스코트 5종 일러스트 / Hero 일러스트 / 아이콘) — 그때 UXML img 경로만 교체
- 사용자 테스트에서 터치 누락 보고 → 56pt CTA 더 키워야 할지 검토
- iPad 전용 레이아웃 (1024×1366) 필요 시 PanelSettings 분기 (현재 Expand로 stretch)
