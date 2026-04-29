using System.Collections.Generic;
using NUnit.Framework;
using Boxy.App.Levels;
using Boxy.App.Levels.Generation;

namespace Boxy.Tests.EditMode
{
    [TestFixture]
    public class LevelGeneratorTests
    {
        [Test]
        public void Generate_FillsEntireGrid_TotalCellsEqualGridArea()
        {
            var generator = new LevelGenerator(seed: 42);
            var items = generator.Generate(4, 4, ShapeCatalog.Tutorial, ItemKeyPool.SchoolBag);

            int totalCells = 0;
            foreach (var item in items) totalCells += item.Cells.Count;

            // boxy-plan §B-3-2 거꾸로 생성 보장: 그리드 가득
            Assert.AreEqual(16, totalCells, "Generator should fill entire 4x4 grid (16 cells)");
        }

        [Test]
        public void Generate_FillsLargerGrid_WithStandardShapes()
        {
            var generator = new LevelGenerator(seed: 42);
            var items = generator.Generate(6, 8, ShapeCatalog.Standard, ItemKeyPool.SchoolBag);

            int totalCells = 0;
            foreach (var item in items) totalCells += item.Cells.Count;

            Assert.AreEqual(48, totalCells, "6x8 = 48 cells");
        }

        [Test]
        public void Generate_DeterministicWithSameSeed()
        {
            var gen1 = new LevelGenerator(seed: 42);
            var items1 = gen1.Generate(6, 6, ShapeCatalog.Standard, ItemKeyPool.SchoolBag);

            var gen2 = new LevelGenerator(seed: 42);
            var items2 = gen2.Generate(6, 6, ShapeCatalog.Standard, ItemKeyPool.SchoolBag);

            Assert.AreEqual(items1.Count, items2.Count);
            for (int i = 0; i < items1.Count; i++)
            {
                Assert.AreEqual(items1[i].ItemKey, items2[i].ItemKey);
                Assert.AreEqual(items1[i].Cells.Count, items2[i].Cells.Count);
            }
        }

        [Test]
        public void Generate_DifferentSeed_ProducesDifferentLayout()
        {
            var gen1 = new LevelGenerator(seed: 1);
            var items1 = gen1.Generate(6, 6, ShapeCatalog.Standard, ItemKeyPool.SchoolBag);

            var gen2 = new LevelGenerator(seed: 999);
            var items2 = gen2.Generate(6, 6, ShapeCatalog.Standard, ItemKeyPool.SchoolBag);

            // 다른 seed → 최소 한 차이는 있어야 (item 개수 또는 첫 모양)
            bool different = items1.Count != items2.Count ||
                             items1[0].Cells.Count != items2[0].Cells.Count;
            Assert.IsTrue(different, "Different seeds should produce different layouts");
        }

        [Test]
        public void Generate_AssignsUniqueItemKeys()
        {
            var generator = new LevelGenerator(seed: 42);
            var items = generator.Generate(4, 4, ShapeCatalog.Tutorial, ItemKeyPool.SchoolBag);

            var keys = new HashSet<string>();
            foreach (var item in items)
            {
                Assert.IsTrue(keys.Add(item.ItemKey), $"Duplicate itemKey: {item.ItemKey}");
            }
        }

        [Test]
        public void Generate_EmptyShapes_Throws()
        {
            var generator = new LevelGenerator(seed: 42);
            Assert.Throws<System.ArgumentException>(() =>
                generator.Generate(4, 4, new System.Collections.Generic.List<Boxy.App.Gameplay.Domain.ItemShape>(),
                    ItemKeyPool.SchoolBag));
        }
    }
}
