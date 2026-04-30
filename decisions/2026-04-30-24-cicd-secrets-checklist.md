# 2026-04-30 — CI/CD GitHub Secrets 체크리스트

> CI 파이프라인(`.github/workflows/`)이 동작하려면 GitHub Repo Secrets에 키 18개 등록 필요.
> AI가 자동 등록 가능한 8개는 이미 등록 완료 (2026-04-30). 나머지는 정수가 직접.
> 위치: `https://github.com/dugout26/boxy/settings/secrets/actions`

## ✅ AI 자동 등록 완료 (8개)

이 키들은 로컬 `secrets/` 폴더 또는 알려진 값에서 자동 추출/등록.

| Secret | 출처 | 용도 |
|---|---|---|
| `ASC_KEY_ID` | secrets/asc-credentials.env | App Store Connect API key ID |
| `ASC_ISSUER_ID` | secrets/asc-credentials.env | App Store Connect Issuer ID |
| `TEAM_ID` | secrets/asc-credentials.env | Apple Developer Team ID |
| `ASC_KEY_BASE64` | base64(secrets/AuthKey_B4ZS7Y9XXM.p8) | iOS 업로드용 .p8 |
| `SUPABASE_ACCESS_TOKEN` | sbp_... | Edge Function 배포 (필요 시) |
| `SUPABASE_DB_PASSWORD` | /tmp/boxy-supabase-creds.txt | Supabase DB 마이그레이션 |
| `GOOGLE_SERVICES_JSON_DEV` | base64(secrets/google-services-dev.json) | Firebase Android dev |
| `GOOGLE_SERVICES_JSON_PROD` | base64(secrets/google-services-prod.json) | Firebase Android prod |

## 🔧 정수가 직접 등록할 것 (10개)

### Unity 라이선스 (3개) — 모든 빌드 잡 필수

```bash
# https://license.unity3d.com/manual 에서 .ulf 파일 받기 (또는 Unity Hub → Manage Licenses → Manual)
gh secret set UNITY_LICENSE --body "$(cat path/to/Unity.ulf)" --repo dugout26/boxy
gh secret set UNITY_EMAIL --body "정수의 Unity 계정 이메일" --repo dugout26/boxy
gh secret set UNITY_PASSWORD --body "Unity 계정 비밀번호" --repo dugout26/boxy
```

### Android keystore (4개) — Play Store 배포 서명

```bash
# 기존 .keystore 파일 있으면 base64 인코딩 후 업로드
base64 -i path/to/boxy-release.keystore | gh secret set ANDROID_KEYSTORE_BASE64 --repo dugout26/boxy
gh secret set ANDROID_KEYSTORE_PASSWORD --body "키스토어 비밀번호" --repo dugout26/boxy
gh secret set ANDROID_KEY_ALIAS --body "키 alias" --repo dugout26/boxy
gh secret set ANDROID_KEY_PASSWORD --body "key alias 비밀번호" --repo dugout26/boxy
```

키스토어 없으면 1회만 생성:
```bash
keytool -genkey -v -keystore secrets/boxy-release.keystore \
  -keyalg RSA -keysize 2048 -validity 10000 -alias boxy-release
# secrets/는 .gitignore. 분실 시 v1.1 업그레이드 영원히 막힘 — 즉시 1Password/iCloud 백업.
```

### Google Play Console (1개) — Internal Testing 자동 업로드

```bash
# Play Console → Setup → API access → Create new service account → JSON 다운로드
gh secret set GOOGLE_PLAY_SERVICE_ACCOUNT_JSON --body "$(cat path/to/google-play-sa.json)" --repo dugout26/boxy
```

서비스 계정 권한: "Release manager" 이상. com.mound.boxy + com.mound.boxy.dev 두 앱 모두에 access 부여.

### Firebase iOS plist (2개) — Crashlytics + Analytics

```bash
# Firebase Console → 프로젝트 설정 → iOS 앱 → GoogleService-Info.plist 다운로드
# boxy-mound-dev (com.mound.boxy.dev), boxy-mound (com.mound.boxy) 각각
base64 -i path/to/GoogleService-Info-dev.plist  | gh secret set GOOGLE_SERVICE_INFO_PLIST_DEV  --repo dugout26/boxy
base64 -i path/to/GoogleService-Info-prod.plist | gh secret set GOOGLE_SERVICE_INFO_PLIST_PROD --repo dugout26/boxy
```

### EnvironmentConfig.asset (4개 + 4개 meta) — IAP/광고/AppsFlyer 키

Unity Editor에서 1회 생성 (Assets → Create → Mound → Environment Config), nested 필드 입력 후:

```bash
# 두 개 파일 (.asset + .asset.meta) 환경별 2세트
base64 -i Assets/Mound/Monetization/EnvironmentConfig-dev.asset       | gh secret set ENVIRONMENT_CONFIG_ASSET_DEV       --repo dugout26/boxy
base64 -i Assets/Mound/Monetization/EnvironmentConfig-dev.asset.meta  | gh secret set ENVIRONMENT_CONFIG_ASSET_DEV_META  --repo dugout26/boxy
base64 -i Assets/Mound/Monetization/EnvironmentConfig-prod.asset      | gh secret set ENVIRONMENT_CONFIG_ASSET_PROD      --repo dugout26/boxy
base64 -i Assets/Mound/Monetization/EnvironmentConfig-prod.asset.meta | gh secret set ENVIRONMENT_CONFIG_ASSET_PROD_META --repo dugout26/boxy
```

내부 필드:
- AdMob (App ID, Rewarded/Interstitial/Banner): AdMob 콘솔 발급
- AppLovin SDK Key + 단위 ID: applovin.com 가입 후
- AppsFlyer Dev Key: appsflyer.com Zero Plan 가입
- Apple Shared Secret: App Store Connect → My Apps → App Information → App-Specific Shared Secret
- Google License Key: Play Console → 앱 → Monetization setup → 페이지 하단 RSA public key
- Supabase URL/anonKey: 이미 박혀있음 (`https://lckhgvndkvmpqgmcqaea.supabase.co`)

## 🔍 등록 확인

```bash
gh secret list --repo dugout26/boxy
# 18개 모두 표시되면 OK. 빠진 키는 위 명령으로 추가.
```

## ⚠️ 키 회전 주기

| 키 | 회전 |
|---|---|
| Unity license | Unity 정책 따라 (보통 1년) |
| Android keystore | **절대 회전 X** — 한 번 출시되면 영원히 같은 키. 분실 시 앱 재등록 |
| Google Play SA JSON | 1년 |
| ASC API key | 정수 결정 — Apple 정책 강제 X |
| Apple Shared Secret | 6개월 권장 |
| Firebase API key | 무한 (자동 회전 안 됨) |
| AppLovin/AppsFlyer/AdMob ID | 무한 |
| Supabase anon key | RLS 보호되므로 노출 OK, 그래도 의심 시 회전 |

## 🚨 누출 사고 대응

1. 즉시 GitHub Secret 삭제: `gh secret delete <NAME> --repo dugout26/boxy`
2. 발급처에서 키 revoke (App Store Connect / AdMob / Firebase Console)
3. 새 키 발급 → GitHub Secret 다시 등록
4. git history 검사 (`scripts/check-violations.sh` 실행) — 코드에 박혔는지 재확인
