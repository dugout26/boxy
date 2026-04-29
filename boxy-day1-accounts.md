# Boxy — Day 1 계정 생성 가이드 (정수용)

순서대로 진행. AdMob부터 (검증 가장 오래 걸림). 각 서비스 끝나면 **AI에게 넘길 값** 섹션 채워서 알려주면 EnvironmentConfig.asset에 자동 입력됨.

**Mound 사업자 정보 (모든 서비스에 동일)**:
- 상호: Mound
- 사업자등록번호: 709-09-03510
- 등록일: 2026-03-01
- 대표: 백정수
- 주소: [정수 사업자 주소]
- 이메일: dugout26.gm@gmail.com (또는 사업자 이메일)
- 전화: [정수 전화]

---

## 1. AdMob — **기존 Mound (Prepit) 계정 재사용** ✅ (Day 1~2 검증 1~2일 절약)

### 핵심 발견 (2026-04-29 AI 조사)
Dugout/Prepit 프로젝트(`/Users/jeki/dugout/lib/features/ads/data/services/admob_service.dart`) 분석 결과 Mound 소유 publisher 계정 확인:
- **Publisher ID**: `pub-6728704748176389` (Mound = 백정수 보유)
- **재사용 가능** (Prepit 출시 때 이미 완료):
  - ✅ AdMob 계정 가입 + AdSense 약관
  - ✅ 결제 정보 (W-8BEN, 한국 은행 SWIFT, 주소 PIN 검증)
  - ✅ 세금 정보 (TIN: 709-09-03510)
- **재사용 불가** (Boxy 전용 신규 필요):
  - ❌ 광고 단위 ID — Prepit 앱 전용 (`6728704748176389/7853262529` 등). Boxy에 그대로 사용 시 **AdMob 정책 위반 → 계정 정지**
  - ❌ 앱 등록 — `com.mound.boxy.dev` / `com.mound.boxy` 신규 등록 필요

### Step 1: AdMob 로그인 (가입 단계 스킵 ✅)
1. https://admob.google.com/ 접속
2. 기존 Mound Google 계정으로 로그인 (Prepit 때 사용한 계정 — `dugout26.gm@gmail.com` 추정)
3. **약관 동의 / 결제 / 세금 / 통화 설정 모두 스킵** — 기존 설정 그대로 사용

### Step 2: 결제 정보 (이미 완료 — 스킵 ✅)
> Prepit 출시 때 W-8BEN + SWIFT + 주소 PIN 모두 완료. 변경 사항 없으면 그대로 사용.
> **단, AdMob 대시보드에서 "Payments → Settings" 진입해서 모두 활성 상태인지 1분 확인** (계정 dormant 상태면 reactivate 필요)

### Step 3: 앱 등록 (Boxy 전용 신규)
"앱" 메뉴 → 앱 추가:
- **앱 1 (Dev)**: 이름 "Boxy Dev", 플랫폼 Android, Bundle `com.mound.boxy.dev`, "앱이 아직 게시되지 않았습니다"
- **앱 2 (Production)**: 이름 "Boxy Pack", Android, Bundle `com.mound.boxy`, 동일
- iOS도 각각 1개씩 (Bundle 동일, 플랫폼만 iOS) — iOS는 §J SDK 통합 단계에 등록해도 무방
- **2-tier 전략** (`decisions/2026-04-29-09-app-id-strategy-2tier.md`) 따름 — `staging`은 폐지

> v1.0 출시까지는 production만 등록하고 dev는 **테스트 ID** 사용해도 충분 (CLAUDE.md §8-2 권장 — 정책 위반 회피)

### Step 4: 광고 단위 생성 (각 앱마다 3개)
앱 → 광고 단위 → 광고 단위 추가:

| 광고 단위 이름 | 형식 | 위치 |
|---|---|---|
| `Boxy_Rewarded` | **보상형** | 힌트/되돌리기/이어하기 광고 |
| `Boxy_Interstitial` | **전면 광고** | 90~120초마다 |
| `Boxy_Banner` | **적응형 배너** | 메뉴 화면 하단 |

각각 **광고 단위 ID** (`ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX`) 발급됨 — 복사 보관.

### 🟢 AI에게 넘길 값
**publisher 부분은 이미 알고 있음**: `ca-app-pub-6728704748176389` (Mound 계정). 정수가 채울 부분은 `~` / `/` 뒤 unit ID만.
```
AdMob App ID (Production):           ca-app-pub-6728704748176389~__________
AdMob Rewarded ID (Production):      ca-app-pub-6728704748176389/__________
AdMob Interstitial ID (Production):  ca-app-pub-6728704748176389/__________
AdMob Banner ID (Production):        ca-app-pub-6728704748176389/__________

(dev — 만든 경우만)
AdMob App ID (Dev):                  ca-app-pub-6728704748176389~__________
```
**참고 — Prepit 광고 unit ID** (Mound 계정 동일하지만 Prepit 앱 전용, Boxy에서 사용 시 정책 위반):
- Prepit Banner Android: `ca-app-pub-6728704748176389/7853262529`
- Prepit Interstitial Android: `ca-app-pub-6728704748176389/6685221327`
- Prepit Rewarded Android: `ca-app-pub-6728704748176389/9222184514`

### 주의
- **테스트 빌드에 실제 광고 ID 사용 금지** — 정책 위반 → 계정 정지 (CLAUDE.md §8-2)
- 개발 중에는 Google 공식 테스트 ID 사용:
  - Rewarded: `ca-app-pub-3940256099942544/5224354917`
  - Interstitial: `ca-app-pub-3940256099942544/1033173712`
  - Banner: `ca-app-pub-3940256099942544/6300978111`

---

## 2. AppLovin MAX (미디에이션 — Boxy의 메인 광고 플랫폼)

### Step 1: 가입
1. https://www.applovin.com/grow/max-mediation/ 접속
2. "Sign up" → Publisher 계정 생성
3. 회사 정보: Mound / 사업자번호 / 한국
4. 검증 메일 확인 → 활성화

### Step 2: 앱 등록 (3개 환경, 또는 prod만)
Dashboard → MAX → Applications → New Application:
- App Name: `Boxy Production` (Android)
- Package Name: `com.mound.boxy`
- Platform: Google Play

(dev/stg 별도 등록 권장하지만, AppLovin은 정책상 테스트 ID 별도 제공 → prod만 만들고 테스트 빌드는 SDK debug 모드 사용해도 OK)

### Step 3: 광고 단위 생성 (Ad Units)
앱 클릭 → MAX Ad Units → Create:

| 이름 | 형식 |
|---|---|
| `Boxy_Rewarded` | Rewarded |
| `Boxy_Interstitial` | Interstitial |
| `Boxy_Banner` | Banner |

각각 **Ad Unit ID** 발급.

### Step 4: 미디에이션 네트워크 추가
MAX → Mediation → Networks:
- **AdMob (필수)**: Step 1의 AdMob 계정 연동 — Google Ad Manager API 연결 필요
- **Meta Audience Network**: Facebook 개발자 계정 → 앱 등록 필요 (선택)
- **Unity Ads**: Unity Cloud → 자동 연결
- **Vungle / IronSource / Mintegral**: 트래픽 봐서 추가

> v1.0은 AdMob + Unity Ads + Meta 3개로 충분. 트래픽 늘면 추가.

### Step 5: SDK Key 발급
MAX → Account → Keys → **SDK Key** 복사 (한 번만 발급)

### 🟢 AI에게 넘길 값
```
AppLovin SDK Key:                    __________________________________________
AppLovin Rewarded Unit ID:           __________________________________________
AppLovin Interstitial Unit ID:       __________________________________________
AppLovin Banner Unit ID:             __________________________________________
```

---

## 3. AppsFlyer (어트리뷰션 — 무료 티어)

### Step 1: 가입
1. https://www.appsflyer.com/ → "Try Free" 또는 "Get Started"
2. **Zero Plan (무료)** 선택 → 월 12,000 conversion 무료
3. 회사 정보: Mound, 한국, 게임 카테고리

### Step 2: 앱 등록
Dashboard → My Apps → Add App:
- Name: `Boxy`
- Platform: **Android**
- Store: Google Play
- Bundle ID: `com.mound.boxy`

### Step 3: Dev Key 발급
앱 클릭 → App Settings → SDK → **Dev Key** 복사 (모든 환경 공유 가능)

### Step 4: 이벤트 정의 (선택, Day 1 안 해도 됨)
가장 중요한 이벤트:
- `af_complete_tutorial` (튜토리얼 완료 — Boxy 5레벨)
- `af_purchase` (IAP 자동 추적)
- `af_ad_view` (광고 시청, MAX SDK 자동)

이건 코드 단계에서 통합하므로 Day 1엔 Dev Key만 챙기면 됨.

### 🟢 AI에게 넘길 값
```
AppsFlyer Dev Key:                   __________________________________________
```

---

## 4. (선택) Meta Audience Network

AppLovin MAX에 Meta를 미디에이션 네트워크로 추가하려면:

1. https://developers.facebook.com/ → 앱 만들기
2. Audience Network 제품 추가
3. Placement 생성 (Rewarded / Interstitial / Banner)
4. Placement ID를 AppLovin MAX → Mediation → Meta 설정에 입력

**Day 1 보류 OK** — Week 3 SDK 통합 시점에 추가해도 됨.

---

## 5. (선택) Unity Cloud — 빌드/IAP/Ads

Unity Hub 로그인 시 Unity ID 자동 생성 → Unity Cloud 자동 사용 가능. 별도 가입 X.

---

## 끝났을 때 AI에게 보내는 형식

이 파일에서 🟢 섹션의 값들 채워서 그대로 채팅에 붙여주시면, AI가:
1. `secrets/EnvironmentConfig.dev.asset.example` 같은 ScriptableObject 템플릿 생성
2. CLAUDE.md §8-2 환경별 분기 표 채워줌
3. 코드에서 SDK 초기화 시 사용할 wrapper 인터페이스 작성

---

## 시간 예산

| 서비스 | 가입 | 검증 | 광고 단위 |
|---|---|---|---|
| AdMob | 10분 | 1~2일 (PIN 우편) | 5분/환경 |
| AppLovin | 5분 | 즉시 또는 24시간 | 5분 |
| AppsFlyer | 5분 | 즉시 | (선택) |
| **총** | **20분** | 검증 대기 | 20분 |

가입 자체는 30~40분이면 끝. 검증/입금 임계는 Week 1 내 처리되면 충분.

---

## 우선순위 (오늘 기준)

🔴 **AdMob** 가장 먼저 — 검증이 가장 오래 걸림
🟠 AppLovin MAX — 검증 빠름
🟡 AppsFlyer — 무료 티어, 즉시
🟢 Meta — Week 3에 미뤄도 OK

세금 정보 양식(W-8BEN)이 가장 오래 걸리므로 AdMob부터 시작하시면 다른 거 가입하는 동안 검증 진행됨.
