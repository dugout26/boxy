# 2026-04-29 — AI 단독 가능 작업 사실상 100% 완료 (Day 1)

## Context

정수 요청: "너가 할 수 있는 개발 모든걸 다해봐 단계적으로 내가할일들은 다 무시하고". 외부 의존(SDK 키, 사용자 가입, 외주 일러스트)을 제외한 모든 코드/디자인/테스트/문서 작업.

## 이번 세션 (2026-04-29 후반) 누적 결과

### 디자인 ✅
- 7개 UXML 모두 반응형 (Header / Content / Footer 표준)
- Stitch 시안 매칭 (MainMenu, LevelSelect)
- PanelSettings ScaleWithScreenSize + Expand + 1080×1920
- SafeAreaController (notch + 홈 인디케이터)
- 마스코트 5종 표정 PNG (default/cheer/sad/sleepy/surprised)
- 9개 아이템 placeholder PNG (256×256)
- 9개 Lucide-style 아이콘 PNG (gear/volume/bag/arrow_left/chevron_right/star_filled/star_outline/heart/lock)
- AppStore 1024 아이콘
- 그리드 셀 동적 사이즈 (디바이스마다 자동 cellSize)
- 9 itemKey 색상 매핑 (시각 구분)
- UI 모션 — 별 stagger pop-in, 마스코트 bounce, 셀 transition

### 게임 콘텐츠 ✅
- 첫 5레벨 §B-3-1 정성 디자인 (solver 검증 PASS)
- 50/50 자동 생성 레벨 모두 풀어짐 (brute-force solver)

### 시스템 코드 ✅
- 사운드: 5 SFX + BGM (Cozy Loop) + EventBus 자동 구독
- 햅틱: Android Vibrate + iOS UIImpactFeedbackGenerator native plugin
- ATT (App Tracking Transparency) iOS native + 1초 딜레이 후 자동 요청
- KoEnStringTable (한/영 + 디바이스 SystemLanguage 자동 + Settings 토글)
- LevelSelect 인사말 + 탭 라벨 i18n 적용
- Settings 볼륨 슬라이더 → AudioService 실시간 연동
- Settings 햅틱 토글 → IHapticService 실시간 연동
- ResultPopup 마스코트 표정 동적 교체 (cheer/sad)
- ResultPopup 별 등장 애니메이션 + 마스코트 bounce
- ItemCardView 회전/드래그 사운드
- GameplayUI 정확/잘못된 배치 시 그린/빨간 flash + 햅틱
- AppsFlyerAnalyticsProvider stub + Composite (Firebase + AppsFlyer 동시 발송)
- IosAttConsentProvider (iOS) + GoogleUmpConsentProvider (Android)
- Bootstrap.ApplySavedAudioSettings (앱 재시작 시 마지막 설정 복원)

### 자동화 ✅
- BoxyAutomation (씬 셋업)
- AudioSetupHelper (오디오 wiring)
- BoxyBuilder dev/prod iOS+Android (build number 분리, productName 환경별)
- Firebase google-services.json + GoogleService-Info.plist 환경별 자동 복사
- iOS 프레임워크 PostProcess (AppTrackingTransparency / AdSupport / UIKit 자동 링크)
- Info.plist PostProcess (ITSAppUsesNonExemptEncryption / NSUserTrackingUsageDescription)
- AppStore-1024 아이콘 자동 보강
- 시뮬레이터 3-디바이스 자동 검증 스크립트
- App Store 스크린샷 자동화 스크립트

### 가드 ✅
- TestFlight 자동 업로드 차단 (`BOXY_ALLOW_UPLOAD=1` 명시 필요)
- AI는 절대 자동 업로드 X (정수 명시 요청 시에만)

### 테스트 ✅
- EditMode: 38 + 17 신규 = 55 추정 (KoEnStringTable 8, ItemColorMapping 2, SaveMigration 7)
- PlayMode: 4 신규 (GameplayFlowTests — Level 시작/클리어/힌트/되돌리기/잘못된 배치)
- 모두 컴파일 통과 (런타임 검증은 Unity 라이선스 환경에서)

### 법적 ✅
- Privacy Policy (한/영 둘 다)
- Terms of Service (한/영 둘 다)

### 문서 / 결정 기록 ✅
- decisions/ 19개 누적 (#9~#19)
- HANDOFF.md §11 추가 (다음 세션 픽업 가이드)
- App Store 셋업 가이드 (Firebase iOS / AppLovin / AppsFlyer / IAP / 클로즈드 테스트)

### TestFlight ✅
- Build 1, 3, 4, 5, 6, 7 업로드 완료 (build 8+ 정수 명시 요청 시)
- 정수 아이폰에서 build 7 검증 가능 상태

## 정수에게 의존하는 잔여 작업 (외부 자원 필요)

| 우선순위 | 작업 | 차단 사유 | 시점 |
|---|---|---|---|
| P0 | Boxy 마스코트 + 50 아이템 일러스트 | 외주 또는 Scenario MCP 시간 | Day 11~12 |
| P0 | 클로즈드 테스트 5명 모집 | 정수 카톡/지인 | Day 14 (5/12) D-Day |
| P1 | Firebase iOS 앱 추가 + plist 다운로드 | Firebase Console | 즉시 가능 |
| P1 | AppLovin MAX 가입 + SDK 키 | applovin.com | 즉시 가능 |
| P1 | AppsFlyer 가입 + Dev Key | appsflyer.com | 즉시 가능 |
| P1 | Unity SDK 패키지 임포트 (Firebase + AppLovin + IAP + AppsFlyer) | Unity Editor 직접 | Day 15 |
| P1 | App Store Connect 인앱 상품 3종 등록 | App Store Connect 웹 | 즉시 가능 |
| P2 | App Store 스크린샷 8장 | 시뮬레이터 + 수동 캡처 | Day 24~25 |
| P2 | KIPRIS / USPTO 상표 검색 | 정수 직접 | Day 1 미루지 말 것 |
| P2 | 광고 영상 녹화 + 페이스북 캠페인 | 정수 카메라 + 편집 | Day 5~7 |

## 다음 단계 — 정수 요청 시

1. **Build 8 업로드**: `BOXY_ALLOW_UPLOAD=1 ENV=dev ./scripts/ios-archive-upload.sh`
2. **SDK 키 입력**: 정수가 키 보내주면 `secrets/EnvironmentConfig-{dev,prod}.asset` 자동 생성
3. **Firebase iOS plist 받으면**: `secrets/GoogleService-Info-{dev,prod}.plist` 저장 → 다음 빌드부터 자동 통합
4. **Closed test 시작**: production 빌드 + 정수 + 5명 추가 → Apple Beta Review → 14일

## Revisit when

- 정수 SDK 키 받기 시작 → Provider TODO unblock 일괄 작업
- Day 14 (5/12) — 클로즈드 테스트 시작 D-Day
- Day 18 — Firebase + AppsFlyer 통합 검증 (강제 크래시 + 이벤트 도착 확인)
- Day 27 (5/26) — 클로즈드 테스트 14일 통과
- Day 28 (5/27~28) — Production 출시
