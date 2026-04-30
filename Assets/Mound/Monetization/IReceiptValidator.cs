using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Monetization
{
    // CLAUDE.md §11-4 P0-1: IAP 영수증 서버 검증.
    // Apple sandbox: https://sandbox.itunes.apple.com/verifyReceipt
    // Google: androidpublisher.googleapis.com/androidpublisher/v3/applications/{pkg}/purchases/products/{sku}/tokens/{token}
    // 자체 서버 없으면 클라이언트 직검증 (캐주얼 99% 우회 차단). v1.1 자체 서버로 강화 예정.
    public interface IReceiptValidator
    {
        Task<ReceiptValidationResult> ValidateAsync(
            ReceiptInput input,
            CancellationToken ct);
    }

    public readonly struct ReceiptInput
    {
        public readonly string ProductId;
        public readonly string Receipt;             // iOS: app receipt base64. Android: purchase token + signature JSON
        public readonly string TransactionId;       // iOS: originalTransactionId. Android: orderId 또는 purchaseToken
        public readonly RuntimePlatform Platform;

        public ReceiptInput(string productId, string receipt, string transactionId, RuntimePlatform platform)
        {
            ProductId = productId;
            Receipt = receipt;
            TransactionId = transactionId;
            Platform = platform;
        }
    }

    public readonly struct ReceiptValidationResult
    {
        public readonly bool IsValid;
        public readonly string TransactionId;
        public readonly string Reason;              // 실패 시 사유 (로그 + crashReporter.Log)

        public ReceiptValidationResult(bool isValid, string transactionId, string reason = "")
        {
            IsValid = isValid;
            TransactionId = transactionId;
            Reason = reason ?? "";
        }

        public static ReceiptValidationResult Ok(string transactionId) =>
            new ReceiptValidationResult(true, transactionId);
        public static ReceiptValidationResult Fail(string reason, string transactionId = "") =>
            new ReceiptValidationResult(false, transactionId, reason);
    }

    // dev/Editor 스텁 — 항상 valid. Production 빌드는 AppleReceiptValidator 또는 GoogleReceiptValidator 또는 CompositeReceiptValidator로 교체.
    public sealed class NullReceiptValidator : IReceiptValidator
    {
        public Task<ReceiptValidationResult> ValidateAsync(ReceiptInput input, CancellationToken ct)
        {
            Debug.LogWarning($"[NullReceiptValidator] dev/Editor 스텁 — 항상 통과. 출시 전 실제 검증기로 교체 필수.");
            return Task.FromResult(ReceiptValidationResult.Ok(input.TransactionId));
        }
    }

    // 플랫폼별 자동 라우팅 — RuntimePlatform 값으로 Apple / Google validator 선택.
    public sealed class CompositeReceiptValidator : IReceiptValidator
    {
        readonly IReceiptValidator apple;
        readonly IReceiptValidator google;

        public CompositeReceiptValidator(IReceiptValidator apple, IReceiptValidator google)
        {
            this.apple = apple;
            this.google = google;
        }

        public Task<ReceiptValidationResult> ValidateAsync(ReceiptInput input, CancellationToken ct)
        {
            switch (input.Platform)
            {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXPlayer:
                    return apple?.ValidateAsync(input, ct) ?? Task.FromResult(ReceiptValidationResult.Fail("apple validator missing"));
                case RuntimePlatform.Android:
                    return google?.ValidateAsync(input, ct) ?? Task.FromResult(ReceiptValidationResult.Fail("google validator missing"));
                default:
                    // dev/Editor — null 검증기 fallback
                    return Task.FromResult(ReceiptValidationResult.Ok(input.TransactionId));
            }
        }
    }
}
