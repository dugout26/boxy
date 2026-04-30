using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Mound.Core.Save;
using Mound.Core.Scenes;
using Mound.Localization;
using Boxy.App.Levels;
using Boxy.App.Save;

namespace Boxy.App.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class LevelSelectController : MonoBehaviour
    {
        // Inspector 연결: 모든 LevelData asset 드래그 (Boxy v1.0 = 50개)
        [SerializeField] LevelData[] levelAssets;

        UIDocument uiDocument;
        ISceneLoader sceneLoader;
        ProgressionService progression;
        LevelRepository repository;

        Button backButton;
        Button tabSchool;
        Button tabBox;
        Button tabTrunk;
        VisualElement levelGrid;

        string activeTheme = "school_bag";

        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            sceneLoader = new SceneLoader();
            progression = new ProgressionService(new PlayerPrefsSaveSystem(SaveMigrator.MigrateJsonIfNeeded));
            repository = new LevelRepository(levelAssets ?? new LevelData[0]);

            var root = uiDocument.rootVisualElement;
            backButton = root.Q<Button>("back-button");
            tabSchool = root.Q<Button>("tab-school");
            tabBox = root.Q<Button>("tab-box");
            tabTrunk = root.Q<Button>("tab-trunk");
            levelGrid = root.Q<VisualElement>("level-grid");

            if (backButton != null) backButton.clicked += OnBack;
            if (tabSchool != null) tabSchool.clicked += () => SwitchTheme("school_bag", tabSchool);
            if (tabBox != null) tabBox.clicked += () => SwitchTheme("moving_box", tabBox);
            if (tabTrunk != null) tabTrunk.clicked += () => SwitchTheme("travel_trunk", tabTrunk);

            ApplyLocalization();
            RebuildGrid();
        }

        // 인사말 + 탭 라벨을 IStringTable로 — 디바이스 SystemLanguage / 사용자 토글에 자동 반응.
        void ApplyLocalization()
        {
            var strings = BoxyBootstrap.Instance?.Strings;
            if (strings == null) return;
            var root = uiDocument.rootVisualElement;
            var greeting = root.Q<Label>("greeting-label");
            if (greeting != null) greeting.text = strings.Get(StringKey.LevelSelectGreeting);
            if (tabSchool != null) tabSchool.text = strings.Get(StringKey.ThemeSchoolBag);
            if (tabBox != null) tabBox.text = strings.Get(StringKey.ThemeMovingBox);
            if (tabTrunk != null) tabTrunk.text = strings.Get(StringKey.ThemeTravelTrunk);
        }

        void OnDestroy()
        {
            if (backButton != null) backButton.clicked -= OnBack;
            // 익명 lambda는 -= 매칭 어렵지만 OnDestroy 시 컴포넌트와 함께 GC됨 (CLAUDE.md §4-2 짝 패턴 예외 — 리소스 누수 없음)
        }

        void SwitchTheme(string themeKey, Button activated)
        {
            activeTheme = themeKey;
            ToggleTabClass(tabSchool, activated == tabSchool);
            ToggleTabClass(tabBox, activated == tabBox);
            ToggleTabClass(tabTrunk, activated == tabTrunk);
            RebuildGrid();
        }

        static void ToggleTabClass(Button btn, bool active)
        {
            if (btn == null) return;
            const string activeClass = "theme-tab-active";
            if (active) btn.AddToClassList(activeClass);
            else btn.RemoveFromClassList(activeClass);
        }

        void RebuildGrid()
        {
            if (levelGrid == null) return;
            levelGrid.Clear();

            VisualElement currentLevelBtn = null;

            foreach (var level in FilterByTheme(activeTheme))
            {
                int levelNum = level.Id.Value;
                var btn = new Button();
                btn.AddToClassList("level-button");

                if (!progression.IsUnlocked(levelNum))
                {
                    btn.AddToClassList("level-button-locked");
                    // 잠긴 레벨도 번호 표시 (저채도) — 이모지는 Pretendard 미지원이라 깨지므로 미사용.
                    // §A-5 디자인 시스템: 미해금 = 흐리게.
                    btn.text = levelNum.ToString();
                    btn.SetEnabled(false);
                }
                else if (levelNum == progression.Data.currentLevel)
                {
                    btn.AddToClassList("level-button-current");
                    btn.text = levelNum.ToString();
                    btn.clicked += () => StartLevel(level);
                    currentLevelBtn = btn;  // 현재 레벨 버튼으로 자동 스크롤 위해 저장
                }
                else
                {
                    btn.AddToClassList("level-button-unlocked");
                    btn.text = levelNum.ToString();
                    btn.clicked += () => StartLevel(level);
                }

                levelGrid.Add(btn);
            }

            // 현재 레벨 버튼이 보이도록 스크롤 — 한 프레임 후 layout 완료 시점에 실행.
            if (currentLevelBtn != null)
            {
                currentLevelBtn.schedule.Execute(() =>
                {
                    var scroll = uiDocument.rootVisualElement.Q<ScrollView>("level-scroll");
                    scroll?.ScrollTo(currentLevelBtn);
                }).StartingIn(50);
            }
        }

        IEnumerable<LevelData> FilterByTheme(string themeKey)
        {
            foreach (var level in repository.All)
            {
                if (level.ThemeKey == themeKey) yield return level;
            }
        }

        void StartLevel(LevelData level)
        {
            GameSession.PendingLevel = level;
            progression.SetCurrentLevel(level.Id.Value);
            sceneLoader.LoadScene(BoxySceneNames.Gameplay);
        }

        void OnBack() => sceneLoader.LoadScene(BoxySceneNames.MainMenu);
    }
}
