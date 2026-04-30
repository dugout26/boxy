using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boxy.App.Levels
{
    public sealed class LevelRepository
    {
        readonly Dictionary<LevelId, LevelData> levels;

        public int Count => levels.Count;
        public IReadOnlyCollection<LevelData> All => levels.Values;

        public LevelRepository(IEnumerable<LevelData> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            levels = new Dictionary<LevelId, LevelData>();
            foreach (var level in source)
            {
                if (level == null) continue;
                if (!levels.TryAdd(level.Id, level))
                {
                    Debug.LogWarning($"[LevelRepository] Duplicate {level.Id} in {level.name} — keeping first");
                }
            }
        }

        public LevelData Get(LevelId id)
        {
            if (!levels.TryGetValue(id, out var data))
                throw new KeyNotFoundException($"LevelData for {id} not found");
            return data;
        }

        public bool TryGet(LevelId id, out LevelData data) => levels.TryGetValue(id, out data);

        public bool Contains(LevelId id) => levels.ContainsKey(id);
    }
}
