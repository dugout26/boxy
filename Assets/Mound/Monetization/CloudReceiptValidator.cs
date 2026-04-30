using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Mound.Monetization
{
    // CLAUDE.md §11-4 P0-1: Supabase Edge Function (validate-iap-receipt)으로 서버 검증.
    // 클라 단독 검증 (Apple/GoogleReceiptValidator)보다 안전 — HTTPS proxy 응답 위조 차단 + transactions 테이블 중복 지급 차단.
    //
    // 흐름:
    //   1) Unity IAP 결제 성공 → Apple receipt or Google signed payload 획득
    //   2) CloudReceiptValidator.ValidateAsync() → POST {functionUrl}/validate-iap-receipt
    //   3) 서버에서 Apple/Google 직검증 + transactions 테이블 INSERT (UNIQUE 제약)
    //   4) {valid: true, transaction_id} 또는 {valid: false, reason}
    //
    // 클라 단독 (Apple/GoogleReceiptValidator)와 함께 사용 가능 — CompositeReceiptValidator로 fallback.
    public sealed class CloudReceiptValidator : IReceiptValidator
    {
        readonly string functionUrl;   // 예: https://<project-ref>.supabase.co/functions/v1/validate-iap-receipt
        readonly string anonKey;       // Supabase anon key (Authorization Bearer 헤더)

        public CloudReceiptValidator(string functionUrl, string anonKey)
        {
            this.functionUrl = functionUrl ?? "";
            this.anonKey = anonKey ?? "";
        }

        public async Task<ReceiptValidationResult> ValidateAsync(ReceiptInput input, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(functionUrl))
                return ReceiptValidationResult.Fail("cloud validator not configured");

            string platform = input.Platform switch
            {
                RuntimePlatform.IPhonePlayer => "apple",
                RuntimePlatform.OSXPlayer => "apple",
                RuntimePlatform.Android => "google",
                _ => "unknown"
            };
            if (platform == "unknown")
                return ReceiptValidationResult.Fail("unsupported platform for cloud validation");

            string body = $"{{\"platform\":\"{platform}\",\"receipt\":\"{Escape(input.Receipt)}\",\"productId\":\"{Escape(input.ProductId)}\",\"transactionId\":\"{Escape(input.TransactionId)}\"}}";

            using var req = new UnityWebRequest(functionUrl, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(anonKey))
                req.SetRequestHeader("Authorization", $"Bearer {anonKey}");
            req.timeout = 15;

            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested)
                {
                    req.Abort();
                    throw new OperationCanceledException(ct);
                }
                await Task.Yield();
            }

            if (req.result != UnityWebRequest.Result.Success)
                return ReceiptValidationResult.Fail($"http: {req.error}", input.TransactionId);

            try
            {
                var parsed = JsonUtility.FromJson<CloudResponse>(req.downloadHandler.text);
                if (parsed == null) return ReceiptValidationResult.Fail("empty response");
                if (parsed.valid) return ReceiptValidationResult.Ok(parsed.transaction_id ?? input.TransactionId);
                return ReceiptValidationResult.Fail(parsed.reason ?? "invalid", input.TransactionId);
            }
            catch (Exception e)
            {
                return ReceiptValidationResult.Fail($"parse: {e.Message}", input.TransactionId);
            }
        }

        static string Escape(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");

        [Serializable]
        class CloudResponse
        {
            public bool valid;
            public bool duplicate;
            public string transaction_id;
            public string reason;
        }
    }
}
