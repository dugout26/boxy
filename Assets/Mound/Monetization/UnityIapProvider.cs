using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Monetization
{
    // SDK 통합 가이드: decisions/2026-04-29-18-applovin-appsflyer-setup-guide.md
    // SDK 미통합 시 모든 메서드 Failed/0/false 반환 — 게임 진행은 차단되지 않음.
    // CLAUDE.md §11-4 P0-1: 영수증 서버 검증 통과한 트랜잭션만 Success 반환. validator null이면 검증 생략 (dev/Editor).
    public sealed class UnityIapProvider : IIapProvider
    {
        const string ProductRemoveAds = "boxy_remove_ads";
        const string ProductHintBundle10 = "boxy_hint_bundle_10";
        const string ProductStarterPack = "boxy_starter_pack";

        readonly IReceiptValidator validator;

        public UnityIapProvider(IReceiptValidator validator = null)
        {
            this.validator = validator;
            Debug.LogWarning($"[UnityIapProvider] SDK 미통합 — 모든 호출 Failed 반환. (validator={(validator==null?"null":"set")})");
        }

        public Task<IapPurchaseInfo> PurchaseAsync(IapProduct product, CancellationToken ct)
        {
            // SDK 통합 후 흐름: storeController.InitiatePurchase() → ProcessPurchase 콜백 → ValidateAsync(receipt) → Success/Failed
            return Task.FromResult(new IapPurchaseInfo(product, IapResult.Failed, ""));
        }

        public Task<int> RestorePurchasesAsync(CancellationToken ct)
        {
            return Task.FromResult(0);
        }

        public bool IsOwned(IapProduct product)
        {
            return false;
        }

        public string GetLocalizedPrice(IapProduct product)
        {
            return product switch
            {
                IapProduct.RemoveAds => "₩3,900",
                IapProduct.HintBundle10 => "₩2,500",
                IapProduct.StarterPack => "₩6,600",
                _ => "—"
            };
        }
    }
}
