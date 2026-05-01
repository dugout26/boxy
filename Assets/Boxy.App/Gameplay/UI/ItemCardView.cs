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

        // tint 인자는 backwards 호환용. 실제 컬러는 ItemVisualPalette에서 itemKey로 조회 — 다양화 효과.
        public ItemCardView(string itemKey, ItemShape shape, Color tint)
        {
            ItemKey = itemKey;
            baseShape = shape;
            CurrentShape = shape;
            CurrentRotation = Rotation.Deg0;
            this.tint = ItemVisualPalette.GetColor(itemKey);
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

            // 모서리 둥근 borderColor — Cozy 톤의 따뜻한 갈색 (mound-design-system §2 cozy 보더)
            var borderColor = new Color(0.32f, 0.21f, 0.13f, 0.8f);   // #523523 cc

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
                cell.style.borderTopWidth = 1.5f;
                cell.style.borderRightWidth = 1.5f;
                cell.style.borderBottomWidth = 1.5f;
                cell.style.borderLeftWidth = 1.5f;
                cell.style.borderTopColor = borderColor;
                cell.style.borderRightColor = borderColor;
                cell.style.borderBottomColor = borderColor;
                cell.style.borderLeftColor = borderColor;
                bounds.Add(cell);
            }

            // 도형 영역 가운데에 큰 이모지 — 시각적 의미 부여 (책=📖 / 노트=📓 / 사과=🍎 ...).
            // emoji는 system fallback 폰트 (Apple Color Emoji on Mac/iOS, NotoColorEmoji on Android).
            var emoji = new Label(ItemVisualPalette.GetEmoji(ItemKey));
            emoji.pickingMode = PickingMode.Ignore;   // 드래그 입력은 bounds 그대로 받음
            emoji.style.position = Position.Absolute;
            emoji.style.left = 0;
            emoji.style.right = 0;
            emoji.style.top = 0;
            emoji.style.bottom = 0;
            emoji.style.unityTextAlign = TextAnchor.MiddleCenter;
            // 도형 셀 수에 비례한 emoji 크기 (작은 도형 = 작은 emoji, 큰 도형 = 큰 emoji)
            int totalCells = CurrentShape.Cells.Count;
            float emojiSize = totalCells == 1 ? CellSizePx * 0.65f
                             : totalCells <= 3 ? CellSizePx * 0.85f
                             : CellSizePx * 1.1f;
            emoji.style.fontSize = emojiSize;
            bounds.Add(emoji);
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
