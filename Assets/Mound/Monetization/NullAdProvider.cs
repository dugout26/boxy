using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Monetization
{
    // Editor / dev 빌드용 광고 스텁. 실제 SDK 없이 보상형 광고를 즉시 Rewarded로 시뮬레이션.
    // Production 빌드 사용 금지 — AppLovin/AdMob wrapper로 교체 (Week 3 SDK 통합).
    public sealed class NullAdProvider : IAdProvider
    {
        readonly int simulatedDelayMs;

        public NullAdProvider(int simulatedDelayMs = 1000)
        {
            this.simulatedDelayMs = simulatedDelayMs;
        }

        public async Task<AdResult> ShowRewardedAsync(AdPlacement placement, CancellationToken ct)
        {
            Debug.Log($"[NullAdProvider] ShowRewarded {placement} (sim {simulatedDelayMs}ms)");
            try
            {
                await Task.Delay(simulatedDelayMs, ct);
            }
            catch (TaskCanceledException)
            {
                return AdResult.Dismissed;
            }
            return AdResult.Rewarded;
        }

        public bool IsRewardedReady(AdPlacement placement) => true;

        public void TryShowInterstitial()
        {
            Debug.Log("[NullAdProvider] TryShowInterstitial (no-op)");
        }

        public void ShowBanner()
        {
            Debug.Log("[NullAdProvider] ShowBanner (no-op)");
        }

        public void HideBanner()
        {
            Debug.Log("[NullAdProvider] HideBanner (no-op)");
        }
    }
}
