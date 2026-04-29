using System.Threading;
using System.Threading.Tasks;

namespace Mound.Monetization
{
    public enum AdPlacement
    {
        RewardedHint,
        RewardedUndo,
        RewardedRetry,
        Interstitial,
        Banner
    }

    public enum AdResult
    {
        Rewarded,    // 시청 완료 + 보상 지급
        Dismissed,   // 시청 중단
        Failed       // 로드/표시 실패 (인벤토리 부족 포함)
    }

    // CLAUDE.md §5-3 SDK 격리: 광고 SDK(AppLovin/AdMob)는 이 인터페이스 뒤로 감춤. Boxy.App은 외부 SDK 타입 직접 노출 X.
    public interface IAdProvider
    {
        Task<AdResult> ShowRewardedAsync(AdPlacement placement, CancellationToken ct);
        bool IsRewardedReady(AdPlacement placement);
        void TryShowInterstitial();
        void ShowBanner();
        void HideBanner();
    }
}
