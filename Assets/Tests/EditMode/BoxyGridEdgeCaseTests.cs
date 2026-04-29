using NUnit.Framework;
using UnityEngine;
using Boxy.App.Gameplay;
using Boxy.App.Gameplay.Domain;

namespace Boxy.Tests.EditMode
{
    // BoxyGrid 경계 케이스 테스트 — 그리드의 정확성 보장.
    public class BoxyGridEdgeCaseTests
    {
        [Test]
        public void NewGrid_AllCellsEmpty()
        {
            var grid = new BoxyGrid(3, 3);
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    Assert.IsTrue(grid.IsEmpty(new Vector2Int(x, y)), $"({x},{y}) 빈 셀이어야");
            Assert.IsFalse(grid.IsFull());
        }

        [Test]
        public void Place_OutOfBounds_DoesNotAffectGrid()
        {
            var grid = new BoxyGrid(3, 3);
            var shape = ItemShape.FromCells(new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) });
            // 그리드 밖 anchor는 PlacementValidator에서 거부 — Place 자체는 호출하면 안 됨.
            Assert.AreEqual(PlacementResult.OutOfBounds,
                PlacementValidator.Check(grid, shape, new Vector2Int(2, 0)));  // 1,0이 grid 밖
        }

        [Test]
        public void IsInBounds_NegativeCoord_ReturnsFalse()
        {
            var grid = new BoxyGrid(3, 3);
            Assert.IsFalse(grid.IsInBounds(new Vector2Int(-1, 0)));
            Assert.IsFalse(grid.IsInBounds(new Vector2Int(0, -1)));
            Assert.IsFalse(grid.IsInBounds(new Vector2Int(3, 0)));
            Assert.IsFalse(grid.IsInBounds(new Vector2Int(0, 3)));
        }

        [Test]
        public void IsInBounds_BoundaryCoord_ReturnsTrue()
        {
            var grid = new BoxyGrid(3, 3);
            Assert.IsTrue(grid.IsInBounds(new Vector2Int(0, 0)));
            Assert.IsTrue(grid.IsInBounds(new Vector2Int(2, 2)));
            Assert.IsTrue(grid.IsInBounds(new Vector2Int(0, 2)));
            Assert.IsTrue(grid.IsInBounds(new Vector2Int(2, 0)));
        }

        [Test]
        public void Place_ThenRemove_RestoresEmpty()
        {
            var grid = new BoxyGrid(3, 3);
            var shape = ItemShape.FromCells(new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) });
            grid.Place("test_item", shape, new Vector2Int(0, 0));
            Assert.IsFalse(grid.IsEmpty(new Vector2Int(0, 0)));
            Assert.IsFalse(grid.IsEmpty(new Vector2Int(1, 0)));
            grid.Remove("test_item");
            Assert.IsTrue(grid.IsEmpty(new Vector2Int(0, 0)));
            Assert.IsTrue(grid.IsEmpty(new Vector2Int(1, 0)));
        }

        [Test]
        public void GetItemKeyAt_PlacedCell_ReturnsKey()
        {
            var grid = new BoxyGrid(3, 3);
            var shape = ItemShape.FromCells(new[] { new Vector2Int(0, 0) });
            grid.Place("apple_5", shape, new Vector2Int(1, 1));
            Assert.AreEqual("apple_5", grid.GetItemKeyAt(new Vector2Int(1, 1)));
        }

        [Test]
        public void IsFull_SmallGridPlacedFull_ReturnsTrue()
        {
            var grid = new BoxyGrid(2, 2);
            var shape = ItemShape.FromCells(new[]
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1)
            });
            grid.Place("test", shape, new Vector2Int(0, 0));
            Assert.IsTrue(grid.IsFull());
        }

        [Test]
        public void IsFull_PartiallyFilled_ReturnsFalse()
        {
            var grid = new BoxyGrid(2, 2);
            var shape = ItemShape.FromCells(new[] { new Vector2Int(0, 0) });
            grid.Place("a", shape, new Vector2Int(0, 0));
            Assert.IsFalse(grid.IsFull());
        }

        [Test]
        public void Place_OverlappingCells_ThrowsOrFails()
        {
            var grid = new BoxyGrid(3, 3);
            var shape = ItemShape.FromCells(new[] { new Vector2Int(0, 0) });
            grid.Place("first", shape, new Vector2Int(1, 1));
            // 같은 셀에 다시 — Validator가 Collision 반환해야 함
            Assert.AreEqual(PlacementResult.Collision,
                PlacementValidator.Check(grid, shape, new Vector2Int(1, 1)));
        }
    }
}
