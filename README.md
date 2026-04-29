# Boxy

> Cozy packing puzzle — drag, rotate, fill the bag without gaps. Mound Studio's first game.

**Target launch**: 2026-05-28 · Solo dev · Unity 6.3 LTS

---

## 빠른 시작

### 정수 (개발자) 첫 실행

1. Unity Hub 실행 → Sign in with Google → Personal License 자동 활성
2. Hub → Open → 이 프로젝트 폴더 선택
3. 첫 컴파일 통과 후 메뉴 → `Boxy → Generate 50 Levels` 클릭
4. 4 씬 생성 + `Boxy → Scene Setup → Wire as [씬 종류]` 메뉴로 자동 wire
5. Build Settings에 4 씬 추가 → Play

세부 절차: [`boxy-unity-setup-guide.md`](boxy-unity-setup-guide.md)

### 단위 테스트

Window → Test Runner → EditMode → Run All. 39 cases 자동 검증.

### 정적 검사

```bash
./scripts/check-violations.sh all
```

CLAUDE.md §19 규칙 위반 자동 감지. pre-commit hook으로 자동 실행.

---

## 프로젝트 구조

```
Boxy/
├── Assets/
│   ├── Mound/                  # 재사용 자산 (다음 게임 그대로)
│   │   ├── Core/               # EventBus, ObjectPool, SaveSystem, SceneLoader, Audio
│   │   ├── UI/                 # tokens.uss, components.uss, PopupManager, ToastManager, HapticService
│   │   ├── Monetization/       # IAdProvider, IIapProvider, IConsentProvider + 래퍼
│   │   ├── Analytics/          # IAnalyticsProvider + Firebase 래퍼
│   │   └── Localization/       # IStringTable + StringKey
│   ├── Boxy.App/               # 게임 고유 로직
│   │   ├── Gameplay/           # 그리드 + 드래그 + 회전 + 광고 컨트롤러
│   │   ├── Levels/             # LevelData SO + 절차적 생성 + Tutorial
│   │   ├── Save/               # 진행도 + 별 + 인벤토리
│   │   ├── UI/                 # 5 UXML + Controller
│   │   └── BoxyBootstrap.cs    # 단일 진입점 (DI)
│   ├── Editor/                 # LevelGeneratorMenu + Build/Scene Setup
│   └── Tests/EditMode/         # 39 cases (5 fixtures)
├── boxy-plan.md                # 4주 일정 + 기획서
├── CLAUDE.md                   # AI 코드 규칙 (§0~§19)
├── ~/.mound/mound-design-system.md  # 디자인 시스템 v2
├── decisions/                  # 의사결정 기록 (10개)
├── marketing/                  # 광고 시나리오 + 피치덱 + ASO
├── legal/                      # Privacy/Terms 한국어 템플릿
├── design/                     # Stitch 시안 + App icon mockup
└── scripts/                    # check-violations.sh + setup-hooks.sh
```

---

## 개발 흐름

### Day 1 (4/29) — Unity Hub 활성 후
- KIPRIS / USPTO "Boxy" 상표 검색
- Top 50 패킹 퍼즐 직접 플레이 → 메커닉 후킹 결정
- Google Play Console / AdMob / AppLovin / AppsFlyer 가입

### Week 1 (4/29~5/5) — 영상 + 퍼블리셔
- Day 5-7: 광고 영상 2종 녹화/편집 ([`marketing/ad-scripts.md`](marketing/ad-scripts.md))
- Day 7: 퍼블리셔 6곳 콜드 이메일 ([`marketing/publishers/cold-emails.md`](marketing/publishers/cold-emails.md))

### Week 2 (5/6~12) — 캠페인 + 클로즈드 테스트
- Day 8: Facebook 광고 캠페인 ₩100,000 발사
- Day 13: 50레벨 직접 플레이 검증
- **Day 14 (5/12): 클로즈드 테스트 빌드 업로드 — 절대 마감**

### Week 3 (5/13~19) — SDK 통합
- Day 15-16: AppLovin MAX → AppLovinAdProvider TODO 채움
- Day 17: Unity IAP → UnityIapProvider TODO 채움
- Day 18: Firebase Analytics + Crashlytics → FirebaseAnalyticsProvider TODO 채움
- Day 20: GDPR/UMP → GoogleUmpConsentProvider TODO 채움
- Day 21: Crashlytics 강제 크래시 1회 → 대시보드 도착 확인

### Week 4 (5/20~27) — 폴리싱 + 출시
- Day 22-23: Profiler 측정 (60fps / GC 0B 목표)
- Day 24-25: 스토어 등록 ([`marketing/store-description-ko.md`](marketing/store-description-ko.md))
- Day 26: 5/12 시작 클로즈드 테스트 14일 통과
- **Day 27-28 (5/27-28): 프로덕션 출시**

---

## Mound 시리즈 비전

Boxy는 첫 게임. 같은 인프라 + Boxy 마스코트로:
- 6월: Boxy Sort (색깔 분류, 2주 개발)
- 7월: Boxy Cafe (방치형, 카페 사장)
- ASO에서 "Boxy" 검색 시 시리즈 전체 노출 → 누적 효과

---

## 라이선스

[`LICENSE.md`](LICENSE.md) 참조. © 2026 Mound Studio (백정수).

---

## 문의

dugout26.gm@gmail.com
