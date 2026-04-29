using NUnit.Framework;
using UnityEngine;
using Boxy.App.Gameplay.Domain;

namespace Boxy.Tests.EditMode
{
    [TestFixture]
    public class ItemShapeTests
    {
        [Test]
        public void FromCells_PreservesCount()
        {
            var shape = ItemShape.FromCells(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1)
            });

            Assert.AreEqual(3, shape.CellCount);
        }

        [Test]
        public void FromCells_NormalizesToOriginAtZero()
        {
            // 시작 좌표 (2,3) → 정규화 후 (0,0) (1,0) 이어야 함
            var shape = ItemShape.FromCells(new[]
            {
                new Vector2Int(2, 3),
                new Vector2Int(3, 3)
            });

            Assert.IsTrue(ContainsCoord(shape, 0, 0));
            Assert.IsTrue(ContainsCoord(shape, 1, 0));
        }

        [Test]
        public void Rotate90Cw_PreservesCellCount()
        {
            var horizontal = ItemShape.FromCells(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            });

            var rotated = horizontal.Rotate90Cw();
            Assert.AreEqual(3, rotated.CellCount);
        }

        [Test]
        public void Rotate_FourTimes_HasSameCellCount()
        {
            var lShape = ItemShape.FromCells(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 0)
            });

            var rotated = lShape.Rotate(Rotation.Deg90)
                .Rotate(Rotation.Deg90)
                .Rotate(Rotation.Deg90)
                .Rotate(Rotation.Deg90);

            Assert.AreEqual(lShape.CellCount, rotated.CellCount);
        }

        [Test]
        public void Rotate_NormalizedToNonNegativeCoords()
        {
            var shape = ItemShape.FromCells(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            });

            var rotated = shape.Rotate90Cw();
            for (int i = 0; i < rotated.Cells.Count; i++)
            {
                Assert.GreaterOrEqual(rotated.Cells[i].x, 0, "Rotated cell x must be >= 0");
                Assert.GreaterOrEqual(rotated.Cells[i].y, 0, "Rotated cell y must be >= 0");
            }
        }

        [Test]
        public void FromCells_EmptyInput_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => ItemShape.FromCells(new Vector2Int[0]));
        }

        static bool ContainsCoord(ItemShape shape, int x, int y)
        {
            for (int i = 0; i < shape.Cells.Count; i++)
            {
                if (shape.Cells[i].x == x && shape.Cells[i].y == y) return true;
            }
            return false;
        }
    }
}
