using System;
using UnityEngine;

namespace Mound.Core.Save
{
    public interface ISaveSystem
    {
        void Save<T>(string key, T data) where T : SaveData;
        bool TryLoad<T>(string key, out T data) where T : SaveData, new();
        void Delete(string key);
        void DeleteAll();
    }

    // JsonUtility는 property를 직렬화하지 못하고 public 필드만 처리하므로 인터페이스 대신 abstract 클래스 + 필드로 강제
    [Serializable]
    public abstract class SaveData
    {
        // CLAUDE.md §7-1: 스키마 변경 시 saveVersion 증가 + 마이그레이션 코드 동반
        public int saveVersion;
    }

    public sealed class PlayerPrefsSaveSystem : ISaveSystem
    {
        readonly Func<string, string> jsonMigrator;

        // jsonMigrator: 디스크에서 읽은 raw JSON을 deserialize 직전 변환. CLAUDE.md §7-2 마이그레이션 동반 규칙.
        // null 허용 (테스트 / 마이그레이션 불필요한 경우)
        public PlayerPrefsSaveSystem(Func<string, string> jsonMigrator = null)
        {
            this.jsonMigrator = jsonMigrator;
        }

        public void Save<T>(string key, T data) where T : SaveData
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key empty", nameof(key));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public bool TryLoad<T>(string key, out T data) where T : SaveData, new()
        {
            data = null;
            if (string.IsNullOrEmpty(key)) return false;
            if (!PlayerPrefs.HasKey(key)) return false;

            var json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json)) return false;

            // 손상된 세이브가 게임 실행을 막지 않도록 — 실패 시 false 반환, 호출자가 기본값 복구 (CLAUDE.md §7-1)
            try
            {
                if (jsonMigrator != null) json = jsonMigrator(json);
                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Load failed for '{key}': {e.Message}");
                data = null;
                return false;
            }
        }

        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
