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

    // Google User Messaging Platform (UMP) 래퍼 — Google Mobile Ads Unity Plugin v11.0.0 포함.
    // 통합 시점: boxy-plan §C Week 3 Day 20.
    //
    // Week 3 통합 절차:
    //   1. Google Mobile Ads Unity Plugin 임포트 (이미 AppLovin 미디에이션 구성 시 포함)
    //   2. AdMob 대시보드에서 UMP form 생성 + GDPR 메시지 등록
    //   3. 아래 TODO 마크 SDK 호출로 교체
    //   4. CLAUDE.md §11-3 iOS ATT는 별도 (이 클래스는 GDPR/CCPA 전용)
    //
    // 현재 상태: 스켈레톤 — Status를 항상 NotRequired로 반환 (동의 흐름 우회).
    public sealed class GoogleUmpConsentProvider : IConsentProvider
    {
        public ConsentStatus Status { get; private set; } = ConsentStatus.Unknown;

        public Task<ConsentStatus> RequestAsync(CancellationToken ct)
        {
            // TODO Week 3 Day 20:
            //   var tcs = new TaskCompletionSource<ConsentStatus>();
            //   var requestParams = new ConsentRequestParameters { TagForUnderAgeOfConsent = false };
            //   ConsentInformation.Update(requestParams, error => {
            //     if (error != null) { tcs.TrySetResult(ConsentStatus.NotRequired); return; }
            //     ConsentForm.LoadAndShowConsentFormIfRequired(formError => {
            //       Status = ConsentInformation.ConsentStatus == ConsentStatus.Obtained
            //         ? ConsentStatus.Obtained : ConsentStatus.NotRequired;
            //       tcs.TrySetResult(Status);
            //     });
            //   });
            //   ct.Register(() => tcs.TrySetCanceled());
            //   return tcs.Task;
            Debug.LogWarning("[GoogleUmpConsentProvider] SDK 미통합 — Week 3 Day 20 대기. NotRequired 반환.");
            Status = ConsentStatus.NotRequired;
            return Task.FromResult(Status);
        }

        public bool CanShowAds()
        {
            // TODO Week 3: ConsentInformation.CanRequestAds()
            return Status != ConsentStatus.Denied;
        }

        public bool CanShowPersonalizedAds()
        {
            // TODO Week 3: ConsentInformation.PrivacyOptionsRequirementStatus 등 체크
            return Status == ConsentStatus.Obtained || Status == ConsentStatus.NotRequired;
        }
    }
}
