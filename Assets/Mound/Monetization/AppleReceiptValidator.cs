using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Mound.Monetization
{
    // CLAUDE.md §11-4 P0-1: Apple App Store 영수증 검증 (클라 직검증).
    //
    // Apple verifyReceipt API (deprecated 2026 일정 발표 — App Store Server API 권장이지만 v1.0은 verifyReceipt 그대로).
    // 자체 서버 없으면 클라 직검증 — 캐주얼 99% 우회 차단. v1.1 자체 서버로 강화.
    //
    // 흐름:
    //   1. Sandbox endpoint(/verifyReceipt) 우선 시도 (status 21007 = production receipt → production endpoint 재시도)
    //   2. status 0 = valid, 그 외 코드는 실패 사유
    //   3. originalTransactionId 추출 → 중복 지급 방지 키로 사용
    //
    // **보안 한계 (왜 8.5/10인가)**:
    //   - shared_secret이 클라에 박힘 → 추출 가능. 자체 서버에서 검증해야 100%.
    //   - HTTPS proxy로 응답 위조 가능. v1.1 자체 서버 + cert pinning 권장.
    public sealed class AppleReceiptValidator : IReceiptValidator
    {
        const string SandboxUrl = "https://sandbox.itunes.apple.com/verifyReceipt";
        const string ProductionUrl = "https://buy.itunes.apple.com/verifyReceipt";

        readonly string sharedSecret;
        readonly bool startInSandbox;

        public AppleReceiptValidator(EnvironmentConfig.AppleReceiptConfig config)
        {
            this.sharedSecret = config?.SharedSecret ?? "";
            this.startInSandbox = config?.UseSandbox ?? true;
        }

        public async Task<ReceiptValidationResult> ValidateAsync(ReceiptInput input, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(input.Receipt))
                return ReceiptValidationResult.Fail("empty receipt");

            // 1차 endpoint
            string firstUrl = startInSandbox ? SandboxUrl : ProductionUrl;
            var first = await VerifyAsync(firstUrl, input.Receipt, ct);

            // 21007 = sandbox receipt sent to production / 21008 = production receipt sent to sandbox → 다른 endpoint 재시도
            if (first.StatusCode == 21007)
                return MapResult(await VerifyAsync(SandboxUrl, input.Receipt, ct), input);
            if (first.StatusCode == 21008)
                return MapResult(await VerifyAsync(ProductionUrl, input.Receipt, ct), input);

            return MapResult(first, input);
        }

        ReceiptValidationResult MapResult(VerifyResponse response, ReceiptInput input)
        {
            if (response.StatusCode == 0)
            {
                // valid — originalTransactionId 우선, 없으면 input의 transactionId
                string txId = string.IsNullOrEmpty(response.OriginalTransactionId)
                    ? input.TransactionId
                    : response.OriginalTransactionId;
                return ReceiptValidationResult.Ok(txId);
            }
            return ReceiptValidationResult.Fail($"apple status {response.StatusCode}: {AppleStatusMessage(response.StatusCode)}", input.TransactionId);
        }

        async Task<VerifyResponse> VerifyAsync(string url, string receiptBase64, CancellationToken ct)
        {
            // Apple 권장 body: { "receipt-data": "...", "password": "shared_secret", "exclude-old-transactions": false }
            string body;
            if (string.IsNullOrEmpty(sharedSecret))
                body = $"{{\"receipt-data\":\"{EscapeJson(receiptBase64)}\"}}";
            else
                body = $"{{\"receipt-data\":\"{EscapeJson(receiptBase64)}\",\"password\":\"{EscapeJson(sharedSecret)}\"}}";

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
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
                return new VerifyResponse { StatusCode = -1, ErrorMessage = req.error };

            return ParseResponse(req.downloadHandler.text);
        }

        static VerifyResponse ParseResponse(string json)
        {
            // 단순 파싱 — JsonUtility는 nested 'receipt' 객체 일부만 필요해서 wrapper 클래스 사용.
            try
            {
                var parsed = JsonUtility.FromJson<AppleVerifyJson>(json);
                return new VerifyResponse
                {
                    StatusCode = parsed?.status ?? -1,
                    OriginalTransactionId = ExtractOriginalTxId(json),
                };
            }
            catch (Exception)
            {
                return new VerifyResponse { StatusCode = -1, ErrorMessage = "json parse failed" };
            }
        }

        // verifyReceipt 응답 깊이가 가변이라 정확한 파싱은 외부 lib 필요. 실용적 접근:
        // top-level "original_transaction_id" 또는 "in_app[*].original_transaction_id" 찾기.
        static string ExtractOriginalTxId(string json)
        {
            const string key = "\"original_transaction_id\"";
            int idx = json.IndexOf(key, StringComparison.Ordinal);
            if (idx < 0) return "";
            int colon = json.IndexOf(':', idx + key.Length);
            if (colon < 0) return "";
            int q1 = json.IndexOf('"', colon);
            if (q1 < 0) return "";
            int q2 = json.IndexOf('"', q1 + 1);
            if (q2 < 0) return "";
            return json.Substring(q1 + 1, q2 - q1 - 1);
        }

        static string EscapeJson(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        static string AppleStatusMessage(int status) => status switch
        {
            21000 => "App Store가 보낸 JSON 객체를 읽을 수 없음",
            21002 => "receipt-data 데이터 형식이 잘못됨 또는 인증 실패",
            21003 => "영수증 인증 불가",
            21004 => "shared secret 불일치",
            21005 => "영수증 서버 일시 사용 불가",
            21006 => "구독 만료",
            21007 => "sandbox 영수증을 production endpoint에 전송 (재시도 필요)",
            21008 => "production 영수증을 sandbox endpoint에 전송 (재시도 필요)",
            21010 => "영수증 인증 불가 (아무 transaction history 없음)",
            _ => "알 수 없는 상태"
        };

        class VerifyResponse
        {
            public int StatusCode;
            public string OriginalTransactionId;
            public string ErrorMessage;
        }

        [Serializable]
        class AppleVerifyJson
        {
            public int status;
            public string environment;
        }
    }
}
