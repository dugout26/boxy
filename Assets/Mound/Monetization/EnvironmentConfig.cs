using UnityEngine;

namespace Mound.Monetization
{
    public enum BuildEnvironment
    {
        Dev,
        Staging,
        Production
    }

    // CLAUDE.md §8-2 빌드 환경 분리 + §9-2 시크릿 보호.
    // 이 SO의 .asset 파일은 secrets/ 폴더 또는 EnvironmentConfig.asset 자체가 .gitignore로 제외됨.
    [CreateAssetMenu(menuName = "Mound/Environment Config", fileName = "EnvironmentConfig")]
    public sealed class EnvironmentConfig : ScriptableObject
    {
        [Header("Build Environment")]
        [SerializeField] BuildEnvironment environment = BuildEnvironment.Dev;
        [SerializeField] bool useTestAdIds = true;

        [Header("AdMob")]
        [SerializeField] AdmobConfig admob = new AdmobConfig();

        [Header("AppLovin MAX")]
        [SerializeField] AppLovinConfig applovin = new AppLovinConfig();

        [Header("Attribution / Analytics")]
        [SerializeField] AttributionConfig attribution = new AttributionConfig();

        [Header("IAP Receipt Validation (P0-1)")]
        [SerializeField] AppleReceiptConfig appleReceipt = new AppleReceiptConfig();
        [SerializeField] GoogleReceiptConfig googleReceipt = new GoogleReceiptConfig();

        [Header("Supabase Edge Functions (P0-1 cloud + Ad SSV)")]
        [SerializeField] SupabaseConfig supabase = new SupabaseConfig();

        public BuildEnvironment Environment => environment;
        public bool UseTestAdIds => useTestAdIds;

        // §5-3 SDK 격리: SDK별 nested config 직접 노출 — Boxy.App은 이 객체 자체만 알 뿐 내부 필드명 모름.
        // 광고/IAP/분석 SDK 교체 시 nested config 클래스만 변경.
        public AdmobConfig Admob => admob;
        public AppLovinConfig AppLovin => applovin;
        public AttributionConfig Attribution => attribution;
        public AppleReceiptConfig AppleReceipt => appleReceipt;
        public GoogleReceiptConfig GoogleReceipt => googleReceipt;
        public SupabaseConfig Supabase => supabase;

        // 호환 위임: useTestAdIds=true면 Google 공식 테스트 ID, false면 nested config의 실제 값.
        // (caller 코드가 점진적으로 nested 접근으로 마이그레이션될 때까지 유지)
        public string AdmobAppId => useTestAdIds ? TestAdIds.AdmobAppId : admob.AppId;
        public string AdmobRewardedId => useTestAdIds ? TestAdIds.AdmobRewarded : admob.RewardedId;
        public string AdmobInterstitialId => useTestAdIds ? TestAdIds.AdmobInterstitial : admob.InterstitialId;
        public string AdmobBannerId => useTestAdIds ? TestAdIds.AdmobBanner : admob.BannerId;

        public string ApplovinSdkKey => applovin.SdkKey;
        public string ApplovinRewardedUnitId => applovin.RewardedUnitId;
        public string ApplovinInterstitialUnitId => applovin.InterstitialUnitId;
        public string ApplovinBannerUnitId => applovin.BannerUnitId;
        public string AppsFlyerDevKey => attribution.AppsFlyerDevKey;

        [System.Serializable]
        public sealed class AdmobConfig
        {
            [SerializeField] string appId;
            [SerializeField] string rewardedId;
            [SerializeField] string interstitialId;
            [SerializeField] string bannerId;
            public string AppId => appId;
            public string RewardedId => rewardedId;
            public string InterstitialId => interstitialId;
            public string BannerId => bannerId;
        }

        [System.Serializable]
        public sealed class AppLovinConfig
        {
            [SerializeField] string sdkKey;
            [SerializeField] string rewardedUnitId;
            [SerializeField] string interstitialUnitId;
            [SerializeField] string bannerUnitId;
            public string SdkKey => sdkKey;
            public string RewardedUnitId => rewardedUnitId;
            public string InterstitialUnitId => interstitialUnitId;
            public string BannerUnitId => bannerUnitId;
        }

        [System.Serializable]
        public sealed class AttributionConfig
        {
            [SerializeField] string appsFlyerDevKey;
            public string AppsFlyerDevKey => appsFlyerDevKey;
        }

        // Apple App Store Connect → My Apps → App-Specific Shared Secret
        // 자체 서버 없으면 클라 직검증 (sandbox + production endpoint 동시 시도)
        [System.Serializable]
        public sealed class AppleReceiptConfig
        {
            [SerializeField] string sharedSecret;
            [SerializeField] bool useSandbox = true;     // dev/Editor true, production false
            public string SharedSecret => sharedSecret;
            public bool UseSandbox => useSandbox;
        }

        // Google Play Console → Setup → API access → Licensing key (Base-64 RSA Public Key)
        // Google Play Billing v6 클라 측 signature verification 표준
        [System.Serializable]
        public sealed class GoogleReceiptConfig
        {
            [SerializeField] string licenseKeyBase64;    // RSA public key (Play Console "Monetization setup" 페이지)
            public string LicenseKeyBase64 => licenseKeyBase64;
        }

        // Supabase Edge Functions — IAP 서버 검증 + 광고 SSV.
        // Project: boxy-mound (Tokyo) — decisions/2026-04-30-22-supabase-edge-functions.md
        [System.Serializable]
        public sealed class SupabaseConfig
        {
            [SerializeField] string url = "https://lckhgvndkvmpqgmcqaea.supabase.co";
            [SerializeField] string anonKey = "";        // RLS 보호 — 클라 노출 OK. .asset 시크릿 폴더에서만 git ignore
            [SerializeField] string validateIapEndpoint = "/functions/v1/validate-iap-receipt";
            [SerializeField] string adSsvEndpoint = "/functions/v1/reward-ad-ssv";

            public string Url => url;
            public string AnonKey => anonKey;
            public string ValidateIapUrl => string.IsNullOrEmpty(url) ? "" : url + validateIapEndpoint;
            public string AdSsvUrl => string.IsNullOrEmpty(url) ? "" : url + adSsvEndpoint;
        }
    }

    // Google AdMob 공식 테스트 ID — 공개 문서화됨, 보안 시크릿 아님.
    // 출처: https://developers.google.com/admob/android/test-ads
    public static class TestAdIds
    {
        public const string AdmobAppId = "ca-app-pub-3940256099942544~3347511713";
        public const string AdmobRewarded = "ca-app-pub-3940256099942544/5224354917";
        public const string AdmobInterstitial = "ca-app-pub-3940256099942544/1033173712";
        public const string AdmobBanner = "ca-app-pub-3940256099942544/6300978111";
    }
}
