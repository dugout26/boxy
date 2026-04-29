# 2026-04-29 — 반응형 레이아웃 표준 채택

## Context

iPhone 17 시뮬레이터에서 빌드 후 정수가 "휴대폰 비율마다 다르면 어떡하는가, 하드코딩이 무섭다"고 지적. 당시 코드는:

- `PanelSettings.scaleMode = ScaleWithScreenSize`, `screenMatchMode = MatchWidthOrHeight`, `match = 0.5`
- UXML 모든 사이즈 px 하드코딩 (`width: 240px`, `margin-top: 64px`)
- SafeAreaController가 직접 비율 계산해서 padding 적용 (수식 부정확)

업계 표준 + Unity 공식 권장 조사 (`research agent` 결과)에서 다음을 확인:

1. **PanelScreenMatchMode**에 `MatchWidthOrHeight` 외에 **`Expand`**, `Shrink` 모드 존재. 그리드 게임(Boxy 6×6)은 **Expand**가 정답 — 참조 해상도가 항상 보장되고 추가 공간은 위/아래로 확장.
2. **Reference Resolution 1080×1920 (9:16)**이 모바일 portrait 표준 — Space Ape Games 등 다수 검증 패턴.
3. **Unity 6에 USS `aspect-ratio` 속성 정식 추가** (`UIE-USS-SupportedProperties.html` 직접 확인) — 6×6 그리드 정사각형 강제에 결정적.
4. **Safe Area는 `Screen.safeArea` + `RuntimePanelUtils.ScreenToPanel(panel, ...)`** 패턴 — 직접 비율 계산은 부정확 (Y축 뒤집힘 + 다양한 panel 모드 무시).
5. **레이아웃 표준 패턴**: Header(고정 px) / Content(flex-grow:1) / Footer(고정 px). 마스코트·버튼은 `max-width` + `width: 100%`로 컨테이너 너비 추종.

## Options

- **A. 현 상태 유지** (Match=0.5 + 하드코딩 px) — 최소 노력, iPhone 17 1대만 잘 보임
- **B. Match=1 (height)로만 변경** — 세로 좌표 일관, 가로 공간 가변
- **C. Expand 모드 + `aspect-ratio` + RuntimePanelUtils + 표준 layout 패턴** — 업계 표준
- **D. UGUI로 회귀** + Canvas Scaler — 검증된 옛 패턴, UI Toolkit 일부 기능 포기

## Decision

**C 채택**. 업계 표준이고 Unity 6 공식 권장이며, 1인 4주 일정에도 1~2시간이면 적용 가능. UGUI 회귀(D)는 §0-2 재작업 너무 많음.

구체 적용:

### 1. PanelSettings (BoxyPanelSettings.asset)
```
scaleMode: ScaleWithScreenSize
screenMatchMode: Expand           ← 핵심 변경 (MatchWidthOrHeight 폐기)
referenceResolution: 1080 × 1920
match: 0 (Expand 모드에선 무시되지만 명시)
```

### 2. UXML 표준 패턴 (모든 씬)
```xml
<root style="flex-grow: 1; flex-direction: column;">
  <SafeAreaContainer> <!-- top inset 자동 -->
    <header style="height: 80px; flex-shrink: 0;" />
    <content style="flex-grow: 1; align-items: center; justify-content: center;">
      <!-- Boxy 그리드: aspect-ratio: 1 + max-width 600px -->
    </content>
    <footer style="height: 120px; flex-shrink: 0;" />
  </SafeAreaContainer>
</root>
```

### 3. Mound.UI/SafeAreaContainer.cs
- VisualElement 서브클래스 또는 [UxmlElement]
- `AttachToPanelEvent` + `GeometryChangedEvent` 등록
- `Screen.safeArea` → `RuntimePanelUtils.ScreenToPanel(panel, ...)`로 panel 좌표 변환
- padding-top/bottom/left/right 자동 적용
- iPhone (notch/Dynamic Island), Android, iPad 모두 단일 코드

### 4. 그리드 (B-3-2 6×6)
```css
.bag-frame {
    width: 100%;
    max-width: 600px;
    aspect-ratio: 1;
    align-self: center;
}
.grid-cell {
    flex-grow: 1;
    aspect-ratio: 1;
}
```
- iPhone (좁음): 그리드가 너비 가득
- iPad (넓음): 600px 캡 + 가운데 정렬, 양옆 빈공간

### 5. 지원 디바이스
- Primary: iPhone 11+, Galaxy S20+ (19.5:9~20:9) → 1차 디자인 타겟
- iPhone SE (16:9): 같은 빌드, ContentArea 자동 축소. iPad: 같은 빌드, 그리드 600px 캡으로 가운데. **별도 빌드 X.**
- maxAspectRatio Android는 Unity 기본값 2.4 유지 (폴드 펼침 1:2.4 안팎까지 자동 처리).

## Consequences

### 좋은 것
- 모든 디바이스에서 그리드(게임 핵심) 정사각형 보장
- 하드코딩 px 제거 → 비율 변경 시 1곳만 수정
- Mound.UI 자산 재사용 가능 (SafeAreaContainer는 Boxy Sort/Cafe에도)
- §11-3 플랫폼 분기 양쪽 검증 가능 (iOS notch + Android 평판 모두 한 코드)
- 표준 패턴이라 향후 외주/팀 확장 시 이해도 높음

### 나쁜 것 / 위험
- USS `aspect-ratio` 속성이 Unity 6.3.14f1에서 안정 동작 확인 필요 (research에서 본 건 6000.3.0a3 알파). Day 2~3에 작은 테스트 씬으로 1회 검증.
- iPad에서 그리드 600px 캡은 추정값 — 실기에서 정수 직접 보고 조정 가능.
- iPhone SE 사용자에게 그리드 셀 작아져 터치 정확도 저하 가능 — Crashlytics·QA로 확인.

### 검증 게이트
- 시뮬레이터 3개 (iPhone SE 3 / iPhone 17 / iPad) 빌드 후 메인 메뉴 + 게임플레이 화면 비교 → 핵심 요소 위치 일관성 확인
- Day 2~3 검증 후 §0-1 가짜 완료 금지 — 실제 보고 OK 한 후에만 결정 확정

## Revisit when

- USS `aspect-ratio`가 Unity 6.3.14f1에서 안정 동작 확인 안 되면 C# 커스텀 컨트롤(공식 문서 `UIE-create-aspect-ratios-custom-control.html` 참조)로 fallback
- Royal Match 같은 톱 게임의 iPad UI 캡처 분석 후 max-width 600px 조정 필요시
- Mound 시리즈 2번째 작품(Boxy Sort?) 시작 시 SafeAreaContainer 재사용 검증
