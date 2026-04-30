using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Mound.UI;
using Boxy.App.UI;

namespace Boxy.App.Gameplay.UI
{
    public enum AdRewardContext
    {
        Hint,
        Undo,
        Retry
    }

    // AdRewardPopup + PopupManager + AdRewardPopupController 통합 래퍼.
    // boxy-plan §B-2-3 광고 동기 4종 중 보상형 (Hint/Undo/Retry) 진입 흐름.
    // 사용 패턴: bool consent = await flow.RequestAsync(context, ct); if (consent) → 실제 광고 표시
    public sealed class AdRewardFlow
    {
        readonly VisualElement popupRoot;
        readonly PopupManager popupManager;
        readonly AdRewardPopupController controller;
        TaskCompletionSource<bool> currentTcs;

        public AdRewardFlow(VisualElement popupRoot, PopupManager popupManager)
        {
            this.popupRoot = popupRoot ?? throw new ArgumentNullException(nameof(popupRoot));
            this.popupManager = popupManager ?? throw new ArgumentNullException(nameof(popupManager));

            controller = new AdRewardPopupController(popupRoot);
            controller.OnWatchAd += OnWatch;
            controller.OnDismiss += OnDismiss;
        }

        // 컨텍스트별 팝업 띄우고 사용자 동의 결과 비동기 반환. 이미 표시 중이면 기존 결과 공유.
        public Task<bool> RequestAsync(AdRewardContext context, CancellationToken ct)
        {
            if (currentTcs != null) return currentTcs.Task;

            currentTcs = new TaskCompletionSource<bool>();
            ct.Register(() =>
            {
                if (currentTcs == null) return;
                popupManager.Hide();
                currentTcs.TrySetCanceled();
                currentTcs = null;
            });

            switch (context)
            {
                case AdRewardContext.Hint: controller.ConfigureForHint(); break;
                case AdRewardContext.Undo: controller.ConfigureForUndo(); break;
                case AdRewardContext.Retry: controller.ConfigureForRetry(); break;
            }

            popupManager.Show(popupRoot);
            return currentTcs.Task;
        }

        public void Dispose()
        {
            controller.OnWatchAd -= OnWatch;
            controller.OnDismiss -= OnDismiss;
            controller.Detach();
            currentTcs?.TrySetCanceled();
            currentTcs = null;
        }

        void OnWatch()
        {
            popupManager.Hide();
            currentTcs?.TrySetResult(true);
            currentTcs = null;
        }

        void OnDismiss()
        {
            popupManager.Hide();
            currentTcs?.TrySetResult(false);
            currentTcs = null;
        }
    }
}
