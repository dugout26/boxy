# 2026-04-29 — Unity 프로젝트 부트스트랩 (Phase 1)

## Context
Unity 6.3 LTS `6000.3.14f1` 설치 후 프로젝트 구조 셋업. CLAUDE.md §5 모듈 경계 강제 (asmdef).

## Status
**부분 완료** — Unity Editor 라이선스 미활성으로 batchmode `-createProject` 실패. 수동 구조 생성으로 우회.

### 진행 상황
- ✅ `Assets/Mound/Core/` 폴더 + `Mound.Core.asmdef`
- ✅ `Assets/Boxy.App/` 폴더 + `Boxy.App.asmdef` (Mound.Core 참조)
- ❌ `ProjectSettings/`, `Packages/`, `Library/` — 정수 Unity Hub 로그인 후 자동 생성됨

### asmdef 의존 방향 강제
```
Boxy.App ──▶ Mound.Core
역방향 의존 컴파일 단계에서 차단 (CLAUDE.md §5-3)
```

## Options 검토
- **A) batchmode -createProject**: 라이선스 필요, 실패 (Code 404 entitlements)
- **B) 완전 빈 프로젝트 생성 후 Hub 로그인**: 구조 안 잡힘
- **C) 수동 폴더 + asmdef 작성, Hub 로그인 후 자동 인식** ← 채택

## Decision
**C 선택**. 이유:
1. 정수 작업과 병렬 가능 (Hub 로그인은 나중에 5분만 들이면 됨)
2. asmdef는 Unity 자동 인식 — Hub 로그인 후 첫 컴파일에 모듈 경계 강제 즉시 적용
3. 다른 Phase (Mound.Core 코드 / UXML 등) 작업 차단 없음

## 정수 직접 액션 (다음 단계)
1. **Unity Hub 실행** (`/Applications/Unity Hub.app` 또는 spotlight)
2. **Sign in** → Unity ID 로그인 (없으면 가입, 무료 Personal License 자동 부여)
3. Unity Hub → Projects → "Open" → `/Users/jeki/Boxy/` 선택
4. Unity Editor 자동 시작, 누락된 `ProjectSettings/`, `Packages/`, `Library/` 자동 생성
5. 첫 컴파일에서 asmdef 인식 + Mound.Core / Boxy.App 모듈 분리 확인

## Phase 1 검증 기준 (CLAUDE.md §12-1)
- [ ] Unity Hub에서 프로젝트 인식 OK
- [ ] Editor 첫 실행 시 콘솔 에러 0
- [ ] Project 창에서 Mound.Core.asmdef + Boxy.App.asmdef 인식 (옅은 회색 ≠ 빨간색)
- [ ] Boxy.App에서 Mound.Core 클래스 사용 가능 / 역방향은 컴파일 에러

## Consequences
**좋은 것**:
- 의존 방향 컴파일 강제 → AI가 역방향 의존 코드 짜도 자동 차단
- Mound.Core 자체적 (외부 의존 0) → 다음 게임 그대로 복사 가능

**위험**:
- 정수 Hub 로그인 안 하면 Phase 2 코드 작성 후에도 컴파일 검증 불가
- 라이선스 활성 후 일부 .meta 파일이 Unity에 의해 새로 생성될 수 있음 (자연스러움)

## Revisit when
- 정수 Hub 로그인 완료 후 → Phase 1 검증 4개 항목 체크
- Mound.Monetization / Analytics / UI / Localization 모듈 신설 시 (Phase 2~6) — asmdef 추가
