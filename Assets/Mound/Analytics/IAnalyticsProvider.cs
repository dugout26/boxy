using System.Collections.Generic;

namespace Mound.Analytics
{
    public enum AnalyticsEvent
    {
        // 튜토리얼 / 진행도 (boxy-plan §D-1, §D-2 funnel)
        TutorialStarted,
        TutorialCompleted,
        LevelStarted,
        LevelCompleted,
        LevelFailed,
        LevelRetried,

        // 광고 (보상형 시청률 측정)
        AdRewardedRequested,
        AdRewardedCompleted,
        AdRewardedDismissed,
        AdInterstitialShown,

        // IAP (ARPDAU)
        IapPurchaseInitiated,
        IapPurchaseCompleted,
        IapPurchaseFailed,

        // 세션 / 잔존
        SessionStarted,
        SessionEnded
    }

    // CLAUDE.md §5-3 SDK 격리 — Firebase Analytics / GameAnalytics / AppsFlyer를 이 인터페이스 뒤로 감춤
    // Boxy.App은 외부 SDK 타입 직접 노출 금지
    public interface IAnalyticsProvider
    {
        void Track(AnalyticsEvent evt, IReadOnlyDictionary<string, object> parameters = null);
        void SetUserProperty(string key, string value);
        void Flush();
    }

    // dev/Editor 빌드용 스텁. Production 빌드는 FirebaseAnalyticsProvider로 교체 (Week 3).
    public sealed class NullAnalyticsProvider : IAnalyticsProvider
    {
        public void Track(AnalyticsEvent evt, IReadOnlyDictionary<string, object> parameters = null)
        {
            // no-op stub
        }

        public void SetUserProperty(string key, string value)
        {
            // no-op stub
        }

        public void Flush()
        {
            // no-op stub
        }
    }
}
