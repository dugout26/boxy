# Boxy — CLAUDE.md

> Mound 모바일 게임 1번 빌드. Unity 6 LTS · C# · UI Toolkit.
> 이 문서는 모든 코드 생성/수정 시 반드시 따른다. 위반 시 그 자리에서 거절하고 재작성.
> **바이브코딩 프로젝트**. AI가 환각/폭주하기 쉬움 — 가드레일 우선.

---

## 0. 메타 규칙 — 가장 중요

이 섹션 위반은 다른 모든 규칙 위반보다 무겁다.

### 0-1. 가짜 완료 금지
- **컴파일을 직접 돌리지 않았으면 "컴파일 완료"라고 말하지 말 것**
- **Play Mode 검증을 하지 않았으면 "테스트 완료"라고 말하지 말 것**
- **SDK 연동은 대시보드에서 이벤트 도착 확인 전까지 "완료" 아님**
- **추정과 확인을 명확히 분리해서 말함**: "이 코드는 동작할 것으로 추정됨" vs "Editor에서 실행 확인함"

### 0-2. AI 자신감 의심 휴리스틱
다음 신호가 보이면 **그 코드 한 번 더 의심하고 공식 문서 대조**:
- 외부 SDK/API 호출이 너무 매끄럽게 들어가 있음 (실제 시그니처가 다를 수 있음)
- "최신 패턴입니다" / "이렇게 하시면 됩니다" 같은 단정적 표현
- 한 번도 "확인이 필요합니다"가 안 나옴
- 에러 처리가 너무 깔끔함 (실제 SDK 예외는 더 지저분함)
- "그럴듯한 메서드명"이 IntelliSense 없이 자신 있게 호출됨

### 0-3. 의심 → 검증 절차
1. 의심되는 API 즉시 Unity 공식 문서 / 패키지 ChangeLog 검색
2. IntelliSense / Roslyn으로 실재 검증
3. 외부 SDK는 해당 버전 공식 샘플과 시그니처 대조
4. 환각 1개 발견 시 **그 세션 전체 의심** — 같은 채팅에서 만든 다른 코드도 재검증

### 0-4. 세션 시작 강제 로드 (절대 우회 금지)
모든 코드 생성/수정 전 **다음 3단계를 출력**해야 작업 시작:

1. **문서 확인**: "§0~§19 끝까지 읽음, 핵심 규칙 N개 인지" (1줄)
2. **적용 § 명시**: 이번 작업에서 따를 § 번호 3개 이상 (예: §2-2, §6-3, §13-1)
3. **SDK 버전 확인**: `SDK_VERSIONS.md` 어느 버전 기준인지 명시 (또는 "버전 무관")

**위 3단계 출력 없이 코드 생성 금지.** 정수가 발견 즉시 거절.

**왜 이 메커니즘인가**: AI가 이 문서를 안 읽거나 잊은 채 코드 짜는 게 4주 일정 최대 리스크. 절차적 강제는 깨지기 쉬우므로 §19 기술적 강제(pre-commit hook)로 보완.

### 0-5. 새 세션 시작 신호
다음 신호가 보이면 **현재 세션 종료 → 새 세션에서 CLAUDE.md 재로드**:
- AI가 이전 턴의 결정/규칙을 잊은 듯한 답변
- 같은 파일을 5회 이상 수정 중
- 큰 리팩터링 / 새 모듈 신설 직전
- 환각 1건 발견 (§0-3에 의해 세션 전체 의심)

긴 세션은 환각/규칙 무시 빈도를 누진적으로 높임.

---

## 1. 참조 문서

- 기획서: `boxy-plan.md`
- 디자인 시스템: `~/.mound/mound-design-system.md`
- 게임 액센트: `#FFD60A` (액센트 위 텍스트는 항상 검정 `#1A1A1A` — 대비 4.5+ 강제)
- SDK 버전 메모: `SDK_VERSIONS.md` (Day 1 작성, 코드 생성 시 이 버전 기준)
- 결정 기록: `decisions/` 폴더 (§17)
- 기술적 강제: `scripts/check-violations.sh`, `.editorconfig`, `.gitignore` (§19)

---

## 2. AI 가드레일 (바이브코딩 폭주 차단)

### 2-1. AI 설계 변경 금지
- 기획서/CLAUDE.md에 없는 **새 매니저, 새 싱글톤, 새 프레임워크, 새 폴더 구조** 임의 생성 금지
- 기존 구조로 해결 가능하면 **반드시** 기존 구조 사용
- 변경이 필요하다면: **변경 전에** "왜 기존 구조로 안 되는지" 설명 → 정수 승인 후 진행

### 2-2. 파일 생성 제한
- **새 파일 생성은 최소화**. 기존 파일 수정으로 풀 수 있으면 새 파일 만들지 말 것
- **한 기능당 신규 파일 3개 초과 금지**
- 새 파일 생성 시 반드시 명시:
  - 목적 1줄
  - 사용처 (어디서 import하는가)
  - 참조 관계 (어떤 모듈 의존)

### 2-3. 작업 단위 제한
- **한 프롬프트당 새 코드 200줄 또는 파일 3개 이하**
- **한 턴에 5개 이상 파일 수정 금지**
- **리팩터링과 기능 추가를 같은 작업에서 하지 말 것** (분리 후 순차)
- 대규모 변경 필요 시: **변경 계획 먼저 출력 → 정수 승인 → 진행**
- "전체 시스템 한 번에 짜줘" 금지

### 2-4. API 환각 방지
```csharp
// 환각 예시 — 실제로 존재하지 않는 호출
HapticFeedback.Vibrate(HapticType.Success);  // Unity에 이런 것 없음
Application.RequestATT();                     // 이런 메서드 없음
PlayerPrefs.GetBoolWithDefault(key, true);    // 이런 오버로드 없음
```
규칙:
- 외부 SDK 호출은 **Day 1 SDK_VERSIONS.md에 박힌 버전**의 공식 샘플 대조
- "그럴듯한 메서드명"이 보이면 즉시 IntelliSense / 공식 문서 검증
- Unity 6에서 옛날 패턴(2021 이하) 의심
- Unity Haptic은 표준 API 없음 → **Lofelt Nice Vibrations**만 사용

### 2-5. Inspector 연결 검증 (Unity 특화)
- `[SerializeField]` 추가 시 어느 GameObject/프리팹에 연결해야 하는지 **반드시 명시**
- 새 컴포넌트 생성 시 다음 체크리스트 출력:
  ```
  [ ] 어떤 씬/프리팹에 부착하는가?
  [ ] 인스펙터에서 연결할 필드: <리스트>
  [ ] Missing Reference 시 동작: <fallback 또는 명확한 예외>
  ```
- null 방어 코드는 넣되 **null 상태를 정상 흐름으로 숨기지 말 것** (조용한 실패 금지)
- Cursor/Claude는 씬 상태를 직접 못 보므로 **추정 금지** — 정수가 연결 확인

---

## 3. 코드 품질 — 절대 규칙

### 3-1. 데드코드 0
- 사용처 없는 함수/클래스/필드/`using` 즉시 삭제
- **주석 처리된 코드 금지** (git이 히스토리 보관함)
- `TODO` 단독 금지 — 적을 거면 `TODO(#issue): 구체 설명`
- public이지만 외부 사용 0인 멤버 → `internal`/`private` 강등 → 그래도 사용처 0이면 삭제

### 3-2. 중복 코드 0
- **동일 로직 2회 등장 = 즉시 추출** (3회 기다리지 말 것 — 4주 일정에선 너무 늦음)
- 동일 매직 넘버 2회 = `const` 또는 ScriptableObject로
- 유사 데이터 클래스 2개 = 공통 베이스/인터페이스로 통합
- "비슷한데 살짝 다름" = 추상화 신호 — 분기점만 파라미터로 빼고 본체 통합

### 3-3. 매직 넘버/문자열 금지
```csharp
// 나쁨
if (level > 5) ShowAd();
PlayerPrefs.GetInt("coin");

// 좋음
const int TutorialEndLevel = 5;
if (level > TutorialEndLevel) ShowAd();
PlayerPrefs.GetInt(PrefsKey.Coin);
```
- 레벨/타이밍/사이즈 상수는 `Boxy.App.GameplayConstants` 또는 SO
- 키 문자열은 `PrefsKey`, `EventKey`, `AnalyticsKey` 정적 클래스

### 3-4. 주석은 WHY만
- WHAT은 코드/네이밍이 말함 — 절대 주석 X
- WHY만 적음: 왜 이 우회인가, 왜 이 순서인가, 어떤 SDK 버그 회피인가
- doc comment는 Mound.* 모듈의 외부 노출 API에만
- 현재 작업/PR/이슈 번호 박지 말 것 (코드 따라 늙음)

---

## 4. Unity 특화 규칙

### 4-1. 금지 패턴

| 패턴 | 이유 | 대체 |
|---|---|---|
| `GameObject.Find`, `FindObjectOfType` | 성능 + 결합도 | `[SerializeField]` 또는 DI |
| `Camera.main`을 Update에서 호출 | 매 프레임 lookup | Awake에서 캐싱 |
| `public` 필드 | 캡슐화 깨짐 | `[SerializeField] private` |
| Singleton 남용 | 테스트/재사용 불가 | `Mound.Core.EventBus` |
| Update의 `string +`, LINQ, 박싱 | GC alloc → 프레임 드랍 | 캐싱 / StringBuilder / 배열 |
| `Resources.Load` + Addressables 혼용 | 일관성/번들링 깨짐 | 한 가지로 통일 |
| 코루틴 + async/await 혼용 | 취소/예외 처리 불일관 | async/await + CancellationToken |
| `Object.Instantiate`/`Destroy` 반복 | GC + 인스턴스화 비용 | `Mound.Core.ObjectPool` |

### 4-2. SerializeField 패턴
```csharp
public class HintButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] AdRewardConfig config;

    void Awake() => button.onClick.AddListener(OnClick);
    void OnDestroy() => button.onClick.RemoveListener(OnClick);

    void OnClick() { /* ... */ }
}
```
- `private` 키워드 생략 OK (`[SerializeField]` 다음 필드는 기본 private)
- 이벤트 등록은 Awake/OnEnable, 해제는 OnDestroy/OnDisable에서 **반드시 짝**

### 4-3. ScriptableObject 우선
- 게임 데이터(레벨/테마/아이템/광고 설정)는 SO
- 하드코딩 배열/리스트로 인스펙터 노출 X — SO로 빼야 디자이너/툴이 편집 가능

### 4-4. EventBus 사용 룰
**Command성 작업은 EventBus 금지. 상태 변경 결과 알림에만 허용.**

✅ 허용 (Notification — 일어난 사실을 알림):
- `LevelClearedEvent`, `ItemPlacedEvent`, `CurrencyChangedEvent`, `AchievementUnlockedEvent`

❌ 금지 (Command — 무엇을 해라):
- `StartLevelEvent`, `BuyItemEvent`, `SaveGameEvent`, `ShowAdEvent`

**왜**: Command를 이벤트로 던지면 (1) 누가 처리하는지 추적 불가, (2) 처리 순서 보장 X, (3) 실패 시 응답 없음, (4) 단위 테스트 어려움. **명령은 명시적 Service 호출** (`await iapProvider.PurchaseAsync(...)`, `levelManager.StartLevel(id)` 등).

---

## 5. 모듈 경계

```
Boxy.App  ──►  Mound.*  ──►  외부 SDK (인터페이스 경유)
역방향 의존 절대 금지 (asmdef로 컴파일 단계에서 차단)
```

### 5-1. Mound.* (재사용 자산)
- `Mound.Core`, `Mound.Monetization`, `Mound.Analytics`, `Mound.UI`, `Mound.Localization`
- **`Boxy.App` 참조 절대 금지** — 다음 게임에 그대로 들어가야 함
- 변경 시 자문: "이게 Boxy Sort/Boxy Cafe에서도 말이 되는가?" 아니라면 `Boxy.App`으로 빼라

### 5-2. Boxy.App (게임별)
- `Boxy.App.Gameplay`, `Boxy.App.Levels`, `Boxy.App.Themes`, `Boxy.App.Tutorial`
- Mound.* 자유 사용
- 다른 게임 코드/이름 섞지 말 것

### 5-3. SDK 격리 (광고/IAP/분석/Crashlytics)
- AdMob/AppLovin/Firebase/IAP 코드를 `Boxy.App`에 직접 넣지 말 것
- **외부 SDK 타입을 `Boxy.App`에 노출 금지** (`AdValue`, `Purchase` 등 SDK 클래스 노출 X)
- `Mound.Monetization`에서 인터페이스로 한 번 감쌈:
  ```csharp
  public interface IAdProvider {
      UniTask<bool> ShowRewarded(string placement, CancellationToken ct);
  }
  ```
- **SDK 초기화 실패 시 게임 플레이는 계속 가능해야 함**
- 광고 로드 실패는 실패 상태로 기록하되 **게임 진행을 막지 말 것**

### 5-4. asmdef 강제
- `Mound.Core.asmdef`는 외부 의존 0
- `Boxy.App.asmdef`는 `Mound.*` 참조만
- asmdef 없이 폴더만 분리하면 의존 방향 보장 안 됨 — **Day 1 셋업 필수**

---

## 6. Map / Dictionary 직접 사용 최소화

### 6-1. 원칙
`Dictionary<K, V>`를 public/internal API에 노출 금지. 도메인 타입으로 wrap.
**이유**: 외부에서 임의 키 추가/삭제 가능 → 불변식 깨짐. 키 의미가 코드에 안 드러남.

### 6-2. 안티 패턴
```csharp
// 나쁨
public Dictionary<int, LevelData> Levels;
public Dictionary<string, int> Inventory;
```

### 6-3. 권장 패턴
```csharp
public readonly struct LevelId : IEquatable<LevelId>
{
    public readonly int Value;
    public LevelId(int v) { Value = v; }
    public bool Equals(LevelId other) => Value == other.Value;
    public override int GetHashCode() => Value;
}

public sealed class LevelRepository
{
    readonly Dictionary<LevelId, LevelData> levels;

    public LevelData Get(LevelId id) => levels[id];
    public bool TryGet(LevelId id, out LevelData data) => levels.TryGetValue(id, out data);
    public IReadOnlyCollection<LevelData> All => levels.Values;
}
```

### 6-4. enum 키는 배열로
```csharp
// 나쁨
Dictionary<ItemType, Sprite> sprites;

// 좋음
Sprite[] sprites; // index = (int)ItemType — 캐시 친화 + alloc 0
Sprite Get(ItemType t) => sprites[(int)t];
```

### 6-5. Dictionary 허용 케이스 (이때만)
- 메서드 **내부 로컬** 자료구조 (스코프 안에서 끝남)
- 직렬화 직전 단계 (JSON 변환 buffer)
- 외부 SDK가 강제하는 경우

이외엔 Repository/Registry/도메인 타입으로 감쌈.

---

## 7. 세이브 데이터 / 마이그레이션

### 7-1. 절대 규칙
- **저장 키 삭제/변경 금지** — deprecate만 가능, 새 키는 추가
- **저장 구조 변경 시 `SaveVersion` 필드 증가**
- **로드 실패 시 기본값 복구** — 절대 게임 시작 막히지 않게
- **세이브 손상이 게임 실행 차단으로 이어지면 안 됨**

### 7-2. 스키마 변경 시 마이그레이션 동반
```csharp
// 나쁨: 필드명만 바꿈 → 기존 유저 진행도 날림
[Serializable] class SaveData { public int currentLevel; } // 이전: level

// 좋음: 마이그레이션 코드 같이 작성
public static class SaveMigrator {
    public static SaveData Migrate(JObject raw) {
        int version = raw["SaveVersion"]?.Value<int>() ?? 1;
        if (version < 2) raw["currentLevel"] ??= raw["level"]; // v1 → v2
        if (version < 3) /* ... */;
        return raw.ToObject<SaveData>();
    }
}
```

### 7-3. 출시 후 첫 업데이트 마이그레이션 테스트 케이스 필수
- v1.0 세이브 파일을 v1.1 빌드에서 로드 → 정상 동작 확인
- 이게 안 되면 출시 후 평점 죽음

---

## 8. 빌드 환경 분리 (CI/CD)

> **2-tier 전략** — 결정 기록: `decisions/2026-04-29-09-app-id-strategy-2tier.md`
> 솔로 개발 + 4주 일정 부담을 고려하여 `staging`은 폐지. closed test 기능은 production 앱의 TestFlight External / Play Closed Testing 트랙으로 흡수.

### 8-1. 2개 환경 강제
| 환경 | 용도 | 배포 채널 |
|---|---|---|
| `dev` | 정수 디바이스 매일 빌드 검증, 디버깅 | 직접 설치 (TestFlight Internal 또는 ad-hoc) |
| `production` | closed test → 실배포 | TestFlight Internal/External, Play Internal/Closed Testing → App Store/Play Production |

### 8-2. 환경별 분기 항목 (모두 필수)

| 항목 | dev | production |
|---|---|---|
| Bundle ID | `com.mound.boxy.dev` | `com.mound.boxy` |
| Build Type | Development + AllowDebugging | Release |
| AdMob ID | **테스트 ID 강제** | 실제 ID |
| AdMob 테스트 디바이스 | 정수 디바이스 ID 등록 | 정수 + 외부 테스터 디바이스 ID 등록 (closed test 기간) |
| AppLovin Mediation Key | 테스트 키 | 실제 키 |
| Firebase 프로젝트 | `boxy-dev` | `boxy-prod` |
| Firebase Analytics | `setAnalyticsCollectionEnabled(false)` | 활성 |
| AppsFlyer Dev Key | 분리 | 실제 키 |
| IAP 상품 | 샌드박스 | TestFlight=자동 Sandbox / App Store=실서버 |
| Crashlytics | 비활성 또는 분리 | 활성 |
| 로그 레벨 | Debug | Warning+ |

**위반 시 발생하는 사고**:
- AdMob 실 ID로 dev 빌드 → AdMob 정책 위반 → 계정 정지
- Firebase 단일 프로젝트 → 분석 데이터 오염 → KPI 측정 불가
- IAP 실서버로 테스트 → 환불 사고
- closed test 기간 광고 트래픽이 prod 매출 KPI에 섞임 → 디바이스 ID 필터로 분리 필수

### 8-3. Unity 셋업
- **Build Profile** (Unity 6) 또는 ScriptingDefineSymbols로 분기:
  - `BOXY_DEV`, `BOXY_PROD`
- 환경별 `EnvironmentConfig.asset` (ScriptableObject) 1개씩 (`EnvironmentConfig-dev`, `EnvironmentConfig-prod`)
  - 키 정보 보관, **gitignored**
- 빌드 진입점: `Boxy.Editor.BoxyBuilder.BuildDevAndroid|BuildDevIOS|BuildProductionAndroid|BuildProductionIOS`

### 8-4. 출시 게이트 (절대 우회 금지)
```
Editor (dev 빌드)
  ↓ § 12-1 검증 통과 (정수 디바이스에서 5레벨 완주)
production 빌드
  ↓ TestFlight Internal / Play Internal Testing (정수 본인 검증)
  ↓ TestFlight External / Play Closed Testing — 5명 × 14일 closed test
  ↓ § 12-3 출시 직전 검증 통과
App Store / Play Production
```

**dev 빌드 검증 없이 production 빌드로 점프 금지. closed test 14일 미완료 시 release 절대 금지.**

---

## 9. 시크릿 / 민감 정보

### 9-1. 절대 금지
```csharp
// 절대 금지 — 코드에 하드코딩
const string AdMobAppId = "ca-app-pub-1234567890123456~1234567890";
const string FirebaseApiKey = "AIza...";
const string AppsFlyerDevKey = "abc123...";
```

### 9-2. 보관 위치
- API 키/토큰은 환경별 `EnvironmentConfig.asset` ScriptableObject
- **`.gitignore`에 EnvironmentConfig.asset 추가**
- `EnvironmentConfig.asset.example` (더미 값)만 커밋
- 또는 환경 변수 / Unity Cloud Build 시크릿 변수

### 9-3. 코드 리뷰 시
- "키처럼 보이는 문자열" 정규식 검색 (`AIza`, `ca-app-pub`, `pk_`, `sk_` 등)
- 발견 즉시 git history에서 제거 + 키 로테이션

---

## 10. 성능 예산

### 10-1. 목표 수치 (출시 전 검증)
| 지표 | 목표 |
|---|---|
| FPS | 60 |
| 로딩 시간 (콜드 스타트 → 메인 메뉴) | ≤ 3초 |
| 게임 중 GC Alloc/frame | 0B 목표 |
| APK 크기 | ≤ 60MB |
| 텍스처 크기 | 1024px 이하 (초과 시 사유 명시) |

### 10-2. 에셋 import 규칙
- 텍스처: 압축 ASTC (Android) / ASTC (iOS), Mip Map은 UI엔 끔
- 사운드: SFX는 PCM (짧고 빠름), BGM은 Vorbis
- 폰트: Pretendard만, Inter 빼서 -3~5MB 절약

### 10-3. 프로파일링
- Week 4 Day 22~23에 Unity Profiler로 1회 측정
- GC Alloc/frame > 0이면 원인 함수 추적
- Frame time > 16.6ms면 즉시 수정

---

## 11. 에러 처리 / 비결정성

### 11-1. 에러 처리
- **빈 catch 블록 절대 금지** (`catch { }` 발견 즉시 거절)
- SDK 호출은 `try/catch + 폴백 동작`:
  - 광고 로드 실패 → 게임 진행은 계속, 광고 트리거 비활성화
  - IAP 실패 → 결제 실패/검증 실패/중복 구매 케이스 분리 처리
  - 네트워크 timeout / 오프라인 / 응답 깨짐 분리
- catch한 예외는 **반드시 로그** (Crashlytics에 non-fatal 보고)

### 11-2. 비결정성 회피
- **async 결과 무시는 명시적 표기**: `_ = SomeAsync();` (의도임을 코드에 박음)
- `async void` 금지 (Unity 이벤트 핸들러 제외)
- Awake/Start/OnEnable 호출 순서에 의존하는 코드 금지
- Singleton 초기화 타이밍 의존 금지 — 명시적 초기화 순서 (`GameBootstrapper`)
- 멀티 씬 로딩 시 의존성 명시적으로 await

### 11-3. 플랫폼 분기
- `#if UNITY_IOS` / `#if UNITY_ANDROID` 분기 시 **양쪽 다 검증**
- 햅틱, IAP, 광고, 권한, 파일 경로는 플랫폼 차이 큼 — 양쪽 디바이스 빌드 필수
- Editor에서만 돌려보고 끝내지 말 것 — **iOS 실기 + Android 실기 둘 다 빌드 검증**

### 11-4. 보안 / 안정성 (출시 차단 P0)
출시 전 다음 5건 미해결 시 **출시 보류**:

1. **IAP 영수증 검증** — `Mound.Monetization.IReceiptValidator` Port + Apple/Google 서버 직검증 필수.
   - Apple sandbox: `https://sandbox.itunes.apple.com/verifyReceipt`
   - Google: `androidpublisher.googleapis.com/androidpublisher/v3/applications/.../purchases/products/...`
   - 중복 지급 방지: `originalTransactionId` (iOS) / `purchaseToken` (Android) 추적
   - 자체 서버 없으면 클라이언트 직검증 — 캐주얼 99% 결제 우회 차단. ARPDAU $0.15+ 도달 후 자체 서버 검증으로 강화 (v1.1)

2. **세이브 무결성** — `BoxySaveData`에 다음 필드 + 직렬화/역직렬화 시 검증:
   - `schemaVersion` (이미 있음 — `saveVersion`)
   - `checksum` (HMAC-SHA256 with device key)
   - `createdAt` / `updatedAt` (Unix sec)
   - `migrationHistory` (적용된 v0→v1 등 기록)
   - `backupSave` (이전 세이브 저장 — 변조/손상 감지 시 fallback)
   - 변조 감지 시 `Debug.LogError` + `crashReporter.Report(non-fatal)` + backup 로드 (없으면 기본값 새로 시작)

3. **크래시 텔레메트리** — `Mound.Core.Diagnostics.ICrashReporter` + `FirebaseCrashlyticsProvider` + `CompositeCrashReporter`. SDK 미통합 시 `NullCrashReporter`. catch한 모든 비치명 예외 `crashReporter.Report(e, contextDict)` 호출.

4. **SDK 콜백 메인 스레드 디스패치** — AppLovin / Firebase / AppsFlyer 네이티브 콜백은 메인 스레드 보장 X. Adapter에서 `UnityMainThreadDispatcher` 또는 `UniTask.SwitchToMainThread()` 강제. Unity API 호출 (PlayerPrefs, Object.Destroy 등) 백그라운드 스레드에서 → 즉시 크래시.

5. **Remote Config / Feature Flag** — `Mound.Core.IRemoteConfigProvider` Port + `FirebaseRemoteConfigProvider`. 광고 빈도 / 보상량 / 난이도 곡선 / 일일 보상 등 **코드에 박지 X**. 앱 심사 없이 밸런스 조정 = 캐주얼 게임 운영의 절반.

**왜 P0인가**: IAP 검증 없으면 출시 1주일 안에 결제 우회. 세이브 무결성 없으면 평점 1점 리뷰 폭격. Crashlytics 없으면 출시 후 어디서 무엇이 깨졌는지 모름. SDK 콜백 스레드 안전성 위반 = 무작위 크래시.

---

## 12. 검증 루프 — 변경 후 매번

### 12-1. 코드 생성 직후 (생략 금지)
1. **컴파일**: Unity 콘솔 에러 0
2. **워닝**: 신규 워닝 0 (기존 워닝 카운트 늘리지 말 것)
3. **Missing Reference**: 인스펙터 노란 삼각형 0
4. **Play Mode 1회**: 튜토리얼 5레벨 끝까지 진행
5. **변경 모듈 회귀**: EditMode/PlayMode 테스트 있으면 실행
6. **빌드 사이즈**: 5MB 이상 증가 시 즉시 원인 추적 (asset/font/SDK 의심)

### 12-2. 손대지 않은 영역 스모크
- 광고 보상 시청 → 보상 지급
- 되돌리기 3회 소진 후 광고 트리거
- 일시정지 → 재개 → 같은 상태
- 앱 강제종료 → 재실행 → 진행도 유지 (SaveSystem)

### 12-3. 출시 직전 (Week 4)
- Crashlytics 강제 크래시 1회 → 대시보드 도착 확인
- AppsFlyer 설치/구매 이벤트 도착
- IAP 샌드박스 3종 모두 결제 성공
- ATT 프롬프트 (iOS v1.1)
- GDPR 동의 화면 (EU 시뮬)
- iOS 실기 + Android 실기 양쪽 빌드 + 5레벨 완주
- 세이브 마이그레이션 (이전 빌드 세이브 파일 → 신 빌드 로드)

### 12-4. 검증 실패 시
- **절대 금지**: "다음에 고치자" 하고 넘어가기
- **반드시**: 발견 즉시 수정 → 검증 루프 재실행

---

## 13. AI 코드 생성 자기 점검 체크리스트

체크리스트는 **트리거 기반**으로 분리. 모든 항목을 매번 점검 X — 작업 종류에 따라 해당 그룹만.

### 13-1. 매번 (3개 — 코드 생성 직후 항상)
1. ☐ "완료"라 말하기 전에 컴파일/Play Mode 직접 검증했는가? (§0-1)
2. ☐ 외부 API/메서드/패키지가 실제 존재하는지 확인했는가? (§2-4 환각 검증)
3. ☐ 새 매니저/싱글톤/구조를 임의로 추가했는가? (§2-1)

### 13-2. 신규 파일 생성 시 (+4개)
4. ☐ 한 기능당 신규 파일 3개 이내인가? (§2-2)
5. ☐ 한 턴에 5개 이상 파일을 수정했는가? (§2-3)
6. ☐ `[SerializeField]` 추가 시 인스펙터 연결 가이드를 출력했는가? (§2-5)
7. ☐ `Mound.*` asmdef가 `Boxy.App`을 참조하는가? (§5-1, 역방향 즉시 거절)

### 13-3. SDK / 세이브 / 시크릿 변경 시 (+5개)
8. ☐ `Dictionary<,>`를 public/internal API로 노출했는가? (§6-1)
9. ☐ 세이브 스키마 변경 시 마이그레이션이 있는가? (§7)
10. ☐ async 결과 무시는 명시적 `_ =` 인가? `catch { }` 빈 블록은 없는가? (§11)
11. ☐ API 키/시크릿이 코드에 박혀있지 않은가? (§9)
12. ☐ iOS/Android 양쪽 분기를 처리했는가? (§11-3)

### 13-4. 큰 변경 / 리팩터링 시 (+3개)
13. ☐ 같은 로직이 이미 코드베이스에 있는가? (있으면 재사용 — §3-2)
14. ☐ public 필드 / 매직 넘버/문자열 / Update의 GC alloc 패턴이 있는가? (§3-3, §4-1)
15. ☐ 사용처 0인 멤버, 주석 처리된 코드, WHAT 설명 주석이 있는가? (§3-1, §3-4)

**자동 보조**: §19 기술적 강제 스크립트가 13-2 일부 + 13-3 일부를 자동 검출 — 사람이 잊어도 커밋 차단됨.

---

## 14. 디자인 시스템 적용 (UI 코드 생성 시)

1. `~/.mound/mound-design-system.md` 참조 — 토큰/컴포넌트 명세 따름
2. 액센트 컬러: `#FFD60A` / 액센트 위 텍스트: 검정 (대비 4.5+)
3. 단위: **px** (sp/dp 변환 금지 — USS는 px만 인식)
4. 햅틱: **Lofelt Nice Vibrations** API (Light/Medium/Heavy)
5. i18n: **Unity Localization Package** 키-값 (커스텀 JSON 포맷 X)
6. 아이콘: Lucide, stroke 2px, 24px 기본
7. 모션 듀레이션: 마이크로 100ms / 표준 250ms / 강조 500ms / 페이지 300ms

---

## 15. 커밋 / 작업 단위

- 커밋은 작은 단위, 한 가지 변경
- 형식: `[모듈] 동사로 시작하는 요약` (예: `[Gameplay] 회전 트윈 0.15s 적용`)
- 검증 루프 § 12-1 통과 후 커밋
- 푸시는 § 12-2까지 통과 후

---

## 16. 스코프 보호 — 절대 추가 금지 (v1.0)

기획서 B-6 동기화. 다음을 추가하려는 충동이 들면 즉시 거절:

- ❌ 로그인/계정/클라우드 세이브
- ❌ 멀티플레이/PvP/채팅/SNS 공유
- ❌ 꾸미기/일일 퍼즐/랭킹/리더보드
- ❌ 가챠/뽑기/랜덤박스 (영구)
- ❌ 복잡한 스토리/대사 시스템
- ❌ "있으면 좋을 것 같은" 기능 일체

v1.1 백로그에 1줄 적고 넘어갈 것.

추가로 §18 Feature Flag로 감싸면 v1.0 안에 실험 형태로 검증 가능 (off 기본).

---

## 17. 결정 기록 — 사후 추적 가능성

### 17-1. 강제 트리거 (4종 변경 시 필수)
다음 중 하나를 변경하면 **변경 전에** `decisions/YYYY-MM-DD-주제.md` 1장 작성:

1. **구조 변경**: 모듈 분리/통합, asmdef 추가/삭제, 폴더 구조 재편
2. **SDK 교체**: 광고/IAP/분석/Crashlytics 중 어느 하나라도 다른 SDK로 변경
3. **수익화 방식 변경**: 보상형/인터스티셜/배너 빈도 또는 IAP 가격/상품 변경
4. **KPI 기준 변경**: D1/D7/ARPDAU 목표치 수정

**기록 없으면 변경 금지.** 작은 변경(버그 수정, UI 폴리싱)은 트리거 아님.

### 17-2. 결정 기록 템플릿
```markdown
# YYYY-MM-DD — 결정 주제

## Context
무엇을 하려 했나, 왜 변경이 필요한가

## Options
- A: ...
- B: ...
- C: ...

## Decision
선택 및 그 이유 (3줄 내)

## Consequences
- 좋은 것: ...
- 나쁜 것 / 위험: ...

## Revisit when
이 결정을 재검토해야 할 조건 (예: D7 < 8% 시)
```

### 17-3. 운영
- `decisions/` 폴더는 git 커밋 (히스토리 자산)
- 결정 후 며칠 지나 무엇을/왜 바꿨는지 기억 안 남는 사고 방지
- AI가 "전엔 A로 했는데 B로 바꾸자" 하면 **이 폴더부터 검색** 후 답

---

## 18. 실험 코드 / Feature Flag

### 18-1. 원칙
A/B 테스트, 광고 빈도 조정, 새 메커닉 시도 등 **실험적 코드**는 Feature Flag로 감싼다. 실패 시 즉시 off — 코드베이스 오염 방지.

### 18-2. 구현 (간단 버전)
```csharp
// Mound.Core/FeatureFlags.cs (ScriptableObject)
[CreateAssetMenu(menuName = "Mound/FeatureFlags")]
public sealed class FeatureFlags : ScriptableObject
{
    public bool Experiment_NewAdTiming;
    public bool Experiment_HardCorePack;
    public bool Experiment_LongerTutorial;
}

// 사용처
if (flags.Experiment_NewAdTiming) {
    ShowInterstitialAfterLevel();   // 실험
} else {
    ShowInterstitialEvery90s();     // 기존 (안전 기본값)
}
```

### 18-3. 규칙
- **기존 로직을 직접 수정하지 말 것** — 분기 추가만
- 실험 실패 시 플래그 off → 즉시 롤백 가능 상태 유지
- 실험 성공해서 기본 동작 됐으면: 다음 빌드에서 플래그 + 옛 분기 제거 (데드코드 §3-1)
- 한 번에 활성 실험 **3개 초과 금지** — 서로 영향 측정 불가능

### 18-4. 결정 기록 연동
모든 실험은 §17 결정 기록 동반. `decisions/` 안에 실험 가설/측정 지표/종료 조건 명시.

---

## 19. 기술적 강제 (Pre-commit / Static Analysis)

### 19-1. 절차적 강제의 한계
§0-4 세션 시작 강제 로드, §13 자기 점검은 **AI 자기 보고에 의존** — 거짓 보고 가능. 보완:

### 19-2. Pre-commit Hook
- 위치: `scripts/check-violations.sh`
- 설치: `scripts/setup-hooks.sh` (1회 실행)
- 자동 검출:
  - §9-1 하드코딩 API 키 (AIza... / ca-app-pub-... / sk_live_... 등)
  - §4-1 `GameObject.Find` / `FindObjectOfType`
  - §11-1 빈 `catch { }` 블록
  - §6-1 `public Dictionary` / `public List` / `public HashSet` 노출
  - §11-2 `async void` (경고)
  - §3-1 `TODO` 단독 (경고)
- **에러 발견 시 커밋 차단**
- 수동 검사: `scripts/check-violations.sh all` (전체 .cs 스캔)

### 19-3. .editorconfig
- 위치: 프로젝트 루트 `.editorconfig`
- IDE/Roslyn 자동 적용:
  - IDE0051/IDE0052: 미사용 private 멤버 → 경고 (§3-1)
  - CS0168/CS0219: 미사용 변수 → 에러
  - 명명 규칙 (PascalCase const 등)

### 19-4. .gitignore
- `EnvironmentConfig.asset` (시크릿 보관) 자동 제외 (§9-2)
- `google-services.json`, `GoogleService-Info.plist`, keystore 자동 제외
- `.env*` 자동 제외

### 19-5. 한계
- Pre-commit hook은 **로컬 검사** — CI에서도 한 번 더 돌릴 것 (Unity Cloud Build prebuild 스텝)
- AI가 검사 회피하려고 패턴을 살짝 비틀면 못 잡음 — 정수가 코드 리뷰 시 한 번 더 봄
- 완벽한 강제는 아니지만 **반복 위반은 자동 차단**

---

## 20. v1.1 출시 후 리팩터링 백로그

상세 plan: `decisions/2026-04-30-21-v1.1-refactor-plan.md`

v1.0 출시 후 D7 ≥ 10% 도달 시 1-2주 작업으로 진행. **출시 직전 절대 도입 X (회귀 risk).**

### 20-1. UniTask (1일)
- 패키지: `com.cysharp.unitask` (UPM Git URL)
- 목적: 광고/IAP/Consent 비동기 흐름 정리. GC alloc 감소.
- 우선 대상: `IAdProvider` / `IIapProvider` / `IConsentProvider` / `IReceiptValidator` Task → UniTask 교체
- v1.0 준비층: `Mound.Core.MainThreadDispatcher` (UniTask.SwitchToMainThread 대체)

### 20-2. Addressables (2-3일)
- 패키지: `com.unity.addressables` 2.6.0+
- 목적: APK -10~30%, 메모리 -20%, 동적 컨텐츠 (시즌 한정 스킨) 가능
- 우선 대상: `Resources/Mascot/`, `Resources/Icons/` → Addressables Group 분리
- v1.0 준비층: `Mound.Core.IAssetProvider` + `ResourcesAssetProvider` (Addressables 자리)

### 20-3. VContainer (3-5일)
- 패키지: `jp.hadashikick.vcontainer` 1.16.0+
- 목적: Service Locator 누수 (BoxyBootstrap.Instance 직접 참조 10+곳) 제거. 단위 테스트 mock 주입.
- 우선 대상: `BoxyBootstrap` Provider 인스턴스화 → `RootLifetimeScope.Configure(IContainerBuilder)` 이전
- 절대 출시 직전 X — 회귀 risk 큼

### 20-4. v1.1 KPI 게이트
다음 모두 충족 시에만 P2 진행:
- D7 retention ≥ 10% (`boxy-final-checklist.md` §KPI)
- 평균 평점 ≥ 4.0
- ARPDAU ≥ $0.10
- 1주일 동안 critical 크래시 발생률 < 1%

미달 시 P2 보류 + Boxy Sort 즉시 착수 (시리즈 IP 분산 전략).
