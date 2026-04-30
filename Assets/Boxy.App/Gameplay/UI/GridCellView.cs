using UnityEngine;
using UnityEngine.UIElements;

namespace Boxy.App.Gameplay.UI
{
    public static class GridCellView
    {
        // 반응형 동작 — 실제 셀 크기는 grid-container 사이즈로부터 런타임 계산됨.
        // GameplayUI가 BuildGrid에서 컨테이너 layout 콜백으로 갱신.
        public static float CurrentCellSizePx { get; set; } = 36f;

        // grid-container 안 절대 위치 배치. y-up 좌표계 (UI Toolkit 기본 top-down 뒤집음).
        // cellSize는 동적 — bag-frame 크기 따라 변동.
        public static VisualElement Create(Vector2Int coord, float cellSize)
        {
            var cell = new VisualElement
            {
                name = $"cell-{coord.x}-{coord.y}"
            };
            cell.AddToClassList("grid-cell");
            cell.style.position = Position.Absolute;
            cell.style.width = cellSize;
            cell.style.height = cellSize;
            cell.style.left = coord.x * cellSize;
            cell.style.bottom = coord.y * cellSize;
            cell.style.borderTopWidth = 1f;
            cell.style.borderRightWidth = 1f;
            cell.style.borderBottomWidth = 1f;
            cell.style.borderLeftWidth = 1f;
            cell.style.borderTopColor = new Color(0.9f, 0.92f, 0.93f);
            cell.style.borderRightColor = new Color(0.9f, 0.92f, 0.93f);
            cell.style.borderBottomColor = new Color(0.9f, 0.92f, 0.93f);
            cell.style.borderLeftColor = new Color(0.9f, 0.92f, 0.93f);
            return cell;
        }

        // 셀 사이즈 변경 시 호출 — 기존 셀들의 width/height/left/bottom 갱신
        public static void Resize(VisualElement cell, Vector2Int coord, float cellSize)
        {
            cell.style.width = cellSize;
            cell.style.height = cellSize;
            cell.style.left = coord.x * cellSize;
            cell.style.bottom = coord.y * cellSize;
        }

        public static void SetOccupied(VisualElement cell, bool occupied, Color filledColor)
        {
            cell.style.backgroundColor = occupied ? filledColor : Color.clear;
        }

        public enum HighlightState { None, Valid, Invalid }

        // 드래그 중 drop preview — backgroundColor 일시 덮어쓰기. 드래그 종료 시 RefreshGridFromController로 복원
        public static void SetHighlight(VisualElement cell, HighlightState state)
        {
            switch (state)
            {
                case HighlightState.Valid:
                    cell.style.backgroundColor = new Color(0.29f, 0.87f, 0.5f, 0.45f);   // green
                    break;
                case HighlightState.Invalid:
                    cell.style.backgroundColor = new Color(0.86f, 0.15f, 0.15f, 0.45f);  // red
                    break;
                default:
                    cell.style.backgroundColor = Color.clear;
                    break;
            }
        }
    }
}
