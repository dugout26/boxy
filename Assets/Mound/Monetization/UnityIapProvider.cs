using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Monetization
{
    // SDK_VERSIONS.md: Unity Gaming Services IAP — Day 1에 최신 패키지 버전 확인
    // 통합 시점: boxy-plan §C Week 3 Day 17
    //
    // Week 3 통합 절차:
    //   1. Window → Package Manager → Unity Registry → "In App Purchasing" 설치
    //   2. UGS 대시보드에서 Boxy 프로젝트 연결 + 상품 3종 등록
    //      - boxy_remove_ads (NonConsumable, ₩3,900)
    //      - boxy_hint_bundle_10 (Consumable, ₩2,500)
    //      - boxy_starter_pack (NonConsumable + Consumable hybrid, ₩6,600)
    //   3. Mound.Monetization.asmdef references에 UnityEngine.Purchasing 추가
    //   4. 아래 TODO 마크 SDK 호출로 교체 (CodelessIAPStoreListener / IStoreController)
    //   5. 영수증 검증: Unity ReceiptValidator 또는 자체 서버 검증 (v1.1)
    //
    // 현재 상태: 스켈레톤 — 모든 메서드 NullProvider 동작 (Failed 반환).
    public sealed class UnityIapProvider : IIapProvider
    {
        const string ProductRemoveAds = "boxy_remove_ads";
        const string ProductHintBundle10 = "boxy_hint_bundle_10";
        const string ProductStarterPack = "boxy_starter_pack";

        public UnityIapProvider()
        {
            // TODO Week 3:
            //   var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            //   builder.AddProduct(ProductRemoveAds, ProductType.NonConsumable);
            //   builder.AddProduct(ProductHintBundle10, ProductType.Consumable);
            //   builder.AddProduct(ProductStarterPack, ProductType.NonConsumable);
            //   UnityPurchasing.Initialize(this, builder);
            Debug.LogWarning("[UnityIapProvider] SDK 미통합 — Week 3 Day 17 대기. 모든 호출 Failed 반환.");
        }

        public Task<IapPurchaseInfo> PurchaseAsync(IapProduct product, CancellationToken ct)
        {
            // TODO Week 3:
            //   var tcs = new TaskCompletionSource<IapPurchaseInfo>();
            //   storeController.InitiatePurchase(MapProductId(product));
            //   pendingPurchaseTcs[product] = tcs;
            //   ProcessPurchase 콜백 → tcs.TrySetResult(...)
            //   ct.Register(() => tcs.TrySetCanceled());
            //   return tcs.Task;
            return Task.FromResult(new IapPurchaseInfo(product, IapResult.Failed, ""));
        }

        public Task<int> RestorePurchasesAsync(CancellationToken ct)
        {
            // TODO Week 3:
            //   #if UNITY_IOS
            //     storeExtensions.GetExtension<IAppleExtensions>().RestoreTransactions(...);
            //   #else
            //     // Android는 InitializeOnRestore에서 자동 복원
            //   #endif
            return Task.FromResult(0);
        }

        public bool IsOwned(IapProduct product)
        {
            // TODO Week 3: storeController.products.WithID(MapProductId(product)).hasReceipt
            return false;
        }

        public string GetLocalizedPrice(IapProduct product)
        {
            // TODO Week 3: storeController.products.WithID(MapProductId(product)).metadata.localizedPriceString
            return product switch
            {
                IapProduct.RemoveAds => "₩3,900",
                IapProduct.HintBundle10 => "₩2,500",
                IapProduct.StarterPack => "₩6,600",
                _ => "—"
            };
        }

        // TODO Week 3: IStoreListener 인터페이스 구현 (OnInitialized / OnInitializeFailed / ProcessPurchase / OnPurchaseFailed)
    }
}
