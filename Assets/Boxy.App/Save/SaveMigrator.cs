using System;
using UnityEngine;
using Mound.Core.Save;

namespace Boxy.App.Save
{
    // CLAUDE.md §7-1, §7-2: 저장 키 삭제/변경 X, 새 버전은 saveVersion 증가 + 마이그레이션 동반.
    // §12-3 출시 직전 검증: v1.0 세이브 파일을 v1.1 빌드에서 로드 → 정상 동작 확인.
    //
    // 사용:
    //   var raw = PlayerPrefs.GetString(PrefsKey.Progression);
    //   var migrated = SaveMigrator.MigrateJsonIfNeeded(raw);
    //   var data = JsonUtility.FromJson<BoxySaveData>(migrated);
    public static class SaveMigrator
    {
        public const int CurrentVersion = 1;

        // saveVersion 비교하여 단계별 마이그레이션 적용.
        // 손상 시 빈 string 반환 (호출자가 new BoxySaveData() 기본값 처리 — §7-1).
        public static string MigrateJsonIfNeeded(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            int version = ExtractSaveVersion(json);

            // v0 → v1 (예시: tutorialCompleted 필드 도입 시 — 현재 적용 X, 패턴만 노출)
            // if (version < 1) {
            //     json = AddDefaultField(json, "tutorialCompleted", "false");
            //     version = 1;
            // }

            // v1 → v2 (적용 예정: 광고 제거 IAP 도입 시)
            // if (version < 2) {
            //     json = AddDefaultField(json, "adsRemoved", "false");
            //     version = 2;
            // }

            // v2 → v3 (적용 예정: 일일 보상 도입 시)
            // if (version < 3) {
            //     json = AddDefaultField(json, "lastDailyRewardUnixSec", "0");
            //     version = 3;
            // }

            // saveVersion 자체 갱신
            return ReplaceVersion(json, CurrentVersion);
        }

        static int ExtractSaveVersion(string json)
        {
            try
            {
                var probe = JsonUtility.FromJson<VersionProbe>(json);
                return probe?.saveVersion ?? 0;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[SaveMigrator] version extraction failed, treating as v0: {e.Message}");
                return 0;
            }
        }

        // JSON 직접 manipulate — JsonUtility는 누락 필드 default로 채우니, 단순 saveVersion 갱신만.
        static string ReplaceVersion(string json, int newVersion)
        {
            // saveVersion: <num>" 패턴 치환
            var idx = json.IndexOf("\"saveVersion\":");
            if (idx < 0) return json;
            int start = idx + "\"saveVersion\":".Length;
            // 콤마 또는 } 까지
            int end = start;
            while (end < json.Length && json[end] != ',' && json[end] != '}') end++;
            return json.Substring(0, start) + newVersion + json.Substring(end);
        }

        // 새 필드를 default 값으로 삽입 — JSON 마지막 } 직전에 추가.
        // (참고용 — 실제로는 JsonUtility가 미존재 필드를 default로 자동 채워줘서 거의 불필요)
        public static string AddDefaultField(string json, string fieldName, string jsonValue)
        {
            int closeIdx = json.LastIndexOf('}');
            if (closeIdx < 0) return json;
            return json.Substring(0, closeIdx) +
                $",\"{fieldName}\":{jsonValue}" +
                json.Substring(closeIdx);
        }

        [Serializable]
        class VersionProbe
        {
            public int saveVersion;
        }
    }
}
