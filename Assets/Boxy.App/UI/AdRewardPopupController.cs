using System;
using UnityEngine.UIElements;

namespace Boxy.App.UI
{
    // AdRewardPopup.uxml 컨텍스트별 동적 구성 (힌트 / 되돌리기 / 이어하기).
    // PopupManager (Mound.UI)와 함께 사용 — Show 시 PopupManager로 모달 띄움, Hide는 자동 처리.
    public sealed class AdRewardPopupController
    {
        readonly VisualElement popup;
        readonly Label title;
        readonly Label body;
        readonly Button watchButton;
        readonly Button dismissButton;
        readonly Button closeButton;

        public event Action OnWatchAd;
        public event Action OnDismiss;

        public AdRewardPopupController(VisualElement popup)
        {
            this.popup = popup ?? throw new ArgumentNullException(nameof(popup));

            title = popup.Q<Label>("title");
            body = popup.Q<Label>("body");
            watchButton = popup.Q<Button>("watch-ad-button");
            dismissButton = popup.Q<Button>("dismiss-button");
            closeButton = popup.Q<Button>("close-button");

            if (watchButton != null) watchButton.clicked += FireWatch;
            if (dismissButton != null) dismissButton.clicked += FireDismiss;
            if (closeButton != null) closeButton.clicked += FireDismiss;
        }

        public void Detach()
        {
            if (watchButton != null) watchButton.clicked -= FireWatch;
            if (dismissButton != null) dismissButton.clicked -= FireDismiss;
            if (closeButton != null) closeButton.clicked -= FireDismiss;
        }

        public void Configure(string titleText, string bodyText, string ctaText)
        {
            if (title != null) title.text = titleText;
            if (body != null) body.text = bodyText;
            if (watchButton != null) watchButton.text = ctaText;
        }

        // 컨텍스트별 사전 설정 — boxy-plan §B-2-3 광고 동기 4종
        public void ConfigureForHint() =>
            Configure("힌트가 필요해요?", "짧은 광고를 보고 힌트 1개를 받아 퍼즐을 이어가세요", "광고 보기 → 힌트 +1");

        public void ConfigureForUndo() =>
            Configure("되돌리기 추가?", "짧은 광고를 보고 되돌리기 1회를 추가합니다", "광고 보기 → 되돌리기 +1");

        public void ConfigureForRetry() =>
            Configure("한 번 더 시도?", "짧은 광고를 보고 다시 도전하세요", "광고 보기 → 이어하기");

        void FireWatch() => OnWatchAd?.Invoke();
        void FireDismiss() => OnDismiss?.Invoke();
    }
}
