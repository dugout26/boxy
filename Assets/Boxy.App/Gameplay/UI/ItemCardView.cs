using System;
using UnityEngine;
using UnityEngine.UIElements;
using Boxy.App.Gameplay.Domain;
using Boxy.App.Gameplay.Input;

namespace Boxy.App.Gameplay.UI
{
    // 아이템 카드 — 모양 + 회전 상태 + 드래그/회전 입력. 드롭 처리는 GameplayUI 리스너가 결정.
    public sealed class ItemCardView : VisualElement
    {
        public const float CellSizePx = 24f;
        public const float CardPaddingPx = 8f;

        public string ItemKey { get; }
        public ItemShape CurrentShape { get; private set; }
        public Rotation CurrentRotation { get; private set; }

        // OnDropped: panel-space 최종 위치 + 자기 자신 (호출자가 placement/복귀 결정)
        public event Action<ItemCardView, Vector2> OnDropped;
        public event Action<ItemCardView> OnRotated;
        public event Action<ItemCardView, Vector2> OnHovering;   // 드래그 중 panel pos (drop preview용)

        readonly ItemShape baseShape;
        readonly Color tint;
        readonly VisualElement bounds;
        readonly ItemDragManipulator manipulator;

        public ItemCardView(string itemKey, ItemShape shape, Color tint)
        {
            ItemKey = itemKey;
            baseShape = shape;
            CurrentShape = shape;
            CurrentRotation = Rotation.Deg0;
            this.tint = tint;
            name = $"item-{itemKey}";

            AddToClassList("item-card");
            style.flexDirection = FlexDirection.Column;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            style.marginRight = 8f;
            style.paddingTop = CardPaddingPx;
            style.paddingBottom = CardPaddingPx;
            style.paddingLeft = CardPaddingPx;
            style.paddingRight = CardPaddingPx;

            bounds = new VisualElement { name = "shape-bounds" };
            bounds.style.position = Position.Relative;
            Add(bounds);

            Rebuild();

            manipulator = new ItemDragManipulator();
            manipulator.OnDragStart += HandleDragStart;
            manipulator.OnDragMove += HandleDragMove;
            manipulator.OnDragHover += HandleDragHover;
            manipulator.OnDragEnd += HandleDragEnd;
            manipulator.OnLongPress += HandleLongPress;
            this.AddManipulator(manipulator);
        }

        void HandleDragHover(Vector2 panelPos) => OnHovering?.Invoke(this, panelPos);

        public void RestoreVisual()
        {
            transform.position = Vector3.zero;
            BringToFront();
        }

        void Rebuild()
        {
            bounds.Clear();

            int maxX = 0;
            int maxY = 0;
            for (int i = 0; i < CurrentShape.Cells.Count; i++)
            {
                if (CurrentShape.Cells[i].x > maxX) maxX = CurrentShape.Cells[i].x;
                if (CurrentShape.Cells[i].y > maxY) maxY = CurrentShape.Cells[i].y;
            }
            bounds.style.width = (maxX + 1) * CellSizePx;
            bounds.style.height = (maxY + 1) * CellSizePx;

            for (int i = 0; i < CurrentShape.Cells.Count; i++)
            {
                var c = CurrentShape.Cells[i];
                var cell = new VisualElement();
                cell.AddToClassList("item-card-cell");
                cell.style.position = Position.Absolute;
                cell.style.width = CellSizePx;
                cell.style.height = CellSizePx;
                cell.style.left = c.x * CellSizePx;
                cell.style.bottom = c.y * CellSizePx;
                cell.style.backgroundColor = tint;
                cell.style.borderTopWidth = 1f;
                cell.style.borderRightWidth = 1f;
                cell.style.borderBottomWidth = 1f;
                cell.style.borderLeftWidth = 1f;
                cell.style.borderTopColor = Color.black;
                cell.style.borderRightColor = Color.black;
                cell.style.borderBottomColor = Color.black;
                cell.style.borderLeftColor = Color.black;
                bounds.Add(cell);
            }
        }

        void HandleLongPress()
        {
            CurrentRotation = (Rotation)(((int)CurrentRotation + 1) % 4);
            CurrentShape = baseShape.Rotate(CurrentRotation);
            Rebuild();
            OnRotated?.Invoke(this);
            BoxyBootstrap.Instance?.AudioBindings?.PlayRotate();
        }

        void HandleDragStart()
        {
            BringToFront();
            BoxyBootstrap.Instance?.AudioBindings?.PlayDragStart();
        }

        void HandleDragMove(Vector2 delta)
        {
            transform.position = new Vector3(delta.x, delta.y, 0f);
        }

        void HandleDragEnd(Vector2 panelFinalPos)
        {
            OnDropped?.Invoke(this, panelFinalPos);
        }
    }
}
