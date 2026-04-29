using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Mound.UI
{
    // mound-design-system v2 §4.3: 화면 하단 토스트 — bottom 16px, 2.5s 자동 사라짐.
    // 큐 기반: 동시에 한 개만 표시, 다음 토스트는 큐에서 대기.
    // Boxy 사용처: 광고 보상 지급 / 저장 실패 / 네트워크 오류 (boxy-plan §B-2-2 보상 동기 강화)
    public sealed class ToastManager
    {
        const long ToastDurationMs = 2500;

        readonly VisualElement container;
        readonly Queue<string> queue = new Queue<string>();
        VisualElement currentToast;
        IVisualElementScheduledItem hideTimer;

        public ToastManager(VisualElement container)
        {
            this.container = container ?? throw new System.ArgumentNullException(nameof(container));
        }

        public void Show(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            queue.Enqueue(text);
            if (currentToast == null) ShowNext();
        }

        public void Clear()
        {
            queue.Clear();
            hideTimer?.Pause();
            hideTimer = null;
            HideCurrent();
        }

        void ShowNext()
        {
            if (queue.Count == 0 || currentToast != null) return;

            string text = queue.Dequeue();
            var label = new Label(text);
            label.AddToClassList("toast");

            container.Add(label);
            currentToast = label;

            hideTimer = label.schedule.Execute(OnHideTimer).StartingIn(ToastDurationMs);
        }

        void OnHideTimer()
        {
            HideCurrent();
            ShowNext();
        }

        void HideCurrent()
        {
            if (currentToast == null) return;
            currentToast.RemoveFromHierarchy();
            currentToast = null;
        }
    }
}
