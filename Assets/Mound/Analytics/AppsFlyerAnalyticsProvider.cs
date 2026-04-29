using System.Collections.Generic;
using UnityEngine;

namespace Mound.Analytics
{
    // SDK_VERSIONS.md: AppsFlyer Unity SDK v6.x
    // 통합 시점: boxy-plan §C Week 3 Day 18.
    //
    // 통합 절차:
    //   1. https://github.com/AppsFlyerSDK/appsflyer-unity-plugin → .unitypackage 임포트
    //   2. AppsFlyerObject prefab을 첫 씬에 부착 (또는 Bootstrap에 자동 add)
    //   3. EnvironmentConfig.AppsFlyerDevKey 설정
    //   4. iOS는 ATT 동의 후에만 IDFA 사용 — IosAttConsentProvider와 흐름 통합 (BoxyBootstrap.RequestConsentDelayedAsync)
    //
    // 현재 상태: 스켈레톤 — Track 호출 시 Debug.Log만, 실제 AppsFlyer 전송 X.
    public sealed class AppsFlyerAnalyticsProvider : IAnalyticsProvider
    {
        readonly string devKey;

        public AppsFlyerAnalyticsProvider(string devKey)
        {
            this.devKey = devKey;
            // TODO Week 3 Day 18:
            //   AppsFlyer.setAppsFlyerKey(devKey);
            //   AppsFlyer.startSDK();
            //   #if UNITY_IOS
            //     AppsFlyer.setAppleAppID("APPLE_APP_ID");
            //   #endif
            Debug.LogWarning($"[AppsFlyerAnalyticsProvider] SDK 미통합 — Day 18 대기. DevKey={(string.IsNullOrEmpty(devKey)?"미설정":"설정됨")}");
        }

        public void Track(AnalyticsEvent evt, IReadOnlyDictionary<string, object> parameters = null)
        {
            // AppsFlyer 표준 이벤트 매핑 (af_* prefix)
            string afName = MapToAppsFlyerEvent(evt);
            int paramCount = parameters?.Count ?? 0;

            // TODO Week 3:
            //   var afParams = parameters != null ? new Dictionary<string,string>() : null;
            //   if (parameters != null) foreach (var kv in parameters) afParams[kv.Key] = kv.Value?.ToString();
            //   AppsFlyer.sendEvent(afName, afParams);
            Debug.Log($"[AppsFlyer] {afName} ({paramCount} params)");
        }

        public void SetUserProperty(string key, string value)
        {
            // AppsFlyer는 user property가 아닌 customer user ID 또는 in-app event params로 표현
            // TODO Week 3: AppsFlyer.setCustomerUserId(value); 또는 sendEvent("user_property", {key: value})
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
