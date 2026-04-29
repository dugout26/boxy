using System.Threading;
using System.Threading.Tasks;

namespace Mound.Monetization
{
    public enum IapProduct
    {
        RemoveAds,        // boxy-plan §B-4: 광고 제거 ₩3,900 (비소비성)
        HintBundle10,     // 힌트 10개 묶음 ₩2,500 (소비성)
        StarterPack       // 광고제거 + 힌트 15 + 되돌리기 10 ₩6,600 (혼합)
    }

    public enum IapResult
    {
        Purchased,
        Restored,
        Cancelled,
        Failed,
        AlreadyOwned      // 비소비성 중복 구매 시도
    }

    public readonly struct IapPurchaseInfo
    {
        public readonly IapProduct Product;
        public readonly IapResult Result;
        public readonly string TransactionId;

        public IapPurchaseInfo(IapProduct product, IapResult result, string transactionId)
        {
            Product = product;
            Result = result;
            TransactionId = transactionId;
        }
    }

    // CLAUDE.md §5-3 SDK 격리 — Unity Gaming Services IAP는 이 인터페이스 뒤로 감춤
    public interface IIapProvider
    {
        Task<IapPurchaseInfo> PurchaseAsync(IapProduct product, CancellationToken ct);
        Task<int> RestorePurchasesAsync(CancellationToken ct);
        bool IsOwned(IapProduct product);
        string GetLocalizedPrice(IapProduct product);
    }
}
