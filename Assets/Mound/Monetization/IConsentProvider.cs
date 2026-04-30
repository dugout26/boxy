using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Monetization
{
    public enum ConsentStatus
    {
        Unknown,        // 아직 체크 안 함
        Required,       // 동의 필요 (EU GDPR / CCPA / iOS ATT)
        NotRequired,    // 비EU 사용자 등 동의 불필요
        Obtained,       // 사용자 동의 완료
        Denied          // 사용자 거부
    }

    // GDPR (EU) / CCPA (CA) / ATT (iOS 14.5+) 통합 동의 인터페이스.
    // CLAUDE.md §5-3 SDK 격리: Google UMP / iOS ATT 직접 노출 X.
    public interface IConsentProvider
    {
        ConsentStatus Status { get; }
        Task<ConsentStatus> RequestAsync(CancellationToken ct);
        bool CanShowAds();           // 동의 결과에 따라 광고 표시 가능 여부
        bool CanShowPersonalizedAds(); // 비개인화 vs 개인화 광고 분기
    }

    // dev/Editor 스텁. 항상 NotRequired 반환 — 동의 흐름 우회.
    // Production: GoogleUmpConsentProvider 또는 iOS ATTConsentProvider로 교체.
    public sealed class NullConsentProvider : IConsentProvider
    {
        public ConsentStatus Status => ConsentStatus.NotRequired;

        public Task<ConsentStatus> RequestAsync(CancellationToken ct)
        {
            return Task.FromResult(ConsentStatus.NotRequired);
        }

        public bool CanShowAds() => true;
        public bool CanShowPersonalizedAds() => true;
    }

    // Google User Messaging Platform (UMP) 래퍼.
    // SDK 통합 가이드: decisions/2026-04-29-18-applovin-appsflyer-setup-guide.md
    // 미통합 시 Status를 NotRequired로 반환 — 동의 흐름 우회.
    public sealed class GoogleUmpConsentProvider : IConsentProvider
    {
        public ConsentStatus Status { get; private set; } = ConsentStatus.Unknown;

        public Task<ConsentStatus> RequestAsync(CancellationToken ct)
        {
            Debug.LogWarning("[GoogleUmpConsentProvider] SDK 미통합 — NotRequired 반환.");
            Status = ConsentStatus.NotRequired;
            return Task.FromResult(Status);
        }

        public bool CanShowAds()
        {
            return Status != ConsentStatus.Denied;
        }

        public bool CanShowPersonalizedAds()
        {
            return Status == ConsentStatus.Obtained || Status == ConsentStatus.NotRequired;
        }
    }
}
