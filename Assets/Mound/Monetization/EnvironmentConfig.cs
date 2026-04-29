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
        [SerializeField] string admobAppId;
        [SerializeField] string admobRewardedId;
        [SerializeField] string admobInterstitialId;
        [SerializeField] string admobBannerId;

        [Header("AppLovin MAX")]
        [SerializeField] string applovinSdkKey;
        [SerializeField] string applovinRewardedUnitId;
        [SerializeField] string applovinInterstitialUnitId;
        [SerializeField] string applovinBannerUnitId;

        [Header("Attribution")]
        [SerializeField] string appsFlyerDevKey;

        public BuildEnvironment Environment => environment;
        public bool UseTestAdIds => useTestAdIds;

        // useTestAdIds=true면 Google 공식 테스트 ID 반환. CLAUDE.md §8-2: dev/stg에서 실제 ID 사용 시 정책 위반 → 계정 정지
        public string AdmobAppId => useTestAdIds ? TestAdIds.AdmobAppId : admobAppId;
        public string AdmobRewardedId => useTestAdIds ? TestAdIds.AdmobRewarded : admobRewardedId;
        public string AdmobInterstitialId => useTestAdIds ? TestAdIds.AdmobInterstitial : admobInterstitialId;
        public string AdmobBannerId => useTestAdIds ? TestAdIds.AdmobBanner : admobBannerId;

        public string ApplovinSdkKey => applovinSdkKey;
        public string ApplovinRewardedUnitId => applovinRewardedUnitId;
        public string ApplovinInterstitialUnitId => applovinInterstitialUnitId;
        public string ApplovinBannerUnitId => applovinBannerUnitId;
        public string AppsFlyerDevKey => appsFlyerDevKey;
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
