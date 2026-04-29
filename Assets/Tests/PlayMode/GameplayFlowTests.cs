using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Mound.Core.Events;
using Boxy.App.Gameplay;
using Boxy.App.Gameplay.Domain;
using Boxy.App.Gameplay.Events;
using Boxy.App.Levels;

namespace Boxy.Tests.PlayMode
{
    // 게임 흐름 통합 테스트 — Controller + Grid + UndoSystem + EventBus 한 번에 검증.
    // 시뮬레이터/디바이스 검증과 별개 — 로직 정확성 보장.
    public class GameplayFlowTests
    {
        IEventBus bus;
        GameObject host;
        GameplayController controller;

        [SetUp]
        public void Setup()
        {
            bus = new EventBus();
            host = new GameObject("GameplayController");
            controller = host.AddComponent<GameplayController>();
            controller.Bind(bus);
        }

        [TearDown]
        public void Teardown()
        {
            if (host != null) Object.DestroyImmediate(host);
        }

        [UnityTest]
        public IEnumerator StartLevel_3x2_TwoHorizontalItems_ClearsCorrectly()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            // 3×2 grid, 2개 3×1 horizontal items (Level 1 패턴)
            var item1 = new LevelItemTemplate("book_0", new[]
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)
            });
            var item2 = new LevelItemTemplate("notebook_1", new[]
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)
            });
            level.Initialize(1, 3, 2, "school_bag", new[] { item1, item2 });

            int levelStartedCount = 0, itemPlacedCount = 0, levelClearedCount = 0;
            int starsAtClear = 0;
            bus.Subscribe<LevelStartedEvent>(_ => levelStartedCount++);
            bus.Subscribe<ItemPlacedEvent>(_ => itemPlacedCount++);
            bus.Subscribe<LevelClearedEvent>(e => { levelClearedCount++; starsAtClear = e.Stars; });

            controller.StartLevel(level);
            yield return null;

            Assert.AreEqual(1, levelStartedCount, "LevelStartedEvent 1회");
            Assert.IsNotNull(controller.Grid);
            Assert.AreEqual(3, controller.Grid.Width);
            Assert.AreEqual(2, controller.Grid.Height);

            // 첫 아이템 — 하단 행
            var shape1 = item1.ToShape();
            bool placed1 = controller.TryPlace("book_0", shape1, new Vector2Int(0, 0));
            Assert.IsTrue(placed1, "첫 아이템 배치 성공");
            Assert.AreEqual(1, itemPlacedCount);
            Assert.AreEqual(0, levelClearedCount, "절반 채웠는데 클리어 X");

            // 둘째 아이템 — 상단 행 → 그리드 가득 → 클리어
            var shape2 = item2.ToShape();
            bool placed2 = controller.TryPlace("notebook_1", shape2, new Vector2Int(0, 1));
            Assert.IsTrue(placed2, "둘째 아이템 배치 성공");
            Assert.AreEqual(2, itemPlacedCount);
            Assert.AreEqual(1, levelClearedCount, "그리드 가득 → LevelClearedEvent 1회");
            Assert.AreEqual(3, starsAtClear, "힌트 X + 되돌리기 X → 별 3개");
        }

        [UnityTest]
        public IEnumerator UseHint_ResultsInLessStars()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            var item = new LevelItemTemplate("book_0", new[]
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)
            });
            level.Initialize(1, 2, 2, "school_bag", new[] { item });

            int starsAtClear = -1;
            bus.Subscribe<LevelClearedEvent>(e => starsAtClear = e.Stars);

            controller.StartLevel(level);
            controller.MarkHintUsed();
            controller.TryPlace("book_0", item.ToShape(), new Vector2Int(0, 0));
            yield return null;

            Assert.AreEqual(2, starsAtClear, "힌트 사용 → 별 2개 (3개 X)");
        }

        [UnityTest]
        public IEnumerator UseUndo_AffectsStars()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            var item = new LevelItemTemplate("book_0", new[]
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0)
            });
            level.Initialize(1, 2, 1, "school_bag", new[] { item });

            int starsAtClear = -1;
            bus.Subscribe<LevelClearedEvent>(e => starsAtClear = e.Stars);

            controller.StartLevel(level);
            controller.TryPlace("book_0", item.ToShape(), new Vector2Int(0, 0));
            controller.TryUndoFree();  // 되돌리기 사용
            controller.TryPlace("book_0", item.ToShape(), new Vector2Int(0, 0));
            yield return null;

            // 힌트 X, 되돌리기 사용 → 별 1개 (NoHint && NoUndo 둘 다 만족 X)
            Assert.AreEqual(1, starsAtClear, "되돌리기 사용 → 별 1개");
        }

        [UnityTest]
        public IEnumerator InvalidPlacement_ReturnsFalse_NoEvent()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            var item = new LevelItemTemplate("book_0", new[]
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)
            });
            level.Initialize(1, 2, 1, "school_bag", new[] { item });  // 2×1 grid에 3×1 item — 무조건 out-of-bounds

            int placedCount = 0;
            bus.Subscribe<ItemPlacedEvent>(_ => placedCount++);

            controller.StartLevel(level);
            bool placed = controller.TryPlace("book_0", item.ToShape(), new Vector2Int(0, 0));
            yield return null;

            Assert.IsFalse(placed, "out-of-bounds 배치 거부");
            Assert.AreEqual(0, placedCount, "ItemPlacedEvent 발화 X");
        }
    }
}
