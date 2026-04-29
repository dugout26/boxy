using System.Collections.Generic;
using UnityEngine;

namespace Mound.Analytics
{
    // SDK_VERSIONS.md: Firebase Unity SDK v13.10.0
    // 통합 시점: boxy-plan §C Week 3 Day 18
    //
    // Week 3 통합 절차:
    //   1. https://firebase.google.com/download/unity → FirebaseAnalytics.unitypackage 임포트
    //   2. (Crashlytics도 동시) FirebaseCrashlytics.unitypackage 임포트
    //   3. secrets/google-services-{dev,stg,prod}.json → 빌드 환경에 따라 Assets/google-services.json으로 복사 스크립트 (CLAUDE.md §8-2)
    //   4. 아래 TODO 마크 SDK 호출로 교체
    //   5. CLAUDE.md §2-4 환각 방지: Firebase.Analytics.FirebaseAnalytics API를 v13.10.0 공식 샘플과 대조
    //
    // 현재 상태: 스켈레톤 — Track 호출 시 Debug.Log만, 실제 Firebase 전송 X.
    public sealed class FirebaseAnalyticsProvider : IAnalyticsProvider
    {
        public FirebaseAnalyticsProvider()
        {
            // TODO Week 3:
            //   FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            //     if (task.Result == DependencyStatus.Available) {
            //       FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            //     } else {
            //       Debug.LogError($"[FirebaseAnalyticsProvider] dependency error: {task.Result}");
            //     }
            //   });
            Debug.LogWarning("[FirebaseAnalyticsProvider] SDK 미통합 — Week 3 Day 18 대기. Track 호출 시 Debug.Log만.");
        }

        public void Track(AnalyticsEvent evt, IReadOnlyDictionary<string, object> parameters = null)
        {
            // TODO Week 3:
            //   var fbParams = ConvertParameters(parameters);
            //   FirebaseAnalytics.LogEvent(evt.ToString(), fbParams);
            int paramCount = parameters?.Count ?? 0;
            Debug.Log($"[Analytics] {evt} ({paramCount} params)");
        }

        public void SetUserProperty(string key, string value)
        {
            // TODO Week 3: FirebaseAnalytics.SetUserProperty(key, value);
            Debug.Log($"[Analytics] SetUserProperty {key}={value}");
        }

        public void Flush()
        {
            // Firebase Analytics는 자동 배치/플러시 — 별도 호출 불필요
        }

        // TODO Week 3: 헬퍼 — Dictionary<string, object> → Firebase.Analytics.Parameter[]
        // static Parameter[] ConvertParameters(IReadOnlyDictionary<string, object> source) { ... }
    }
}
