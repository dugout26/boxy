using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Boxy.App.Gameplay.Input
{
    // boxy-plan §B-2: 길게 누르기 0.5s = 회전, 드래그 거리 임계 초과 = 이동.
    // UI Toolkit Manipulator + schedule.Execute(타이머)로 구현.
    public sealed class ItemDragManipulator : Manipulator
    {
        const float DragThresholdPx = 8f;
        const long LongPressMs = 500L;

        public event Action OnDragStart;
        public event Action<Vector2> OnDragMove;     // panel-space delta from start
        public event Action<Vector2> OnDragHover;    // panel-space absolute pos during drag (drop preview용)
        public event Action<Vector2> OnDragEnd;      // panel-space final position
        public event Action OnLongPress;

        Vector2 startPos;
        bool dragging;
        bool longPressFired;
        int activePointerId = PointerId.invalidPointerId;
        IVisualElementScheduledItem longPressTimer;

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            CancelLongPressTimer();
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (activePointerId != PointerId.invalidPointerId) return;

            activePointerId = evt.pointerId;
            startPos = evt.position;
            dragging = false;
            longPressFired = false;
            target.CapturePointer(evt.pointerId);

            CancelLongPressTimer();
            longPressTimer = target.schedule.Execute(TryFireLongPress).StartingIn(LongPressMs);

            evt.StopPropagation();
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != activePointerId) return;
            if (!target.HasPointerCapture(evt.pointerId)) return;

            Vector2 delta = (Vector2)evt.position - startPos;

            if (!dragging && delta.sqrMagnitude > DragThresholdPx * DragThresholdPx)
            {
                dragging = true;
                CancelLongPressTimer();    // 드래그 시작하면 long press 취소
                OnDragStart?.Invoke();
            }

            if (dragging)
            {
                OnDragMove?.Invoke(delta);
                OnDragHover?.Invoke(evt.position);
            }
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != activePointerId) return;
            target.ReleasePointer(evt.pointerId);

            CancelLongPressTimer();

            if (dragging)
            {
                OnDragEnd?.Invoke(evt.position);
            }

            ResetState();
        }

        void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId != activePointerId) return;
            target.ReleasePointer(evt.pointerId);
            CancelLongPressTimer();
            ResetState();
        }

        void TryFireLongPress()
        {
            if (dragging || longPressFired) return;
            longPressFired = true;
            OnLongPress?.Invoke();
        }

        void CancelLongPressTimer()
        {
            longPressTimer?.Pause();
            longPressTimer = null;
        }

        void ResetState()
        {
            activePointerId = PointerId.invalidPointerId;
            dragging = false;
            longPressFired = false;
        }
    }
}
