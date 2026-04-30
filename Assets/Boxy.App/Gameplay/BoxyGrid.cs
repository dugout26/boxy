using System;
using UnityEngine;
using Boxy.App.Gameplay.Domain;

namespace Boxy.App.Gameplay
{
    public sealed class BoxyGrid
    {
        readonly int width;
        readonly int height;
        readonly string[,] cells;       // null = 비어있음, 그 외 = 점유 itemKey

        public int Width => width;
        public int Height => height;

        public BoxyGrid(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            this.width = width;
            this.height = height;
            cells = new string[width, height];
        }

        public bool IsInBounds(Vector2Int coord) =>
            coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;

        public bool IsEmpty(Vector2Int coord) =>
            IsInBounds(coord) && cells[coord.x, coord.y] == null;

        public string GetItemKeyAt(Vector2Int coord) =>
            IsInBounds(coord) ? cells[coord.x, coord.y] : null;

        public bool IsFull()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y] == null) return false;
                }
            }
            return true;
        }

        // PlacementValidator로 검증 후 호출 — 호출자 계약. 잘못된 입력은 IndexOutOfRange 발생시켜 빠르게 실패
        public void Place(string itemKey, ItemShape shape, Vector2Int anchor)
        {
            if (string.IsNullOrEmpty(itemKey))
                throw new ArgumentException("itemKey empty", nameof(itemKey));

            for (int i = 0; i < shape.Cells.Count; i++)
            {
                var c = anchor + shape.Cells[i];
                cells[c.x, c.y] = itemKey;
            }
        }

        public void Remove(string itemKey)
        {
            if (string.IsNullOrEmpty(itemKey)) return;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y] == itemKey) cells[x, y] = null;
                }
            }
        }

        public void Clear()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = null;
                }
            }
        }
    }
}
