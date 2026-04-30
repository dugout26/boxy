using System.Collections.Generic;
using UnityEngine;
using Boxy.App.Gameplay.Domain;

namespace Boxy.App.Levels.Generation
{
    public static class ShapeCatalog
    {
        public static ItemShape Cell1x1 { get; } = ItemShape.FromCells(new[]
        {
            new Vector2Int(0, 0)
        });

        public static ItemShape Cell1x2 { get; } = ItemShape.FromCells(new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0)
        });

        public static ItemShape Cell1x3 { get; } = ItemShape.FromCells(new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0)
        });

        public static ItemShape Cell2x2 { get; } = ItemShape.FromCells(new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        });

        public static ItemShape Cell2x3 { get; } = ItemShape.FromCells(new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2)
        });

        public static ItemShape ShapeL { get; } = ItemShape.FromCells(new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 0)
        });

        public static ItemShape ShapeT { get; } = ItemShape.FromCells(new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(1, 1)
        });

        // 튜토리얼 1~5: 단순 직사각형만 (회전 학습 X 또는 1회만)
        public static IReadOnlyList<ItemShape> Tutorial { get; } = new[]
        {
            Cell1x2, Cell2x2, Cell1x3
        };

        // 표준 6~50: 모든 모양 + 1x1 fallback
        public static IReadOnlyList<ItemShape> Standard { get; } = new[]
        {
            Cell1x1, Cell1x2, Cell1x3, Cell2x2, Cell2x3, ShapeL, ShapeT
        };
    }
}
