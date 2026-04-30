# 2026-04-30 — CI/CD 파이프라인 (GitHub Actions, 2-tier)

## Context

지금까지 빌드는 로컬 macOS에서 `BoxyBuilder.BumpAndBuild*` 수동 실행 → `scripts/ios-archive-upload.sh` → TestFlight Internal. Android는 사실상 검증 안 됨. CI 없으니:
- 매 커밋마다 컴파일/테스트 보장 X — 정수가 잊고 push하면 며칠 후 발견
- 외부 환경(Linux 빌드 머신, 깨끗한 캐시)에서 동작하는지 확인 X
- closed test 시작하면 빌드 업로드 횟수 늘어남 → 손으로 해선 못 버팀

§8-1 환경 분리 + decisions/2026-04-29-09 (2-tier app id) 따라 CI도 동일 구조로 분기.

## Options

- **A. 단일 트랙(prod만)**: develop 무시, main push 시 prod 빌드만. 솔로 4주엔 단순. 단점: dev 검증이 모두 로컬 → 실배포 직전 처음 Linux 빌드 → 환경 차이 폭발.
- **B. 2-tier (develop=dev / main=prod)** ← 채택. 두 앱이 이미 분리(`com.mound.boxy.dev` vs `com.mound.boxy`)되어 있으니 CI도 자연스럽게 따라감. 둘 다 Internal 트랙만 사용.
- C. 3-tier (feature/develop/main): External/Beta까지. 솔로엔 과함, External 트랙 운영 비용(테스터 모집/이메일 등) 못 감당.

## Decision

**B. 2-tier**. develop push → Boxy Dev → TestFlight Internal / Play Internal Testing. main push → Boxy Pack → 동일 Internal 트랙. closed test는 정수 본인 + 외부 5명을 Internal 테스터로 추가하는 것으로 흡수 (External 안 함). feature/* 푸시는 검증 X — 매 commit 부담 회피.

워크플로우 4개:

| 파일 | 트리거 | 역할 |
|---|---|---|
| `unity-compile.yml` | PR + push (develop, main) | 컴파일 + EditMode 테스트 (검증만) |
| `android-mobile.yml` | push (develop, main) | dev/prod 분기 → APK/AAB → Play Internal |
| `ios-mobile.yml` | push (develop, main) | dev/prod 분기 → IPA → TestFlight Internal |
| `release.yml` | tag `v*` | GitHub Release notes 자동 생성 |

빌드 번호: `BUILD_NUMBER_BASE(100) + GITHUB_RUN_NUMBER` → BoxyBuilder의 `BUILD_NUMBER_OVERRIDE` env로 주입. ProjectSettings.asset은 안 건드림 → git diff 안 남음.

## 환경 분리 메커니즘

CI 안에서 환경별 분기는 단일 if문 한 줄로:

```yaml
env:
  ENV: ${{ github.ref == 'refs/heads/main' && 'prod' || 'dev' }}
  PACKAGE_NAME: ${{ github.ref == 'refs/heads/main' && 'com.mound.boxy' || 'com.mound.boxy.dev' }}
```

이후 모든 단계에서 `${{ env.ENV }}` 기반:
- BuildMethod: `BumpAndBuildDevAndroid` vs `BumpAndBuildProductionAndroid`
- 산출물: `*.apk` (dev) vs `*.aab` (prod)
- Bundle ID, productName, Firebase config — BoxyBuilder.cs 안에서 envTag 분기 (이미 구현됨)

**비밀 자산 자동 스왑** (이번에 추가):
- `Decode secrets/ files` 단계가 GitHub Secret(base64) → `secrets/{config}-{env}.{ext}` 로컬 파일로 복원
- BoxyBuilder가 빌드 직전 `secrets/{config}-{env}.{ext}` → `Assets/.../{config}.{ext}` 복사 (기존 google-services.json 패턴 + 신규 EnvironmentConfig.asset 패턴)
- 결과: 같은 코드, 같은 ProjectSettings.asset에서 dev/prod 빌드가 자동으로 분리

## Consequences

좋은 것:
- develop 푸시 → 30분 안에 정수 디바이스에 Boxy Dev 도착 (수동 빌드 안 함)
- main 머지 → Boxy Pack TestFlight 자동 업로드 → 출시 직전 검증 (App Store Connect "Submit for Review" 1번 수동 클릭만 남음)
- Linux/macOS 두 환경에서 컴파일 보장 → "내 맥북에선 됐는데" 사고 0
- BUILD_NUMBER 충돌 0 (GITHUB_RUN_NUMBER 단조 증가)

나쁜 것 / 위험:
- Unity license 1좌석 → CI가 동시 빌드 시 충돌 가능. 동일 ref 동시 실행은 `concurrency` 그룹으로 차단. 단 compile + iOS + android 동시 실행은 1개 좌석 부족 → game-ci action이 해결 (returnLicense step 자동).
- macOS-15 러너 분당 사용량(iOS) ≒ Linux의 10배. main 머지 1회당 약 25–40분 소요. 월 50회 머지 가정 시 약 1500분 → 무료 한도(2000분) 안전.
- secrets 18개 (앱 키/Firebase config/keystore/ASC API) — 1회 셋업 부담 큼. 가이드: `decisions/2026-04-30-24-cicd-secrets-checklist.md`에 정수 액션 분리.

## Revisit when

- 매월 GitHub Actions 사용량 > 1500분
- TestFlight Internal로 외부 테스터 5명 한계 도달 (App Store Connect 100명/그룹 가능 — 솔로 v1.0엔 충분)
- 출시 후 v1.1에서 hot-fix 빈도 늘어 main 직접 push 부담 → develop → main PR 강제 필요해질 때
