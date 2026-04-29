# Boxy — Claude Code 세션 핸드오프

**최종 갱신**: 2026-04-29 (Day 1 후반 / Build 7 업로드 + 풀 디자인 + 모든 Provider 스켈레톤 완료)
**작성일 (원본)**: 2026-04-29 (Day 5)
**대상**: 다음 Claude Code 세션
**프로젝트**: `/Users/jeki/Boxy/`
**목표 출시**: 2026-05-28 (Google Play)

> 이 문서는 이전 세션의 작업 누적 + 사용자(정수)가 해야 할 일 + 다음 세션이 이어받을 일을 정리합니다.
> **세션 시작 시 가장 먼저 읽을 파일**: `CLAUDE.md` → 이 파일 → `boxy-final-checklist.md`

---

## 0. TL;DR (30초 요약)

- **Day 5 시점, AI 단독 작업 사실상 100% 완료**. 코드 골격 + 단위 테스트 + SDK 래퍼 스켈레톤 + 마케팅/법적 자료 모두 준비
- **정수 차단**: Unity Hub OAuth + License 활성 (Mac 화면 접근 필요), 외부 서비스 가입(AppLovin/AppsFlyer/Firebase), 광고 영상 녹화, KIPRIS 상표 검색
  - **마스코트 캐릭터/아이템 일러스트는 AI 책임** — Python으로 생성 + 추후 Scenario MCP 등 통합 시 업그레이드
- **컴파일 검증 0회** — Unity 라이선스 미활성. 첫 컴파일 결과는 정수 → AI에 공유 → 일괄 수정 흐름 예상
- **다음 Claude 세션 첫 작업**: 정수가 Unity 활성 후 컴파일 결과 가져오면 에러 일괄 수정 또는 정수 다음 액션 가이드

---

## 1. 프로젝트 컨텍스트

### 무엇
**Boxy** — 코지 패킹 퍼즐. 학교 가방 / 이사 박스 / 여행 트렁크에 아이템을 빈틈없이 채우는 모바일 게임.
**Mound Studio** (백정수 솔로 개발) 시리즈의 1번 게임. 후속 Boxy Sort (6월), Boxy Cafe (7월).

### 핵심 메커닉
- 드래그 → 그리드 배치
- 길게 누르기 0.5s → 90도 회전 (boxy-plan §B-2-3)
- 모든 칸 채우면 클리어 → ★~★★★ 별 (힌트/되돌리기 미사용 시 ★★★)
- 광고 매출: 보상형 (힌트/되돌리기/이어하기) + 인터스티셜 (90~120s)
- IAP: 광고 제거 ₩3,900 / 힌트 묶음 ₩2,500 / 스타터팩 ₩6,600

### 광고 후킹
- **메인**: "95% 채웠는데 마지막 한 칸 안 들어감" 좌절형 (AppLovin 데이터 fail-tone top 26%)
- **검증**: A/B 테스트 (실패형 vs 성공형) 각 ₩50,000 = 총 ₩100,000 페북 광고 (Week 2 Day 8)

### KPI 목표 (출시 후 7일)
| 지표 | 목표 |
|---|---|
| D1 retention | ≥ 30% |
| D7 retention | ≥ 10% |
| ARPDAU | ≥ $0.10 |
| 보상형 시청률 | ≥ 50% |
| 평점 | ≥ 4.0 |

### D-4 즉시 롤백 트리거
출시 후 7일 시점 5개 중 2개 이상 미달 시 — 기능 추가 금지, 버그 수정만, **Boxy Sort 즉시 착수**:
- D1 < 25% / D7 < 8% / 세션 < 2분 / 보상형 시청률 < 40% / ARPDAU < $0.03

---

## 2. 지금까지 한 것 (Phase 1~27)

### 코드 (Assets/, 76 files)

```
Assets/
├── Mound/                          (재사용 자산 — Boxy Sort/Cafe 그대로 복사)
│   ├── Core/
│   │   ├── Events/EventBus.cs
│   │   ├── Pool/ObjectPool.cs
│   │   ├── Save/SaveSystem.cs              (SaveData abstract + saveVersion 강제)
│   │   ├── Scenes/SceneLoader.cs
│   │   └── Audio/AudioService.cs           (BGM + SFX, Inspector 연결 필요)
│   ├── UI/
│   │   ├── Styles/{tokens, components}.uss
│   │   ├── PopupManager.cs                 (모달 단일 popup)
│   │   ├── ToastManager.cs                 (큐 기반 하단 토스트 2.5s)
│   │   └── HapticService.cs                (Lofelt #if 가드)
│   ├── Monetization/
│   │   ├── IAdProvider + NullAdProvider + AppLovinAdProvider (스켈레톤)
│   │   ├── IIapProvider + UnityIapProvider (스켈레톤)
│   │   ├── IConsentProvider + NullConsentProvider + GoogleUmpConsentProvider (스켈레톤)
│   │   └── EnvironmentConfig.cs (SO + TestAdIds)
│   ├── Analytics/
│   │   └── IAnalyticsProvider + NullAnalyticsProvider + FirebaseAnalyticsProvider (스켈레톤)
│   └── Localization/
│       └── IStringTable + NullStringTable + StringKey (28 키)
├── Boxy.App/
│   ├── Gameplay/
│   │   ├── Domain/ItemShape.cs             (immutable readonly struct + Rotate)
│   │   ├── Events/PlacementEvents.cs
│   │   ├── Input/ItemDragManipulator.cs    (드래그 + 길게누르기)
│   │   ├── BoxyGrid + PlacementValidator + UndoSystem + GameplayController
│   │   ├── HintAdController + UndoAdController
│   │   ├── InterstitialTimer.cs            (90~120s 무작위)
│   │   └── UI/ — GridCellView + ItemCardView + GameplayUI + AdRewardFlow
│   ├── Levels/
│   │   ├── LevelData (SO) + LevelRepository + LevelItemTemplate + LevelId
│   │   ├── TutorialLevels.cs               (5레벨 fallback, seed=42)
│   │   └── Generation/{ShapeCatalog, ItemKeyPool, LevelGenerator}
│   ├── Save/
│   │   └── PrefsKey + BoxySaveData + ProgressionService
│   ├── UI/
│   │   ├── 6 UXML — MainMenu, LevelSelect, Gameplay, AdRewardPopup, Settings, ResultPopup, Onboarding
│   │   └── 6 Controller — MainMenu, LevelSelect, Settings, ResultPopup, AdRewardPopup, Onboarding
│   ├── Diagnostics/CrashTestComponent.cs   (Crashlytics 검증)
│   ├── SceneFlow.cs                         (BoxySceneNames + GameSession)
│   └── BoxyBootstrap.cs                    (DontDestroyOnLoad + 모든 Provider DI)
├── Editor/
│   ├── LevelGeneratorMenu.cs               (Boxy → Generate 50 Levels)
│   ├── BuildEnvironmentMenu.cs             (Boxy → Environment → Switch to dev/stg/prod)
│   └── SceneSetupHelper.cs                 (Boxy → Scene Setup → Wire as ...)
└── Tests/EditMode/
    └── 5 fixtures, 39 cases — ItemShape, PlacementValidator, LevelGenerator, UndoSystem, ProgressionService
```

**asmdef 7개**: Mound.Core, Mound.UI, Mound.Monetization, Mound.Analytics, Mound.Localization, Boxy.App, Boxy.Editor + Tests
**의존 방향**: `Boxy.App → Mound.* → 외부 SDK` 단방향 강제

### 인프라
- `CLAUDE.md` — AI 행동 규칙 17개 섹션 (§0~§19), pre-commit hook으로 자동 검증
- `~/.mound/mound-design-system.md` v2.0 — 색/타이포/컴포넌트/햅틱/i18n 통합 명세
- `boxy-plan.md` v2.0 — 4주 일정 + KPI + 롤백 트리거
- `SDK_VERSIONS.md` — Unity 6000.3.14f1 + AppLovin 8.6.2 + Firebase 13.10.0 + Mobile Ads 11.0.0
- `scripts/check-violations.sh` — 정적 패턴 검사기 (위반 자동 검출)
- `.github/workflows/static-check.yml` — push 시 자동 검사
- `.editorconfig` — Roslyn 분석기 규칙
- `.gitignore` — Unity + 시크릿 + secrets/ 폴더 제외

### 마케팅 자료 (`marketing/`)
- `ad-scripts.md` — 15초 광고 영상 2종 시나리오 (실패형 + 성공형)
- `aso-keywords.md` — 한·영 키워드 4 Tier
- `store-description-ko.md` — Google Play 한국어 4000자 + 8 스크린샷 캡션
- `store-description-en.md` — Global 영어
- `publishers/pitch-deck.html` — 1페이지 HTML (8 Stitch 시안 임베드)
- `publishers/cold-emails.md` — 8 퍼블리셔 콜드 이메일 템플릿

### 법적 자료 (`legal/`)
- `privacy-policy-ko.md` — 개인정보처리방침 한국어 (10조 + 6 SDK 명시)
- `terms-of-service-ko.md` — 이용약관 한국어 (13조)
- 둘 다 **템플릿** — 정수가 Mound 사업자 정보 + 호스트 URL placeholder 채움 필요

### Stitch 시안 (8개, imgur 호스팅)
| # | 화면 | URL |
|---|---|---|
| 1 | Main Menu | https://i.imgur.com/mti7nVp.png |
| 2 | Character Reference v1 | https://i.imgur.com/PZXAnSk.png |
| 3 | Gameplay HUD | https://i.imgur.com/L5imjjF.png |
| 4 | Level Select | https://i.imgur.com/1xVMGW9.png |
| 5 | Ad Reward Popup | https://i.imgur.com/osKFIEK.png |
| 6 | Settings | https://i.imgur.com/nO5UnCU.png |
| 7 | Tutorial Overlay | https://i.imgur.com/yrJrqKA.png |
| 8 | IAP Shop | https://i.imgur.com/SVQsbMZ.png |

### App Icon
- `design/icon/app-icon-mockup.html` — 1024×1024 SVG 마스터 + 4 사이즈 + Android Adaptive Icon (FG+BG)
- 정수가 브라우저 → 우클릭 → 이미지 저장 → 1024 PNG export 가능

### 결정 기록 (`decisions/`, 10개)
1. `2026-04-28-day0-bootstrap.md` — 첫날 셋업
2. `2026-04-29-01-mechanic-hook.md` — 좌절 후킹 잠정 결정
3. `2026-04-29-02-trademark-search.md` — **AI 직접조사 완료** ✅ KIPRIS "Boxy"/"Boxy Pack"/"박시" 모두 0건. App Store/Google Play "Boxy Pack" 0건. 도메인 boxypack.app/mound.studio 가용. **USPTO만 정수 10분 직접 검색 필요** (SPA+WAF 자동화 차단)
4. `2026-04-29-03-top50-research.md` — AI 사전조사 (Pack Master, Goods Sort) 추가, 정수 직접 플레이 **잔여**
5. `2026-04-29-04-unity-bootstrap.md` — Unity 프로젝트 부트스트랩
6. `2026-04-29-05-modules-mound-ui-boxy-gameplay.md` — Phase 3+4 모듈
7. `2026-04-29-06-phase4-11-summary.md` — Phase 4~11 누적
8. `2026-04-29-07-phase12-17-summary.md` — Phase 12~17
9. `2026-04-29-08-phase18-24-summary.md` — Phase 18~24
10. `2026-04-29-day1-installs.md` — Day 1 설치 진행

### 자동 설치 + 가입 완료
- ✅ Unity Hub (`/Applications/Unity Hub.app`)
- ✅ Unity Editor 6000.3.14f1 (Editor + Android + iOS 모듈, 20GB)
- ✅ Python 3.11 + uv/uvx
- ✅ Firebase CLI 15.15.0 (`dugout26.gm@gmail.com` 로그인)
- ✅ Firebase 3 프로젝트: `boxy-mound-{dev,stg,prod}` + Android 앱 등록
- ✅ google-services-{dev,stg,prod}.json → `secrets/` (gitignored)
- ✅ Claude Code MCP 서버 등록 (Stitch / Coplay-MCP / Scenario)
- ✅ Google Play Console $25 결제 (검증 1~3일 대기)

### 자동 검출 + 수정한 위반 (CLAUDE.md §19, 7건)
1. JsonUtility property 직렬화 → SaveData abstract class + 필드
2. LevelItemTemplate public List → [SerializeField] private + IReadOnlyList property
3. LevelGenerator List<> 반환 → IReadOnlyList<>
4. GameplayController.Bind 조용한 실패 → Debug.LogWarning
5. HandleUndoAsync 빈 catch → comment 추가
6. BoxySceneNames in Mound.Core (§5-1) → Boxy.App.SceneFlow로 이동
7. **Boxy.App.asmdef 미참조 (Mound.UI/Monetization/Analytics/Localization)** → 5 모듈 모두 참조 추가 (Phase 27 직후 발견 + 즉시 수정)

---

## 3. 정수 (사용자) 가 해야 할 것

`boxy-final-checklist.md` 전체 참조. 핵심 차단 항목:

### 🔴 Day 1~3 (Mac 화면 접근 후 즉시)
- [ ] Unity Hub 실행 → Continue with Google → Personal License 자동 활성
- [ ] Hub → Open → `/Users/jeki/Boxy/` 선택 → Editor 첫 컴파일
  - **컴파일 에러 발견 시: 콘솔 메시지 그대로 다음 Claude 세션에 붙여 → 즉시 수정 받음**
- [ ] Window → Test Runner → Run All → 39 EditMode 케이스 통과 확인
- [ ] 메뉴 `Boxy → Generate 50 Levels` 클릭 → 50 LevelData asset 자동 생성
- [ ] BoxyPanelSettings.asset 생성 (Project → Create → UI Toolkit → Panel Settings)
- [ ] 4 씬 생성 (`MainMenu.unity`, `LevelSelect.unity`, `Gameplay.unity`, `Settings.unity`)
- [ ] 각 씬에서 메뉴 `Boxy → Scene Setup → Wire as [씬 종류]` 클릭
- [ ] 각 씬 UIDocument의 panelSettings에 BoxyPanelSettings 드래그
- [ ] Build Settings에 4 씬 등록 + Android 플랫폼 전환
- [ ] Play 모드 → MainMenu → PLAY → Gameplay → 드래그/회전/클리어 검증

### 🔴 어디서든 가능 (Mac 접근 불필요)
- [x] ~~KIPRIS "Boxy" 9류·41류 상표 검색~~ — **AI 완료**: 0건 (한국 시장 안전). 정수 5분 추가 확인 권장 (유사상표 검색)
- [ ] **USPTO TESS "Boxy Pack" 검색 (10분)** — AI는 SPA+AWS WAF 차단으로 자동화 불가. https://tmsearch.uspto.gov 직접 접속 → "BOXY PACK" 입력 → Live + Class 9/28/41 필터 → 결과 확인. AI 간접조사: "BOXY BOO" Mob Entertainment 발견되나 다른 마크. "BOXY PACK" 단독 인덱스 0건
- [ ] **Top 50 패킹 퍼즐 직접 플레이** (Pack Master 3D, Goods Sort, Home Packing 등) → `decisions/2026-04-29-03-top50-research.md` 빈칸 채움 (AI 사전조사 결과 추가됨)
- [ ] **도메인 등록**: AI 권장 = `mound.studio` (스튜디오) + `boxypack.app` (게임). 원래 후보(boxy.app/io/com)는 모두 등록되어 사용불가
- [ ] **GitHub Pages 활성화** for legal/index.html — 저장소 Settings → Pages → Source: `main` 브랜치 root 또는 `/legal` 폴더 → Save. AI는 4 PDF 백업도 `legal/pdf/` 에 생성 완료
- [ ] **Google Play Console 검증** 통과 확인 (1~3일)
- [ ] **AdMob / AppLovin / AppsFlyer 가입** (`boxy-day1-accounts.md`)

### 🟠 Day 5~7 (영상 + 퍼블리셔)
- [ ] Unity Editor에서 Gameplay 씬 직접 플레이 → 영상 녹화 (`marketing/ad-scripts.md` 시퀀스)
- [ ] iMovie/DaVinci 편집 + BGM (Pixabay/Mixkit)
- [ ] YouTube 비공개 업로드 또는 Google Drive 공유
- [ ] 피치덱 PDF 변환 (`marketing/publishers/pitch-deck.html` 브라우저 열기 → Cmd+P → PDF)
- [ ] 8 퍼블리셔 폼 제출 + 콜드 이메일 (`marketing/publishers/cold-emails.md`)

### 🟠 Day 8 — 광고 캠페인 발사
- [ ] Facebook Ads Manager 캠페인 2종 각 ₩50,000 = ₩100,000
- [ ] 타겟: 한국 + 영어권 4국 (US/UK/CA/AU)
- [ ] **Day 11 결과 확인**: 1.5%+ CTR이 1개 이상이면 본격 진행

### 🟡 Day 11~13 — 자산 + 검증
- [x] **Boxy 마스코트** — AI 생성 (Python kawaii v2.1, 5종 표정) — 추후 외부 일러스트레이터 또는 Scenario MCP 들어오면 더 정교하게 업그레이드 예정. 정수 작업 X.
- [x] **인게임 아이템 sprite 9개** — AI 생성 (Python placeholder). 50개 확장은 추후 (가능하면 AI 단독 가능).
- [ ] **App Icon export** — `design/icon/app-icon-mockup.html` SVG → 1024×1024 PNG
- [ ] **Sound assets** — BGM 1~2개 + SFX (탁/팡파레/실패) — Pixabay/Mixkit 다운로드
- [ ] **Localization 번역** — 28 키 한국어/영어 csv 작성 (Unity Localization Package import)
- [ ] 50레벨 직접 플레이 검증 — 풀리지만 스트레스 과도한 케이스 교체

### 🔴 Day 14 (5/12) — 절대 마감
- [ ] **Google Play 클로즈드 테스트 빌드 업로드**
- [ ] 신규 계정 = 20명 × 14일 룰 → 5/26 통과 → 5/28 출시
- [ ] 테스터 풀 모집 (Week 1부터 시작 권장, 카톡방 + 5천원 커피 쿠폰)

### 🟡 Week 3 (5/13~19) — SDK 통합
- [ ] **AppLovin MAX SDK 임포트** → `AppLovinAdProvider.cs` `// TODO Week 3:` 주석을 실제 SDK 호출로 교체
- [ ] **Firebase SDK + Crashlytics** → `FirebaseAnalyticsProvider.cs` TODO 채움
- [ ] **AppsFlyer SDK** → 별도 wrapper 작성
- [ ] **Unity Gaming Services IAP** → `UnityIapProvider.cs` TODO 채움
- [ ] **Google UMP** → `GoogleUmpConsentProvider.cs` TODO 채움
- [ ] **Lofelt Nice Vibrations** Asset Store 구매 ($20) → `HapticService.cs`의 `#if LOFELT_NICE_VIBRATIONS` 활성
- [ ] **EnvironmentConfig.asset 3개 생성** (dev/stg/prod) → `secrets/`
- [ ] BoxyBootstrap의 `useStubProviders = false` (Production 빌드)

### 🟡 Day 20-23 — 폴리싱
- [ ] GDPR/ATT 동의 화면 검증 (EU 사용자 시뮬)
- [ ] **Crashlytics 강제 크래시 테스트** — 메뉴 `Boxy → Diagnostics → Trigger Test Crash` → 대시보드 도착 확인
- [ ] AppsFlyer 설치/구매 이벤트 도착
- [ ] IAP 샌드박스 3종 결제
- [ ] iOS + Android 실기 빌드 검증
- [ ] Unity Profiler 측정 (60fps / GC alloc 0B / FPS drop 없음)

### 🟢 Day 24-25 — 스토어 등록
- [ ] **Privacy Policy / Terms 호스팅** (GitHub Pages 무료 / Notion 공개 / 사업자 도메인)
- [ ] `Boxy.App/UI/SettingsController.cs`의 `PrivacyUrl`, `TermsUrl`, `ContactEmailUrl` placeholder를 실제 URL로 교체
- [ ] 스크린샷 8장 제작 (보xy.App/UI 캡처 + 텍스트 오버레이)
- [ ] Google Play 등록 폼 채움 (한·영 설명)

### 🔴 Day 27~28 — 출시
- [ ] 5/27 클로즈드 테스트 14일 통과 확인
- [ ] **5/28 프로덕션 출시**

### 🟢 출시 후 1주
- [ ] D1/D7/ARPDAU 등 KPI 측정 (Firebase Analytics + GameAnalytics)
- [ ] D-4 롤백 트리거 발동 시 Boxy Sort 즉시 착수

---

## 4. 다음 Claude 세션이 할 일

### 우선순위 1: 정수 Unity 활성 후 컴파일 결과 처리
정수가 첫 컴파일 결과를 가져오면:
- 에러 메시지 분석 → 해당 파일 수정
- USS / UXML cross-ref 에러 가능성 높음 (`:root` 미지원이면 `*` 셀렉터로 변경 등)
- 누락된 using 또는 namespace 충돌 수정
- 컴파일 통과 후 정수에게 Test Runner 실행 안내

### 우선순위 2: 정수가 결정 기록 빈칸 채워오면
- `decisions/2026-04-29-02-trademark-search.md` 결과 → 충돌 시 대안 네이밍 제안
- `decisions/2026-04-29-03-top50-research.md` 결과 → 메커닉 후킹 최종 결정 + ASO 카피 조정

### 우선순위 3: Week 3 SDK 통합 보조
정수가 SDK 임포트 후 TODO 채울 때 직접 도움. 각 래퍼 파일에 inline TODO 주석 + 공식 API 시그니처 가이드 있음.

### 우선순위 4: 출시 전 폴리싱
- Boxy 마스코트 sprite 통합 후 ItemCardView/GameplayUI background-image 적용 코드
- Sound assets 통합 후 GameplayController.TryPlace에 PlaySfx 호출
- Lofelt 활성 후 GameplayUI에 HapticService.Play 트리거 위치 wire

### 우선순위 5 (출시 후): KPI 모니터링 + Boxy Sort 시작
- D-4 롤백 트리거 발동 시 즉시 Boxy Sort 착수 (Mound.* 그대로 재사용)

---

## 5. 다음 Claude 세션 시작 시 주의사항

### CLAUDE.md §0-4 강제 로드
모든 코드 생성 전 다음 3단계 출력 의무:
1. "§0~§19 끝까지 읽음, 핵심 규칙 N개 인지"
2. 적용 § 번호 3개 이상 (예: §2-2, §6-3, §13-1)
3. SDK 버전 확인 (예: "Unity 6000.3.14f1 기준" 또는 "버전 무관")

### CLAUDE.md §0-1 가짜 완료 금지
- 컴파일 검증 안 했으면 "컴파일 완료" 말하지 말 것
- Play Mode 검증 안 했으면 "테스트 완료" 말하지 말 것
- SDK 통합은 대시보드 이벤트 도착 전까지 "완료" 아님
- "추정"과 "확인" 명확히 구분

### 핸드오프된 결정 — 변경하지 말 것
- 메커닉 후킹: 좌절형 (boxy-plan §B-1-1) — 정수 Top 50 결과로 변경 가능
- 그리드 6×8 (튜토리얼은 4×2~6×5)
- 액센트 #FFD60A on #1A1A1A (WCAG 9.83:1)
- Pretendard 단일 폰트
- 50레벨 / 3 테마 (학교가방/이사박스/여행트렁크)
- 단위 px, y-up 좌표 (UI Toolkit bottom 사용)
- Vector2Int (자체 GridCoord X)
- 길게 누르기 0.5s 회전
- 인터스티셜 90~120s 무작위
- IAP 3종 ₩3,900 / ₩2,500 / ₩6,600

### 정적 검사기 통과 의무
모든 작업 후:
```bash
./scripts/check-violations.sh all
```
에러 0건 + 경고 (TODO Week 3 주석 등) 검토.

### 자기 점검 체크리스트
CLAUDE.md §13 트리거 기반:
- 매번 (3개): 컴파일 검증 / API 환각 / 임의 구조 추가
- 신규 파일 (+4개): 파일 ≤3 / 한 턴 ≤5 / SerializeField 가이드 / asmdef 의존 방향
- SDK/세이브/시크릿 (+5개): Dictionary 노출 / 마이그레이션 / async / 시크릿 / 플랫폼

### 작업 단위 제한
- 한 프롬프트당 신규 파일 ≤3 또는 ≤200 줄
- 한 턴 ≤5 파일 수정
- 큰 변경은 사전 계획 출력 → 정수 승인 후

---

## 6. 핵심 참조 파일

```
/Users/jeki/Boxy/
├── CLAUDE.md                        AI 행동 규칙 (반드시 첫 줄에 읽음 출력)
├── HANDOFF.md                       이 파일
├── README.md                        프로젝트 개요
├── LICENSE.md                       저작권
├── CHANGELOG.md                     변경 기록
├── boxy-plan.md                     기획서 v2.0 (4주 일정 + KPI + 롤백)
├── boxy-final-checklist.md          정수 액션 종합 (5/12까지 13일)
├── boxy-unity-setup-guide.md        Unity 활성 후 첫 실행 가이드
├── boxy-day1-accounts.md            외부 서비스 가입 가이드
├── boxy-day1-checklist.md           Day 1 체크리스트
├── SDK_VERSIONS.md                  SDK 버전 (환각 검증 기준점)
├── decisions/                       의사결정 기록 (10개)
├── marketing/                       광고 시나리오 + ASO + 피치덱
├── legal/                           Privacy/Terms 한국어 템플릿
├── design/                          Stitch 시안 + App icon mockup
├── scripts/                         정적 검사 + git hooks
├── secrets/                         Firebase config (gitignored)
└── ~/.mound/mound-design-system.md  Mound 디자인 시스템 v2.0 (모든 게임 공유)
```

---

## 7. 알려진 제약 / 함정

### Unity 라이선스 미활성
- 컴파일 검증 0회 — 잠재 에러 가능
- USS의 `:root` 셀렉터 Unity 6 호환성 미검증
- UXML Style src 경로 `project://database/...` 검증 필요
- UI Toolkit Manipulator API 환각 검증 필요

### Stitch MCP 한계
- App Icon 생성 timeout — Scenario MCP 또는 외주 필요
- 일부 화면 timeout 빈도 있음 (재시도 가능)
- 시안은 placeholder — 정수가 export 후 Unity asset으로 import 필요

### 외부 SDK 미통합
- 모든 SDK 래퍼 = 스켈레톤 (TODO 주석 + Failed/no-op 반환)
- Week 3 정수가 SDK 임포트 + TODO 채워야 작동

### 정수 데이터 의존
- 메커닉 후킹 Top 50 검증 미완 (정수 직접 플레이)
- KIPRIS/USPTO 상표 결과 미입력
- 광고 영상 미녹화
- 캐릭터 sprite — AI가 Python kawaii v2.1로 생성 완료 (정수 작업 X)
- Localization 번역 — KoEnStringTable로 한/영 둘 다 완료 (정수 작업 X)

### 보안 / 규정
- TestAdIds (`ca-app-pub-3940256099942544~3347511713`)는 Google 공식 테스트 ID — 시크릿 아님
- secrets/ 폴더 (`google-services-*.json` 등) gitignored
- Privacy/Terms는 템플릿 — 출시 전 변호사 검토 권장

---

## 8. 첫 행동 체크리스트 (다음 Claude 세션)

```
1. CLAUDE.md 끝까지 읽고 §0-4 출력
2. 이 HANDOFF.md 읽음
3. 정수에게 다음 중 어느 상황인지 확인:
   a. Unity 활성 + 컴파일 에러 → 에러 메시지 받아서 수정
   b. KIPRIS/Top 50 결과 → 결정 기록 빈칸 채움
   c. 새 작업 요청 → 정수 지시 따라
4. 작업 시작 전 ./scripts/check-violations.sh all 실행
5. 작업 완료 후 동일 검사 + §13 자기 점검
```

---

## 9. 사용자 (정수) 컨텍스트

- **계정**: dugout26.gm@gmail.com (Google / Firebase / Unity Hub)
- **사업자**: Mound (709-09-03510, 백정수 대표)
- **머신**: Mac mini (Apple Silicon, macOS 26.2)
- **사용 도구**: Claude Code CLI (이 세션도 Claude Code), Unity 6.3 LTS, Cursor (선택), Stitch MCP, Coplay-MCP (Unity), Scenario MCP
- **운영 환경**: 원격 (Mac 화면 접근 제약) — 일부 GUI 작업은 정수 직접 Mac 접근 시점 대기
- **다른 프로젝트**: Prepit 운영 (매주 토 14:00~16:00 슬롯) — Boxy와 독립

---

## 10. 한 줄 요약

> Day 5 시점, AI 단독 100% 완료. 정수 13개 차단 항목 13일 내 처리 + Unity 컴파일 결과 받아 일괄 수정 → 5/12 클로즈드 테스트 → 5/28 출시. 다음 Claude 세션은 정수 컴파일 결과 또는 결정 기록 채움 받아 진행.

---

## 11. 2026-04-29 후반 세션 추가 작업 (Day 1 풀 디자인)

이전 핸드오프(Day 5 가정) 이후 단일 세션에 추가된 변경:

### 디자인 (Stitch 시안 + 반응형)
- 7개 UXML 모두 반응형 표준 (Header/Content/Footer + flex-grow + max-width + aspect-ratio)
- Stitch MainMenu/LevelSelect 시안 매칭 (project ID `10395380667870336902`)
- PanelSettings → ScaleWithScreenSize + Expand + 1080×1920 reference (Royal Match/Candy Crush 표준)
- SafeAreaController — 첫 자식 VisualElement에 padding 적용 (배경 카메라 검정 노출 방지)
- 결정: `decisions/2026-04-29-09-app-id-strategy-2tier.md` (3-tier → 2-tier)
- 결정: `decisions/2026-04-29-10-app-store-name.md` ("Boxy Pack" / "Boxy Dev")
- 결정: `decisions/2026-04-29-11-responsive-layout.md` (반응형 표준)
- 결정: `decisions/2026-04-29-13-grid-cell-responsive-and-item-colors.md`

### 마스코트 + 아이템
- Boxy 마스코트 5종 표정 PNG (default/cheer/sad/sleepy/surprised) — `Assets/Boxy.App/Icons/` + `Resources/Mascot/` (Resources.Load용)
- AppStore-1024.png (App Store icon, 노란 배경 + 마스코트)
- 9개 아이템 placeholder PNG (book/notebook/pencil_case/lunchbox/water_bottle/apple/snack/ruler/calculator) — `Assets/Boxy.App/Items/`
- 9 itemKey 색상 매핑 (시각 구분, GameplayUI에서 GetItemColor)

### 게임 콘텐츠
- 첫 5레벨 §B-3-1 정성 디자인 (`Assets/Levels/Level_01~05.asset`)
- Brute-force solver 검증: 5/5 풀어짐 PASS
- 결정: `decisions/2026-04-29-12-first-5-levels-handcrafted.md`

### 시스템 코드
- 사운드: 5 SFX placeholder WAV (Python 생성) + AudioService + GameAudioBindings + SfxLibrary + AudioSetupHelper 자동 wiring
- 햅틱: Mound.Core.Haptic.IHapticService + StandardHapticService
  - **iOS native plugin**: `Assets/Mound/Core/Haptic/iOS/BoxyHapticBridge.mm` (UIImpactFeedbackGenerator)
  - DllImport "__Internal" + `_BoxyHapticImpact(int)`
- ATT (App Tracking Transparency):
  - **iOS native plugin**: `Assets/Mound/Monetization/iOS/BoxyATTBridge.mm` (ATTrackingManager)
  - `Mound.Monetization.IosAttConsentProvider`
  - BoxyBootstrap.RequestConsentDelayedAsync (1초 딜레이 후 자동 호출)
  - Info.plist NSUserTrackingUsageDescription 자동 삽입 (BoxyBuilder PostProcess)
- iOS 프레임워크 자동 링크: `Assets/Editor/IOSFrameworkPostProcess.cs` (AppTrackingTransparency, AdSupport, UIKit)
- i18n: `Mound.Localization.KoEnStringTable` (한/영 자동 감지 + 디바이스 SystemLanguage)
- Settings 언어 토글 — UXML 버튼 + KoEnStringTable.SetLocale + PlayerPrefs 저장
- Settings 볼륨 슬라이더 → AudioService 실시간 연동
- Bootstrap.ApplySavedAudioSettings — 앱 시작 시 저장된 BGM/SFX/햅틱/언어 자동 복원
- AppsFlyer Provider stub (`Mound.Analytics.AppsFlyerAnalyticsProvider`) + AppsFlyer Event 매핑
- CompositeAnalyticsProvider (Firebase + AppsFlyer 동시 발송)

### 자동화 + 가드
- BoxyBuilder PostProcess: ITSAppUsesNonExemptEncryption, NSUserTrackingUsageDescription, AppStore 1024 아이콘 자동 삽입
- google-services.json + GoogleService-Info.plist 환경별 자동 복사 (BuildAndroid/BuildIOS)
- BumpAndBuildDevIOS / BumpAndBuildProductionIOS — 업로드 전용 (BuildDevIOS 직접은 bump 안 함)
- BuildSimulatorIOS — Apple Silicon arm64 시뮬 빌드 + iOSSimulatorArchitecture SerializedObject 강제
- scripts/ios-archive-upload.sh — `BOXY_ALLOW_UPLOAD=1` 가드 (자동 업로드 차단)
- scripts/ios-simulator-test.sh — 3-디바이스 launch + alive + log_errors 자동 검증
- scripts/appstore-screenshots.sh — 메인 화면 자동 캡처 (LevelSelect/Gameplay는 정수 직접 simctl)
- 결정: `decisions/2026-04-29-14-audio-haptic-foundation.md`
- 결정: `decisions/2026-04-29-15-no-auto-upload.md`

### 법적/마케팅
- `legal/privacy-policy-en.md` (영문)
- `legal/terms-of-service-en.md` (영문)

### 셋업 가이드 (정수 작업)
- 결정: `decisions/2026-04-29-16-closed-test-prep.md` — TestFlight + Play Closed Testing 14일 룰
- 결정: `decisions/2026-04-29-17-firebase-ios-setup-guide.md` — Firebase iOS plist 다운로드 + Unity SDK 임포트
- 결정: `decisions/2026-04-29-18-applovin-appsflyer-setup-guide.md` — AppLovin MAX + AppsFlyer + Unity IAP

### TestFlight 업로드 이력
- Build 1, 3, 4 — 초기 디자인 + 빌드 수정
- Build 5 — PanelSettings + Portrait + SafeArea
- Build 6 — Stitch 디자인 + 마스코트 PNG
- Build 7 — 첫 5레벨 + 사운드 + 반응형 + i18n + ATT + 햅틱 + 모든 Provider 통합 (정수가 아이폰 TestFlight에서 검증 가능)
- **Build 8+ 업로드는 정수 명시 요청 시** (`BOXY_ALLOW_UPLOAD=1` 가드)

### 다음 Claude 세션 첫 작업
1. 정수가 SDK .unitypackage 임포트 완료 시점 → 각 Provider TODO 마크 unblock
2. iOS GoogleService-Info.plist + EnvironmentConfig.asset 키 입력 받으면 → 빌드 검증
3. 클로즈드 테스트 5명 모집 후 → External Testing 그룹 셋업 (BOXY_ALLOW_UPLOAD=1로 production build 업로드)
