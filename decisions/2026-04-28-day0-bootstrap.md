# 2026-04-28 — Day 0 부트스트랩

## Context
Boxy 프로젝트 시작 전 외부 정보 수집 및 인프라 부트스트랩.

## Done
1. `/Users/jeki/Boxy/` git 저장소 초기화
2. CLAUDE.md v2 작성 (§0~§19, 메타 규칙 + 가드레일 + 기술적 강제)
3. `.editorconfig`, `.gitignore` (시크릿 보호 포함)
4. `scripts/check-violations.sh` + `scripts/setup-hooks.sh` (pre-commit hook 활성화)
5. `SDK_VERSIONS.md` 갱신 — 실제 버전 박힘:
   - Unity 6.3 LTS `6000.3.8f1`
   - Google Mobile Ads Unity 11.0.0
   - AppLovin MAX Unity 8.6.2
   - Firebase Unity 13.10.0
6. `boxy-day1-checklist.md` 작성 — 정수 직접 실행 항목 정리
7. Stitch 프로젝트 생성 (ID: `10395380667870336902`)
   - Mound 디자인 시스템 v1 등록
   - 3개 화면 시안 생성:
     • Main Menu
     • Boxy Character Reference v1 (5표정 × 4포즈)
     • Gameplay (Level 7 패킹 화면)

## External Info Collected
- 시장 벤치마크: D7 평균 ~8% / ARPDAU 중간값 $0.02, 상위 15% $0.10~0.14
  → 기획서 ARPDAU 합격선 $0.05 → $0.10 상향 권장
- Google Play 신규 계정: 20명 × 14일 클로즈드 테스트 룰 적용
- Pretendard 단일 폰트 결정 (Inter 제외, -3~5MB)
- 퍼블리셔 제출 포털 6곳 URL 확보:
  Voodoo / Homa / CrazyLabs / SayGames / Supersonic / Lion

## Pending (정수 직접 실행 — boxy-day1-checklist.md 참조)
- KIPRIS / USPTO 상표 검색 (Boxy)
- Google Play Console 계정 생성 ($25)
- Unity 6.3 LTS 설치
- Top 50 패킹 퍼즐 플레이 → 메커닉 후킹 결정

## Revisit when
- Day 1 결과 확인 후
- 메커닉 후킹 결정 시
