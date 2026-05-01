using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Mound.Core.Events;
using Boxy.App;
using Boxy.App.Gameplay;
using Boxy.App.Gameplay.Domain;
using Boxy.App.Gameplay.Events;
using Boxy.App.Gameplay.UI;
using Boxy.App.Levels;

namespace Boxy.Tests.PlayMode
{
    // 게임 흐름 통합 테스트 — Controller + Grid + UndoSystem + EventBus + 씬/UI wire 검증.
    // 시뮬레이터/디바이스 검증과 별개 — 로직 정확성 + 자동 wire 보장.
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

        // ─────────── 코어 로직 ─────────── //

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

            // 힌트 = 답 거의 보여줌 → 큰 감점 (별 1개)
            Assert.AreEqual(1, starsAtClear, "힌트 사용 → 별 1개 (큰 감점)");
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

            // 되돌리기 = 자기 수정 시도 → 작은 감점 (별 2개). 힌트보다 가벼움.
            Assert.AreEqual(2, starsAtClear, "되돌리기 사용 → 별 2개 (작은 감점)");
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

        // ─────────── 씬 로드 + 자동 wire ─────────── //

        [UnityTest]
        public IEnumerator MainMenuScene_Loads_BootstrapInstanceCreated()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            yield return null;  // Awake 호출 처리
            yield return null;

            Assert.IsNotNull(BoxyBootstrap.Instance,
                "MainMenu 씬 로드 후 BoxyBootstrap.Instance가 생성되어야 함 (4 씬 모두 BoxyBootstrap GameObject 박힘)");
            Assert.IsNotNull(BoxyBootstrap.Instance.SaveSystem, "SaveSystem provider 초기화");
            Assert.IsNotNull(BoxyBootstrap.Instance.EventBus, "EventBus 초기화");
            Assert.IsNotNull(BoxyBootstrap.Instance.AdProvider, "AdProvider 초기화 (NullAdProvider stub)");
        }

        [UnityTest]
        public IEnumerator GameplayScene_Loads_AllSerializedFieldsWired()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay", LoadSceneMode.Single);
            yield return null;
            yield return null;

            var ui = Object.FindAnyObjectByType<GameplayUI>();
            Assert.IsNotNull(ui, "GameplayUI 컴포넌트가 Gameplay 씬에 존재해야 함");

            // SerializedField 4개 모두 wire — BoxyAutomation이 자동 연결
            var controllerField = typeof(GameplayUI).GetField("controller", BindingFlags.NonPublic | BindingFlags.Instance);
            var defaultLevelField = typeof(GameplayUI).GetField("defaultLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            var resultPopupField = typeof(GameplayUI).GetField("resultPopupAsset", BindingFlags.NonPublic | BindingFlags.Instance);
            var levelAssetsField = typeof(GameplayUI).GetField("levelAssets", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(controllerField.GetValue(ui), "GameplayUI.controller wired");
            Assert.IsNotNull(defaultLevelField.GetValue(ui), "GameplayUI.defaultLevel wired (Level_01.asset)");
            Assert.IsNotNull(resultPopupField.GetValue(ui), "GameplayUI.resultPopupAsset wired (ResultPopup.uxml)");
            var levels = (LevelData[])levelAssetsField.GetValue(ui);
            Assert.IsNotNull(levels, "GameplayUI.levelAssets array wired");
            Assert.AreEqual(50, levels.Length, "50 LevelData 모두 wired");
        }

        [UnityTest]
        public IEnumerator GameplayScene_BuildsGrid_AndItemTrayWithCards()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay", LoadSceneMode.Single);
            yield return null;
            yield return null;
            yield return null;  // OnEnable + StartLevel + BuildGrid + BuildItemTray 처리 시간

            var doc = Object.FindAnyObjectByType<UIDocument>();
            Assert.IsNotNull(doc, "UIDocument 존재");
            var root = doc.rootVisualElement;
            Assert.IsNotNull(root, "rootVisualElement 생성됨 (PanelSettings + visualTreeAsset 정상)");

            var gridContainer = root.Q<VisualElement>("grid-container");
            Assert.IsNotNull(gridContainer, "grid-container UXML 노드 query 성공");

            var itemTray = root.Q<ScrollView>("item-tray");
            Assert.IsNotNull(itemTray, "item-tray ScrollView UXML 노드 query 성공");

            // BuildGrid가 호출됐다면 grid-container 자식 셀 수 = width * height (Level_01: 4×2 = 8)
            // 또는 Tutorial fallback (4×2 = 8) 둘 중 하나라도 셀 1개 이상
            Assert.Greater(gridContainer.childCount, 0,
                "그리드 셀이 그려져야 함 (BuildGrid 실행 결과)");

            // BuildItemTray가 호출됐다면 item-tray content에 ItemCardView 1개 이상
            int cardCount = 0;
            itemTray.contentContainer.Query<ItemCardView>().ForEach(_ => cardCount++);
            Assert.Greater(cardCount, 0,
                "아이템 카드가 트레이에 그려져야 함 (BuildItemTray 실행 결과 — 도형이 화면에 보임)");
        }
    }
}
