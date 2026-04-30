using System;
using Mound.Core.Save;

namespace Boxy.App.Save
{
    public sealed class ProgressionService
    {
        readonly ISaveSystem saveSystem;
        BoxySaveData cache;

        public BoxySaveData Data => cache;

        public ProgressionService(ISaveSystem saveSystem)
        {
            this.saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            Load();
        }

        public void Load()
        {
            if (!saveSystem.TryLoad<BoxySaveData>(PrefsKey.Progression, out cache) || cache == null)
            {
                cache = new BoxySaveData();
            }
        }

        public void Save() => saveSystem.Save(PrefsKey.Progression, cache);

        public int GetStars(int levelNumber)
        {
            int idx = levelNumber - 1;
            if (idx < 0 || idx >= cache.LevelStars.Count) return 0;
            return cache.LevelStars[idx];
        }

        public bool IsUnlocked(int levelNumber) => levelNumber <= cache.unlockedLevel;
        public bool IsCleared(int levelNumber) => GetStars(levelNumber) > 0;

        // boxy-plan §B-2-1 별 1~3 — 더 높은 별만 갱신 (재플레이 시 점수 보존)
        public void RecordCompletion(int levelNumber, int stars)
        {
            if (stars < 1 || stars > 3) throw new ArgumentOutOfRangeException(nameof(stars));
            int idx = levelNumber - 1;
            if (idx < 0) return;

            cache.EnsureStarSlot(idx);
            if (stars > cache.LevelStars[idx]) cache.SetStarAt(idx, stars);

            int next = levelNumber + 1;
            if (next > cache.unlockedLevel) cache.unlockedLevel = next;

            Save();
        }

        public void SetCurrentLevel(int levelNumber)
        {
            cache.currentLevel = levelNumber;
            Save();
        }

        public void MarkTutorialCompleted()
        {
            if (cache.tutorialCompleted) return;
            cache.tutorialCompleted = true;
            Save();
        }

        public void GrantHints(int amount)
        {
            if (amount <= 0) return;
            cache.hintsOwned += amount;
            Save();
        }

        public bool TryConsumeHint()
        {
            if (cache.hintsOwned <= 0) return false;
            cache.hintsOwned--;
            Save();
            return true;
        }

        public void SetAdsRemoved()
        {
            if (cache.adsRemoved) return;
            cache.adsRemoved = true;
            Save();
        }
    }
}
