using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Mound.Monetization;

namespace Boxy.App.Gameplay
{
    // 보상형 광고 시청 후 힌트 1회 사용권 → MarkHintUsed (별 ★★ 자격 박탈, boxy-plan §B-2-1)
    // CLAUDE.md §11-4: 광고 보상 중복 지급 방지 — in-flight lock + 보상 처리 후 nonce 사용 (진행 중 재호출 차단)
    public sealed class HintAdController
    {
        readonly IAdProvider adProvider;
        readonly GameplayController gameplay;
        Task<bool> inFlight;

        public HintAdController(IAdProvider adProvider, GameplayController gameplay)
        {
            this.adProvider = adProvider ?? throw new ArgumentNullException(nameof(adProvider));
            this.gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
        }

        public Task<bool> RequestHintAsync(CancellationToken ct)
        {
            // 이미 진행 중이면 같은 task 공유 — 중복 클릭 시 보상 두 번 지급 방지
            if (inFlight != null) return inFlight;
            inFlight = ExecuteAsync(ct);
            return inFlight;
        }

        async Task<bool> ExecuteAsync(CancellationToken ct)
        {
            try
            {
                if (!adProvider.IsRewardedReady(AdPlacement.RewardedHint))
                {
                    Debug.LogWarning("[HintAdController] Rewarded ad not ready");
                    return false;
                }

                var result = await adProvider.ShowRewardedAsync(AdPlacement.RewardedHint, ct);
                if (result != AdResult.Rewarded) return false;

                gameplay.MarkHintUsed();
                return true;
            }
            finally
            {
                inFlight = null;
            }
        }
    }
}
