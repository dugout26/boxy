using System.Collections.Generic;
using UnityEngine;

namespace Mound.Localization
{
    // Boxy v1.0 i18n 임시 구현 — 한국어 기본 + 영어 fallback.
    // Day 18 ~ Week 3에 Unity Localization Package로 교체 가능 (IStringTable 뒤에 숨김).
    // 현재 디바이스 SystemLanguage 기준 자동 선택.
    public sealed class KoEnStringTable : IStringTable
    {
        readonly Dictionary<string, string> ko = new()
        {
            // Main Menu
            { StringKey.MenuPlay, "시작하기" },
            { StringKey.MenuSettings, "설정" },
            { StringKey.MenuShop, "상점" },

            // Level Select
            { StringKey.LevelSelectTitle, "레벨 선택" },
            { StringKey.ThemeSchoolBag, "학교 가방" },
            { StringKey.ThemeMovingBox, "이사 박스" },
            { StringKey.ThemeTravelTrunk, "여행 트렁크" },

            // Gameplay
            { StringKey.LevelLabelFormat, "Level {0}" },
            { StringKey.ItemsRemaining, "남은 아이템 {0}" },
            { StringKey.HintButton, "힌트 {0}" },
            { StringKey.UndoButton, "되돌리기 {0}" },

            // Result
            { StringKey.ResultClearedTitle, "레벨 클리어!" },
            { StringKey.ResultFailedTitle, "다시 도전!" },
            { StringKey.NextLevelButton, "다음 레벨" },
            { StringKey.RetryButton, "다시 도전" },
            { StringKey.MenuButton, "메뉴로" },

            // Ad Reward
            { StringKey.AdHintTitle, "힌트가 필요해요?" },
            { StringKey.AdHintBody, "짧은 광고를 보고 힌트 1개를 받아 퍼즐을 이어가세요" },
            { StringKey.AdWatchButton, "광고 보기" },
            { StringKey.AdDismissButton, "다음에 할게요" },

            // Settings
            { StringKey.SettingsTitle, "설정" },
            { StringKey.SettingsBgm, "배경음" },
            { StringKey.SettingsSfx, "효과음" },
            { StringKey.SettingsHaptic, "진동" },
            { StringKey.SettingsLanguage, "언어" },
            { StringKey.SettingsResetProgress, "진행 초기화" },

            // Tutorial
            { StringKey.TutorialWelcome, "환영해요!" },
            { StringKey.TutorialDragHint, "아이템을 그리드로 드래그하세요" },
            { StringKey.TutorialRotateHint, "안 맞으면 길게 누르기 (0.5초)로 회전" },
            { StringKey.TutorialClearHint, "모든 칸을 채우면 ★★★ 클리어!" },

            // Greetings
            { StringKey.LevelSelectGreeting, "Boxy와 함께 다음 단계로!" },

            // Branding
            { StringKey.AppSubtitle, "짐 정리의 즐거움" },
        };

        readonly Dictionary<string, string> en = new()
        {
            // Main Menu
            { StringKey.MenuPlay, "Get started" },
            { StringKey.MenuSettings, "Settings" },
            { StringKey.MenuShop, "Shop" },

            // Level Select
            { StringKey.LevelSelectTitle, "Select Level" },
            { StringKey.ThemeSchoolBag, "School Bag" },
            { StringKey.ThemeMovingBox, "Moving Box" },
            { StringKey.ThemeTravelTrunk, "Travel Trunk" },

            // Gameplay
            { StringKey.LevelLabelFormat, "Level {0}" },
            { StringKey.ItemsRemaining, "{0} items left" },
            { StringKey.HintButton, "Hint {0}" },
            { StringKey.UndoButton, "Undo {0}" },

            // Result
            { StringKey.ResultClearedTitle, "Level Cleared!" },
            { StringKey.ResultFailedTitle, "Try Again!" },
            { StringKey.NextLevelButton, "Next Level" },
            { StringKey.RetryButton, "Retry" },
            { StringKey.MenuButton, "Menu" },

            // Ad Reward
            { StringKey.AdHintTitle, "Need a hint?" },
            { StringKey.AdHintBody, "Watch a short ad to get 1 hint and continue solving" },
            { StringKey.AdWatchButton, "Watch Ad" },
            { StringKey.AdDismissButton, "No thanks" },

            // Settings
            { StringKey.SettingsTitle, "Settings" },
            { StringKey.SettingsBgm, "BGM" },
            { StringKey.SettingsSfx, "SFX" },
            { StringKey.SettingsHaptic, "Vibration" },
            { StringKey.SettingsLanguage, "Language" },
            { StringKey.SettingsResetProgress, "Reset Progress" },

            // Tutorial
            { StringKey.TutorialWelcome, "Welcome!" },
            { StringKey.TutorialDragHint, "Drag items into the grid" },
            { StringKey.TutorialRotateHint, "Long-press (0.5s) to rotate if it doesn't fit" },
            { StringKey.TutorialClearHint, "Fill all cells for ★★★!" },

            // Greetings
            { StringKey.LevelSelectGreeting, "Pack on with Boxy!" },

            // Branding
            { StringKey.AppSubtitle, "Cozy packing puzzle" },
        };

        Dictionary<string, string> active;
        string locale = "ko-KR";

        public KoEnStringTable()
        {
            // 디바이스 시스템 언어 기준 초기 로케일 선택
            locale = Application.systemLanguage == SystemLanguage.Korean ? "ko-KR" : "en-US";
            active = locale == "ko-KR" ? ko : en;
        }

        public string CurrentLocale => locale;

        public string Get(string key)
        {
            if (active != null && active.TryGetValue(key, out var v)) return v;
            // 없으면 영어 fallback, 그것도 없으면 키 자체 반환
            if (en.TryGetValue(key, out var fallback)) return fallback;
            return key ?? string.Empty;
        }

        public string GetWithFallback(string key, string fallback)
        {
            if (active != null && active.TryGetValue(key, out var v)) return v;
            return fallback ?? string.Empty;
        }

        public bool Contains(string key) => active != null && active.ContainsKey(key);

        public void SetLocale(string localeCode)
        {
            locale = localeCode == "ko-KR" ? "ko-KR" : "en-US";
            active = locale == "ko-KR" ? ko : en;
        }
    }
}
