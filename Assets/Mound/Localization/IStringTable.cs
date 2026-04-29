namespace Mound.Localization
{
    // CLAUDE.md §5-3 SDK 격리: Unity Localization Package를 이 인터페이스 뒤로 감춤.
    // Boxy.App은 LocalizationSettings.* 직접 호출 X.
    public interface IStringTable
    {
        string CurrentLocale { get; }
        string Get(string key);
        string GetWithFallback(string key, string fallback);
        bool Contains(string key);
        void SetLocale(string localeCode);
    }

    // dev/test/early-build 스텁. Production은 UnityLocalizationStringTable로 교체 (Week 3).
    // 키를 그대로 반환 — 디버깅/개발 시 어떤 키가 등록 안 됐는지 즉시 화면에서 식별 가능.
    public sealed class NullStringTable : IStringTable
    {
        public string CurrentLocale => "ko-KR";

        public string Get(string key) => key ?? string.Empty;

        public string GetWithFallback(string key, string fallback)
            => string.IsNullOrEmpty(key) ? fallback : key;

        public bool Contains(string key) => false;

        public void SetLocale(string localeCode)
        {
            // no-op stub
        }
    }

    // i18n 키 prefix 컨벤션 (Boxy v1.0).
    // mound-design-system v2 §10: 화면별 prefix
    public static class StringKey
    {
        // Main Menu
        public const string MenuPlay = "menu_play";
        public const string MenuSettings = "menu_settings";
        public const string MenuShop = "menu_shop";

        // Level Select
        public const string LevelSelectTitle = "level_select_title";
        public const string ThemeSchoolBag = "theme_school_bag";
        public const string ThemeMovingBox = "theme_moving_box";
        public const string ThemeTravelTrunk = "theme_travel_trunk";

        // Gameplay
        public const string LevelLabelFormat = "level_label_format";
        public const string ItemsRemaining = "items_remaining";
        public const string HintButton = "hint_button";
        public const string UndoButton = "undo_button";

        // Result
        public const string ResultClearedTitle = "result_cleared_title";
        public const string ResultFailedTitle = "result_failed_title";
        public const string NextLevelButton = "next_level_button";
        public const string RetryButton = "retry_button";
        public const string MenuButton = "menu_button";

        // Ad Reward
        public const string AdHintTitle = "ad_hint_title";
        public const string AdHintBody = "ad_hint_body";
        public const string AdWatchButton = "ad_watch_button";
        public const string AdDismissButton = "ad_dismiss_button";

        // Settings
        public const string SettingsTitle = "settings_title";
        public const string SettingsBgm = "settings_bgm";
        public const string SettingsSfx = "settings_sfx";
        public const string SettingsHaptic = "settings_haptic";
        public const string SettingsLanguage = "settings_language";
        public const string SettingsResetProgress = "settings_reset_progress";

        // Tutorial
        public const string TutorialWelcome = "tutorial_welcome";
        public const string TutorialDragHint = "tutorial_drag_hint";
        public const string TutorialRotateHint = "tutorial_rotate_hint";
        public const string TutorialClearHint = "tutorial_clear_hint";

        // Greetings
        public const string LevelSelectGreeting = "level_select_greeting";

        // Branding
        public const string AppSubtitle = "app_subtitle";
    }
}
