# 2026-04-29 — App ID / 환경 전략: 3-tier → 2-tier

## Context

CLAUDE.md §8은 `dev` / `staging` / `production` 3개 환경 + 환경별 Bundle ID 분리를 강제한다.
- dev: `com.mound.boxy.dev`
- staging: `com.mound.boxy.stg`
- production: `com.mound.boxy`

iOS TestFlight 셋업 단계에서 정수가 질문: "앱 1개로 가능한가? 굳이 3개 분리?"

검토한 결과 — **솔로 개발 + 4주 일정 + 1인 운영** 상황에서 3-tier는 과도한 셋업 부담. 한편 dev와 production 두 빌드를 같은 디바이스에 동시 설치할 수 있으면 디버깅 효율 큼 (정수가 dev 빌드 켜면서 prod 동작 비교 가능).

## Options

- **A. 3-tier 유지** (`dev` / `stg` / `prod` 모두 별도 Bundle ID + App Store Connect 앱 3개 + AdMob/Firebase 3개)
- **B. 2-tier** (`dev` + `prod` 만, `stg` 폐지, closed test는 prod 앱의 TestFlight External 트랙으로 대체)
- **C. 1-tier** (`prod` 1개만, 환경 분리는 BuildOptions/Scripting Define만으로)

## Decision

**B 채택 — `dev` + `prod` 2-tier**.
- dev: `com.mound.boxy.dev` (정수 디바이스, 매일 빌드 검증, Development build)
- prod: `com.mound.boxy` (TestFlight Internal/External + App Store, Release build)
- `staging`이 담당하던 클로즈드 테스트 기능 = prod 앱의 **TestFlight External 트랙** (Apple 간단 심사, 외부 테스터 5~100명)

## Consequences

### 좋은 것
- App Store Connect 앱 레코드 3개 → 2개 (관리 부담 -33%)
- AdMob/Firebase/AppsFlyer 등록 3개 → 2개
- dev/prod 빌드를 같은 디바이스 동시 설치 가능 (Bundle ID 다르므로 — A안과 동일하게 보장)
- TestFlight External 트랙은 staging의 closed test 기능을 그대로 제공 + Apple 심사 통과 검증 보너스
- 셋업 시간 -20~30분, §16 스코프 보호 정신 부합

### 나쁜 것 / 위험
- closed test 단계 = prod와 같은 SDK 키 사용 → AdMob 매출 측정에 closed test 트래픽 섞일 수 있음 (소규모이므로 영향 제한적)
- staging 전용 데이터 분리 불가 → KPI 분석 시 closed test 기간(5/12~5/26) 식별 필터 필요
- 만약 closed test 단계에서 결제 사고/광고 정책 위반이 발생하면 prod AdMob/IAP 계정에 직접 영향

### 위험 완화책
1. AdMob: `RequestConfiguration.testDeviceIds` 등록된 디바이스에서만 테스트 광고 노출 (정수 디바이스 + 외부 테스터 디바이스 추가 등록)
2. IAP: TestFlight 빌드는 자동 Sandbox 결제 (실 결제 사고 0)
3. Firebase Analytics: `Analytics.setAnalyticsCollectionEnabled(false)` if `BOXY_DEV` define
4. Crashlytics: 빌드 버전 ID로 closed test 빌드 자동 분리 (prod 빌드와 다른 versionCode)

### CLAUDE.md §8 영향
- 3-tier 표 → 2-tier 표로 갱신
- "출시 게이트" 흐름: `Editor → staging → production` → `Editor (dev 빌드) → production TestFlight Internal → External(closed test) → App Store`

### 코드 영향
- `BoxyBuilder.cs`: `BuildStagingAndroid` / `BuildStagingIOS` 메서드 제거 (§3-1 데드코드 0)
- `EnvironmentConfig.asset.example`: `EnvironmentConfig-stg.asset` 템플릿 제거
- `.gitignore`: stg 패턴 정리
- `decisions/2026-04-28-day0-bootstrap.md`나 후속 결정에서 3-tier 가정이 있으면 footnote 추가

## Revisit when

다음 중 하나 발생 시 3-tier 복원 검토:
- Mound 팀 2인 이상 확장 (dev/staging 빌드를 동시에 다른 사람이 사용)
- staging 단계 KPI 분리 필요 (예: 외부 테스터 100명 이상으로 확장 시 prod 데이터와 분리 측정)
- AdMob 매출 KPI를 sub-percent까지 정확히 측정해야 하는 단계 (D7 retention >15% 도달 후)
- v1.2~v2.0 메이저 업데이트에서 hotfix와 next major를 동시 운영해야 할 때
