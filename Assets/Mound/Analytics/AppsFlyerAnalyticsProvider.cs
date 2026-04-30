using System.Collections.Generic;
using UnityEngine;

namespace Mound.Analytics
{
    // AppsFlyer Analytics 래퍼.
    // SDK 통합 가이드: decisions/2026-04-29-18-applovin-appsflyer-setup-guide.md
    // 미통합 시 Track 호출은 Debug.Log만, 실제 전송 X.
    public sealed class AppsFlyerAnalyticsProvider : IAnalyticsProvider
    {
        readonly string devKey;

        public AppsFlyerAnalyticsProvider(string devKey)
        {
            this.devKey = devKey;
            Debug.LogWarning($"[AppsFlyerAnalyticsProvider] SDK 미통합. DevKey={(string.IsNullOrEmpty(devKey)?"미설정":"설정됨")}");
        }

        public void Track(AnalyticsEvent evt, IReadOnlyDictionary<string, object> parameters = null)
        {
            string afName = MapToAppsFlyerEvent(evt);
            int paramCount = parameters?.Count ?? 0;
            Debug.Log($"[AppsFlyer] {afName} ({paramCount} params)");
        }

        public void SetUserProperty(string key, string value)
        {
            Debug.Log($"[AppsFlyer] user property {key}={value}");
        }

        public void Flush()
        {
            // AppsFlyer는 자동 배치/플러시 — 별도 호출 X
        }

        static string MapToAppsFlyerEvent(AnalyticsEvent evt) => evt switch
        {
            AnalyticsEvent.TutorialStarted => "af_tutorial_started",
            AnalyticsEvent.TutorialCompleted => "af_tutorial_completion",
            AnalyticsEvent.LevelStarted => "af_level_start",
            AnalyticsEvent.LevelCompleted => "af_level_achieved",
            AnalyticsEvent.LevelFailed => "af_level_failed",
            AnalyticsEvent.LevelRetried => "af_level_retried",
            AnalyticsEvent.AdRewardedRequested => "af_ads_request",
            AnalyticsEvent.AdRewardedCompleted => "af_ads_view",
            AnalyticsEvent.AdRewardedDismissed => "af_ads_dismissed",
            AnalyticsEvent.AdInterstitialShown => "af_ads_view",
            AnalyticsEvent.IapPurchaseInitiated => "af_initiated_checkout",
            AnalyticsEvent.IapPurchaseCompleted => "af_purchase",
            AnalyticsEvent.IapPurchaseFailed => "af_purchase_failed",
            AnalyticsEvent.SessionStarted => "af_session_start",
            AnalyticsEvent.SessionEnded => "af_session_end",
            _ => evt.ToString().ToLower()
        };
    }

    // 여러 Analytics provider에 동시 발송 — Firebase + AppsFlyer 병렬 추적.
    // ARPDAU + 출처 추적이 분리된 도구라 둘 다 필요.
    public sealed class CompositeAnalyticsProvider : IAnalyticsProvider
    {
        readonly IAnalyticsProvider[] providers;

        public CompositeAnalyticsProvider(params IAnalyticsProvider[] providers)
        {
            this.providers = providers ?? new IAnalyticsProvider[0];
        }

        public void Track(AnalyticsEvent evt, IReadOnlyDictionary<string, object> parameters = null)
        {
            foreach (var p in providers) p?.Track(evt, parameters);
        }

        public void SetUserProperty(string key, string value)
        {
            foreach (var p in providers) p?.SetUserProperty(key, value);
        }

        public void Flush()
        {
            foreach (var p in providers) p?.Flush();
        }
    }
}
