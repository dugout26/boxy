# Boxy — Unity 활성화 후 셋업 가이드

**용도**: 정수가 Mac mini 화면 접근 후 Unity Hub OAuth 활성화 → Boxy 프로젝트 첫 실행까지 단계별 가이드.

코드는 Phase 1~17 모두 완료. 컴파일 검증만 남음. 이 가이드대로 따르면 30분 내 첫 Boxy 프로토타입 동작 확인 가능.

---

## A. Unity Hub 활성화 (5분)

1. **Unity Hub** 실행 (Spotlight `Cmd+Space` → "Unity Hub")
2. **Sign in** → **Continue with Google** → `dugout26.gm@gmail.com`
3. 자동으로 Personal License 부여
4. Hub → **Projects** → **Open** → `/Users/jeki/Boxy/` 선택
5. Unity Editor가 6000.3.14f1로 자동 열림 (`Library/`, `Packages/`, `ProjectSettings/` 자동 생성)
6. 첫 컴파일 끝나면 **Console 창에 빨간 에러 0** 확인

> 컴파일 에러 발견 시: 메시지 그대로 채팅에 붙여주세요. AI가 즉시 진단/수정.

---

## B. Build Settings 셋업 (3분)

`File → Build Settings`:

1. Platform: **Android** 선택 → **Switch Platform**
2. Scenes In Build에 4개 씬 추가 (없으면 생성):
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/LevelSelect.unity`
   - `Assets/Scenes/Gameplay.unity`
   - `Assets/Scenes/Settings.unity`

씬이 없으면: `File → New Scene` → 빈 씬 → 위 이름으로 저장.

> 씬 이름이 `BoxySceneNames` 상수와 정확히 일치해야 함 (`Boxy.App/SceneFlow.cs`).

---

## C. UI Toolkit Panel Settings 생성 (2분)

각 씬에 UIDocument 컴포넌트가 필요. 공유 Panel Settings 1개 생성:

1. Project 창 → 우클릭 → **Create → UI Toolkit → Panel Settings Asset**
2. 이름: `BoxyPanelSettings`
3. 위치: `Assets/Settings/`
4. 인스펙터에서:
   - **Theme Style Sheet**: Unity 기본 (`UnityDefaultRuntimeTheme`)
   - **Reference Resolution**: 393 × 851 (boxy-plan §B-1 모바일 9:21 기준)

---

## D. 씬별 셋업 (각 씬 5분)

### MainMenu Scene

1. Hierarchy → 우클릭 → **UI Toolkit → UI Document**
2. 생성된 UIDocument의 인스펙터:
   - **Panel Settings**: BoxyPanelSettings 드래그
   - **Source Asset**: `Assets/Boxy.App/UI/MainMenu.uxml` 드래그
3. 같은 GameObject에 **Add Component → Main Menu Controller**

### LevelSelect Scene

1. UIDocument 생성 (위와 동일)
2. Source Asset: `LevelSelect.uxml`
3. **Level Select Controller** 컴포넌트 추가
4. **Level Select Controller**의 `Level Assets` 배열:
   - 정수가 직접 만든 `LevelData.asset` 50개 드래그
   - **또는 비워두면** Tutorial 5 레벨 자동 생성 (Phase 16 fallback)

### Gameplay Scene

1. **두 개의 GameObject 필요**:
   - GameObject A "GameplayUI" → UIDocument + GameplayUI 컴포넌트
   - GameObject B "GameplayController" → GameplayController 컴포넌트
2. UIDocument 설정:
   - Source Asset: `Gameplay.uxml`
3. GameplayUI 컴포넌트 인스펙터:
   - `Controller`: GameObject B 드래그
   - `Default Level`: 테스트용 LevelData asset (없으면 fallback 자동)
   - `Result Popup Asset`: `Assets/Boxy.App/UI/ResultPopup.uxml` 드래그
   - `Level Assets`: LevelData 배열 (LevelSelect와 동일하게)

### Settings Scene

1. UIDocument + Source Asset: `Settings.uxml`
2. **Settings Controller** 컴포넌트 추가

---

## E. LevelData asset 생성 옵션

### 옵션 A: Inspector에서 수동 (정성 디자인, boxy-plan §B-3-1 권장)

1. Project 창 우클릭 → **Create → Boxy → Level Data**
2. 인스펙터에서:
   - Level Number: 1, 2, ...
   - Grid Width / Height: 6 / 8 (또는 작게)
   - Theme Key: `school_bag` / `moving_box` / `travel_trunk`
   - Items: + 버튼으로 LevelItemTemplate 추가
     - itemKey: "book", "notebook" 등
     - cells: List<Vector2Int>로 모양 정의

### 옵션 B: 코드 자동 생성 (boxy-plan §B-3-2 거꾸로 생성)

Editor에서 메뉴 추가 (별도 작업 필요 — Phase 18 이후):

```csharp
// 임시 Editor 스크립트 (Assets/Editor/LevelGeneratorMenu.cs):
[MenuItem("Boxy/Generate 50 Levels")]
public static void GenerateLevels()
{
    var generator = new LevelGenerator(seed: 42);
    for (int i = 1; i <= 50; i++)
    {
        // 진행 단계별 grid 크기 / shape catalog 결정
        var items = generator.Generate(...);
        var level = ScriptableObject.CreateInstance<LevelData>();
        level.Initialize(i, w, h, theme, items);
        AssetDatabase.CreateAsset(level, $"Assets/Levels/Level_{i:D2}.asset");
    }
}
```

지금은 Tutorial fallback으로 5레벨 자동 동작. 정수 직접 디자인 필요한 시점에 작업.

---

## F. 첫 실행 검증 체크리스트

Editor에서 Play 버튼:

1. **MainMenu Scene 시작**
   - [ ] 노란 박스(마스코트 placeholder) + "Boxy" 타이틀 + "PLAY" 버튼 보임
   - [ ] PLAY 클릭 → LevelSelect 씬 전환

2. **LevelSelect Scene**
   - [ ] 3 테마 탭 (학교 가방 활성)
   - [ ] 레벨 그리드에 5개(또는 50개) 레벨 버튼
   - [ ] 레벨 1 클릭 → Gameplay 씬 전환

3. **Gameplay Scene**
   - [ ] 가방 프레임 + 그리드 셀 표시
   - [ ] 트레이에 아이템 카드들
   - [ ] 카드 드래그 → 그리드에 떨어뜨림 → 셀 채워짐 (배치 성공)
   - [ ] 그리드 위에서 카드 길게 누르기 (0.5s) → 90도 회전 ⭐
   - [ ] 잘못된 위치에 드롭 → 카드 트레이로 복귀
   - [ ] 모든 셀 채우면 → ResultPopup 나타남 ("레벨 클리어!" + 별)
   - [ ] 다음 레벨 / 다시 도전 / 메뉴로 버튼 동작

4. **Settings Scene**
   - [ ] BGM/SFX 슬라이더 동작 (PlayerPrefs 저장)
   - [ ] 진동 토글
   - [ ] 뒤로 가기 → MainMenu 복귀

---

## G. 흔한 첫 실행 에러 + 해결

| 에러 | 원인 | 해결 |
|---|---|---|
| `Scene 'X' not in build settings` | Build Settings에 씬 미추가 | File → Build Settings → Add Open Scenes |
| `UIDocument source asset is null` | Source Asset 미연결 | 인스펙터에서 .uxml 드래그 |
| 화면 빈 (UI 안 보임) | Panel Settings 미설정 | UIDocument → Panel Settings 슬롯 채우기 |
| `Q<T>("name") returned null` | UXML name 안 맞음 | UXML 파일 열어 name 속성 확인 |
| 레벨 그리드 비어있음 | LevelData asset 미연결 | LevelSelectController.levelAssets에 SO 드래그 (또는 Tutorial fallback 자동) |
| 드래그 안 됨 | UIDocument의 Pickable 비활성 | 자동으로 Pickable이지만 UIDocument 부모 layout 점검 |
| 드롭 위치 빗나감 | y-up 좌표 변환 버그 | Phase 8 GridCellView CellSizePx + GameplayUI PanelToGridAnchor 검증 |

---

## H. Phase 1~17 코드 누적 검증

```bash
# 정적 검사 — 컴파일 외 패턴 위반 검출
./scripts/check-violations.sh all
```

현재 결과: ✅ 패턴 검사 통과 (Phase 17 시점)

Unity Editor 컴파일 시 추가 발견 가능 — 콘솔 에러 즉시 채팅으로 공유.

---

## I. 다음 단계 (Phase 18+)

첫 컴파일 통과 후 우선순위:
1. **AppLovin SDK 통합** → `AppLovinAdProvider : IAdProvider` (Week 3 Day 15)
2. **Firebase + AppsFlyer** → `FirebaseAnalyticsProvider : IAnalyticsProvider`
3. **Unity IAP** → `UnityIapProvider : IIapProvider`
4. **Boxy 마스코트 sprite** (Scenario MCP, Day 11~12)
5. **인게임 아이템 sprite 50개** (Scenario MCP)
6. **첫 5레벨 정성 디자인** (TutorialLevels 대체, Day 10)
7. **클로즈드 테스트 빌드** (5/12 마감)

`decisions/2026-04-29-06-phase4-11-summary.md` + `2026-04-29-07-phase12-17-summary.md` 참조.
