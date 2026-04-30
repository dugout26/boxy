using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Mound.Monetization;

namespace Boxy.App.Gameplay
{
    // 무료 되돌리기 우선 시도 → 소진 시 광고 시청 → 사용권 부여 + 즉시 1회 소비
    // boxy-plan §B-2-2: 무료 3회 + 광고 보상 추가권
    // CLAUDE.md §11-4: 광고 보상 중복 지급 방지 — in-flight lock으로 진행 중 재호출 차단
    public sealed class UndoAdController
    {
        readonly IAdProvider adProvider;
        readonly GameplayController gameplay;
        Task<bool> inFlight;

        public UndoAdController(IAdProvider adProvider, GameplayController gameplay)
        {
            this.adProvider = adProvider ?? throw new ArgumentNullException(nameof(adProvider));
            this.gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
        }

        public Task<bool> RequestUndoAsync(CancellationToken ct)
        {
            // 무료 사용 가능하면 광고 없이 즉시 — lock도 불필요
            if (gameplay.TryUndoFree()) return Task.FromResult(true);

            if (inFlight != null) return inFlight;
            inFlight = ExecuteAsync(ct);
            return inFlight;
        }

        async Task<bool> ExecuteAsync(CancellationToken ct)
        {
            try
            {
                if (!adProvider.IsRewardedReady(AdPlacement.RewardedUndo))
                {
                    Debug.LogWarning("[UndoAdController] Rewarded ad not ready");
                    return false;
                }

                var result = await adProvider.ShowRewardedAsync(AdPlacement.RewardedUndo, ct);
                if (result != AdResult.Rewarded) return false;

                gameplay.GrantPaidUndo();
                return gameplay.TryUndoPaid();
            }
            finally
            {
                inFlight = null;
            }
        }
    }
}
