# Boxy — 5/12 클로즈드 테스트 마감까지 13일 액션

**Day 5 (2026-04-29) 시점 — 5/12까지 13일 남음**

> AI 작업은 Day 5 완료. 이 문서는 정수 직접 액션만 정리.

---

## 🔴 Day 1~3 (4/29~5/1) — 즉시 실행

### A. Mac mini 화면 접근 가능해진 시점
- [ ] **Unity Hub 실행** → Sign in with Google (`dugout26.gm@gmail.com`)
- [ ] License 자동 활성 (Personal)
- [ ] Hub → Open → `/Users/jeki/Boxy/` 선택
- [ ] 첫 컴파일 결과 확인 → 콘솔 빨간 에러 0
- [ ] `Window → Test Runner` 실행 → 39 EditMode tests 통과 확인
- [ ] `Boxy → Generate 50 Levels` 메뉴 클릭 → `Assets/Levels/Level_01.asset` ~ `Level_50.asset` 자동 생성
- [ ] 4 씬 생성 + Build Settings 등록 (boxy-unity-setup-guide.md §B)
- [ ] UIDocument + Controller 인스펙터 연결 (각 씬, §D)
- [ ] Play 모드 → 흐름 검증 (MainMenu → LevelSelect → Gameplay → Result → 다음 레벨)

### B. 어디서든 가능 (Mac 접근 불필요)
- [x] **KIPRIS** — AI 완료 ✅. `queryText=Boxy` / `Boxy Pack` / `박시` 모두 **총 0건**. 한국 시장 상표 충돌 없음. 정수 5분 추가 확인 권장 (KIPRIS 로그인 → 유사상표 검색 음운/시각적)
- [ ] **USPTO TESS** https://tmsearch.uspto.gov → 정수 직접 10분 (AI: SPA+AWS WAF로 자동 차단). **Boxy Pack** 입력 → Live 필터 → Class 9/28/41 결과 0건 확인. AI 간접조사: "BOXY BOO" Mob Entertainment 소유(Class 28) 발견되나 다른 마크/충돌 X. "BOXY"/"BOXY PACK" 단독은 인덱스 0건
- [x] **App Store / Google Play "Boxy" 검색** — AI 완료. 결과: 단독 "Boxy"는 충돌 多 (kxland 미로퍼즐, Brainy Boxy 브레인퍼즐), **"Boxy Pack" 정확 일치 0건 안전**. 상세: `decisions/2026-04-29-02-trademark-search.md`
- [x] **도메인 boxy.app / boxy.io / boxygame.com 가용성** — AI 완료. boxy.app=$45k 매물, boxy.io=2018부터 등록(미서비스), boxygame.com=2023 placeholder. 모두 사용불가. **대안: ✅ boxypack.app + ✅ mound.studio + ✅ mound.games + ✅ boxypackgame.com**
- [ ] **충돌 발견 시 Day 1 22:00 안에 대안 결정** — AI 권장: **mound.studio (스튜디오) + boxypack.app (게임)** 즉시 등록

### C. 외부 서비스 가입
- [ ] **Google Play Console** $25 결제 (검증 1~3일 — 빨리 시작)
- [x] ~~**AdMob 계정** + W-8BEN + SWIFT~~ — **AI 발견** ✅ Mound publisher `pub-6728704748176389` (Prepit 출시 때 완료) 재사용. 정수는 로그인만 + Boxy 앱 신규 등록 + 6개 신규 unit ID 발급 (15분 작업). 자세히: `boxy-day1-accounts.md §1` 갱신본
- [ ] **AppLovin MAX** 가입 + SDK Key 발급
- [ ] **AppsFlyer Zero Plan** 가입 + Dev Key

> 끝나면 SDK 키들을 secrets/EnvironmentConfig.asset에 입력 (Unity 활성 후)

---

## 🟠 Day 4~7 (5/2~5) — 시장 조사 + 자료 제작

### D. 메커닉 후킹 검증
- [ ] **Top 50 패킹 퍼즐** 직접 플레이 (한·미 각 25개)
  - 우선: Pack Master 3D, Goods Sort Master, Home Packing, 정리정돈
- [ ] `decisions/2026-04-29-03-top50-research.md` 빈칸 채움
- [ ] 잠정 결정 (좌절 후킹 B) 검증 또는 변경
- [ ] **Day 7 22:00 메커닉 후킹 최종 결정**

### E. 광고 영상 2종 제작
- [ ] Unity Editor에서 Gameplay 씬 직접 플레이 → `marketing/ad-scripts.md` 시퀀스 따라 화면 녹화
- [ ] iMovie / DaVinci Resolve 편집 — 텍스트 오버레이 + BGM (Pixabay) + SFX (Mixkit)
- [ ] 영상 1번 (실패형, 좌절 후킹) — 15초
- [ ] 영상 2번 (성공형, 만족 후킹) — 15초
- [ ] YouTube 비공개 업로드 또는 Google Drive 공유 링크 확보

### F. 퍼블리셔 제출
- [ ] 피치덱 PDF 변환: `marketing/publishers/pitch-deck.html` 브라우저 → Cmd+P → PDF
- [ ] APK 빌드 (Internal Testing) → Google Drive 공유 링크
- [ ] 8 퍼블리셔 폼 제출 + 콜드 이메일 발송 (`marketing/publishers/cold-emails.md`)
  - Tier 1: Homa / CrazyLabs / SayGames / Supersonic / Kwalee
  - Tier 2: Voodoo / Rollic / Lion (기대치 낮음)
- [ ] 응답 기록 표 채우기 (`cold-emails.md` 하단)

### G. Boxy 캐릭터 마스코트
- [ ] 정수 손그림 1장 (5개 수치 명시)
  - 비율 1:1.1, 눈 위치 상단 1/3, 입 너비 1/4, 컬러 #FFD60A 단일, 외곽선 검정 2px
- [ ] 또는 외주 ₩50,000~100,000
- [ ] Scenario MCP에 reference로 학습 → 12~20장 변형 생성

### H. 앱 아이콘
- [ ] `design/icon/app-icon-mockup.html` 브라우저로 열기
- [ ] SVG 영역 디자인 도구 import (Figma / Pixelmator) → 1024×1024 PNG export
- [ ] 또는 Scenario MCP로 더 정교한 버전 생성
- [ ] iOS: Xcode 자동 사이즈 생성
- [ ] Android: `mipmap-anydpi-v26/ic_launcher.xml` adaptive icon

---

## 🟡 Day 8~12 (5/6~10) — 캠페인 + 통합 + 빌드

### I. 광고 캠페인 발사 (Day 8 최우선)
- [ ] Facebook Ads Manager — 영상 2종 각 ₩50,000 = 총 ₩100,000
- [ ] 타겟: 한국 + 영어권 4국 (US/UK/CA/AU)
- [ ] 인구통계: 18+ 모바일 게임
- [ ] **Day 11 캠페인 결과 확인**:
  - 1개 이상 영상 CTR ≥ 1.5% → 본격 진행
  - 모두 1.0~1.5% → 영상 수정 + 재시도
  - 모두 < 1.0% → 컨셉 재검토 (영상 시나리오 재작성)

### J. SDK 통합 (Week 3 Day 15-19)
- [ ] AppLovin MAX SDK 임포트 → `AppLovinAdProvider.cs` TODO 채움
- [ ] Firebase SDK 임포트 → `FirebaseAnalyticsProvider.cs` TODO 채움
- [ ] Crashlytics SDK 임포트
- [ ] Unity Gaming Services IAP → `UnityIapProvider.cs` TODO 채움
- [ ] AppsFlyer SDK 통합
- [ ] EnvironmentConfig.asset 3개 생성 (dev/stg/prod) — secrets/ 폴더에 저장
- [ ] BoxyBootstrap의 `useStubProviders` false로 변경 (production 빌드)

### K. 50레벨 검증
- [ ] `Boxy → Generate 50 Levels` 결과 직접 플레이 (Day 13 권장)
- [ ] "풀리지만 스트레스 과도" 케이스 발견 시 수동 교체
- [ ] 첫 5레벨은 정성 디자인 (boxy-plan §B-3-1 — TutorialLevels fallback 대신)

### L. 클로즈드 테스트 빌드
- [ ] **Day 14 (5/12) 빌드 업로드 — 절대 마감일**
  - Google Play Console → Internal Testing or Closed Testing
  - 신규 계정 = 20명 × 14일 룰 (5/26 통과)
- [ ] 테스터 풀 모집 (카톡방 / 지인) — Day 6~7 시작 권장
- [ ] **20명 + 이탈 대비 5명**

---

## 🟢 Day 14~26 (5/12~25) — 클로즈드 테스트 + 폴리싱

### M. 클로즈드 테스트 (5/12~5/26)
- [ ] 테스터 피드백 매일 모니터링
- [ ] 크래시 / IAP / 광고 / 진행도 손실 발견 시 즉시 핫픽스
- [ ] Crashlytics 강제 크래시 1회 → 대시보드 도착 확인 (`CrashTestComponent`)
- [ ] AppsFlyer 설치 이벤트 도착 확인
- [ ] IAP 샌드박스 3종 결제 성공 확인
- [ ] iOS 실기 + Android 실기 양쪽 빌드 + 5레벨 완주 (CLAUDE.md §11-3)
- [ ] 세이브 마이그레이션 테스트 (이전 빌드 → 신 빌드)

### N. 스토어 등록 (Day 24~25)
- [ ] 스크린샷 8장 제작 (`marketing/store-description-ko.md` § 캡션 가이드)
- [ ] 프로모션 영상 (광고 영상 1번 30초 컷 또는 15초)
- [x] **Privacy Policy + Terms 정적 사이트 작성** — AI 완료 (`legal/index.html`, KO/EN 토글 + Privacy/Terms 토글, marked.js 런타임 렌더, 다크모드 지원). 정수가 호스팅만 활성화하면 됨:
  - **GitHub Pages 활성**: 저장소 → Settings → Pages → Source: `main` 브랜치 `/` (root) 또는 `/legal` 폴더 선택 → Save
  - 결과 URL 예: `https://<github-user>.github.io/<repo>/legal/` (root 선택 시) 또는 도메인 매핑 시 `https://mound.studio/legal/`
  - 출시 전 `legal/*.md` 4개 파일 안의 사업자 정보 4곳 (Mound, 백정수, 709-09-03510, 이메일) 정확 확인
- [ ] `SettingsController.cs`의 PrivacyUrl / TermsUrl placeholder를 실제 호스팅 URL로 교체
- [ ] Google Play 등록 폼 채움 (한국어 설명 + 영어 설명) — 한/영 카피 이미 완료 (`marketing/store-description-{ko,en}.md`)

---

## 🔵 Day 27~28 (5/27~28) — 출시

- [ ] **5/27 클로즈드 테스트 14일 통과** 확인 (5/12 시작 → 5/26 종료)
- [ ] **5/28 프로덕션 출시** 발표 (메모리얼데이 5/25 + 부처님오신날 5/24 글로벌 윈도)
- [ ] 출시 직후 24시간 모니터링 — 크래시 / 평점 / 첫 다운로드

---

## 🟣 출시 후 (5/28~6/4) — 1주 KPI 측정

### KPI 합격선 (boxy-plan §D-2)
- [ ] D1 retention ≥ 30%
- [ ] D7 retention ≥ 10%
- [ ] 평균 세션 ≥ 3분
- [ ] 보상형 시청률 ≥ 50%
- [ ] 인터스티셜 이탈률 ≤ 15%
- [ ] **ARPDAU ≥ $0.10** (실패선 $0.05, 합격선 $0.15)
- [ ] 평균 평점 ≥ 4.0

### D-4 즉시 롤백 트리거 (5/28+7=6/4 시점)
다음 5개 중 2개 이상 미달 시:
- [ ] D1 < 25%
- [ ] D7 < 8%
- [ ] 평균 세션 < 2분
- [ ] 보상형 시청률 < 40%
- [ ] ARPDAU < $0.03

→ 즉시 발효: 기능 추가 금지, 버그 수정만, **Boxy Sort 즉시 착수**

---

## 📞 막히는 지점 → AI 호출

| 막힘 | AI에게 줄 정보 |
|---|---|
| Unity 컴파일 에러 | 콘솔 메시지 그대로 |
| Test Runner 실패 케이스 | 실패한 테스트 이름 + 콘솔 로그 |
| 인스펙터 연결 모호 | 어느 씬, 어느 GameObject, 어느 컴포넌트, 어떤 필드 |
| 광고 SDK 에러 | SDK 종류 + 에러 메시지 + AppLovin/Firebase 대시보드 상태 |
| 퍼블리셔 응답 (긍정/부정) | 응답 본문 → AI가 다음 액션 추천 |
| Privacy/Terms 변호사 검토 코멘트 | 코멘트 그대로 → AI가 한국 법률 부합 검토 |

---

## 한 줄 요약

**Day 5 시점**: 코드 + 마케팅 + 법적 + SDK 래퍼 + 테스트 모두 준비됨. 정수가 Mac 화면 접근 + 외부 서비스 가입 + 광고 영상 녹화 + Privacy 호스팅만 끝나면 5/12 클로즈드 테스트 빌드 업로드 가능.
