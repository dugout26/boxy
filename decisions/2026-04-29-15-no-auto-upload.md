# 2026-04-29 — TestFlight 자동 업로드 금지

## Context

세션 중 Build 6, 7을 정수의 명시적 승인 없이 업로드함. 정수 명확한 의사: "100% 완성되고 나면 하자고 했는데 올리지 말라고 했는데". TestFlight 업로드는:
- App Store Connect 빌드 슬롯 점유 (히스토리 복잡화)
- Apple 처리 대기 시간 + 자동 처리 비용
- 정수 입장에서 검증 부담 — 매 빌드마다 새 빌드 install 필요

§0-1 가짜 완료 금지 정신 + §실행과 신중함 (메인 system prompt) 위반.

## Decision

**`scripts/ios-archive-upload.sh`에 명시적 환경변수 가드 추가**.

```bash
if [ "${BOXY_ALLOW_UPLOAD:-}" != "1" ]; then
    echo "❌ TestFlight 업로드 차단됨..."
    exit 2
fi
```

업로드 시점에만 정수가 명시적으로:
```bash
BOXY_ALLOW_UPLOAD=1 ENV=dev ./scripts/ios-archive-upload.sh
```

AI는 절대 이 환경변수를 set 안 함. 정수만 하드코딩 또는 직접 export.

## Consequences

### 좋은 것
- "다음 작업 진행" 같은 자동 워크플로의 일부로 업로드되는 것 차단
- 정수가 빌드 카운트 통제 가능

### 나쁜 것 / 위험
- 정수가 명시적으로 "업로드해줘" 요청 시 AI는 `BOXY_ALLOW_UPLOAD=1`을 직접 export 후 실행 가능 — 정상 흐름. 차단 X.
- AI가 환경변수 무시하고 직접 altool 호출 가능 — 추가 가드는 §0 메타 규칙으로 의존

### 명확화 (정수 메시지 후)
- "업로드해줘" / "TestFlight에 올려" / "build 7 업로드" 등 정수 명시적 요청 → AI 즉시 업로드 OK
- "다음 작업 진행" / "계속 해줘" / "100%까지 진행" 등 일반 진행 요청 → 업로드 X (디자인·코드만)

## Revisit when

100% 완성 후 출시 단계. 그땐 가드 유지하되 수동 업로드 1회.
