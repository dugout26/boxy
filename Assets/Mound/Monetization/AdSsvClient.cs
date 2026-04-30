using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Mound.Monetization
{
    // CLAUDE.md §11-4: 보상형 광고 SSV (Server-Side Verification) 클라이언트.
    // 흐름:
    //   1) 클라가 nonce 생성 (Guid.NewGuid().ToString("N"))
    //   2) AppLovin/AdMob ShowRewardedAd 호출 시 nonce를 custom_data로 전달
    //   3) 광고 네트워크가 보상 발생 시 Edge Function (reward-ad-ssv)에 콜백 — nonce + granted=true INSERT
    //   4) 클라는 광고 종료 후 PollAsync(nonce) → granted=true면 보상 지급
    //
    // 자동화 우회 차단 효과: 광고 네트워크 콜백 없으면 nonce 미존재 → 보상 지급 차단.
    public sealed class AdSsvClient
    {
        readonly string functionUrl;   // 예: https://<project-ref>.supabase.co/functions/v1/reward-ad-ssv
        readonly string anonKey;
        readonly int pollMaxSeconds;

        public AdSsvClient(string functionUrl, string anonKey, int pollMaxSeconds = 8)
        {
            this.functionUrl = functionUrl ?? "";
            this.anonKey = anonKey ?? "";
            this.pollMaxSeconds = Math.Max(1, pollMaxSeconds);
        }

        public static string GenerateNonce() => Guid.NewGuid().ToString("N");

        // 광고 종료 후 호출 — granted=true 반환 시 보상 지급. timeout/실패 시 false.
        public async Task<bool> PollGrantedAsync(string nonce, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(functionUrl) || string.IsNullOrEmpty(nonce)) return false;

            // 1초 간격 polling, max pollMaxSeconds초.
            // 일반적으로 광고 네트워크가 완료 직후 1-3초 안에 SSV 콜백 발사. 8초면 충분.
            int attempts = pollMaxSeconds;
            while (attempts > 0 && !ct.IsCancellationRequested)
            {
                bool granted = await CheckOnce(nonce, ct);
                if (granted) return true;
                await Task.Delay(1000, ct);
                attempts--;
            }
            return false;
        }

        async Task<bool> CheckOnce(string nonce, CancellationToken ct)
        {
            string url = $"{functionUrl}?poll=1&nonce={Uri.EscapeDataString(nonce)}";
            using var req = UnityWebRequest.Get(url);
            if (!string.IsNullOrEmpty(anonKey))
                req.SetRequestHeader("Authorization", $"Bearer {anonKey}");
            req.timeout = 5;

            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested)
                {
                    req.Abort();
                    return false;
                }
                await Task.Yield();
            }

            if (req.result != UnityWebRequest.Result.Success) return false;
            try
            {
                var parsed = JsonUtility.FromJson<PollResponse>(req.downloadHandler.text);
                return parsed?.granted == true;
            }
            catch
            {
                return false;
            }
        }

        [Serializable]
        class PollResponse
        {
            public bool granted;
            public bool found;
            public string placement;
        }
    }
}
