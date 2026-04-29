# 2026-04-29 — Day 1 자동 설치 진행 상황

## ✅ 완료 (AI가 직접 설치)

### Unity Hub
- 위치: `/Applications/Unity Hub.app`
- 방법: `brew install --cask unity-hub`
- 버전: 3.17.3

### Python 3.11
- 위치: `/opt/homebrew/Cellar/python@3.11/3.11.14_3`
- 방법: `brew install python@3.11`
- 이유: Coplay/Unity MCP 서버 의존성

### uv / uvx
- 위치: `/Users/jeki/.local/bin/uv`, `uvx`
- 버전: 0.11.8
- 방법: `curl -LsSf https://astral.sh/uv/install.sh | sh`

### Claude Code MCP 서버 등록 (3개)
| 서버 | 상태 | 비고 |
|---|---|---|
| stitch | ✓ Connected | UI 시안 생성 (이미 사용 중) |
| coplay-mcp | ✓ Connected | Unity MCP 서버 — Unity 가동 후 활용 |
| scenario | ⚠️ **인증 필요** | OAuth — 처음 사용 시 브라우저 자동 열림 |

추가 명령:
```bash
claude mcp add --transport http scenario https://mcp.scenario.com/mcp
claude mcp add --scope user --transport stdio coplay-mcp \
  --env MCP_TOOL_TIMEOUT=720000 \
  -- /Users/jeki/.local/bin/uvx --python ">=3.11" coplay-mcp-server@latest
```

## ⏳ 진행 중 (백그라운드)

### Unity Editor 6000.3.14f1 ✅ 완료
- 위치: `/Applications/Unity/Hub/Editor/6000.3.14f1/Unity.app`
- 크기: 20GB
- 모듈: AndroidPlayer, iOSSupport (둘 다 설치됨)
- 다음 단계: 정수가 Unity Hub 실행 → Unity ID 로그인 → Personal License 활성화

### Firebase CLI + 3개 프로젝트 ✅ 완료
- Firebase CLI 15.15.0 설치 (`npm install -g firebase-tools`)
- 로그인: `dugout26.gm@gmail.com` (기존 계정 활용)
- 생성된 프로젝트:
  | 프로젝트 ID | Display Name | Bundle ID | App ID |
  |---|---|---|---|
  | `boxy-mound-dev` | Boxy Dev | com.mound.boxy.dev | 1:32152825590:android:c2d085f95c7eb0e609c6d7 |
  | `boxy-mound-stg` | Boxy Staging | com.mound.boxy.stg | 1:78917800783:android:66caf6b26a12864c8beb3a |
  | `boxy-mound-prod` | Boxy Production | com.mound.boxy | 1:117738831986:android:abdfc6ad6d8881f07dc2eb |
- google-services.json 3개 다운로드: `secrets/google-services-{dev,stg,prod}.json` (gitignored)
- Firebase Console:
  - https://console.firebase.google.com/project/boxy-mound-dev/overview
  - https://console.firebase.google.com/project/boxy-mound-stg/overview
  - https://console.firebase.google.com/project/boxy-mound-prod/overview

> Crashlytics는 SDK 첫 크래시 보고 시 자동 활성. Analytics는 프로젝트 생성 시 자동 활성.

## 🛑 정수만 가능 (AI 불가)

### 1. Unity ID 로그인 (Unity Hub)
- 이유: Unity Personal License는 Unity ID 인증 후 자동 부여 — Hub GUI에서만 가능
- 절차: Unity Hub 앱 실행 → Sign in → Unity ID 생성 또는 로그인

### 2. Asset Store 구매 (재정 거래)
재정 거래는 자동화 불가. AI가 안전상 거절.

| 자산 | 가격 | 링크 |
|---|---|---|
| Lofelt Nice Vibrations | $20 | https://assetstore.unity.com/packages/tools/audio/nice-vibrations-haptic-feedback-for-mobile-and-gamepads-246159 |
| 패킹 퍼즐 템플릿 | $30~50 | Asset Store에서 "pack puzzle" / "fit puzzle" / "block packing" 검색 후 비교 |
| Mobile Monetization Pro V2 | ~$50 | Asset Store에서 검색 |

### 3. Scenario MCP OAuth 인증
- Claude Code 다음 사용 시 Scenario 도구 호출하면 OAuth 자동으로 시작
- 또는 직접 인증: scenario.com 가입 → 다음 명령 실행 시 브라우저 자동 열림
- API key 방식 선호 시: `claude mcp remove scenario` 후 재추가:
  ```bash
  claude mcp add --transport http scenario https://mcp.scenario.com/mcp \
    --header "Authorization: Basic <base64-encoded-key:secret>"
  ```

### 4. Unity-MCP 패키지 (Unity 내부 설치)
- Unity Editor 다운로드 + 라이선스 활성화 후
- Unity 메뉴: Window → Package Manager → + → "Add package from git URL" 
- URL: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#beta`
- 그 다음: Window → MCP for Unity → Start Server
- → Claude Code의 coplay-mcp가 Unity 내부 서버에 연결됨

### 5. 기타 계정 생성 (사업자 인증)
- Google Play Console ($25, 1~3일 검증)
- AdMob (Mound 사업자 정보)
- AppLovin MAX
- AppsFlyer (무료 티어)
- Firebase (3개 프로젝트: dev/stg/prod)

## 다음 단계 (정수가 시작)

**가장 먼저** (다른 작업 차단):
1. **상표 검색** — `decisions/2026-04-29-02-trademark-search.md` 절차대로

**Unity Editor 다운로드 끝나면** (백그라운드 진행):
2. Unity Hub 실행 → Unity ID 로그인
3. 빈 Unity 프로젝트 생성 (테스트용) → 디자인 시스템 호환성 검증 (Primary Button 빌드)
4. Window → Package Manager → unity-mcp git URL 추가 → MCP 서버 시작

**병렬 가능**:
5. Top 50 패킹 퍼즐 직접 플레이
6. Google Play Console / AdMob / AppLovin / AppsFlyer / Firebase 계정
7. Asset Store에서 Lofelt + 패킹 퍼즐 템플릿 비교 후 구매

## Revisit when
- Unity Editor 다운로드 완료 (이 파일에 완료 시각 추가 필요)
- 상표 검색 결과 확정
- 첫 빈 Unity 프로젝트 빌드 검증 통과
