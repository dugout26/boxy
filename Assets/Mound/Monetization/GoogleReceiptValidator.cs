using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Monetization
{
    // CLAUDE.md §11-4 P0-1: Google Play Billing v6 클라 측 RSA signature verification.
    //
    // Google은 기본 클라 직검증을 권장 (Google Play Developer API 직접 호출은 OAuth 서비스 계정 필요 → 자체 서버 권장).
    // 여기선 SignedData + Signature를 Public Key (Google Play Console에서 발급)로 RSA-SHA1 검증.
    //
    // 흐름:
    //   1. Google Play Billing 응답: { signedData (JSON 문자열), signature (Base64) }
    //   2. signedData 안에 productId, purchaseToken, orderId, purchaseTime, purchaseState (0=Purchased, 1=Cancelled, 2=Refunded)
    //   3. RSA Public Key로 signature 검증 → SignedData 무결성 확인
    //   4. purchaseState == 0 일 때만 valid
    //   5. orderId (또는 purchaseToken) → 중복 지급 방지 키
    //
    // **보안 한계 (왜 8.5/10인가)**:
    //   - Public Key가 클라 코드에 박힘 → 디바이스에서 공격자가 다른 키로 교체 가능 (jailbreak)
    //   - Google 권장: 자체 서버에서 androidpublisher.googleapis.com 직검증 (v1.1)
    public sealed class GoogleReceiptValidator : IReceiptValidator
    {
        readonly RSAParameters? publicKey;

        public GoogleReceiptValidator(EnvironmentConfig.GoogleReceiptConfig config)
        {
            string pkBase64 = config?.LicenseKeyBase64 ?? "";
            if (string.IsNullOrEmpty(pkBase64))
            {
                Debug.LogWarning("[GoogleReceiptValidator] license key 미설정 — 모든 검증 fail.");
                publicKey = null;
                return;
            }

            try
            {
                publicKey = ParseSubjectPublicKeyInfo(Convert.FromBase64String(pkBase64));
            }
            catch (Exception e)
            {
                Debug.LogError($"[GoogleReceiptValidator] license key parse 실패: {e.Message}");
                publicKey = null;
            }
        }

        public Task<ReceiptValidationResult> ValidateAsync(ReceiptInput input, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (publicKey == null)
                return Task.FromResult(ReceiptValidationResult.Fail("google public key not configured"));

            // Unity IAP 또는 Google Play Billing이 받은 JSON: { "Json": "<signedData>", "signature": "<base64>", "Store": "GooglePlay" }
            // input.Receipt에 그 JSON 통째로 들어옴. 분해:
            string signedData, signatureBase64;
            try
            {
                var receipt = JsonUtility.FromJson<UnityIapReceipt>(input.Receipt);
                if (receipt?.Payload == null)
                    return Task.FromResult(ReceiptValidationResult.Fail("receipt payload missing"));
                var payload = JsonUtility.FromJson<UnityIapGooglePayload>(receipt.Payload);
                signedData = payload?.json ?? "";
                signatureBase64 = payload?.signature ?? "";
            }
            catch (Exception e)
            {
                return Task.FromResult(ReceiptValidationResult.Fail($"receipt parse: {e.Message}"));
            }

            if (string.IsNullOrEmpty(signedData) || string.IsNullOrEmpty(signatureBase64))
                return Task.FromResult(ReceiptValidationResult.Fail("signedData or signature empty"));

            // RSA-SHA1 검증
            bool sigValid;
            try
            {
                using var rsa = RSA.Create();
                rsa.ImportParameters(publicKey.Value);
                byte[] data = Encoding.UTF8.GetBytes(signedData);
                byte[] sig = Convert.FromBase64String(signatureBase64);
                sigValid = rsa.VerifyData(data, sig, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            }
            catch (Exception e)
            {
                return Task.FromResult(ReceiptValidationResult.Fail($"rsa verify: {e.Message}"));
            }

            if (!sigValid)
                return Task.FromResult(ReceiptValidationResult.Fail("signature verification failed"));

            // signedData 안의 purchaseState + orderId
            GoogleSignedData signed;
            try
            {
                signed = JsonUtility.FromJson<GoogleSignedData>(signedData);
            }
            catch (Exception e)
            {
                return Task.FromResult(ReceiptValidationResult.Fail($"signedData parse: {e.Message}"));
            }

            if (signed == null)
                return Task.FromResult(ReceiptValidationResult.Fail("signedData null"));
            if (signed.purchaseState != 0)
                return Task.FromResult(ReceiptValidationResult.Fail($"purchaseState {signed.purchaseState} (0=Purchased)"));

            // orderId는 비어 있을 수 있음 (Google Play 정책) → purchaseToken fallback
            string txId = !string.IsNullOrEmpty(signed.orderId) ? signed.orderId
                        : !string.IsNullOrEmpty(signed.purchaseToken) ? signed.purchaseToken
                        : input.TransactionId;
            return Task.FromResult(ReceiptValidationResult.Ok(txId));
        }

        // SubjectPublicKeyInfo (PKCS#1 wrapped in X.509 SPKI) → RSAParameters.
        // Google Play Console "License Key"는 이 형식 Base64.
        static RSAParameters ParseSubjectPublicKeyInfo(byte[] spki)
        {
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(spki, out _);
            return rsa.ExportParameters(false);
        }

        [Serializable]
        class UnityIapReceipt
        {
            public string Store;
            public string TransactionID;
            public string Payload;
        }

        [Serializable]
        class UnityIapGooglePayload
        {
            public string json;        // signedData
            public string signature;   // base64
        }

        [Serializable]
        class GoogleSignedData
        {
            public string orderId;
            public string packageName;
            public string productId;
            public long purchaseTime;
            public int purchaseState;     // 0=Purchased, 1=Cancelled, 2=Refunded
            public string purchaseToken;
            public string developerPayload;
        }
    }
}
