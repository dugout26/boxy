using System;
using UnityEngine;
using Boxy.App.Gameplay.Domain;

namespace Boxy.App.Gameplay
{
    public enum PlacementResult
    {
        Valid,
        OutOfBounds,
        Collision
    }

    public static class PlacementValidator
    {
        public static PlacementResult Check(BoxyGrid grid, ItemShape shape, Vector2Int anchor)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            for (int i = 0; i < shape.Cells.Count; i++)
            {
                var c = anchor + shape.Cells[i];
                if (!grid.IsInBounds(c)) return PlacementResult.OutOfBounds;
                if (!grid.IsEmpty(c)) return PlacementResult.Collision;
            }
            return PlacementResult.Valid;
        }

        public static bool IsValid(BoxyGrid grid, ItemShape shape, Vector2Int anchor) =>
            Check(grid, shape, anchor) == PlacementResult.Valid;
    }
}
