# 2026-04-29 — Firebase iOS 셋업 가이드

## Context

Firebase Android (`secrets/google-services-{dev,prod}.json`) 셋업 완료. iOS는 다른 형식 (`GoogleService-Info.plist`) — 별도 다운로드 필요.

## 정수 작업 (5분)

### iOS Firebase 앱 등록 (dev + prod 각각)

1. https://console.firebase.google.com/ 접속
2. **boxy-mound-dev** 프로젝트 클릭
3. ⚙ 톱니바퀴 → **프로젝트 설정** → **앱 추가** → **iOS**
4. 입력:
   - Apple Bundle ID: `com.mound.boxy.dev`
   - 닉네임 (선택): `Boxy Dev iOS`
   - App Store ID (선택): 비워둠 (출시 전이라)
5. **앱 등록**
6. **GoogleService-Info.plist 다운로드** → `/Users/jeki/Boxy/secrets/GoogleService-Info-dev.plist`로 저장
7. 나머지 단계 (SDK 설치, 초기화 코드 등)는 스킵 — Unity Firebase SDK가 처리

### prod 프로젝트도 동일
- Firebase Console → **boxy-mound-prod** → iOS 앱 추가
- Bundle ID: `com.mound.boxy`
- 다운로드 → `secrets/GoogleService-Info-prod.plist`

## 자동화 — BoxyBuilder가 자동 적용

`Boxy.Editor.BoxyBuilder.BuildIOS`가 빌드 시 자동으로:
- envTag=dev → `secrets/GoogleService-Info-dev.plist` → `Assets/GoogleService-Info.plist` 복사
- envTag=prod → `secrets/GoogleService-Info-prod.plist` → `Assets/GoogleService-Info.plist` 복사

미존재 시 silent skip (Firebase SDK 미통합 상태에선 어차피 동작 X).

## Firebase Unity SDK 설치 (정수 작업, 5분)

1. https://firebase.google.com/download/unity 접속
2. 최신 SDK ZIP 다운로드 (~50MB)
3. ZIP 해제 → 다음 .unitypackage 파일들:
   - `FirebaseAnalytics.unitypackage` (필수)
   - `FirebaseCrashlytics.unitypackage` (필수)
   - `FirebaseMessaging.unitypackage` (선택, push 알림 시)
4. Unity 열기 → 위 파일들 드래그 → Import
5. Unity가 자동으로 google-services.json / GoogleService-Info.plist 감지하여 빌드 시 통합

## Mound.Analytics.FirebaseAnalyticsProvider 활성화

SDK 설치 후 `Assets/Mound/Analytics/FirebaseAnalyticsProvider.cs`의 TODO 주석 unblock:

```csharp
// 변경 전:
Debug.LogWarning("[FirebaseAnalyticsProvider] SDK 미통합...");

// 변경 후:
FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
    if (task.Result == DependencyStatus.Available) {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
    }
});
```

CLAUDE.md §0-4에 따라 SDK 버전 (Firebase Unity SDK v13.10.0) `SDK_VERSIONS.md` 명시. 환각 방지.

## Crashlytics 강제 크래시 테스트 (Day 21)

```csharp
// 자체 플레이 30회 + 중간에 1번 강제 크래시 → Firebase Console에서 5분 내 확인
Crashlytics.LogException(new System.Exception("Test crash"));
```

## Revisit when

- Firebase Unity SDK 설치 후 → Provider TODO 활성화
- Day 21 검증 — Crashlytics 대시보드에서 강제 크래시 확인되어야 §12-3 출시 직전 검증 통과
