# 2026-04-29 — App Store 표시 이름: "Boxy Pack"

## Context

iOS App Store Connect 앱 레코드 생성 단계에서 정수가 "Boxy" 이름을 사용하려 했으나 이미 다른 앱이 등록되어 있어 사용 불가. App Store는 앱 이름 글로벌 고유 강제.

대체 후보 검토:
- **Mound Boxy** — 스튜디오 브랜드 prefix
- **Boxy Pack** — 게임플레이 설명 suffix
- **Boxy Bag / Boxy Puzzle / Boxy!** 등 변형

내부 IP 명칭(마스코트 "Boxy")은 그대로 유지. 변경되는 건 App Store 표시 이름과 PlayerSettings.productName 뿐.

## Options

- **A. Mound Boxy** — 스튜디오 브랜드 prefix, 시리즈 확장 시 일관성 (Mound Boxy Sort 등)
- **B. Boxy Pack** — 게임플레이 설명, ASO 키워드 "pack" 흡수
- **C. Boxy Bag / Boxy Puzzle** — 변형
- **D. 박시 (한글)** — 한국 시장 한정

## Decision

**B 채택 — "Boxy Pack"**.

근거 (정수 결정):
> "Mound라는 브랜드가 인지도가 없는 상태에서 Mound Boxy는 이상하다."

추가 분석:
- 스튜디오 브랜드가 무명일 땐 prefix가 오히려 신호 약화 (Spotify는 Spotify, "ABC Spotify"가 아니다)
- "Pack"이 패킹 퍼즐 장르를 직접 표현 → ASO 키워드 자연 흡수
- 글자수 9 — 홈 화면/검색 결과에서 잘림 없음
- 시리즈 확장은 후속작에서 별도 명명 (`Boxy Sort`, `Boxy Cafe`) — IP 일관성은 마스코트 디자인으로 유지

## Consequences

### 좋은 것
- App Store 검색에서 "boxy" + "pack" 양쪽 키워드 노출
- 글로벌 시장에서 "pack"이 직관적 (한국 시장은 부제목 "가방 챌린지 패킹 퍼즐"로 보강)
- 후속 시리즈와 명명 충돌 없음 (`Boxy Sort` 등)

### 나쁜 것 / 위험
- 검색에서 다른 "Boxy" 앱과 함께 노출됨 (기존 "Boxy" 앱이 무엇인지 정수가 확인 권장)
- Bundle ID는 `com.mound.boxy`로 유지 (마스코트/IP 명) — 표시명과 미스매치 발생, 향후 운영자 혼동 가능성 (이 결정 기록으로 추적 가능)

### 적용 범위
1. `PlayerSettings.productName`:
   - dev: `Boxy Dev` (App Store Connect 등록명과 일치 — 정수가 dev 앱 먼저 만들면서 이름이 `Boxy Dev`로 확정됨, 외부 공개되지 않으므로 OK)
   - prod: `Boxy Pack`
2. Info.plist `CFBundleName` / `CFBundleDisplayName` 자동 갱신 (Unity가 productName으로 채움)
3. App Store Connect 앱 이름:
   - dev: `Boxy Dev` (Bundle ID `com.mound.boxy.dev`, 정수 검증용, 외부 공개 X)
   - prod: `Boxy Pack` (Bundle ID `com.mound.boxy`, App Store 출시용)
4. App Store Subtitle (한국어, prod만): `가방 챌린지 패킹 퍼즐`
5. Google Play Store 동일 (`Boxy Pack`)
6. 마스코트/IP/코드/네임스페이스는 그대로 `Boxy` 유지 (`Boxy.App.*`, `Mound.*`)

### 변경 안 되는 것
- 코드 네임스페이스 `Boxy.App.*` 유지
- Bundle ID `com.mound.boxy.dev` / `com.mound.boxy` 유지
- 마스코트 이름 "Boxy" 유지
- Mound 스튜디오 브랜드, 시리즈 IP 컨셉 유지

## Revisit when

다음 중 하나 발생 시 재검토:
- Apple/Google이 표시명 변경을 강제하는 정책 위반 통보
- D7 retention 추적에서 "Boxy Pack" 이름 인지 가설이 D1 30% 이하로 부정 (오히려 게임 자체 문제일 가능성 큼)
- 시리즈 2~3편 출시 후 브랜드 일관성을 위해 prefix 통일이 필요해질 때 (`Boxy Pack` → `Mound Boxy` 일괄 리네임)
