using UnityEngine;

namespace Boxy.App.Gameplay.Events
{
    public readonly struct LevelStartedEvent
    {
        public readonly int LevelNumber;
        public LevelStartedEvent(int level) { LevelNumber = level; }
    }

    public readonly struct ItemPlacedEvent
    {
        public readonly string ItemKey;
        public readonly Vector2Int Anchor;

        public ItemPlacedEvent(string key, Vector2Int anchor)
        {
            ItemKey = key;
            Anchor = anchor;
        }
    }

    public readonly struct ItemRemovedEvent
    {
        public readonly string ItemKey;
        public ItemRemovedEvent(string key) { ItemKey = key; }
    }

    public readonly struct LevelClearedEvent
    {
        public readonly int LevelNumber;
        public readonly int Stars;
        public readonly bool UsedHint;
        public readonly bool UsedUndo;

        public LevelClearedEvent(int level, int stars, bool usedHint, bool usedUndo)
        {
            LevelNumber = level;
            Stars = stars;
            UsedHint = usedHint;
            UsedUndo = usedUndo;
        }
    }
}
