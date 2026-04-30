namespace Boxy.App.Save
{
    // CLAUDE.md §3-3: PlayerPrefs 매직 문자열 금지 — 모든 키는 여기에 등록
    // _v1 suffix: §7-1 마이그레이션 시 _v2로 갱신 + Migration 코드 추가
    public static class PrefsKey
    {
        public const string Progression = "boxy_progression_v1";
        public const string SettingsBgmVolume = "boxy_settings_bgm_v1";
        public const string SettingsSfxVolume = "boxy_settings_sfx_v1";
        public const string SettingsHapticEnabled = "boxy_settings_haptic_v1";
        public const string LanguageCode = "boxy_language_v1";
        public const string LastInterstitialAtUnixSec = "boxy_last_interstitial_v1";
    }
}
