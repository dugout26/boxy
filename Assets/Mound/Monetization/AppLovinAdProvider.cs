using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Monetization
{
    // AppLovin MAX Unity Plugin 래퍼.
    // SDK 통합 가이드: decisions/2026-04-29-18-applovin-appsflyer-setup-guide.md
    // 미통합 시 모든 메서드 Failed/false 반환 — 게임 진행 차단되지 않음.
    public sealed class AppLovinAdProvider : IAdProvider
    {
        readonly string sdkKey;
        readonly string rewardedUnitId;
        readonly string interstitialUnitId;
        readonly string bannerUnitId;

        public AppLovinAdProvider(EnvironmentConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            sdkKey = config.ApplovinSdkKey;
            rewardedUnitId = config.ApplovinRewardedUnitId;
            interstitialUnitId = config.ApplovinInterstitialUnitId;
            bannerUnitId = config.ApplovinBannerUnitId;

            Debug.LogWarning("[AppLovinAdProvider] SDK 미통합 — 모든 호출 Failed 반환.");
        }

        public Task<AdResult> ShowRewardedAsync(AdPlacement placement, CancellationToken ct)
        {
            return Task.FromResult(AdResult.Failed);
        }

        public bool IsRewardedReady(AdPlacement placement)
        {
            return false;
        }

        public void TryShowInterstitial() { }

        public void ShowBanner() { }

        public void HideBanner() { }
    }
}
