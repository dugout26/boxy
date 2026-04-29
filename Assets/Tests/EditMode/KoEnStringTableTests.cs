using NUnit.Framework;
using Mound.Localization;

namespace Boxy.Tests.EditMode
{
    public class KoEnStringTableTests
    {
        [Test]
        public void DefaultLocale_HasKoreanStrings()
        {
            var table = new KoEnStringTable();
            // 디바이스 SystemLanguage에 따라 다르지만, 키는 항상 fallback 값 반환
            Assert.That(table.Get(StringKey.MenuPlay), Is.Not.Empty);
        }

        [Test]
        public void SetLocale_KoToEn_ReturnsEnglishStrings()
        {
            var table = new KoEnStringTable();
            table.SetLocale("en-US");
            Assert.AreEqual("Settings", table.Get(StringKey.MenuSettings));
            Assert.AreEqual("Select Level", table.Get(StringKey.LevelSelectTitle));
        }

        [Test]
        public void SetLocale_EnToKo_ReturnsKoreanStrings()
        {
            var table = new KoEnStringTable();
            table.SetLocale("ko-KR");
            Assert.AreEqual("설정", table.Get(StringKey.MenuSettings));
            Assert.AreEqual("레벨 선택", table.Get(StringKey.LevelSelectTitle));
        }

        [Test]
        public void Get_UnknownKey_FallsBackToEnglishOrKey()
        {
            var table = new KoEnStringTable();
            string result = table.Get("unknown_random_key_999");
            Assert.That(result, Is.EqualTo("unknown_random_key_999"));  // 키 자체 반환
        }

        [Test]
        public void GetWithFallback_UnknownKey_ReturnsFallback()
        {
            var table = new KoEnStringTable();
            Assert.AreEqual("custom fallback", table.GetWithFallback("nope", "custom fallback"));
        }

        [Test]
        public void Contains_KnownKey_ReturnsTrue()
        {
            var table = new KoEnStringTable();
            table.SetLocale("ko-KR");
            Assert.IsTrue(table.Contains(StringKey.MenuPlay));
        }

        [Test]
        public void Contains_UnknownKey_ReturnsFalse()
        {
            var table = new KoEnStringTable();
            Assert.IsFalse(table.Contains("definitely_not_a_key"));
        }

        [Test]
        public void AllStringKeysHaveBothKoEnTranslations()
        {
            // 컴파일 타임 보장 — StringKey 상수 모두 ko + en 매핑되어 있는지
            // 새 키 추가 시 빠뜨리지 않게 보호
            var table = new KoEnStringTable();
            string[] allKeys = new[]
            {
                StringKey.MenuPlay, StringKey.MenuSettings, StringKey.MenuShop,
                StringKey.LevelSelectTitle, StringKey.ThemeSchoolBag, StringKey.ThemeMovingBox, StringKey.ThemeTravelTrunk,
                StringKey.LevelLabelFormat, StringKey.ItemsRemaining, StringKey.HintButton, StringKey.UndoButton,
                StringKey.ResultClearedTitle, StringKey.ResultFailedTitle,
                StringKey.NextLevelButton, StringKey.RetryButton, StringKey.MenuButton,
                StringKey.AdHintTitle, StringKey.AdHintBody, StringKey.AdWatchButton, StringKey.AdDismissButton,
                StringKey.SettingsTitle, StringKey.SettingsBgm, StringKey.SettingsSfx,
                StringKey.SettingsHaptic, StringKey.SettingsLanguage, StringKey.SettingsResetProgress,
                StringKey.TutorialWelcome, StringKey.TutorialDragHint,
                StringKey.TutorialRotateHint, StringKey.TutorialClearHint,
                StringKey.LevelSelectGreeting, StringKey.AppSubtitle
            };

            table.SetLocale("ko-KR");
            foreach (var key in allKeys)
            {
                Assert.IsTrue(table.Contains(key), $"한국어 키 누락: {key}");
            }
            table.SetLocale("en-US");
            foreach (var key in allKeys)
            {
                Assert.IsTrue(table.Contains(key), $"영어 키 누락: {key}");
            }
        }
    }
}
