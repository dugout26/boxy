using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Boxy.App.Gameplay.Domain;

// Boxy.Editor 어셈블리에서 LevelData.Initialize internal 메서드 사용 가능하도록 노출
[assembly: InternalsVisibleTo("Boxy.Editor")]
// PlayMode 테스트도 동일 권한 — 테스트용 LevelData 즉석 생성
[assembly: InternalsVisibleTo("Boxy.Tests.PlayMode")]

namespace Boxy.App.Levels
{
    public readonly struct LevelId : IEquatable<LevelId>
    {
        public readonly int Value;
        public LevelId(int value) { Value = value; }

        public bool Equals(LevelId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is LevelId id && Equals(id);
        public override int GetHashCode() => Value;
        public override string ToString() => $"L{Value:D2}";

        public static bool operator ==(LevelId a, LevelId b) => a.Equals(b);
        public static bool operator !=(LevelId a, LevelId b) => !a.Equals(b);
    }

    [Serializable]
    public sealed class LevelItemTemplate
    {
        // CLAUDE.md §6-1: public 컬렉션 노출 X. [SerializeField] private + 읽기 전용 property 노출.
        // JsonUtility/Unity Inspector 모두 [SerializeField] private를 정상 직렬화/편집함.
        [SerializeField] string itemKey;
        [SerializeField] List<Vector2Int> cells = new();

        public string ItemKey => itemKey;
        public IReadOnlyList<Vector2Int> Cells => cells;

        public LevelItemTemplate() { }

        public LevelItemTemplate(string itemKey, IEnumerable<Vector2Int> cellSource)
        {
            this.itemKey = itemKey;
            cells = cellSource != null ? new List<Vector2Int>(cellSource) : new List<Vector2Int>();
        }

        public ItemShape ToShape()
        {
            if (cells == null || cells.Count == 0)
                throw new InvalidOperationException($"LevelItemTemplate '{itemKey}' has no cells");
            return ItemShape.FromCells(cells);
        }
    }

    [CreateAssetMenu(menuName = "Boxy/Level Data", fileName = "Level_New")]
    public sealed class LevelData : ScriptableObject
    {
        [SerializeField] int levelNumber;
        [SerializeField] int gridWidth = 6;
        [SerializeField] int gridHeight = 8;
        [SerializeField] string themeKey = "school_bag";
        [SerializeField] List<LevelItemTemplate> items = new();
        [SerializeField] int rotationLimit = -1;       // -1 = 무제한
        [SerializeField] int timeLimitSeconds = -1;    // -1 = 시간 제한 없음

        public LevelId Id => new(levelNumber);
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public string ThemeKey => themeKey;
        public IReadOnlyList<LevelItemTemplate> Items => items;
        public int RotationLimit => rotationLimit;
        public int TimeLimitSeconds => timeLimitSeconds;
        public bool HasTimeLimit => timeLimitSeconds > 0;
        public bool HasRotationLimit => rotationLimit >= 0;

        // 런타임 LevelData 생성용 (TutorialLevels / Editor 툴). 어셈블리 내부 호출만.
        // .asset 파일 없이 ScriptableObject.CreateInstance<LevelData>() + Initialize 패턴.
        internal void Initialize(int levelNumber, int gridWidth, int gridHeight, string themeKey,
            IEnumerable<LevelItemTemplate> sourceItems)
        {
            this.levelNumber = levelNumber;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.themeKey = themeKey;
            this.items = sourceItems != null ? new List<LevelItemTemplate>(sourceItems) : new List<LevelItemTemplate>();
        }
    }
}
