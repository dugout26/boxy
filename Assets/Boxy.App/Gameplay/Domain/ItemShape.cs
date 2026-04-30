using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boxy.App.Gameplay.Domain
{
    public enum Rotation
    {
        Deg0 = 0,
        Deg90 = 1,
        Deg180 = 2,
        Deg270 = 3
    }

    public readonly struct ItemShape
    {
        readonly Vector2Int[] cells;

        public IReadOnlyList<Vector2Int> Cells => cells;
        public int CellCount => cells?.Length ?? 0;

        ItemShape(Vector2Int[] cells)
        {
            this.cells = cells;
        }

        public static ItemShape FromCells(IReadOnlyList<Vector2Int> source)
        {
            if (source == null || source.Count == 0)
                throw new ArgumentException("ItemShape requires at least 1 cell", nameof(source));

            var copy = new Vector2Int[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            return new ItemShape(Normalize(copy));
        }

        public ItemShape Rotate90Cw()
        {
            if (cells == null) return this;
            var rotated = new Vector2Int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                rotated[i] = new Vector2Int(cells[i].y, -cells[i].x);
            }
            return new ItemShape(Normalize(rotated));
        }

        public ItemShape Rotate(Rotation r)
        {
            int turns = (int)r;
            var s = this;
            for (int i = 0; i < turns; i++) s = s.Rotate90Cw();
            return s;
        }

        // 회전 후 음수 좌표를 0 이상으로 시프트 — 모양 동일성 비교 가능하게 (예: (1,0)(2,0) ≡ (0,0)(1,0))
        static Vector2Int[] Normalize(Vector2Int[] arr)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].x < minX) minX = arr[i].x;
                if (arr[i].y < minY) minY = arr[i].y;
            }
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = new Vector2Int(arr[i].x - minX, arr[i].y - minY);
            }
            return arr;
        }
    }
}
