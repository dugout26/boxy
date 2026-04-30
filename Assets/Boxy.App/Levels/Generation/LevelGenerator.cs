using System;
using System.Collections.Generic;
using UnityEngine;
using Boxy.App.Gameplay;
using Boxy.App.Gameplay.Domain;

namespace Boxy.App.Levels.Generation
{
    public sealed class LevelGenerator
    {
        readonly System.Random rng;

        public LevelGenerator(int seed)
        {
            rng = new System.Random(seed);
        }

        public IReadOnlyList<LevelItemTemplate> Generate(int width, int height,
            IReadOnlyList<ItemShape> allowedShapes, IReadOnlyList<string> itemKeyPool)
        {
            if (allowedShapes == null || allowedShapes.Count == 0)
                throw new ArgumentException("allowedShapes empty", nameof(allowedShapes));
            if (itemKeyPool == null || itemKeyPool.Count == 0)
                throw new ArgumentException("itemKeyPool empty", nameof(itemKeyPool));

            var grid = new BoxyGrid(width, height);
            var result = new List<LevelItemTemplate>();
            int placedIndex = 0;

            // boxy-plan §B-3-2: top-left 스캔 → 빈 셀 첫 발견 위치에 무작위 모양/회전 시도 → 첫 적합 배치
            while (true)
            {
                var nextEmpty = FindFirstEmpty(grid);
                if (!nextEmpty.HasValue) break;

                var anchor = nextEmpty.Value;
                var rotated = TryFindFitting(grid, anchor, allowedShapes);

                var key = $"{itemKeyPool[placedIndex % itemKeyPool.Count]}_{placedIndex}";
                grid.Place(key, rotated, anchor);

                result.Add(new LevelItemTemplate(key, rotated.Cells));
                placedIndex++;
            }

            return result;
        }

        Vector2Int? FindFirstEmpty(BoxyGrid grid)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var c = new Vector2Int(x, y);
                    if (grid.IsEmpty(c)) return c;
                }
            }
            return null;
        }

        // 무작위 순서로 모양/회전 시도. 모두 실패 시 1x1 fallback (anchor 자체가 빈 셀이므로 항상 성공 → 그리드 가득 채움 보장)
        ItemShape TryFindFitting(BoxyGrid grid, Vector2Int anchor, IReadOnlyList<ItemShape> shapes)
        {
            var shapeOrder = ShuffleIndices(shapes.Count);
            var rotOrder = ShuffleIndices(4);

            for (int si = 0; si < shapeOrder.Length; si++)
            {
                var baseShape = shapes[shapeOrder[si]];
                for (int ri = 0; ri < rotOrder.Length; ri++)
                {
                    var rotated = baseShape.Rotate((Rotation)rotOrder[ri]);
                    if (PlacementValidator.IsValid(grid, rotated, anchor))
                        return rotated;
                }
            }

            return ShapeCatalog.Cell1x1;
        }

        int[] ShuffleIndices(int count)
        {
            var arr = new int[count];
            for (int i = 0; i < count; i++) arr[i] = i;

            // Fisher-Yates
            for (int i = count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            return arr;
        }

    }
}
