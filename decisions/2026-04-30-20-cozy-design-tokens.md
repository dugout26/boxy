# 2026-04-30 — Boxy Cozy 디자인 토큰 채택 (Mound v3.0)

## Context
이전 시도 4번 모두 디자인 quality 부족 (Python 합성 마스코트 / 직접 UXML / Stitch MCP / Stitch HTML→UXML 자동변환). 정수가 외주받은 **온보딩 디자인 핸드오프** (Drive `Boxy/design_handoff_snug_onboarding/`, 로컬 `design/handoff/cozy_onboarding/`) 도착 — README + 13 파일 + 7 화면 명세 + 토큰 정의.

핸드오프 원본은 다른 게임명/마스코트 명세였으나, 같은 장르(짐 정리 퍼즐) + 같은 Mound 디자인 시스템 진화 — 토큰만 차용하고 게임 정체성은 Boxy 그대로 유지.

## Options
- **A. 완전 리브랜드** — Boxy → 다른 이름, 큐브 → 둥근 캐릭터, 노랑 → 오렌지 모두 변경. App Store Connect 등록 진행 중인 com.mound.boxy 번들/decisions/마케팅 50+ 자료 재작업
- **B. 디자인 시스템만 차용 + Boxy 브랜드 유지** — 컬러/타이포/스페이싱/컴포넌트 토큰 채택, 게임명·번들 ID·decisions 자료 그대로
- **C. 핸드오프 그대로 + Boxy 코드 위 덮기** — 게임명 혼용 → 혼란

## Decision
**B**. 정수 결정.

## Consequences
좋은 것:
- Cozy warm orange (#F59E4B + cream #FFF6EC) 톤 확보 — 이전 차가운 #FFD60A + #F8F9FA 대비 정성 큼
- Stacked shadow 시그니처, speech bubble, step dots, lang pill, permission row 등 컴포넌트 라이브러리 완성
- Pretendard 단일 폰트 정책 유지 (Mound v2.0 호환)
- 일주일 작업한 Boxy 자산 (App Store 번들 com.mound.boxy / com.mound.boxy.dev / 19 decisions / 마케팅 카피 / 법적 문서) 그대로 보존

나쁜 것 / 위험:
- 마스코트 yellow cube (Boxy) + UI accent orange (#F59E4B) 색 충돌 가능 → 마스코트 일러스트 외주 또는 Scenario LoRA로 cozy 베이지 톤 + 액센트 오렌지 디테일 추가 필요 (v1.0 출시 전)
- accent-boxy.uss의 노란 #FFD60A 정의 제거 — 기존 marketing/screenshots 8장 톤이 옛 톤 (재캡처 완료)
- 핸드오프엔 LevelSelect/Settings/Gameplay 등 게임 본체 화면 명세 X (온보딩 7단계만) → 같은 디자인 토큰으로 게임 본체 화면 자체 작성 완료

## 변경 사항
1. `Assets/Mound/UI/Styles/tokens.uss` v2.0 → v3.0:
   - 신규 토큰: `--bg-cozy` (#FFF6EC), `--accent` (#F59E4B), `--accent-soft` (#FFE3CC), `--accent-deep` (#C46B1F), `--accent-cream`, `--on-accent` (#FFFFFF), `--border-warm` (#F1ECE5), `--piece-1~6` (게임 아이템 6색)
   - radius: button 12 → 14, card 16, bubble 18, board 20, pill 9999
   - text size: tiny 12px 추가
2. `Assets/Mound/UI/Styles/components.uss` cozy 컴포넌트 추가:
   - `.btn-primary` 높이 48 → 52, stacked shadow 시뮬레이션 (bottom border 2px accent-deep)
   - `.bg-cozy`, `.wordmark`, `.speech-bubble`, `.step-dots` + `.step-dot` + `.step-dot-active`, `.lang-pill` + 자식, `.permission-row` + `.permission-card`, `.cozy-cell`, `.cozy-board`, `.bottom-sheet` + `.bottom-sheet-handle`
3. `Assets/Boxy.App/UI/accent-boxy.uss` `:root` 노란 #FFD60A override 제거 (tokens.uss 토큰 그대로 사용)
4. 7개 UXML 화면 cozy 톤 재작성 (MainMenu / LevelSelect / Gameplay / Settings / ResultPopup / AdRewardPopup / Onboarding)
5. KoEnStringTable.cs `MenuPlay` ko "PLAY" → "시작하기", en → "Get started"

## 잔여 작업
- 마스코트 일러스트 cozy 톤 업그레이드 (외주 / Scenario LoRA)
- Settings 햅틱 토글 둥근 segment 모양 (현재 checkbox 기본 모양)
- ASO 키워드 "정리 퍼즐 / 패킹" 한국어 카피 점검 (핸드오프 카피 "꼭 맞게, 차곡차곡" 차용 가능)

## Revisit when
- 마스코트 yellow + 오렌지 UI 색 충돌이 시각적으로 어색하면 → 마스코트 cozy 베이지 톤 외주 (또는 노란 유지하되 채도 낮춰 cream 톤에 어울리게)
- 출시 후 D1 retention < 25% → 디자인 톤 자체 재검토
