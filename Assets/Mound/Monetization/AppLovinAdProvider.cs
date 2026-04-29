using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Monetization
{
    // SDK_VERSIONS.md: AppLovin MAX Unity Plugin v8.6.2 (2025-03-30)
    // 통합 시점: boxy-plan §C Week 3 Day 15-16
    //
    // Week 3 통합 절차:
    //   1. Unity Asset Store에서 AppLovin MAX Unity Plugin 임포트
    //   2. Mound.Monetization.asmdef references 또는 precompiledReferences에 AppLovin asmdef 추가
    //   3. 아래 TODO 마크 SDK 호출로 교체 (MaxSdk.ShowRewardedAd 등)
    //   4. AppLovin 대시보드에서 AdMob을 미디에이션 네트워크로 등록
    //   5. CLAUDE.md §2-4 환각 방지: 실제 API 시그니처를 SDK_VERSIONS의 v8.6.2 공식 샘플과 대조
    //
    // 현재 상태: 스켈레톤 — 모든 메서드 NullAdProvider 동작과 동일 (Failed 반환).
    // GameplayUI에서 NullAdProvider 대신 AppLovinAdProvider 인스턴스화는 Week 3 통합 후.
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

            // TODO Week 3:
            //   MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfig => {
            //       MaxSdk.SetUserId(SystemInfo.deviceUniqueIdentifier);
            //       PrefetchAds();
            //   };
            //   MaxSdk.SetSdkKey(sdkKey);
            //   MaxSdk.InitializeSdk();
            Debug.LogWarning("[AppLovinAdProvider] SDK 미통합 — Week 3 Day 15 작업 대기. 현재 모든 호출 Failed 반환.");
        }

        public Task<AdResult> ShowRewardedAsync(AdPlacement placement, CancellationToken ct)
        {
            // TODO Week 3:
            //   var tcs = new TaskCompletionSource<AdResult>();
            //   MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (id, reward, info) => tcs.TrySetResult(AdResult.Rewarded);
            //   MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (id, info) => tcs.TrySetResult(AdResult.Dismissed);
            //   MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += (id, error) => tcs.TrySetResult(AdResult.Failed);
            //   ct.Register(() => tcs.TrySetCanceled());
            //   if (!MaxSdk.IsRewardedAdReady(rewardedUnitId)) MaxSdk.LoadRewardedAd(rewardedUnitId);
            //   MaxSdk.ShowRewardedAd(rewardedUnitId, placement.ToString());
            //   return tcs.Task;
            return Task.FromResult(AdResult.Failed);
        }

        public bool IsRewardedReady(AdPlacement placement)
        {
            // TODO Week 3: return MaxSdk.IsRewardedAdReady(rewardedUnitId);
            return false;
        }

        public void TryShowInterstitial()
        {
            // TODO Week 3:
            //   if (MaxSdk.IsInterstitialReady(interstitialUnitId)) MaxSdk.ShowInterstitial(interstitialUnitId);
            //   else MaxSdk.LoadInterstitial(interstitialUnitId);
        }

        public void ShowBanner()
        {
            // TODO Week 3: MaxSdk.CreateBanner(bannerUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        }

        public void HideBanner()
        {
            // TODO Week 3: MaxSdk.HideBanner(bannerUnitId);
        }
    }
}
