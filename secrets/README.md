# secrets/ — 환경별 키 / 설정 파일

이 폴더는 `.gitignore`에 의해 **절대 커밋되지 않음**. 시크릿/키/환경 설정만 보관.

## 현재 보관

### Firebase 설정 (2-tier, decisions/2026-04-29-09-app-id-strategy-2tier.md)
- `google-services-dev.json` → Project: `boxy-mound-dev`, Bundle ID: `com.mound.boxy.dev`
- `google-services-prod.json` → Project: `boxy-mound-prod`, Bundle ID: `com.mound.boxy`

각각 환경별 빌드에서 `Assets/google-services.json`으로 복사 (`Boxy → Environment → Switch to Dev/Production` 메뉴 자동화).

### App Store Connect API Key (iOS TestFlight 업로드용)
- `AuthKey_XXXXXXXXXX.p8` — App Store Connect에서 발급한 .p8 private key
- `asc-credentials.env` — 환경변수 (Key ID, Issuer ID, Team ID) 보관

## 향후 추가될 것

- `GoogleService-Info-{dev,prod}.plist` — iOS Firebase 설정
- `keystore.{dev,prod}.jks` — Android 서명 키 (Production 출시 시)
- AdMob App ID / 광고 단위 ID — 계정 생성 후
- AppLovin SDK Key
- AppsFlyer Dev Key

## 주의

- **이 폴더는 git 추적 안 됨** → 다른 컴퓨터에서 클론하면 비어 있음
- 새 컴퓨터 이전 시 Firebase Console에서 google-services.json 직접 다운로드 또는 firebase CLI로 재생성:
  ```bash
  firebase apps:sdkconfig ANDROID <APP_ID> --project boxy-mound-{dev,prod} --out google-services-{dev,prod}.json
  ```
- 빌드 자동화 시 Unity Cloud Build의 시크릿 변수 또는 환경 변수로 주입
