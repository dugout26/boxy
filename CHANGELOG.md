# Boxy — Change Log

All notable changes to Boxy + Mound modules tracked here.
Format: [Semver-ish] - YYYY-MM-DD

---

## [Unreleased] — Day 5 (2026-04-29)

### 🎉 Project bootstrap + 27 phases complete

**Code skeleton 100% — Compile pending Unity license activation**

#### Added — Mound modules
- `Mound.Core` — EventBus, ObjectPool, SaveSystem (with SaveData abstract + saveVersion), SceneLoader, AudioService
- `Mound.UI` — tokens.uss + components.uss (USS design system v2), PopupManager, ToastManager, HapticService (Lofelt wrapper)
- `Mound.Monetization` — IAdProvider + NullAdProvider + AppLovinAdProvider (skeleton), IIapProvider + UnityIapProvider (skeleton), IConsentProvider + GoogleUmpConsentProvider (skeleton), EnvironmentConfig SO + TestAdIds
- `Mound.Analytics` — IAnalyticsProvider + NullAnalyticsProvider + FirebaseAnalyticsProvider (skeleton)
- `Mound.Localization` — IStringTable + NullStringTable + StringKey (28 keys)

#### Added — Boxy.App
- `Gameplay/Domain` — ItemShape (immutable readonly struct, rotation + normalization)
- `Gameplay` — BoxyGrid, PlacementValidator, UndoSystem, GameplayController, HintAdController, UndoAdController, InterstitialTimer
- `Gameplay/Input` — ItemDragManipulator (drag + long-press 0.5s rotation)
- `Gameplay/UI` — GridCellView, ItemCardView, GameplayUI, AdRewardFlow
- `Gameplay/Events` — PlacementEvents (struct events for EventBus)
- `Levels` — LevelData SO + LevelRepository + LevelItemTemplate + LevelId struct
- `Levels/Generation` — ShapeCatalog (1×1, 1×2, 1×3, 2×2, 2×3, L, T), ItemKeyPool (school/box/trunk), LevelGenerator (procedural fill)
- `Levels/TutorialLevels` — first 5 levels runtime fallback (boxy-plan §B-3-1)
- `Save` — PrefsKey + BoxySaveData + ProgressionService (stars + unlocks + inventory)
- `UI` — 6 UXML (MainMenu/LevelSelect/Gameplay/AdRewardPopup/Settings/ResultPopup/Onboarding)
- `UI` — 5 Controllers (MainMenu/LevelSelect/Settings/ResultPopup/AdRewardPopup/Onboarding)
- `Diagnostics/CrashTestComponent` — Firebase Crashlytics 검증
- `BoxyBootstrap` — DontDestroyOnLoad 단일 진입점 (DI Service Locator)

#### Added — Editor tooling
- `Boxy.Editor.asmdef` (includePlatforms: Editor)
- `LevelGeneratorMenu` — 메뉴 "Boxy → Generate 50 Levels" (boxy-plan §B-3 레벨 구조 자동)
- `BuildEnvironmentMenu` — dev/stg/prod 빌드 환경 자동 전환 (CLAUDE.md §8 빌드 분리)
- `SceneSetupHelper` — 4 씬 자동 wire (UIDocument + Controller 컴포넌트)

#### Added — Tests
- `Boxy.Tests.EditMode.asmdef` (UNITY_INCLUDE_TESTS + nunit)
- 5 fixtures, 39 cases:
  - ItemShapeTests (6) — FromCells / Rotate / Normalize
  - PlacementValidatorTests (8) — Valid/OutOfBounds/Collision + BoxyGrid
  - LevelGeneratorTests (7) — 충전 보장 + 결정성 + 고유 키
  - UndoSystemTests (9) — free/paid/grant/reset
  - ProgressionServiceTests (9) — 별 갱신 + Save round-trip + InMemorySaveSystem mock

#### Added — 마케팅 자료
- `marketing/ad-scripts.md` — 15초 광고 영상 2종 시나리오 (실패형 + 성공형)
- `marketing/aso-keywords.md` — 한·영 키워드 4 Tier
- `marketing/store-description-ko.md` — Google Play 한국어 (4000자)
- `marketing/store-description-en.md` — Google Play / App Store 영어
- `marketing/publishers/pitch-deck.html` — 1페이지 HTML (8 Stitch 시안 임베드)
- `marketing/publishers/cold-emails.md` — 8 퍼블리셔 콜드 이메일 (영/한)

#### Added — 법적
- `legal/privacy-policy-ko.md` — 개인정보처리방침 (10조 + 6 SDK 명시)
- `legal/terms-of-service-ko.md` — 이용약관 (13조)

#### Added — 인프라
- `CLAUDE.md` — AI 코드 규칙 17개 섹션 (§0~§19)
- `~/.mound/mound-design-system.md` v2.0 — Mound 브랜드 디자인 시스템
- `boxy-plan.md` v2.0 — 4주 일정 + KPI + 롤백 트리거
- `SDK_VERSIONS.md` — Unity 6000.3.14f1 + AppLovin 8.6.2 + Firebase 13.10.0 + Mobile Ads 11.0.0
- `boxy-unity-setup-guide.md` — Hub OAuth → 첫 실행 가이드
- `boxy-final-checklist.md` — 5/12까지 13일 정수 액션
- `boxy-day1-accounts.md` — 외부 서비스 가입 가이드
- `boxy-day1-checklist.md` — Day 1 실행 체크리스트
- `decisions/` — 10 decision records (Phase별 결정 추적)
- `scripts/check-violations.sh` — 정적 패턴 검사기 (CLAUDE.md §19)
- `scripts/setup-hooks.sh` — git pre-commit hook 설치
- `.github/workflows/static-check.yml` — push 시 자동 정적 검사
- `.editorconfig` — Roslyn 분석기 규칙
- `.gitignore` — Unity + 시크릿 보호

#### Added — Stitch 시안 (8개, 모두 imgur 호스팅)
- Main Menu / Character Reference v1 / Gameplay HUD
- Level Select / Ad Reward Popup / Settings
- Tutorial Overlay / IAP Shop

#### Added — App Icon
- `design/icon/app-icon-mockup.html` — 1024×1024 SVG 마스터 + 4 사이즈 + Adaptive Icon (FG+BG)

---

### 자동 검출 + 수정한 위반 (CLAUDE.md §19)

7건:
1. JsonUtility property 직렬화 — ISaveData → SaveData abstract class
2. LevelItemTemplate public List → [SerializeField] private + IReadOnlyList property
3. LevelGenerator List<> 반환 → IReadOnlyList<>
4. GameplayController.Bind 조용한 실패 → Debug.LogWarning
5. HandleUndoAsync 빈 catch → comment 추가
6. BoxySceneNames in Mound.Core (§5-1) → 제거 (Boxy.App.SceneFlow로 이동)
7. ProgressionServiceTests 어색한 extension 헬퍼 → Data.unlockedLevel 직접 접근

---

## 알려진 미구현 (Day 5 시점)

- iOS 빌드 (v1.1)
- Apple Privacy Manifest (PrivacyInfo.xcprivacy)
- Localization 실제 csv 번역 파일
- Notification icon (푸시 도입 시)
- 다크모드 (mound-design-system v2.1+)
- 일일 퍼즐 / 랭킹 / 꾸미기 (boxy-plan §B-6 v1.1)

---

## Mound Brand Design System

### v2.0 (2026-04-28)
**Breaking from v1**:
- 단위 sp/dp → px (USS 호환)
- 액센트 위 텍스트 흰색 → `--on-accent` 토큰 (WCAG AA 4.5+)
- 햅틱 "Unity Haptic API" → Lofelt Nice Vibrations
- Body Light(300) → Regular(400) (모바일 가독성)
- 폰트 Pretendard + Inter → Pretendard 단일 (-3~5MB)
- 화면 비율 9:16 → 9:19.5~9:21
- i18n 자체 JSON → Unity Localization Package
- 다크모드 → v2.0은 라이트 전용 명시

---

## 다음 출시 (Boxy v1.0)
- 예정일: 2026-05-28
- 플랫폼: Google Play (Android), App Store v1.1 보류
- 50 levels, 3 themes (school bag / moving box / travel trunk)
- IAP: 광고 제거 ₩3,900 / 힌트 묶음 ₩2,500 / 스타터팩 ₩6,600

---

## Day 1 후반 (2026-04-29) — 풀 디자인 + 모든 Provider + 실제 자산

### 디자인 (Stitch 시안 + 반응형 + 아트)
- 7개 UXML 반응형 표준 (Header/Content/Footer + flex-grow + max-width + aspect-ratio)
- Stitch MainMenu/LevelSelect 시안 매칭 (project 10395380667870336902)
- PanelSettings ScaleWithScreenSize + Expand + 1080×1920 (decision #11)
- SafeAreaController (iPhone 노치 + 홈 인디케이터)
- 그리드 셀 동적 사이즈 (디바이스마다 자동 cellSize, decision #13)
- 9 itemKey → 25+ base 색상 매핑 (50 itemKey 모두 커버)

### 마스코트 + 아이템 (AI 책임, 정수 작업 0)
- Boxy 마스코트 v2.1 kawaii 5종 표정 (default / cheer / sad / sleepy / surprised)
- AppStore 1024 아이콘 (마스코트 중앙, 노란 배경)
- 34개 base 아이템 PNG (책/노트/필통/도시락/물병/사과/과자/자/계산기 등)
- Boxy 스플래시 로고

### 게임 콘텐츠
- 첫 5레벨 §B-3-1 정성 디자인 (decision #12, brute-force solver 검증 PASS)
- 50/50 자동 생성 레벨 모두 풀어짐 (Python solver)

### 시스템 코드
- 사운드: 5 SFX + BGM (Cozy Loop) + EventBus 자동 구독
- 햅틱: Android Handheld.Vibrate + iOS UIImpactFeedbackGenerator native plugin
- ATT (App Tracking Transparency) iOS native + 1초 딜레이 자동 요청
- iOS 프레임워크 PostProcess (AppTrackingTransparency, AdSupport, UIKit 자동 링크)
- Info.plist PostProcess (ITSAppUsesNonExemptEncryption, NSUserTrackingUsageDescription)
- KoEnStringTable 한/영 (32 키, 디바이스 SystemLanguage 자동 + Settings 토글)
- ResultPopup 마스코트 표정 동적 교체 + 별 stagger pop-in + 마스코트 bounce
- ItemCardView 회전/드래그 사운드 + 햅틱
- GameplayUI 정확/잘못된 배치 cell flash (그린/빨강) + 햅틱
- LevelSelect 현재 레벨 자동 스크롤 + 인사말 i18n
- MainMenu 마스코트 idle bob (sin 곡선 ±8px)
- Settings 진행도 초기화 확인 다이얼로그 (destructive 빨강) + 버전 동적
- Settings 볼륨 슬라이더 → AudioService 실시간

### Provider / SDK 격리
- AppsFlyerAnalyticsProvider stub (Day 18 SDK 통합 후 unblock)
- CompositeAnalyticsProvider (Firebase + AppsFlyer 동시 발송)
- IosAttConsentProvider (iOS) + GoogleUmpConsentProvider stub (Android)
- Firebase google-services.json + GoogleService-Info.plist 환경별 자동 복사

### i18n / 법적
- KoEnStringTable.cs (in-memory 한/영)
- legal/privacy-policy-en.md
- legal/terms-of-service-en.md

### 자동화 + 가드
- BoxyBuilder dev/prod iOS+Android (build number 분리, productName 환경별)
- Bundle ID 2-tier (decision #9, dev=com.mound.boxy.dev / prod=com.mound.boxy)
- App Store 표시명 (decision #10, "Boxy Pack" / "Boxy Dev")
- BoxyAutomation / AudioSetupHelper / IOSFrameworkPostProcess
- 시뮬레이터 3-디바이스 자동 검증 + 4 씬 스크린샷 자동화
- TestFlight 자동 업로드 차단 (`BOXY_ALLOW_UPLOAD=1` 명시 필요, decision #15)

### 테스트
- EditMode 17개 신규 (KoEnStringTable 8 / ItemColor 2 / SaveMigration 7)
- EditMode 9개 추가 (BoxyGridEdgeCase)
- PlayMode 4개 신규 (GameplayFlow)
- 총 26+ 신규 테스트 컴파일 통과 (38 기존 + 26 = 64+)

### TestFlight 업로드 이력
- Build 1, 3, 4, 5, 6, 7 — 디자인 점진 진화 + 풀 통합
- Build 8+ 대기 (정수 명시 요청 시)

### 결정 기록 누적
- #09 ~ #19 (총 11개 신규)
- HANDOFF.md §11 추가 (다음 세션 픽업)
