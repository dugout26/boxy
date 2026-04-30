# Boxy — UI/UX 디자인 설계서 (v3 — handoff adopted)

**작성일**: 2026-04-30 (v3 갱신 동일자)
**버전**: 3.0 (정수 핸드오프 채택 — `design/handoff/app_onboarding_v2/` 기준으로 좌표/사이즈/토큰 1:1 동기화)
**v2 폐기 사유**: Unity 1080×1920 ref 좌표를 임의 가정 → 핸드오프(390×800pt) 와 비례 불일치 → 모든 컴포넌트 사이즈 재산정 필요했음.
**클라이언트**: Mound Studio (백정수, dugout26.gm@gmail.com, 사업자 709-09-03510)
**프로젝트**: Boxy Pack — 모바일 패킹 퍼즐 게임 v1.0
**출시 목표**: 2026-05-28 (Google Play + App Store)
**구현**: Unity 6.3 + UI Toolkit (UXML + USS), 참조 해상도 **390×844 pt** (iPhone 14/15 base), Expand mode
**핸드오프 원본**: `design/handoff/app_onboarding_v2/Boxy Pack.html` (React 프로토타입, Drive `Boxy/App Onboarding (2).zip`)

---

## 0. 이 문서의 목적

v1 디자인이 실패했다. 원인은 두 가지:

1. **웹/데스크탑 스케일 박힘** — 패딩, 폰트, 버튼 크기가 모바일 한 손 조작에 너무 작거나 어색. iPhone 17 Pro 6.3" 화면을 종이로 인쇄해보면 "이건 PC 사이트지 앱이 아니다"라는 인상.
2. **클릭 불가 상태** — `display: none`로 숨겨진 버튼, 컨버터가 `<Button>`을 `<VisualElement>`로 변환, 터치 영역 44pt 미달.

이 문서는 두 문제를 **사이즈 표 + 컴포넌트 명세 + 화면별 와이어프레임**으로 봉쇄한다. 모든 수치는 1080×1920 좌표계 (1x = 1px). 출시 후 v1.1에서 디자이너 외주가 이 문서를 받았을 때, 그대로 Figma로 옮겨 그릴 수 있도록 구체.

---

## 1. 우리는 누구 / 이 게임은 무엇

### Mound Studio
한국의 1인 모바일 게임 스튜디오. 백정수(전 Prepit 운영자)가 2026년에 시작한 시리즈 IP 빌드. **"하나의 마스코트, 여러 게임"** 전략으로 짧고 코지(cozy)한 캐주얼 게임을 시리즈로 출시 — 첫 작품이 Boxy Pack, 후속작 Boxy Sort (6월), Boxy Cafe (7월) 예정.

### Boxy Pack
**"복잡한 짐도 한 칸씩, 꼭 맞게 끼워 넣어요."**

학생 가방 / 이사 박스 / 여행 트렁크에 다양한 모양의 아이템을 빈틈없이 채우는 **코지 패킹 퍼즐**. 총 50 레벨, 3 테마, 회전·되돌리기·힌트 시스템. 핵심 후킹은 **"95% 채웠는데 마지막 한 칸이 안 들어가는"** 좌절 모먼트 (AppLovin 2026 데이터 top 26% 패턴). 노란 큐브 마스코트 **Boxy**가 함께 응원·위로하며 시리즈 IP 캐릭터로 확장. 무료 + 보상형 광고 + 광고 제거 IAP (₩3,900).

### 타겟
- **1차**: 한국 18+ 캐주얼 모바일 게이머 (출퇴근/소파 5분 플레이)
- **2차**: 영어권 4국 (US/UK/CA/AU) 18+
- **디바이스**: iPhone 12 이상 + Android Pixel 5 이상 (98% 커버) — 320dp 너비 미지원 (iPhone SE 1세대 무시)

### 톤
- **Cozy** — 따뜻함, 피곤한 저녁에 위로받는 인상
- **Premium-friendly** — 디테일 있는 일러스트, 8pt 그리드, 절제된 모션. 하지만 무겁지 않음
- **확실히 모바일** — 한 손 조작, 큰 터치, 짧은 텍스트, 즉각적 피드백

피해야 할 것: ❌ 웹사이트 룩, ❌ Material Design 기본, ❌ 차가운 sterile white, ❌ 정성 없는 평면 도형, ❌ 빈 공간만 가득

---

## 2. 모바일 사이즈 표 — 핸드오프 기준 (절대 규칙)

> **모든 수치는 390×844pt reference (1pt = 1px in UXML/USS @ Unity refRes 390×844).**
> Unity가 실기 해상도(iPhone Pro 1170×2532, iPad mini 744×1133 등)에 자동 스케일.
> 출처: `design/handoff/app_onboarding_v2/ui.jsx` 컴포넌트 정의.

### 2-1. 터치 타겟
| 요소 | 너비 | 높이 | 최소 간격 |
|---|---|---|---|
| Primary CTA (시작/계속/구매) | 100% (좌우 패딩 제외) | **56pt** | 위 8, 아래 자유 |
| Secondary 버튼 | 100% | **48pt** | 위 8 |
| Ghost 버튼 (이어하기/닫기) | 100% | **44pt** | — |
| Icon Button (round) | **44×44pt** (sm 40×40) | — | 모서리 16 |
| 토글 (Toggle) | 시스템 기본 | — | row 56pt 안에서 우측 정렬 |
| 레벨 셀 (그리드) | aspect 1:1.15 | (auto) | grid gap 12 |
| 탭 바 항목 | flex (균등) | **48pt** | — |

iOS HIG 최소 터치 44pt 충족. 위반 시 핸드오프 기준 재확인.

### 2-2. 폰트 크기 (Pretendard Variable)
| 토큰 | 크기 | 사용처 |
|---|---|---|
| Display (워드마크) | **44pt Bold, ls -2** | Splash/Main의 "Boxy" |
| H1 | **26pt Bold, ls -1** | 결과 팝업 "축하합니다", 온보딩 step 제목 |
| H2 | **20pt Bold, ls -1** | 광고 모달 제목, 섹션 제목 |
| Body Large | **17pt Regular** | 본문 (settings row, primary text) |
| Body | **15pt Regular** | 일반 본문, 리스트 항목 |
| Caption | **13pt Regular** | 부제, pill, version, hint label |
| Section header | **13pt Bold, ls 0.5** | settings 섹션 헤더 (uppercase tone) |
| Button | **15-17pt Bold** | btn-primary 17, btn-secondary 16, btn-ghost 15 |

### 2-3. 간격 / 레이아웃
- 화면 좌우 패딩: **16-24pt** (Topbar: 16, 일반: 20, Onboarding: 24)
- 컴포넌트 사이 수직 간격: **8-16pt** (그룹 사이 24pt)
- 카드 내부 패딩: **16pt** (settings card는 row pad 4-16)
- Topbar minHeight: **56pt**, padding 14×16
- 안전 영역: iOS frame (notch + home indicator)은 PanelSettings의 `Safe Area` 처리. UXML 안에서는 추가 padding 안 박음.

### 2-4. 모서리(Radius)
- 버튼 (Primary/Secondary/Ghost): **10pt**
- Card: **16pt**
- 모달 카드 (popup-card): **22pt**
- Mascot frame: **28pt**
- Icon Button (round): **50%** (반지름 = width/2)
- Pill / Badge: **999px**
- Level cell: **14pt**

### 2-5. 그림자
USS `box-shadow` Unity 6 미지원 → **2-layer VisualElement** 패턴:

```uxml
<ui:VisualElement class="card-shadow"> <!-- 아래 그림자 레이어 -->
  <ui:VisualElement class="card"> <!-- 실제 카드 -->
    ...
  </ui:VisualElement>
</ui:VisualElement>
```

`.card-shadow`: `background-color: rgba(160, 90, 40, 0.12); translate: 0 6px; border-radius: 32px;`
`.card`: 카드 본체. **모든 카드/버튼은 이 패턴**. cozy 따뜻한 그림자(짙은 갈색이 아닌 peach/orange 30% alpha).

---

## 3. 디자인 토큰 (재정의)

### 컬러 팔레트 — Cozy Boxy
| 토큰 | HEX | 용도 |
|---|---|---|
| `--bg-cozy` | `#FFF6EC` | 메인 배경 (cream + 약간 peach) |
| `--bg-elevated` | `#FFFFFF` | 카드/팝업 표면 |
| `--accent-primary` | `#F59E4B` | **Cozy orange — 메인 CTA, 강조** (Boxy yellow를 대체) |
| `--accent-on` | `#FFFFFF` | 오렌지 위 텍스트 (대비 4.5+) |
| `--accent-soft` | `#FFE4C8` | 부드러운 오렌지 hover/pressed 배경 |
| `--mascot-yellow` | `#FFD60A` | Boxy 마스코트 본체 (브랜드 보존) |
| `--text-primary` | `#3D2B1F` | 본문 (warm dark brown — 차가운 검정 X) |
| `--text-secondary` | `#8B6F5C` | 부제, 캡션 (warm gray-brown) |
| `--border-soft` | `#F0E0CC` | 카드 보더, 구분선 |
| `--success` | `#7CB342` | 성공 (warm olive — 차가운 green X) |
| `--warning` | `#F4A93D` | 경고 |
| `--error` | `#E87764` | 오류 (terracotta — 차가운 red X) |

**핵심 변경 (v1 → v2)**: 메인 강조를 **노란색(#FFD60A)에서 cozy orange(#F59E4B)로** 이동. 노란색은 마스코트에만 남김 → 캐릭터 정체성 보존하면서 UI는 따뜻한 오렌지로 통일감 확보. cozy 일관성과 시인성(노란 위 검정 텍스트의 독서 피로) 동시 해결.

### 모션 토큰
| 토큰 | duration | easing |
|---|---|---|
| `--motion-instant` | 100ms | `ease-out` |
| `--motion-standard` | 250ms | `cubic-bezier(0.2, 0, 0, 1)` |
| `--motion-emphasized` | 500ms | `cubic-bezier(0.2, 0, 0, 1.2)` (살짝 overshoot — cozy bounce) |
| `--motion-page` | 350ms | `ease-in-out` |

---

## 4. 컴포넌트 명세

### 4-1. Primary Button (`.btn-primary`)
```
높이: 132px (고정)
배경: var(--accent-primary)
텍스트: 22px Bold, var(--accent-on), 가운데 정렬
모서리: 20px
패딩: 좌우 32px
그림자: 2-layer 패턴 (alpha 0.18 oranges shadow)
pressed 상태: scale 0.97 + 그림자 alpha 0.30
disabled: opacity 0.5
```

### 4-2. Secondary Button (`.btn-secondary`)
```
높이: 108px
배경: transparent
보더: 2px var(--accent-primary)
텍스트: 22px Bold, var(--accent-primary)
모서리: 20px
hover/pressed: background var(--accent-soft)
```

### 4-3. Ghost Button (`.btn-ghost`)
```
높이: 88px
배경: transparent
텍스트: 18px Regular, var(--text-secondary)
모서리: 20px
hover: background var(--accent-soft)
용도: "이어하기", "건너뛰기" 같은 부 액션
```

### 4-4. Icon Button (`.btn-icon`)
```
크기: 108×108px (정사각)
배경: var(--bg-elevated)
보더: 1px var(--border-soft)
모서리: 50% (원형)
아이콘: 48×48 가운데 정렬, stroke 3, var(--text-primary)
그림자: 2-layer 패턴
```

### 4-5. Card (`.card`)
```
배경: var(--bg-elevated)
모서리: 32px
패딩: 32px
보더: 1px var(--border-soft)
그림자: 2-layer 패턴 (alpha 0.10)
```

### 4-6. Mascot Container (`.mascot-frame`)
```
크기: 320×320px (메인) / 200×200px (인라인)
배경: var(--bg-elevated)
모서리: 48px
패딩: 24px
그림자: 2-layer 패턴 (alpha 0.15)
배경 일러스트: 마스코트 뒤 light peach radial gradient
```

### 4-7. Pill / Badge (`.pill`)
```
높이: 56px
패딩: 12 24
모서리: 999px
배경: var(--accent-soft)
텍스트: 18px Bold, var(--accent-primary)
```

### 4-8. Progress Bar (`.progress-bar`)
```
높이: 16px
배경: var(--border-soft)
fill: var(--accent-primary)
모서리: 8px
```

### 4-9. Text Input (`.input`)
```
높이: 108px
배경: var(--bg-elevated)
보더: 2px var(--border-soft)
focus 보더: 2px var(--accent-primary)
모서리: 20px
폰트: 22px Regular
패딩 좌우: 28px
```

### 4-10. Toast / Snackbar (`.toast`)
```
높이: 88px (자동)
배경: var(--text-primary) (dark brown)
텍스트: var(--bg-elevated), 18px Regular
모서리: 20px
하단에서 24px 위로 슬라이드 인, 3초 후 자동 사라짐
```

---

## 5. 화면별 와이어프레임 (7개)

> 모든 화면은 1080×1920 기준. Y 좌표는 위에서부터.
> 상단 88px / 하단 132px은 안전 영역 — 콘텐츠 X.

### 5-1. Splash / 부팅 (S0)
```
0–88     [safe area]
88–520   [빈 공간]
520–840  마스코트(Boxy 앉아있는 포즈) 320×320 가운데
880–940  워드마크 "Boxy" 48px Bold 가운데
940–1000 부제 "Mound Studio" 18px Regular var(--text-secondary)
1000–    [빈 공간]
1700–1788 progress bar 화면폭 60% 가운데
1788–    [safe area]
배경: var(--bg-cozy)
```
1.5–2.0초 후 자동 메인 메뉴 전환.

### 5-2. Onboarding (3 step swipe — S1)
```
0–88     [safe area]
88–168   "건너뛰기" 우측 상단 (Ghost Button)
168–280  step 인디케이터 ●●● (40×16, 가운데, var(--accent-primary)/var(--border-soft))
280–800  Hero 일러스트 720×520 가운데 (각 step별 다른 그림: 가방·박스·트렁크)
800–880  H1 단계별 제목 36px Bold 가운데 좌우 80 패딩
880–1000 본문 22px Regular var(--text-secondary) 가운데
1000–    [빈 공간 — flex grow]
1640–1772 Primary CTA "다음" / 마지막 step "시작하기" (132 높이, 화면폭 -80)
1772–1788 [safe area buffer]
배경: var(--bg-cozy)

스와이프 좌→우/우→좌로 step 변경, step 인디케이터 dot scale 1.0→1.4 강조
```

### 5-3. 메인 메뉴 (S2)
```
0–88     [safe area]
88–196   상단 우측: 언어 토글 (KO/EN pill, 88×56) 좌측: 코인 잔액 pill
196–360  [빈 공간 — 마스코트와 거리]
360–680  마스코트 320×320 가운데 (mascot-frame 패턴, 아래 그림자)
680–760  워드마크 "Boxy" 48px Bold 가운데
760–840  부제 "짐 정리의 즐거움" 22px var(--text-secondary) 가운데
840–    [빈 공간 flex]
1432–1564 Primary CTA "시작하기" 132 높이
1564–1672 Ghost "이어하기" 108 높이 (저장 데이터 있을 때만 표시)
1672–1716 [16 간격]
1716–1788 좌측: 설정 icon 108 / 우측: 상점 icon 108 (양 끝에서 40 패딩)
배경: var(--bg-cozy)
```

### 5-4. 레벨 셀렉트 (S3)
```
0–88     [safe area]
88–220   상단바: 좌측 "←뒤로" icon-btn 108 / 가운데 H2 "레벨 선택" 28px / 우측 코인 pill
220–260  [구분선 1px, 화면폭 -80]
260–340  테마 탭 (3개 가로 균등): 가방 / 박스 / 트렁크 (132 높이, 활성=accent-primary 밑줄 4px)
340–    [스크롤 영역 시작]
[그리드: 3열 × N행, 셀 220×280 (icon+레벨번호+별점), gap 24, 좌우 40 패딩]
[스크롤 영역 끝]
1700–1788 [하단 fade gradient — 스크롤 가능 hint]
배경: var(--bg-cozy)

레벨 셀:
  배경: var(--bg-elevated)
  잠김: opacity 0.5 + 자물쇠 아이콘 가운데
  완료: 별 1~3개 표시 (각 별 40×40)
  현재 진행: 보더 4px var(--accent-primary)
  220×280, 모서리 24, 그림자 2-layer
```

### 5-5. 게임플레이 (S4 — 핵심 화면)
```
0–88     [safe area]
88–240   상단바:
  좌측: 일시정지 icon 108 (Pause)
  가운데: 레벨 명 "1-7" 28px Bold + 진행도 progress-bar 50% 너비
  우측: 힌트(전구) icon 108 + 되돌리기(↶) icon 108
240–280  [구분선]
280–1280 가방/박스 캔버스 (1000×1000 정사각 — 6×6 그리드 한 칸 ≒ 165px)
  배경: 갈색 가죽 텍스처 (가방) / 갈색 종이 텍스처 (박스) / 청록 직물 (트렁크) — Theme별
  그리드 셀 보더: 1px rgba(255,255,255,0.15)
1280–    [16 간격]
1296–1672 아이템 트레이 (가로 스크롤):
  높이 376px
  배경: var(--bg-elevated), 모서리 32, 좌우 40 마진
  내부: 가로 스크롤 아이템 카드 (각 280×280, 모양 미리보기 + 회전 버튼)
  활성 아이템: 보더 4px var(--accent-primary) + scale 1.05
1672–1700 [페이지 인디케이터 작은 ●●●]
1700–1788 [safe area]
배경: var(--bg-cozy)

상호작용:
  - 트레이에서 아이템 탭 → 활성화 (한 번에 하나)
  - 캔버스 그리드 셀 탭 → 활성 아이템 placement (회전 상태 유지)
  - 회전 버튼 (트레이 아이템 카드 우상단) → 90° 회전
  - placement 시 마스코트가 캔버스 우상단에서 점프 (+ haptic light)
  - 마지막 셀 채움 → 클리어 → S5 전환
```

### 5-6. 결과 팝업 (S5 — 모달 오버레이)
```
[배경: rgba(61, 43, 31, 0.65) 전체 덮음]

가운데 정렬 카드 (840×1300, 모서리 32, var(--bg-elevated)):

  56–280  마스코트 환호 일러스트 240×240 (성공) / 위로 일러스트 (실패)
  280–340 H1 36px Bold 가운데
          성공: "축하합니다!"
          실패: "아쉬워요!"
  340–420 본문 22px Regular var(--text-secondary)
          성공: "{level}단계 클리어"
          실패: "다시 한 번 도전해볼까요?"
  420–540 별 점수 표시 (3개 ★ 80×80, 획득 별 색=accent, 미획득=border-soft)
  540–600 점수 진행도 progress (성공 시만)
  600–760 통계 row 3개:
          - 사용한 시간 / 사용한 힌트 / 되돌리기 횟수
          (각 row 88px, 좌우 패딩 32, var(--text-secondary))
  760–820 [구분선]
  820–   [flex]
  1100–1232 Primary CTA "다음 레벨" (성공) / "다시 시도" (실패) — 화면폭 -160
  1232–1300 Ghost "메뉴로" 108 높이

진입: 카드 1500→가운데로 슬라이드 + scale 0.9→1.0 (motion-emphasized)
배경 fade: 350ms
```

### 5-7. 광고 보상 팝업 (S6 — 모달)
```
[배경: rgba(61, 43, 31, 0.65)]

가운데 카드 (760×920, 모서리 32):

  56–296  마스코트 "광고 보고 보상받기" 포즈 일러스트 240×240
  296–376 H1 "힌트가 필요한가요?" 28px Bold 가운데
  376–456 본문 "광고를 시청하고 힌트를 받아요" 22px var(--text-secondary)
  456–536 보상 미리보기 row (icon 64 + "+1 힌트" 22px Bold)
  536–    [flex]
  720–852 Primary CTA "광고 보기 (15초)" — 화면폭 -120
  852–920 Ghost "닫기" 108

배경 fade 350ms, 카드 scale 0.9→1.0
```

### 5-8. 설정 (S7)
```
0–88     [safe area]
88–220   상단바: 좌측 "← 뒤로" icon 108 / 가운데 H2 "설정" 28px / 우측 빈 공간
220–260  [구분선]
260–    [스크롤 영역]

[섹션 1: 사운드 (각 row 132 높이, 좌우 40 패딩)]
  Row "효과음" + 토글 ON/OFF (88px tap, 56 visual)
  Row "배경음악" + 토글
  Row "햅틱(진동)" + 토글
  Row "마스코트 보이스" + 토글

[섹션 헤더 88 높이, 24px Bold var(--text-primary), 좌측 정렬]
[섹션 2: 게임]
  Row "언어" + value pill "한국어 ▾"
  Row "테마" + value pill "라이트 ▾"
  Row "튜토리얼 다시 보기" → tap 시 Onboarding으로

[섹션 3: 정보]
  Row "광고 제거" + value pill "₩3,900"
  Row "구매 복원" → tap
  Row "이용약관" → 외부 브라우저 (확인 다이얼로그)
  Row "개인정보처리방침" → 외부 브라우저
  Row "버전" + caption "1.0.0 (build 8)"

[섹션 4: 위험 액션]
  Row "진행도 초기화" → confirm → 위험 액션 (var(--error) 텍스트)

배경: var(--bg-cozy)
스크롤 끝에 132 buffer (safe area + Primary CTA 자리 없음)
```

---

## 6. 마스코트 디자인 가이드

### 6-1. 본체 사양
- **본체 색**: var(--mascot-yellow) `#FFD60A` 유지 (브랜드)
- **포즈 5종 필수** (각 1024×1024 PNG, 투명 배경):
  1. **default** — 정면 서있기, 살짝 미소 (메인 메뉴, 부팅)
  2. **happy** — 양손 위로 환호 (결과 성공, 레벨 클리어)
  3. **sad** — 어깨 처진 위로 포즈 (결과 실패, 라이프 0)
  4. **thinking** — 손가락 턱 (튜토리얼, 힌트)
  5. **sleeping** — 눈 감고 옆으로 (장기 미접속 복귀 후)

### 6-2. 절대 피해야 할 것
- ❌ 단순 큐브 + 점 두 개 (정성 부족)
- ❌ flat color만 (그림자, 입체감 필수 — soft cell shading)
- ❌ 너무 비싼 디테일 (디즈니풍 X — Slack/Duolingo 마스코트 수준)
- ❌ 한국 색채 강함 (글로벌 무난)

### 6-3. 표현
- **얼굴**: 두 개의 검은 점 (눈) + 작은 곡선 (입). 표정은 눈썹 곡선과 입 모양으로
- **팔다리**: 짧고 둥근 (스튜비 — 친근함)
- **그림자**: 발 아래 타원 그림자 1개 (지면 인식)
- **하이라이트**: 윗면에 흰색 부드러운 highlight 1개 (입체감)

### 6-4. v1.0 임시 (정성 일러스트 외주 전)
- 정수가 Python PIL로 만든 default 포즈 1종으로 시작 → 외주 일러스트 도착 시 5종 교체
- 외주 단가 가이드: 5종 × ₩50–80만 = ₩300–400만 예산 (선택)

---

## 7. 화면 전환 / 애니메이션

| 전환 | duration | easing | 비고 |
|---|---|---|---|
| 화면 push (메뉴 → 레벨 셀렉트) | 350ms | motion-page | 우→좌 슬라이드 |
| 화면 pop (← 뒤로) | 350ms | motion-page | 좌→우 슬라이드 |
| 모달 진입 (결과 팝업) | 350ms | motion-emphasized | scale 0.9→1.0 + 배경 fade |
| 모달 종료 | 250ms | motion-standard | scale 1.0→0.95 + fade out |
| 버튼 press | 100ms | motion-instant | scale 1.0→0.97 |
| 마스코트 reaction (placement) | 500ms | motion-emphasized | 위로 점프 (translate Y -40 → 0) |
| Toast 진입 | 250ms | motion-standard | 하단에서 24px 위로 |

---

## 8. 사운드 / 햅틱

### 8-1. 햅틱 (Lofelt Nice Vibrations 사용 강제 — Unity 내장 X)
| 이벤트 | 햅틱 |
|---|---|
| 버튼 탭 | Light |
| 아이템 placement (정상) | Medium |
| 아이템 placement (잘못 — 회색 표시) | Failure |
| 레벨 클리어 | Success |
| 레벨 실패 | Warning |
| 광고 보상 수령 | Success |
| 회전 | Selection |

### 8-2. 사운드 (placeholder — 외주 또는 Storyblocks 라이선스)
- **BGM**: Lofi cozy piano loop, 90 BPM, 60-90초 반복 (메인 메뉴 / 게임플레이 별도)
- **SFX**: tap 02_short / placement 01_pop / clear 04_sparkle / fail 03_thud / coin 05_chime

---

## 9. 화면 별 컴포넌트 매핑 (UXML 작성 시 참조)

| 화면 | 사용 컴포넌트 | 스크롤 |
|---|---|---|
| Splash | mascot-frame, progress-bar, wordmark | X |
| Onboarding | btn-primary, btn-ghost, hero-illust, step-dot | X (스와이프) |
| Main Menu | mascot-frame, wordmark, btn-primary, btn-ghost, btn-icon (×2), pill (lang) | X |
| Level Select | btn-icon (back), tab-bar, level-cell (grid), pill (coin) | Y |
| Gameplay | btn-icon (×4), progress-bar, canvas-grid, item-tray (가로 scroll) | X (메인) |
| Result Popup | mascot, h1, body, star-row, stat-row, btn-primary, btn-ghost | X |
| Ad Reward Popup | mascot, h1, body, reward-row, btn-primary, btn-ghost | X |
| Settings | btn-icon (back), section-header, settings-row (toggle/value/link), btn-danger | Y |

---

## 10. 디자인 작업 단계 (구현 순서)

> AI 자체 작업. 정수는 검수만.

1. **Tokens 재작성** — `tokens.uss` v2 컬러/폰트/spacing 토큰 (§3) — **30분**
2. **Components.uss 재작성** — §4 컴포넌트 10종 — **2시간**
3. **MainMenu.uxml 재작성** — §5-3 와이어 그대로, 모든 버튼 작동 — **1시간**
4. **Editor 시뮬레이터 검증** — Build Profile dev → iOS Simulator iPhone 17 Pro Max → 모든 버튼 탭 검증 — **30분**
5. **나머지 6개 화면 순차 작성** — §5-1, 5-2, 5-4, 5-5, 5-6, 5-7, 5-8 — **각 1시간 × 6 = 6시간**
6. **씬 wiring 검증** — Unity Scene별 PanelSettings/UIDocument 참조 → Onboarding/Settings 누락 없음 — **30분**
7. **마스코트 일러스트 5종 교체** (정수 외주 시점) — **외주 후 1시간**

**총 예상**: 11시간 + 마스코트 외주 별도. 4주 일정 중 1.5–2일 분량.

---

## 11. 검수 체크리스트 (출시 직전)

- [ ] iPhone 17 Pro Max + iPhone SE 3 (4.7") 모두에서 모든 버튼 88px+ 터치 영역
- [ ] iPad mini 8.3" 1024×1366에서 레이아웃 안 깨짐 (Expand mode)
- [ ] 다크모드 OS에서 cozy bg가 자동 다크 변환되지 않고 의도적 light bg 유지
- [ ] Pretendard 폰트 18px 이하 텍스트 0건
- [ ] 모든 화면 safe area 상단 88, 하단 132 준수
- [ ] 모든 모달 진입 350ms 안에 완료, 닫기 250ms
- [ ] 햅틱 7종 모두 트리거 검증
- [ ] 한국어 + English 텍스트 길이 차이로 잘림 없음 (특히 버튼 라벨)
- [ ] 스크린샷 4개 화면 (메인/레벨/게임/결과) 업데이트 → marketing/screenshots/ 신규

---

## 12. v1.1 디자인 백로그 (출시 후)

- 다크모드 (현재 v1.0은 light only)
- iPad 전용 큰 캔버스 레이아웃 (현재 1080 그대로 stretch)
- 마스코트 외주 일러스트 5종 → 7종 + 코스튬 시즌제
- Lottie 애니메이션 도입 (현재 정적 PNG)
- 사용자 커스텀 테마 (광고 제거 IAP 사용자 한정)
