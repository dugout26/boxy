using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Boxy.App.Gameplay.Domain;
using Boxy.App.Levels;
using Boxy.App.Levels.Generation;

namespace Boxy.Editor
{
    // boxy-plan §B-3-2 거꾸로 생성 알고리즘으로 50 LevelData asset 일괄 생성.
    // 정수 Unity 활성 후 메뉴 → "Boxy/Generate 50 Levels" 클릭.
    public static class LevelGeneratorMenu
    {
        const string LevelsFolder = "Assets/Levels";
        const int TotalLevels = 50;
        const int Seed = 42;     // 결정성 — 재실행 시 같은 결과

        [MenuItem("Boxy/Generate 50 Levels (overwrites existing)")]
        public static void Generate50Levels()
        {
            if (!EditorUtility.DisplayDialog("Generate Levels",
                $"Will create/overwrite Level_01 ~ Level_50.asset in {LevelsFolder}. Continue?",
                "Yes, generate", "Cancel"))
            {
                return;
            }

            GenerateAllLevels();

            EditorUtility.DisplayDialog("Done",
                $"Generated {TotalLevels} levels in {LevelsFolder}",
                "OK");
        }

        // batchmode/automation 진입점 — dialog 없음. BoxyAutomation에서 호출.
        public static void GenerateAllLevels()
        {
            EnsureFolderExists();

            var generator = new LevelGenerator(Seed);

            for (int i = 1; i <= TotalLevels; i++)
            {
                var (width, height, themeKey, shapes) = ResolveLevelConfig(i);
                var keyPool = ItemKeyPool.ForTheme(themeKey);
                var items = generator.Generate(width, height, shapes, keyPool);

                var level = ScriptableObject.CreateInstance<LevelData>();
                level.Initialize(i, width, height, themeKey, items);

                string path = $"{LevelsFolder}/Level_{i:D2}.asset";
                AssetDatabase.CreateAsset(level, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // boxy-plan §B-3 레벨 구조: 5 튜토리얼 / 6-20 초반 / 21-33 이사박스 / 34-45 트렁크 / 46-50 챌린지
        static (int width, int height, string themeKey, IReadOnlyList<ItemShape> shapes) ResolveLevelConfig(int levelNumber)
        {
            if (levelNumber <= 5)
            {
                return (
                    width: 4 + (levelNumber - 1) / 2,
                    height: 2 + (levelNumber - 1) / 2,
                    themeKey: "school_bag",
                    shapes: ShapeCatalog.Tutorial);
            }
            if (levelNumber <= 17)
            {
                return (6, 6, "school_bag", ShapeCatalog.Standard);
            }
            if (levelNumber <= 33)
            {
                return (6, 7, "moving_box", ShapeCatalog.Standard);
            }
            // 34-50 trunk (마지막 5개는 챌린지 — 향후 timeLimitSeconds 설정 추가 권장)
            return (6, 8, "travel_trunk", ShapeCatalog.Standard);
        }

        static void EnsureFolderExists()
        {
            if (!Directory.Exists(LevelsFolder))
            {
                Directory.CreateDirectory(LevelsFolder);
                AssetDatabase.Refresh();
            }
        }
    }
}
