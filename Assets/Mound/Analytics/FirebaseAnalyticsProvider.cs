using System.Collections.Generic;
using UnityEngine;

namespace Mound.Analytics
{
    // Firebase Analytics 래퍼.
    // SDK 통합 가이드: decisions/2026-04-29-17-firebase-ios-setup-guide.md
    // 미통합 시 Track 호출은 Debug.Log만, 실제 전송 X.
    public sealed class FirebaseAnalyticsProvider : IAnalyticsProvider
    {
        public FirebaseAnalyticsProvider()
        {
            Debug.LogWarning("[FirebaseAnalyticsProvider] SDK 미통합 — Track 호출 시 Debug.Log만.");
        }

        public void Track(AnalyticsEvent evt, IReadOnlyDictionary<string, object> parameters = null)
        {
            int paramCount = parameters?.Count ?? 0;
            Debug.Log($"[Analytics] {evt} ({paramCount} params)");
        }

        public void SetUserProperty(string key, string value)
        {
            Debug.Log($"[Analytics] SetUserProperty {key}={value}");
        }

        public void Flush()
        {
            // Firebase Analytics는 자동 배치/플러시 — 별도 호출 불필요
        }
    }
}
