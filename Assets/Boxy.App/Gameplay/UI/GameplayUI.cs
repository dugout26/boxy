using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using Mound.Core.Events;
using Mound.Core.Save;
using Mound.Core.Scenes;
using Mound.Monetization;
using Boxy.App.Gameplay.Events;
using Boxy.App.Gameplay.Domain;
using Boxy.App.Levels;
using Boxy.App.Save;
using Boxy.App.UI;

namespace Boxy.App.Gameplay.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class GameplayUI : MonoBehaviour
    {
        // Inspector 연결:
        //   - controller: 별도 GameObject의 GameplayController
        //   - defaultLevel: 시작 LevelData asset (테스트 시작용)
        //   - resultPopupAsset: ResultPopup.uxml 드래그
        //   - levelAssets: 모든 LevelData asset 배열 (다음 레벨 탐색)
        [SerializeField] GameplayController controller;
        [SerializeField] LevelData defaultLevel;
        [SerializeField] VisualTreeAsset resultPopupAsset;
        [SerializeField] LevelData[] levelAssets;

        UIDocument uiDocument;
        VisualElement gridContainer;
        VisualElement itemTrayContent;
        Label levelLabel;
        VisualElement progressFill;
        Label[] starLabels;
        Button hintButton;
        Button undoButton;
        Button backButton;

        IEventBus eventBus;
        Dictionary<Vector2Int, VisualElement> cellViews;

        IAdProvider adProvider;
        HintAdController hintCtrl;
        UndoAdController undoCtrl;
        CancellationTokenSource cts;

        ProgressionService progression;
        LevelRepository repository;
        ISceneLoader sceneLoader;
        ResultPopupController resultCtrl;
        InterstitialTimer interstitialTimer;

        static readonly Color StarFilled = new Color(1f, 0.84f, 0.04f);     // #FFD60A
        static readonly Color StarEmpty = new Color(0.82f, 0.84f, 0.86f);   // #D1D5DB
        static readonly Color FilledCellColor = new Color(1f, 0.84f, 0.04f, 0.85f);
        // 같은 itemKey base ("book_0", "book_12" 모두 "book")는 같은 색을 공유 — 같은 종류 식별성을 시각적으로 유지하기 위함.
        static readonly System.Collections.Generic.Dictionary<string, Color> BaseColorMap = new()
        {
            // School bag
            { "book",           new Color(0.95f, 0.42f, 0.42f) },  // 빨강
            { "notebook",       new Color(0.42f, 0.66f, 0.95f) },  // 파랑
            { "pencil_case",    new Color(0.62f, 0.42f, 0.95f) },  // 보라
            { "lunchbox",       new Color(0.95f, 0.62f, 0.42f) },  // 주황
            { "water_bottle",   new Color(0.42f, 0.85f, 0.85f) },  // 청록
            { "apple",          new Color(0.95f, 0.55f, 0.55f) },  // 분홍
            { "snack",          new Color(0.95f, 0.78f, 0.30f) },  // 황토
            { "ruler",          new Color(0.50f, 0.85f, 0.50f) },  // 초록
            { "calculator",     new Color(0.55f, 0.55f, 0.65f) },  // 회보
            { "headphones",     new Color(0.30f, 0.30f, 0.40f) },  // 짙은회
            { "plant",          new Color(0.40f, 0.75f, 0.40f) },  // 진초록
            { "boxy_keychain",  new Color(1f, 0.84f, 0.04f) },     // 노란 (마스코트 색)

            // Moving box
            { "frame",          new Color(0.78f, 0.62f, 0.42f) },  // 갈색 (액자)
            { "lamp",           new Color(0.95f, 0.85f, 0.55f) },  // 크림
            { "jeans",          new Color(0.30f, 0.50f, 0.78f) },  // 진청
            { "sweater",        new Color(0.85f, 0.62f, 0.78f) },  // 라일락
            { "vase",           new Color(0.78f, 0.55f, 0.42f) },  // 테라코타
            { "cushion",        new Color(0.95f, 0.70f, 0.55f) },  // 살구
            { "books_stack",    new Color(0.65f, 0.42f, 0.42f) },  // 와인
            { "hat",            new Color(0.42f, 0.42f, 0.55f) },  // 보라회
            { "plates",         new Color(0.92f, 0.92f, 0.95f) },  // 도자기 흰
            { "magazines",      new Color(0.62f, 0.78f, 0.42f) },  // 라임
            { "boxy_figure",    new Color(1f, 0.84f, 0.04f) },     // 노란
            { "socks_pe",       new Color(0.55f, 0.55f, 0.45f) },  // 진베이지
            { "umbrella",       new Color(0.42f, 0.62f, 0.78f) },  // 청회

            // Travel trunk
            { "tshirt",         new Color(0.95f, 0.95f, 0.92f) },  // 흰 (티셔츠)
            { "shoes",          new Color(0.30f, 0.30f, 0.30f) },  // 검정
            { "passport",       new Color(0.55f, 0.30f, 0.30f) },  // 진빨강
            { "speaker",        new Color(0.42f, 0.42f, 0.45f) },  // 진회
            { "sunglasses",     new Color(0.20f, 0.20f, 0.25f) },  // 검정
            { "candle",         new Color(0.95f, 0.85f, 0.78f) },  // 살구크림
            { "blanket",        new Color(0.78f, 0.62f, 0.55f) },  // 베이지핑크
            { "camera",         new Color(0.35f, 0.35f, 0.40f) },  // 어두운회
            { "toiletries",     new Color(0.62f, 0.85f, 0.95f) },  // 하늘
            { "guidebook",      new Color(0.85f, 0.45f, 0.30f) },  // 주황빨강
            { "boxy_souvenir",  new Color(1f, 0.84f, 0.04f) },     // 노란
        };
        static readonly Color ItemTintFallback = new Color(1f, 0.84f, 0.04f);

        static Color GetItemColor(string itemKey)
        {
            if (string.IsNullOrEmpty(itemKey)) return ItemTintFallback;
            int underscore = itemKey.LastIndexOf('_');
            string baseKey = itemKey;
            if (underscore > 0 && underscore < itemKey.Length - 1)
            {
                bool tail_isdigit = true;
                for (int i = underscore + 1; i < itemKey.Length; i++)
                {
                    if (!char.IsDigit(itemKey[i])) { tail_isdigit = false; break; }
                }
                if (tail_isdigit) baseKey = itemKey.Substring(0, underscore);
            }
            return BaseColorMap.TryGetValue(baseKey, out var c) ? c : ItemTintFallback;
        }

        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            cellViews = new Dictionary<Vector2Int, VisualElement>();
            CacheUIElements();

            if (controller == null)
            {
                Debug.LogError("[GameplayUI] GameplayController가 Inspector에 연결 안 됨. Scene의 GameplayController GameObject를 controller 필드에 드래그.");
                return;
            }

            eventBus = new EventBus();
            controller.Bind(eventBus);
            SubscribeEvents();
            BindButtons();

            // Phase 9 MVP: dev/Editor 광고 스텁. Week 3 SDK 통합 시 AppLovin/AdMob 래퍼로 교체
            adProvider = new NullAdProvider();
            hintCtrl = new HintAdController(adProvider, controller);
            undoCtrl = new UndoAdController(adProvider, controller);
            cts = new CancellationTokenSource();

            progression = new ProgressionService(new PlayerPrefsSaveSystem(SaveMigrator.MigrateJsonIfNeeded));

            // levelAssets 미연결 시 — Tutorial 5 레벨 런타임 생성 (boxy-plan §B-3-1 fallback).
            // 정수 Unity 활성 후 .asset 파일 만들고 Inspector에 드래그 권장.
            if (levelAssets == null || levelAssets.Length == 0)
            {
                var tutorial = TutorialLevels.Create();
                levelAssets = new LevelData[tutorial.Count];
                for (int i = 0; i < tutorial.Count; i++) levelAssets[i] = tutorial[i];
                Debug.LogWarning("[GameplayUI] levelAssets Inspector 미연결 — Tutorial 5 레벨 런타임 생성 fallback");
            }
            repository = new LevelRepository(levelAssets);

            sceneLoader = new SceneLoader();
            interstitialTimer = new InterstitialTimer(adProvider);

            if (progression.Data.adsRemoved) interstitialTimer.Suppress();

            // ResultPopup UXML을 root에 add 후 controller 생성. 초기 hidden.
            if (resultPopupAsset != null)
            {
                var popupRoot = resultPopupAsset.Instantiate();
                popupRoot.style.position = Position.Absolute;
                popupRoot.style.left = 0;
                popupRoot.style.top = 0;
                popupRoot.style.right = 0;
                popupRoot.style.bottom = 0;
                uiDocument.rootVisualElement.Add(popupRoot);

                resultCtrl = new ResultPopupController(popupRoot, sceneLoader);
                resultCtrl.OnNext += GoToNextLevel;
                resultCtrl.OnRetry += RetryCurrentLevel;
            }
        }

        void Update()
        {
            interstitialTimer?.Tick();
        }

        void OnEnable()
        {
            if (controller == null) return;

            // 우선순위: GameSession.PendingLevel (LevelSelect → Gameplay 전이) > defaultLevel (Inspector) > 첫 fallback
            LevelData level = GameSession.PendingLevel ?? defaultLevel;
            if (level == null && repository != null && repository.Count > 0)
            {
                foreach (var lv in repository.All) { level = lv; break; }
            }

            if (level != null) StartLevel(level);
            GameSession.PendingLevel = null;   // 한 번 사용 후 클리어
        }

        void OnDestroy()
        {
            UnsubscribeEvents();
            UnbindButtons();
            cts?.Cancel();
            cts?.Dispose();

            if (resultCtrl != null)
            {
                resultCtrl.OnNext -= GoToNextLevel;
                resultCtrl.OnRetry -= RetryCurrentLevel;
                resultCtrl.Detach();
            }
        }

        public void StartLevel(LevelData level)
        {
            if (level == null || controller == null) return;

            controller.StartLevel(level);
            if (levelLabel != null) levelLabel.text = $"Level {level.Id.Value}";
            BuildGrid(level.GridWidth, level.GridHeight);
            BuildItemTray(level.Items);
            UpdateProgress();
            UpdateStars(0);
            resultCtrl?.Hide();
        }

        void CacheUIElements()
        {
            var root = uiDocument.rootVisualElement;
            gridContainer = root.Q<VisualElement>("grid-container");
            var tray = root.Q<ScrollView>("item-tray");
            itemTrayContent = tray != null ? tray.contentContainer : null;
            levelLabel = root.Q<Label>("level-label");
            progressFill = root.Q<VisualElement>("progress-fill");
            starLabels = new[]
            {
                root.Q<Label>("star-1"),
                root.Q<Label>("star-2"),
                root.Q<Label>("star-3")
            };
            hintButton = root.Q<Button>("hint-button");
            undoButton = root.Q<Button>("undo-button");
            backButton = root.Q<Button>("back-button");
        }

        void BindButtons()
        {
            if (hintButton != null) hintButton.clicked += OnHintClicked;
            if (undoButton != null) undoButton.clicked += OnUndoClicked;
            if (backButton != null) backButton.clicked += OnBackClicked;
        }

        void UnbindButtons()
        {
            if (hintButton != null) hintButton.clicked -= OnHintClicked;
            if (undoButton != null) undoButton.clicked -= OnUndoClicked;
            if (backButton != null) backButton.clicked -= OnBackClicked;
        }

        void SubscribeEvents()
        {
            eventBus.Subscribe<ItemPlacedEvent>(OnItemPlaced);
            eventBus.Subscribe<ItemRemovedEvent>(OnItemRemoved);
            eventBus.Subscribe<LevelClearedEvent>(OnLevelCleared);
        }

        void UnsubscribeEvents()
        {
            if (eventBus == null) return;
            eventBus.Unsubscribe<ItemPlacedEvent>(OnItemPlaced);
            eventBus.Unsubscribe<ItemRemovedEvent>(OnItemRemoved);
            eventBus.Unsubscribe<LevelClearedEvent>(OnLevelCleared);
        }

        void OnItemPlaced(ItemPlacedEvent _) { RefreshGridFromController(); UpdateProgress(); }
        void OnItemRemoved(ItemRemovedEvent _) { RefreshGridFromController(); UpdateProgress(); }

        void OnLevelCleared(LevelClearedEvent e)
        {
            UpdateStars(e.Stars);
            progression?.RecordCompletion(e.LevelNumber, e.Stars);
            GameSession.LastClearedStars = e.Stars;
            resultCtrl?.ShowCleared(e.Stars);
        }

        void BuildGrid(int width, int height)
        {
            if (gridContainer == null) return;
            gridContainer.Clear();
            cellViews.Clear();

            // 셀 크기는 grid-container의 실제 크기로 결정 (반응형).
            // 첫 빌드 시점엔 layout이 아직 0일 수 있어 fallback 36px 사용 + GeometryChangedEvent 콜백으로 재계산.
            float cellSize = ComputeCellSize(width, height);
            GridCellView.CurrentCellSizePx = cellSize;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var coord = new Vector2Int(x, y);
                    var cell = GridCellView.Create(coord, cellSize);
                    gridContainer.Add(cell);
                    cellViews[coord] = cell;
                }
            }

            // grid-container 크기 변경 시 (디바이스 회전, 첫 layout 등) 셀 재배치.
            gridContainer.UnregisterCallback<GeometryChangedEvent>(OnGridGeometryChanged);
            gridContainer.RegisterCallback<GeometryChangedEvent>(OnGridGeometryChanged);
        }

        float ComputeCellSize(int width, int height)
        {
            float w = gridContainer.contentRect.width;
            float h = gridContainer.contentRect.height;
            if (w <= 0 || h <= 0 || float.IsNaN(w) || float.IsNaN(h)) return 36f;  // fallback (첫 frame)
            return Mathf.Floor(Mathf.Min(w / width, h / height));
        }

        void OnGridGeometryChanged(GeometryChangedEvent evt)
        {
            if (controller?.Grid == null || cellViews == null || cellViews.Count == 0) return;
            int gw = controller.Grid.Width;
            int gh = controller.Grid.Height;
            float cellSize = ComputeCellSize(gw, gh);
            if (Mathf.Approximately(cellSize, GridCellView.CurrentCellSizePx)) return;  // 변경 없음
            GridCellView.CurrentCellSizePx = cellSize;
            foreach (var kv in cellViews)
            {
                GridCellView.Resize(kv.Value, kv.Key, cellSize);
            }
        }

        void BuildItemTray(IReadOnlyList<LevelItemTemplate> items)
        {
            if (itemTrayContent == null || items == null) return;
            itemTrayContent.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                var card = new ItemCardView(items[i].ItemKey, items[i].ToShape(), GetItemColor(items[i].ItemKey));
                card.OnDropped += HandleItemDropped;
                card.OnHovering += HandleItemHovering;
                itemTrayContent.Add(card);
            }
        }

        // 드래그 중 panel pos → 그리드 anchor 계산 → 셀 highlight 갱신 (drop preview)
        // 드래그 매 frame 호출 — §10-3 Profiler 핫패스 식별용 마커.
        void HandleItemHovering(ItemCardView card, Vector2 panelPos)
        {
            if (controller?.Grid == null || cellViews == null) return;

            Profiler.BeginSample("Boxy.Hovering");
            ClearHighlights();
            var anchor = PanelToGridAnchor(panelPos);
            var result = PlacementValidator.Check(controller.Grid, card.CurrentShape, anchor);
            var state = result == PlacementResult.Valid
                ? GridCellView.HighlightState.Valid
                : GridCellView.HighlightState.Invalid;

            for (int i = 0; i < card.CurrentShape.Cells.Count; i++)
            {
                var c = anchor + card.CurrentShape.Cells[i];
                if (cellViews.TryGetValue(c, out var view))
                {
                    GridCellView.SetHighlight(view, state);
                }
            }
            Profiler.EndSample();
        }

        void ClearHighlights()
        {
            // 단순 전략: 전체 셀 occupied 상태로 복원 (highlight 덮어씌운 backgroundColor 제거)
            RefreshGridFromController();
        }

        Vector2Int PanelToGridAnchor(Vector2 panelPos)
        {
            Vector2 local = gridContainer.WorldToLocal(panelPos);
            int x = Mathf.FloorToInt(local.x / GridCellView.CurrentCellSizePx);
            int yFromTop = Mathf.FloorToInt(local.y / GridCellView.CurrentCellSizePx);
            int gridHeight = controller.Grid != null ? controller.Grid.Height : 0;
            int y = gridHeight - 1 - yFromTop;
            return new Vector2Int(x, y);
        }

        // 드롭 위치를 그리드 anchor로 변환 → TryPlace. 실패 시 트레이로 복귀.
        void HandleItemDropped(ItemCardView card, Vector2 panelFinalPos)
        {
            if (controller == null || gridContainer == null)
            {
                card.RestoreVisual();
                return;
            }

            ClearHighlights();
            var anchor = PanelToGridAnchor(panelFinalPos);

            if (controller.TryPlace(card.ItemKey, card.CurrentShape, anchor))
            {
                card.RemoveFromHierarchy();
                FlashPlacedCells(card.CurrentShape, anchor);  // 정확 배치 — 그린 flash
            }
            else
            {
                card.RestoreVisual();
                FlashInvalidCells(card.CurrentShape, anchor);   // 잘못된 배치 — 빨간 flash
                BoxyBootstrap.Instance?.AudioBindings?.PlayInvalid();
            }
        }

        // 정확 배치 시 셀들에 짧은 그린 flash (300ms 후 정상 색으로 복귀).
        void FlashPlacedCells(ItemShape shape, Vector2Int anchor)
        {
            var greenFlash = new Color(0.29f, 0.87f, 0.5f, 0.7f);
            for (int i = 0; i < shape.Cells.Count; i++)
            {
                var c = anchor + shape.Cells[i];
                if (cellViews.TryGetValue(c, out var view))
                {
                    view.style.backgroundColor = greenFlash;
                }
            }
            // 300ms 후 정상 색 복귀
            if (cellViews.Count > 0)
            {
                cellViews[anchor].schedule.Execute(RefreshGridFromController).StartingIn(300);
            }
        }

        // 잘못된 배치 시 영향 셀들에 빨간 flash (out-of-bounds 셀은 안 보임 OK).
        void FlashInvalidCells(ItemShape shape, Vector2Int anchor)
        {
            var redFlash = new Color(0.86f, 0.15f, 0.15f, 0.7f);
            VisualElement firstCell = null;
            for (int i = 0; i < shape.Cells.Count; i++)
            {
                var c = anchor + shape.Cells[i];
                if (cellViews.TryGetValue(c, out var view))
                {
                    view.style.backgroundColor = redFlash;
                    firstCell ??= view;
                }
            }
            firstCell?.schedule.Execute(RefreshGridFromController).StartingIn(300);
        }

        // ClearHighlights / Drop / Place 모두 거치는 핫패스 — 호버 매 frame ClearHighlights 통해 호출.
        void RefreshGridFromController()
        {
            if (controller.Grid == null || cellViews == null) return;
            Profiler.BeginSample("Boxy.RefreshGrid");
            for (int y = 0; y < controller.Grid.Height; y++)
            {
                for (int x = 0; x < controller.Grid.Width; x++)
                {
                    var coord = new Vector2Int(x, y);
                    if (!cellViews.TryGetValue(coord, out var view)) continue;
                    bool occupied = !controller.Grid.IsEmpty(coord);
                    Color cellColor = occupied
                        ? GetItemColor(controller.Grid.GetItemKeyAt(coord))
                        : FilledCellColor;
                    GridCellView.SetOccupied(view, occupied, cellColor);
                }
            }
            Profiler.EndSample();
        }

        void UpdateProgress()
        {
            if (controller.Grid == null || progressFill == null) return;
            int total = controller.Grid.Width * controller.Grid.Height;
            int filled = 0;
            for (int y = 0; y < controller.Grid.Height; y++)
            {
                for (int x = 0; x < controller.Grid.Width; x++)
                {
                    if (!controller.Grid.IsEmpty(new Vector2Int(x, y))) filled++;
                }
            }
            float pct = total > 0 ? (float)filled / total * 100f : 0f;
            progressFill.style.width = new Length(pct, LengthUnit.Percent);
        }

        void UpdateStars(int starCount)
        {
            if (starLabels == null) return;
            for (int i = 0; i < starLabels.Length; i++)
            {
                if (starLabels[i] == null) continue;
                bool filled = i < starCount;
                starLabels[i].text = filled ? "★" : "☆";
                starLabels[i].style.color = filled ? StarFilled : StarEmpty;
            }
        }

        // CLAUDE.md §11-2: 명시적 _ = 표기로 fire-and-forget 의도 명시. 예외는 HandleXxxAsync 내부에서 처리
        void OnHintClicked() => _ = HandleHintAsync();
        void OnUndoClicked() => _ = HandleUndoAsync();

        async Task HandleHintAsync()
        {
            if (cts == null || cts.IsCancellationRequested || hintCtrl == null) return;
            try
            {
                await hintCtrl.RequestHintAsync(cts.Token);
            }
            catch (OperationCanceledException) { /* OnDestroy 시 정상 취소 */ }
            catch (Exception e)
            {
                Debug.LogError($"[GameplayUI] Hint flow failed: {e.Message}");
            }
        }

        async Task HandleUndoAsync()
        {
            if (cts == null || cts.IsCancellationRequested || undoCtrl == null) return;
            try
            {
                await undoCtrl.RequestUndoAsync(cts.Token);
            }
            catch (OperationCanceledException) { /* OnDestroy 시 정상 취소 */ }
            catch (Exception e)
            {
                Debug.LogError($"[GameplayUI] Undo flow failed: {e.Message}");
            }
        }

        void OnBackClicked() => sceneLoader?.LoadScene(BoxySceneNames.LevelSelect);

        void GoToNextLevel()
        {
            if (controller?.CurrentLevel == null) return;
            int nextNum = controller.CurrentLevel.Id.Value + 1;
            if (repository.TryGet(new LevelId(nextNum), out var next))
            {
                StartLevel(next);
            }
            else
            {
                // 마지막 레벨 클리어 — 메인 메뉴로 복귀
                sceneLoader.LoadScene(BoxySceneNames.MainMenu);
            }
        }

        void RetryCurrentLevel()
        {
            if (controller?.CurrentLevel != null) StartLevel(controller.CurrentLevel);
        }
    }
}
