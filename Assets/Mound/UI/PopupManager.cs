using System;
using UnityEngine.UIElements;

namespace Mound.UI
{
    // 단일 컨테이너에 한 번에 한 개 popup만 표시 (modal). Show 시 이전 popup 자동 hide.
    // Boxy v1.0 사용처: ResultPopup, AdRewardPopup, IAP confirmation 등.
    public sealed class PopupManager
    {
        readonly VisualElement container;
        VisualElement current;

        public bool IsShowing => current != null;
        public VisualElement Current => current;

        public event Action<VisualElement> OnPopupShown;
        public event Action<VisualElement> OnPopupHidden;

        public PopupManager(VisualElement container)
        {
            this.container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public void Show(VisualElement popup)
        {
            if (popup == null) return;
            if (current == popup) return;     // 이미 표시 중이면 no-op

            HideCurrent();
            container.Add(popup);
            popup.BringToFront();
            current = popup;
            OnPopupShown?.Invoke(popup);
        }

        public void Hide()
        {
            HideCurrent();
        }

        void HideCurrent()
        {
            if (current == null) return;
            var hiding = current;
            current.RemoveFromHierarchy();
            current = null;
            OnPopupHidden?.Invoke(hiding);
        }
    }
}
