using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Core
{
    // CLAUDE.md §11-4 P0-5: Remote Config / Feature Flag.
    // 광고 빈도 / 보상량 / 난이도 곡선 / 일일 보상 등 코드에 박지 X.
    // SDK 통합 가이드: decisions/2026-04-29-17-firebase-ios-setup-guide.md (Firebase Remote Config 동시 활성)
    public interface IRemoteConfigProvider
    {
        Task FetchAsync(CancellationToken ct);
        bool   GetBool(string key, bool defaultValue = false);
        int    GetInt(string key, int defaultValue = 0);
        long   GetLong(string key, long defaultValue = 0);
        float  GetFloat(string key, float defaultValue = 0);
        string GetString(string key, string defaultValue = "");
    }

    // dev/Editor 스텁 — 모든 키에 default 반환. Production: FirebaseRemoteConfigProvider 교체.
    public sealed class NullRemoteConfigProvider : IRemoteConfigProvider
    {
        public Task FetchAsync(CancellationToken ct) => Task.CompletedTask;
        public bool   GetBool(string key, bool defaultValue = false) => defaultValue;
        public int    GetInt(string key, int defaultValue = 0) => defaultValue;
        public long   GetLong(string key, long defaultValue = 0) => defaultValue;
        public float  GetFloat(string key, float defaultValue = 0) => defaultValue;
        public string GetString(string key, string defaultValue = "") => defaultValue;
    }

    // Firebase Remote Config 래퍼.
    // SDK 통합 시 Scripting Define Symbol "FIREBASE_REMOTECONFIG" 추가하면 실 호출 활성.
    public sealed class FirebaseRemoteConfigProvider : IRemoteConfigProvider
    {
        public FirebaseRemoteConfigProvider()
        {
#if FIREBASE_REMOTECONFIG
            Debug.Log("[FirebaseRemoteConfigProvider] active");
#else
            Debug.LogWarning("[FirebaseRemoteConfigProvider] SDK 미통합 — 모든 키 default 반환. Define FIREBASE_REMOTECONFIG to activate.");
#endif
        }

        public async Task FetchAsync(CancellationToken ct)
        {
#if FIREBASE_REMOTECONFIG
            try
            {
                await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FirebaseRemoteConfigProvider] fetch 실패 — default 사용: {e.Message}");
            }
#else
            await Task.CompletedTask;
#endif
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
#if FIREBASE_REMOTECONFIG
            var v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            return v.Source == Firebase.RemoteConfig.ValueSource.StaticValue ? defaultValue : v.BooleanValue;
#else
            return defaultValue;
#endif
        }

        public int GetInt(string key, int defaultValue = 0)
        {
#if FIREBASE_REMOTECONFIG
            var v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            return v.Source == Firebase.RemoteConfig.ValueSource.StaticValue ? defaultValue : (int)v.LongValue;
#else
            return defaultValue;
#endif
        }

        public long GetLong(string key, long defaultValue = 0)
        {
#if FIREBASE_REMOTECONFIG
            var v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            return v.Source == Firebase.RemoteConfig.ValueSource.StaticValue ? defaultValue : v.LongValue;
#else
            return defaultValue;
#endif
        }

        public float GetFloat(string key, float defaultValue = 0)
        {
#if FIREBASE_REMOTECONFIG
            var v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            return v.Source == Firebase.RemoteConfig.ValueSource.StaticValue ? defaultValue : (float)v.DoubleValue;
#else
            return defaultValue;
#endif
        }

        public string GetString(string key, string defaultValue = "")
        {
#if FIREBASE_REMOTECONFIG
            var v = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            return v.Source == Firebase.RemoteConfig.ValueSource.StaticValue ? defaultValue : v.StringValue;
#else
            return defaultValue;
#endif
        }
    }

    // Boxy 게임 핵심 Remote Config 키 — string 매직 방지 위해 정적 클래스로 관리.
    // 출시 후 Firebase 콘솔에서 이 키 값 변경 → 앱 심사 없이 밸런스 조정.
    public static class RemoteConfigKey
    {
        public const string InterstitialIntervalSec = "interstitial_interval_sec";  // 기본 90
        public const string HintsPerAd = "hints_per_ad";                            // 기본 1
        public const string UndosPerAd = "undos_per_ad";                            // 기본 1
        public const string DailyRewardCoins = "daily_reward_coins";                // 기본 50
        public const string TutorialEndLevel = "tutorial_end_level";                // 기본 5
        public const string EnableHardCorePack = "enable_hardcore_pack";            // 실험 플래그
        public const string EnableNewAdTiming = "enable_new_ad_timing";             // A/B 실험
    }
}
