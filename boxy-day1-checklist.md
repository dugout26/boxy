# Boxy — Day 1 실행 체크리스트

**Date**: 2026-04-29 (실제 시작일에 갱신)

> 정수 본인이 직접 실행해야 하는 항목 (계정 생성/결제/디바이스 설치).
> AI가 대신 못 함 — 시간 박아두고 한 번에 끝낼 것.

---

## A. 법적 / IP 검증 (1~2시간)

### A-1. Boxy 상표 검색 — 한국
- [ ] KIPRIS 접속: https://www.kipris.or.kr/
- [ ] 상표 탭 → "Boxy" 검색
- [ ] 분류 필터:
  - **9류** (소프트웨어 / 다운로드 가능 게임)
  - **41류** (게임 서비스 / 엔터테인먼트)
- [ ] 결과: 등록/출원/거절 상태 확인
- [ ] 충돌 발견 시: 즉시 네이밍 재검토 (Boxy → Boxy Pack / Boxy Fit 등)
- [ ] 결과를 `decisions/2026-04-29-boxy-trademark.md`에 기록

### A-2. Boxy 상표 검색 — 미국 (글로벌 출시 대비)
- [ ] USPTO TESS 접속: https://tmsearch.uspto.gov/
- [ ] "Boxy" 검색 → 분류 9, 28, 41 필터
- [ ] 알려진 충돌 사례:
  - "Boxy Scents" (소프트 - 게임 카테고리 아님 OK)
  - 게임 카테고리 충돌은 직접 검색해서 확인
- [ ] 결과 동일 decision 파일에 기록

### A-3. 게임물 자체등급분류 (출시 전이지만 미리)
- [ ] IARC 등록 정보 사전 확인 (Google Play Console에서 함께 처리됨)
- [ ] 전체이용가 / 비폭력 / 광고 포함 / 인앱 구매 — 4가지 분류 미리 파악

---

## B. 계정 생성 (2~3시간, 일부 결제)

### B-1. Google Play Console — 신규 개발자 계정
- [ ] 접속: https://play.google.com/console/signup
- [ ] **유의: $25 일회성 결제** (사업자 정보 Mound로 입력)
- [ ] 결제 후 계정 검증 1~3일 소요 — Day 1 안에 시작 필수
- [ ] **20명 × 14일 클로즈드 테스트 룰 적용 신규 계정** — 카톡방 테스터 풀 미리 모으기

### B-2. Apple Developer (v1.1 iOS 출시 대비)
- [ ] 접속: https://developer.apple.com/programs/enroll/
- [ ] $99/년, v1.0(Android만)에선 보류 가능
- [ ] v1.1 시작 시점에 가입

### B-3. AdMob 계정
- [ ] 접속: https://apps.admob.com/
- [ ] Google 계정으로 로그인 → 사업자 정보(Mound) 입력
- [ ] 본인 인증 후 계정 활성
- [ ] **테스트 광고 ID와 실제 광고 ID 분리 보관** — `EnvironmentConfig.asset`에 환경별로

### B-4. AppLovin MAX 계정 (미디에이션)
- [ ] 접속: https://www.applovin.com/grow/max-mediation/
- [ ] 가입 → SDK 키 발급
- [ ] AdMob을 미디에이션 네트워크로 추가 (이중 미디에이션 X)

### B-5. Firebase 프로젝트 (2-tier, decisions/2026-04-29-09-app-id-strategy-2tier.md)
- [ ] 접속: https://console.firebase.google.com/
- [ ] **2개 프로젝트 생성**: `boxy-dev`, `boxy-prod`
- [ ] 각각 Crashlytics + Analytics 활성화
- [ ] `google-services.json` 2개 다운로드 → 환경별 분기 (CLAUDE.md §8-2)
- [ ] iOS도 동시 등록: `GoogleService-Info-{dev,prod}.plist` 다운로드

### B-6. AppsFlyer 무료 티어
- [ ] 접속: https://www.appsflyer.com/
- [ ] 무료 티어 가입 (월 12,000 conversion 무료)
- [ ] Dev Key 발급 → `EnvironmentConfig.asset` 보관

---

## C. 도구 설치 (2~3시간, 일부 결제)

### C-1. Unity 6.3 LTS
- [ ] Unity Hub 다운로드: https://unity.com/download
- [ ] Hub → Installs → Add → **6000.3.8f1** 또는 그 이후 LTS
- [ ] 모듈 추가:
  - Android Build Support (+ SDK & NDK)
  - iOS Build Support
  - Mac Build Support (Editor용)
- [ ] 라이선스: Personal (수익 $200K 미만 무료)

### C-2. Asset Store 구매
- [ ] **Mobile Monetization Pro V2** (~$50)
  - Asset Store에서 최신 버전 확인
  - Ad+IAP+Firebase+GDPR 통합
- [ ] **패킹 퍼즐 템플릿 1개** (~$30~50)
  - "Pack" / "Fit" / "Puzzle Bag" 키워드로 3개 후보 비교 후 선택
- [ ] **Lofelt Nice Vibrations** (~$20)
  - iOS/Android 햅틱 통합 (CLAUDE.md §14)

### C-3. MCP 셋업 검증
- [ ] Unity MCP (CoplayDev) 설치 + Cursor 연동
- [ ] Stitch MCP 연동 확인 (이미 작동 — 프로젝트 ID `10395380667870336902` 생성됨)
- [ ] Scenario MCP 연동 (인게임 아이템 스프라이트용)

### C-4. Pretendard 폰트
- [ ] https://github.com/orioncactus/pretendard/releases 최신 다운로드
- [ ] OTF/Variable Font 모두 받아 Unity Assets/Fonts에 추가
- [ ] TextMeshPro Font Asset 생성

---

## D. 시장 조사 / 차별화 (3~4시간 — 가장 중요)

### D-1. Top 50 패킹 퍼즐 직접 플레이
- [ ] 한국 Play Store / 미국 Play Store / App Store 각 Top 25
- [ ] 키워드: "pack", "fit", "sort", "organize", "puzzle bag"
- [ ] 알려진 경쟁작 (직접 다운로드 권장):
  - **Pack Master 3D** (CrazyLabs)
  - **Goods Sort Master** / Goods Sort
  - **Goods Puzzle: Sort Challenge**
  - **Home Packing**
  - Wood Block 류 (참고만)
- [ ] 각 게임 5분 플레이하며 노트:
  - 메커닉 후킹 (트릭형 vs 좌절형)
  - 광고 빈도 (인터스티셜 간격)
  - 광고 영상 후킹 (스토어 영상 보기)
  - 한국어 카피 / ASO
  - UI 톤 (코지 / 다크 / 화려 등)
- [ ] **결과**: 메커닉 후킹 1줄 결정 → `decisions/2026-04-29-mechanic-hook.md`

### D-2. Boxy 메커닉 후킹 결정
- [ ] 1번 공간 트릭 vs 2번 좌절 후킹 중 1개 선택
- [ ] 시장 갭이 더 큰 쪽 + 정수가 잘 만들 수 있는 쪽
- [ ] 결정 후 광고 영상 4종 시나리오 컨셉도 결정

### D-3. ASO 한국어 후보
- [ ] "Boxy" 외 한국어 부제 후보 3개:
  - "Boxy - 정리 퍼즐"
  - "Boxy: 박스 정리 게임"
  - "Boxy: 가방 패킹"
- [ ] Google Play 한국어 검색 결과 비교 → 가장 적은 경쟁

---

## E. 캐릭터 / 디자인 자산 (2~3시간)

### E-1. Stitch에서 생성된 Boxy 자산 검토
- [ ] Stitch 프로젝트: ID `10395380667870336902` ("Boxy - Mound Mobile Game v1")
- [ ] 생성된 화면 (Stitch UI에서 확인):
  - Main Menu (메인 메뉴)
  - Boxy Character Reference v1 (캐릭터 시트 — 5표정 × 4포즈)
  - Gameplay (게임플레이 HUD)
- [ ] 마음에 들면 HTML/이미지 export → 프로토타입 참고용

### E-2. 캐릭터 시트 검증
- [ ] Stitch 출력의 Boxy 일관성 확인 (5표정 × 4포즈)
- [ ] 일관성 부족 시: 정수가 손그림 1장 직접 작성하여 reference 보강
  - 비율 1:1.1 / 눈 위치 상단 1/3 / 입 너비 얼굴의 1/4 / 컬러 #FFD60A 단일 / 외곽선 검정 2px
- [ ] 채택 후: `Assets/Art/Characters/Boxy/` 폴더에 PNG export

### E-3. Scenario MCP 셋업 (인게임 아이템 양산용 — 다음 주)
- [ ] Scenario.gg 가입 → API 키 발급
- [ ] Claude Code에 MCP 연동
- [ ] 학습용 reference image 12~20장 준비

---

## F. 퍼블리셔 제출 (1~2시간 — 영상/빌드 준비된 후 Week 1 후반)

### F-1. 제출 포털 (계정 미리 만들어두기)
- [ ] **Voodoo Publishing**: https://voodoo.io/publishing
- [ ] **Homa Games (Homa Lab)**: https://www.homagames.com/homa-lab/submissions-and-creatives
- [ ] **CrazyLabs Publishing**: https://www.crazylabs.com/publishing/
- [ ] **SayGames**: https://saygames.io/publishing/
- [ ] **Supersonic (Unity)**: https://supersonic.com/publishing
- [ ] **Lion Studios**: https://lionstudios.cc/

### F-2. 제출 자산 (Week 1 끝에 준비 — Day 1엔 계정만)
- [ ] 15초 광고 영상 4종 중 CTR 높은 1~2개
- [ ] APK 또는 빌드 링크
- [ ] 1페이지 피치덱 (Claude Code로 PDF 생성)
- [ ] CTR / CPI 초기 데이터

---

## G. Day 1 마무리 — 첫 결정 기록

- [ ] `decisions/2026-04-29-boxy-trademark.md` 작성 (A 결과)
- [ ] `decisions/2026-04-29-mechanic-hook.md` 작성 (D 결과)
- [ ] `decisions/2026-04-29-day1-summary.md` 작성 (오늘 한 일 + 막힌 곳)

---

## H. Day 1에 시간 안 되면 미루기 OK

- B-2 Apple Developer (v1.1까지 보류 가능)
- C-2 Lofelt Nice Vibrations (햅틱 작업 시점에 구매)
- E-3 Scenario MCP (Week 2 시작 전까지)
- F 퍼블리셔 계정 (Week 1 후반)

**오늘 안에 무조건 끝낼 4개 (D-day 압축 시)**:
1. A-1 KIPRIS 검색
2. B-1 Google Play Console $25
3. D-1 Top 50 플레이 + D-2 메커닉 후킹 결정
4. C-1 Unity 6.3 LTS 설치

이 4개가 Week 1 출발선. 나머지는 Day 2~3에 분산.
