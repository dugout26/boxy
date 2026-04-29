using NUnit.Framework;
using UnityEngine;
using Boxy.App.Gameplay;
using Boxy.App.Gameplay.Domain;

namespace Boxy.Tests.EditMode
{
    [TestFixture]
    public class PlacementValidatorTests
    {
        ItemShape Single() => ItemShape.FromCells(new[] { new Vector2Int(0, 0) });
        ItemShape Domino() => ItemShape.FromCells(new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) });

        [Test]
        public void Check_ValidPlacement_InEmptyGrid_ReturnsValid()
        {
            var grid = new BoxyGrid(4, 4);
            var result = PlacementValidator.Check(grid, Domino(), new Vector2Int(0, 0));
            Assert.AreEqual(PlacementResult.Valid, result);
        }

        [Test]
        public void Check_OutOfBounds_HorizontalEdge_ReturnsOutOfBounds()
        {
            var grid = new BoxyGrid(4, 4);
            // anchor (3,0) + cell offset (1,0) = (4,0) — width=4면 인덱스 0~3, (4,0)은 out
            var result = PlacementValidator.Check(grid, Domino(), new Vector2Int(3, 0));
            Assert.AreEqual(PlacementResult.OutOfBounds, result);
        }

        [Test]
        public void Check_OutOfBounds_NegativeAnchor_ReturnsOutOfBounds()
        {
            var grid = new BoxyGrid(4, 4);
            var result = PlacementValidator.Check(grid, Single(), new Vector2Int(-1, 0));
            Assert.AreEqual(PlacementResult.OutOfBounds, result);
        }

        [Test]
        public void Check_Collision_WithExistingItem_ReturnsCollision()
        {
            var grid = new BoxyGrid(4, 4);
            grid.Place("item-a", Single(), new Vector2Int(2, 2));

            var result = PlacementValidator.Check(grid, Single(), new Vector2Int(2, 2));
            Assert.AreEqual(PlacementResult.Collision, result);
        }

        [Test]
        public void IsValid_WrapsCheck()
        {
            var grid = new BoxyGrid(4, 4);
            Assert.IsTrue(PlacementValidator.IsValid(grid, Single(), new Vector2Int(0, 0)));
            Assert.IsFalse(PlacementValidator.IsValid(grid, Single(), new Vector2Int(4, 4)));
        }

        [Test]
        public void Check_NullGrid_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                PlacementValidator.Check(null, Single(), new Vector2Int(0, 0)));
        }

        [Test]
        public void BoxyGrid_AfterPlace_IsFull_TrueWhenAllCellsOccupied()
        {
            var grid = new BoxyGrid(2, 2);
            // 4 셀 모두 채움
            grid.Place("a", Single(), new Vector2Int(0, 0));
            grid.Place("b", Single(), new Vector2Int(1, 0));
            grid.Place("c", Single(), new Vector2Int(0, 1));
            Assert.IsFalse(grid.IsFull());

            grid.Place("d", Single(), new Vector2Int(1, 1));
            Assert.IsTrue(grid.IsFull());
        }

        [Test]
        public void BoxyGrid_Remove_RestoresEmptyState()
        {
            var grid = new BoxyGrid(2, 2);
            grid.Place("removable", Domino(), new Vector2Int(0, 0));
            Assert.IsFalse(grid.IsEmpty(new Vector2Int(0, 0)));

            grid.Remove("removable");
            Assert.IsTrue(grid.IsEmpty(new Vector2Int(0, 0)));
            Assert.IsTrue(grid.IsEmpty(new Vector2Int(1, 0)));
        }
    }
}
