# 2026-04-29 — AppLovin MAX + AppsFlyer 셋업 가이드 (Day 15~16, 18)

## Context

기획서 §C Week 3:
- Day 15~16: AppLovin MAX 통합 (단일 미디에이션, AdMob은 어댑터)
- Day 18: AppsFlyer 무료 티어 통합

현재 둘 다 stub 클래스만 (Mound.Monetization.AppLovinAdProvider, Mound.Monetization.AppsFlyerProvider 미존재).

## AppLovin MAX 셋업 (Day 15~16)

### 정수 작업
1. https://www.applovin.com/grow/max-mediation/ 접속
2. 가입 → 앱 추가 (com.mound.boxy + com.mound.boxy.dev 두 개)
3. 광고 단위 생성:
   - **rewarded_v1** (보상형 — 힌트/되돌리기/이어하기)
   - **interstitial_v1** (전면 광고 — 레벨 클리어 후 5번 중 1번)
4. SDK 키 발급 → `secrets/EnvironmentConfig-dev.asset` + `EnvironmentConfig-prod.asset`에 저장
5. AdMob을 미디에이션 네트워크로 추가 (이중 미디에이션 X)

### Unity SDK 설치
1. https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin/releases 최신 .unitypackage 다운로드
2. Import → Unity가 자동 IronSource·AdMob 어댑터도 권장
3. **AppLovin Integration Manager** (Unity 메뉴) 열고:
   - SDK Key 입력 (테스트 vs 실)
   - Mediated Networks 체크: AdMob, Meta Audience Network
   - Test Devices 추가 (정수 디바이스 IDFA)

### Mound.Monetization.AppLovinAdProvider.cs 작성 (기존 stub 교체)
```csharp
public sealed class AppLovinAdProvider : IAdProvider {
    EnvironmentConfig config;

    public AppLovinAdProvider(EnvironmentConfig cfg) {
        config = cfg;
        MaxSdk.SetSdkKey(config.applovinSdkKey);
        MaxSdk.SetUserId(SystemInfo.deviceUniqueIdentifier);
        MaxSdk.InitializeSdk();
    }

    public async UniTask<bool> ShowRewarded(string placement, CancellationToken ct) {
        var adId = config.useTestAdIds ? "test_id" : config.rewardedAdUnitId;
        // 로드 + 표시 + 결과 콜백 → UniTask로 wrap
        ...
    }
}
```

### CLAUDE.md §2-4 환각 방지
SDK_VERSIONS.md에 박힌 AppLovin MAX Unity Plugin **v9.0.0** 기준. 공식 샘플과 시그니처 대조 필수.

## AppsFlyer 셋업 (Day 18)

### 정수 작업
1. https://www.appsflyer.com/ 접속
2. 무료 티어 가입 (월 12,000 conversion 무료 — Boxy 첫 1년 충분)
3. 앱 추가:
   - iOS Bundle ID: com.mound.boxy
   - Android Package: com.mound.boxy
4. **AppsFlyer Dev Key** 발급 → secrets/EnvironmentConfig.asset
5. **OneLink** 생성 (Universal Link / Deep Link) — 추후 마케팅 캠페인용

### Unity SDK 설치
1. https://github.com/AppsFlyerSDK/appsflyer-unity-plugin 최신 .unitypackage
2. Import → AppsFlyerObject prefab 추가하라는 가이드 따름
3. `AppsFlyerSDK.AppsFlyer.startSDK()` 호출 (Bootstrap.Awake에)

### iOS ATT 통합
```csharp
// iOS는 ATT 권한 동의 후에만 IDFA 사용 가능
await Consent.RequestAsync(ct);
if (Consent.CanShowPersonalizedAds()) {
    AppsFlyer.startSDK();
}
```

이미 `IosAttConsentProvider` + `BoxyBootstrap.RequestConsentDelayedAsync`로 흐름 구축됨.

### 핵심 이벤트 (출시 시 필수 트래킹)
- af_install (자동)
- af_complete_registration (첫 게임 시작)
- af_purchase (IAP 결제 성공)
- af_level_complete (레벨 클리어)
- af_ad_view (광고 보기)

## Unity IAP 셋업 (Day 17)

### 정수 작업
1. App Store Connect → 앱 → 인앱 구매 → 새로 만들기:
   - **com.mound.boxy.remove_ads** (비소비성, $2.99)
   - **com.mound.boxy.hint_pack_10** (소비성, $0.99)
   - **com.mound.boxy.undo_pass_5** (소비성, $0.99)
2. Google Play Console → 인앱 상품 → 동일 ID로 등록
3. **세금 양식 + 은행 계좌** App Store Connect에 등록 (출시 전 필수)

### Unity Gaming Services
이미 `Mound.Monetization.UnityIapProvider` 스켈레톤 존재. SDK는 Unity Package Manager → Unity Gaming Services → IAP 설치.

## Revisit when

- 키 발급 후 → SDK 통합 (Week 3)
- 첫 빌드에서 광고 표시 안 되면 SDK_VERSIONS.md vs 실 SDK 버전 대조 (§0-3 의심)
