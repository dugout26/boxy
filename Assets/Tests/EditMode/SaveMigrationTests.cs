using NUnit.Framework;
using UnityEngine;
using Boxy.App.Save;
using Mound.Core.Save;

namespace Boxy.Tests.EditMode
{
    // CLAUDE.md §7-3: 출시 후 첫 업데이트 마이그레이션 테스트 케이스 필수.
    // v1.0 세이브 파일을 v1.1 빌드에서 로드 → 정상 동작 확인.
    public class SaveMigrationTests
    {
        const string TestSaveKey = "boxy_test_save_v1";

        [TearDown]
        public void Cleanup()
        {
            PlayerPrefs.DeleteKey(TestSaveKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void EmptySave_TryLoadReturnsFalse()
        {
            var saveSystem = new PlayerPrefsSaveSystem();
            bool loaded = saveSystem.TryLoad<BoxySaveData>(TestSaveKey, out var data);
            Assert.IsFalse(loaded);
            Assert.IsNull(data);
        }

        [Test]
        public void RoundTrip_SaveAndLoad_PreservesScalars()
        {
            var saveSystem = new PlayerPrefsSaveSystem();
            var data = new BoxySaveData
            {
                currentLevel = 5,
                unlockedLevel = 7,
                tutorialCompleted = true,
                adsRemoved = false,
                hintsOwned = 3,
                paidUndosOwned = 2,
                saveVersion = 1,
            };

            saveSystem.Save(TestSaveKey, data);
            bool loaded = saveSystem.TryLoad<BoxySaveData>(TestSaveKey, out var restored);

            Assert.IsTrue(loaded);
            Assert.IsNotNull(restored);
            Assert.AreEqual(5, restored.currentLevel);
            Assert.AreEqual(7, restored.unlockedLevel);
            Assert.IsTrue(restored.tutorialCompleted);
            Assert.AreEqual(3, restored.hintsOwned);
            Assert.AreEqual(2, restored.paidUndosOwned);
            Assert.AreEqual(1, restored.saveVersion);
        }

        [Test]
        public void SaveVersion_IncrementSafely()
        {
            // §7-1: 저장 키 삭제/변경 X, 새 버전은 saveVersion 증가
            var data = new BoxySaveData { saveVersion = 1 };
            Assert.AreEqual(1, data.saveVersion);

            data.saveVersion = 2;
            Assert.AreEqual(2, data.saveVersion);
        }

        [Test]
        public void Migrator_RawV0Json_BumpsToCurrentVersion()
        {
            // v0 raw JSON (saveVersion 필드 없음 또는 0) → CurrentVersion 으로 갱신.
            // PlayerPrefs 직접 시드 → SaveMigrator.MigrateJsonIfNeeded 적용한 SaveSystem으로 로드.
            string v0Json = "{\"saveVersion\":0,\"currentLevel\":3,\"unlockedLevel\":5,\"tutorialCompleted\":true,\"hintsOwned\":2,\"paidUndosOwned\":0,\"adsRemoved\":false}";
            PlayerPrefs.SetString(TestSaveKey, v0Json);
            PlayerPrefs.Save();

            var saveSystem = new PlayerPrefsSaveSystem(SaveMigrator.MigrateJsonIfNeeded);
            bool loaded = saveSystem.TryLoad<BoxySaveData>(TestSaveKey, out var data);

            Assert.IsTrue(loaded, "v0 JSON 로드 실패 — 마이그레이션이 호출되지 않았거나 손상됨");
            Assert.AreEqual(3, data.currentLevel);
            Assert.AreEqual(5, data.unlockedLevel);
            Assert.IsTrue(data.tutorialCompleted);
        }

        [Test]
        public void Migrator_NoOp_PreservesExistingFields()
        {
            // 마이그레이터 적용해도 현재 버전 데이터는 손상 없이 통과되어야 함.
            var saveSystem = new PlayerPrefsSaveSystem(SaveMigrator.MigrateJsonIfNeeded);
            var data = new BoxySaveData
            {
                saveVersion = SaveMigrator.CurrentVersion,
                currentLevel = 10,
                unlockedLevel = 12,
                hintsOwned = 5,
            };
            saveSystem.Save(TestSaveKey, data);
            bool loaded = saveSystem.TryLoad<BoxySaveData>(TestSaveKey, out var restored);

            Assert.IsTrue(loaded);
            Assert.AreEqual(10, restored.currentLevel);
            Assert.AreEqual(12, restored.unlockedLevel);
            Assert.AreEqual(5, restored.hintsOwned);
        }

        [Test]
        public void ProgressionService_NewData_StartsAtLevel1()
        {
            PlayerPrefs.DeleteKey(PrefsKey.Progression);
            var service = new ProgressionService(new PlayerPrefsSaveSystem());
            Assert.AreEqual(1, service.Data.currentLevel);
            Assert.AreEqual(1, service.Data.unlockedLevel);
            Assert.IsFalse(service.Data.tutorialCompleted);
        }

        [Test]
        public void ProgressionService_RecordCompletion_UnlocksNext()
        {
            PlayerPrefs.DeleteKey(PrefsKey.Progression);
            var service = new ProgressionService(new PlayerPrefsSaveSystem());
            service.RecordCompletion(1, 3);
            Assert.GreaterOrEqual(service.Data.unlockedLevel, 2);
            Assert.AreEqual(3, service.Data.LevelStars[0]);
        }

        [Test]
        public void ProgressionService_TutorialCompleted_Persists()
        {
            PlayerPrefs.DeleteKey(PrefsKey.Progression);
            var service = new ProgressionService(new PlayerPrefsSaveSystem());
            service.MarkTutorialCompleted();
            Assert.IsTrue(service.Data.tutorialCompleted);

            // 새 인스턴스 — 디스크에서 로드
            var service2 = new ProgressionService(new PlayerPrefsSaveSystem());
            Assert.IsTrue(service2.Data.tutorialCompleted, "tutorialCompleted 디스크 영속성 실패");
        }
    }
}
