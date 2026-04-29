using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Mound.Core.Save;
using Boxy.App.Save;

namespace Boxy.Tests.EditMode
{
    // 테스트 mock — PlayerPrefs 오염 회피. 메모리 저장소로 ISaveSystem 구현.
    sealed class InMemorySaveSystem : ISaveSystem
    {
        readonly Dictionary<string, string> store = new Dictionary<string, string>();

        public void Save<T>(string key, T data) where T : SaveData
        {
            store[key] = JsonUtility.ToJson(data);
        }

        public bool TryLoad<T>(string key, out T data) where T : SaveData, new()
        {
            data = null;
            if (!store.TryGetValue(key, out var json) || string.IsNullOrEmpty(json)) return false;
            try
            {
                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch (System.Exception)
            {
                // 테스트 mock: JSON 파싱 실패 시 기본값 (null)
                return false;
            }
        }

        public void Delete(string key) => store.Remove(key);
        public void DeleteAll() => store.Clear();
    }

    [TestFixture]
    public class ProgressionServiceTests
    {
        [Test]
        public void NewService_HasDefaultData()
        {
            var svc = new ProgressionService(new InMemorySaveSystem());
            Assert.AreEqual(1, svc.Data.currentLevel);
            Assert.AreEqual(1, svc.Data.unlockedLevel);
            Assert.IsFalse(svc.Data.tutorialCompleted);
            Assert.IsFalse(svc.Data.adsRemoved);
            Assert.AreEqual(0, svc.Data.hintsOwned);
        }

        [Test]
        public void RecordCompletion_UnlocksNextLevel()
        {
            var svc = new ProgressionService(new InMemorySaveSystem());
            svc.RecordCompletion(1, 3);
            Assert.AreEqual(2, svc.Data.unlockedLevel);
            Assert.AreEqual(3, svc.GetStars(1));
            Assert.IsTrue(svc.IsUnlocked(2));
            Assert.IsFalse(svc.IsUnlocked(3));
        }

        [Test]
        public void RecordCompletion_OnlyUpdatesIfHigherStars()
        {
            var svc = new ProgressionService(new InMemorySaveSystem());
            svc.RecordCompletion(1, 3);
            svc.RecordCompletion(1, 1);   // 이전보다 낮은 별 — 보존되어야 함
            Assert.AreEqual(3, svc.GetStars(1));
        }

        [Test]
        public void RecordCompletion_InvalidStars_Throws()
        {
            var svc = new ProgressionService(new InMemorySaveSystem());
            Assert.Throws<System.ArgumentOutOfRangeException>(() => svc.RecordCompletion(1, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => svc.RecordCompletion(1, 4));
        }

        [Test]
        public void IsCleared_ReturnsTrue_WhenStarsEarned()
        {
            var svc = new ProgressionService(new InMemorySaveSystem());
            Assert.IsFalse(svc.IsCleared(1));
            svc.RecordCompletion(1, 1);
            Assert.IsTrue(svc.IsCleared(1));
        }

        [Test]
        public void Save_Load_RoundTrip()
        {
            var saveSystem = new InMemorySaveSystem();
            var svc1 = new ProgressionService(saveSystem);
            svc1.RecordCompletion(1, 3);
            svc1.RecordCompletion(2, 2);
            svc1.GrantHints(5);

            // 새 인스턴스로 Load — 영속성 검증
            var svc2 = new ProgressionService(saveSystem);
            Assert.AreEqual(3, svc2.Data.unlockedLevel);
            Assert.AreEqual(3, svc2.GetStars(1));
            Assert.AreEqual(2, svc2.GetStars(2));
            Assert.AreEqual(5, svc2.Data.hintsOwned);
        }

        [Test]
        public void HintConsumption_DecrementsCount()
        {
            var svc = new ProgressionService(new InMemorySaveSystem());
            svc.GrantHints(2);
            Assert.IsTrue(svc.TryConsumeHint());
            Assert.AreEqual(1, svc.Data.hintsOwned);
            Assert.IsTrue(svc.TryConsumeHint());
            Assert.IsFalse(svc.TryConsumeHint());   // 0개 남음
        }

        [Test]
        public void MarkTutorialCompleted_Persists()
        {
            var saveSystem = new InMemorySaveSystem();
            var svc = new ProgressionService(saveSystem);
            svc.MarkTutorialCompleted();

            var svc2 = new ProgressionService(saveSystem);
            Assert.IsTrue(svc2.Data.tutorialCompleted);
        }

        [Test]
        public void SetAdsRemoved_Persists()
        {
            var saveSystem = new InMemorySaveSystem();
            var svc = new ProgressionService(saveSystem);
            svc.SetAdsRemoved();

            var svc2 = new ProgressionService(saveSystem);
            Assert.IsTrue(svc2.Data.adsRemoved);
        }
    }

}
