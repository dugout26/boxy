using System.Collections.Generic;
using UnityEngine;
using Boxy.App.Levels.Generation;

namespace Boxy.App.Levels
{
    // boxy-plan §B-3-1 첫 5레벨 — D1 retention 70% 결정 핵심.
    // .asset 파일 없이 런타임 생성. Editor 활성 후 정수가 정성 디자인된 .asset으로 export 권장.
    // 시드 고정으로 같은 결과 재현 (seed=42) — A/B 테스트 시 비교 가능.
    public static class TutorialLevels
    {
        public static IReadOnlyList<LevelData> Create()
        {
            var generator = new LevelGenerator(seed: 42);
            var keyPool = ItemKeyPool.SchoolBag;

            var list = new List<LevelData>
            {
                Build(1, 4, 2, generator, keyPool),   // 보장: 90% 30s 클리어, 직사각형 2개
                Build(2, 4, 3, generator, keyPool),   // 회전 1번 학습
                Build(3, 4, 4, generator, keyPool),   // "거의 다 됐는데" 첫 등장
                Build(4, 5, 4, generator, keyPool),   // 회전 2번
                Build(5, 6, 5, generator, keyPool)    // 첫 별 3개 도전
            };
            return list;
        }

        static LevelData Build(int levelNumber, int width, int height,
            LevelGenerator generator, IReadOnlyList<string> keyPool)
        {
            var items = generator.Generate(width, height, ShapeCatalog.Tutorial, keyPool);

            var level = ScriptableObject.CreateInstance<LevelData>();
            level.name = $"Tutorial_L{levelNumber:D2}";
            level.Initialize(levelNumber, width, height, "school_bag", items);
            return level;
        }
    }
}
