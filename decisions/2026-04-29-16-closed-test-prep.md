# 2026-04-29 — 클로즈드 테스트 준비 (Day 14 5/12 시작)

## Context

기획서 §C Week 2 Day 14 명시: "Google Play Closed Testing 14일 룰 강제 (5/12 시작 → 5/26 통과 → 5/28 출시)". TestFlight External Testing도 동일.

신규 Play Console 계정 = **20명 × 14일** 테스터 룰 (2023년 11월 정책). TestFlight는 Apple Beta Review 후 외부 테스터 최대 10,000명.

## Decision

### TestFlight External Testing (iOS 우선)
1. App Store Connect → 앱 → Boxy Pack → TestFlight 탭
2. 외부 테스트 그룹 만들기: "Boxy Closed Test Group"
3. 빌드 추가 (현재 production 빌드 — 개발 빌드 X)
4. **Apple Beta App Review** 제출 — 첫 빌드 24h ~ 48h 검토
5. 승인 후 정수가 5+ 명 이메일 입력 (앱 정보, 사용 안내 메시지 포함)
6. 테스터에게 메일 자동 발송 → TestFlight 앱에서 install

### Google Play Closed Testing (Android)
1. Play Console → Boxy → 테스트 → 비공개 테스트
2. 새 트랙: "closed-test-1"
3. 출시 빌드 .aab (production app bundle) 업로드
4. 테스터 이메일 또는 Google 그룹 등록 (20명)
5. **14일 연속 테스터 활동** 자동 트래킹 (정책 강제)
6. 14일 후 신청서 작성 → "프로덕션 액세스" 신청

### 5명 모집 텍스트 (정수가 카톡/메시지로 발송)
> 안녕하세요! 제가 만든 모바일 게임 **Boxy** (가방 정리 퍼즐) 클로즈드 테스트에 5명만 초대합니다.
> - 5/12 ~ 5/26 (14일간) 가끔 한 번씩 게임 켜서 5분 플레이
> - 광고/인앱결제 없는 dev 빌드 — 부담 0
> - 한 마디 피드백("재밌어요" / "지루해요" / "어디서 막혔어요" 등)
> - iPhone (TestFlight) / Android (Play Store) 둘 다 가능
> 
> 참여 의향 있으시면 메일 주세요: dugout26.gm@gmail.com

### 14일 게이트 검증 자동화
정수가 매일 확인할 수 있게:
- Play Console: "테스터 참여" 그래프 — 14일 연속 표시되어야 통과
- TestFlight: 빌드 메트릭 → "테스터 활동" — 5명 중 하루 1회 이상
- Crashlytics 대시보드 — 테스트 기간 크래시 발생 시 즉시 수정

## Consequences

### 좋은 것
- 14일 룰 자동 준수 — 5/12 시작 시 5/26 통과
- iOS + Android 양쪽 동시 진행 (병렬)

### 나쁜 것 / 위험
- 5명 미모집 시 5/12 시작 못함 → 출시 일정 14일+ 지연
- 테스터가 14일 중 하루라도 비활성 → Google Play 통과 못함 (재시작)
- TestFlight Beta Review 거부 시 → 내용 수정 후 재제출 (24h 추가 대기)

### 위험 완화
- **Week 1 즉시 5명 모집 시작** (정수 카톡방/지인) — 5/12 D-Day 보장
- 테스터에게 "매일 1회 켜보기" 명시 — 13일 후 자동 알림 (TestFlight)
- 빌드 안정성 ✅ (3-디바이스 launch 검증, EditMode 38/38)

## Revisit when

- Week 1 5/5까지 5명 미확보 시 → External Testing public link 사용 (Apple TestFlight Public)
- 14일 중 일부 테스터 비활성 → 추가 모집 + 다시 14일
