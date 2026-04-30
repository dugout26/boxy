using System;
using UnityEngine;
using Mound.Monetization;

namespace Boxy.App.Gameplay
{
    // boxy-plan §B-2-3 인터스티셜 90~120초 시간 기준. ResetInterval로 매번 무작위 — 패턴 예측 회피.
    public sealed class InterstitialTimer
    {
        const float MinIntervalSeconds = 90f;
        const float MaxIntervalSeconds = 120f;

        readonly IAdProvider adProvider;
        float lastShownTime;
        float currentInterval;
        bool suppressed;

        public InterstitialTimer(IAdProvider adProvider)
        {
            this.adProvider = adProvider ?? throw new ArgumentNullException(nameof(adProvider));
            ResetInterval();
            lastShownTime = Time.realtimeSinceStartup;
        }

        // GameplayUI.Update에서 매 프레임 호출. Time.realtimeSinceStartup 기반이라 일시정지 영향 X.
        public void Tick()
        {
            if (suppressed) return;
            float now = Time.realtimeSinceStartup;
            if (now - lastShownTime < currentInterval) return;

            adProvider.TryShowInterstitial();
            lastShownTime = now;
            ResetInterval();
        }

        // 광고 제거 IAP 보유 시 호출 — 인터스티셜 영구 비활성
        public void Suppress()
        {
            suppressed = true;
        }

        public void ResetInterval()
        {
            currentInterval = UnityEngine.Random.Range(MinIntervalSeconds, MaxIntervalSeconds);
        }
    }
}
