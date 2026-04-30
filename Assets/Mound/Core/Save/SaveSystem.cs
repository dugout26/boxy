using System;
using System.Security.Cryptography;
using System.Text;
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

    // CLAUDE.md §11-4 보안: 세이브 변조 방지를 위한 HMAC-SHA256 + checksum.
    // PlayerPrefs에 두 개 키를 저장: <key> = JSON, <key>__sig = HMAC. 변조 감지 시 backup 로드.
    // 클라 단독 저장이라 100% 보안 X (디바이스 키 추출하면 위조 가능). 캐주얼 사용자 99% 차단이 목표.
    public sealed class PlayerPrefsSaveSystem : ISaveSystem
    {
        readonly Func<string, string> jsonMigrator;
        readonly byte[] hmacKey;

        const string SignatureSuffix = "__sig";
        const string BackupSuffix = "__backup";

        // jsonMigrator: 디스크에서 읽은 raw JSON을 deserialize 직전 변환. CLAUDE.md §7-2 마이그레이션 동반 규칙.
        // hmacSeed: 디바이스별 시크릿 (SystemInfo.deviceUniqueIdentifier + 앱 상수). null 시 변조 감지 비활성.
        public PlayerPrefsSaveSystem(Func<string, string> jsonMigrator = null, string hmacSeed = null)
        {
            this.jsonMigrator = jsonMigrator;
            if (string.IsNullOrEmpty(hmacSeed))
            {
                this.hmacKey = null;
            }
            else
            {
                using var sha = SHA256.Create();
                this.hmacKey = sha.ComputeHash(Encoding.UTF8.GetBytes(hmacSeed));
            }
        }

        public void Save<T>(string key, T data) where T : SaveData
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key empty", nameof(key));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var json = JsonUtility.ToJson(data);

            // 변조 방지: 새 저장 전 기존 값을 backup으로 보관 (성공한 직전 세이브로 fallback 가능)
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetString(key + BackupSuffix, PlayerPrefs.GetString(key));
                if (PlayerPrefs.HasKey(key + SignatureSuffix))
                    PlayerPrefs.SetString(key + BackupSuffix + SignatureSuffix, PlayerPrefs.GetString(key + SignatureSuffix));
            }

            PlayerPrefs.SetString(key, json);
            if (hmacKey != null)
                PlayerPrefs.SetString(key + SignatureSuffix, ComputeSignature(json));
            PlayerPrefs.Save();
        }

        public bool TryLoad<T>(string key, out T data) where T : SaveData, new()
        {
            data = null;
            if (string.IsNullOrEmpty(key)) return false;
            if (!PlayerPrefs.HasKey(key)) return false;

            var json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json)) return false;

            // 변조 검증
            if (hmacKey != null && PlayerPrefs.HasKey(key + SignatureSuffix))
            {
                var expected = PlayerPrefs.GetString(key + SignatureSuffix);
                var actual = ComputeSignature(json);
                if (!FixedTimeEquals(expected, actual))
                {
                    Debug.LogError($"[SaveSystem] HMAC mismatch for '{key}' — 변조 감지. backup 시도.");
                    if (TryLoadBackup(key, out data)) return true;
                    return false;
                }
            }

            try
            {
                if (jsonMigrator != null) json = jsonMigrator(json);
                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Load failed for '{key}': {e.Message}. backup 시도.");
                return TryLoadBackup(key, out data);
            }
        }

        bool TryLoadBackup<T>(string key, out T data) where T : SaveData, new()
        {
            data = null;
            string backupKey = key + BackupSuffix;
            if (!PlayerPrefs.HasKey(backupKey)) return false;

            var json = PlayerPrefs.GetString(backupKey);
            if (string.IsNullOrEmpty(json)) return false;

            try
            {
                if (jsonMigrator != null) json = jsonMigrator(json);
                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] backup load also failed: {e.Message}");
                data = null;
                return false;
            }
        }

        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.DeleteKey(key + SignatureSuffix);
            PlayerPrefs.DeleteKey(key + BackupSuffix);
            PlayerPrefs.DeleteKey(key + BackupSuffix + SignatureSuffix);
            PlayerPrefs.Save();
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        string ComputeSignature(string payload)
        {
            using var hmac = new HMACSHA256(hmacKey);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }

        // .NET Standard 2.1 fallback (CryptographicOperations.FixedTimeEquals는 .NET 5+).
        // 길이 다르면 false. 같으면 모든 바이트 XOR한 결과로 판정 (timing attack 방어).
        static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
