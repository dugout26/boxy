using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mound.Core.Diagnostics
{
    // CLAUDE.md §11-4 보안 / 안정성: 크래시 텔레메트리.
    // catch한 모든 비치명 예외 → crashReporter.Report(e, contextDict) 호출.
    // SDK 통합 가이드: decisions/2026-04-29-17-firebase-ios-setup-guide.md
    public interface ICrashReporter
    {
        void Report(Exception e, IReadOnlyDictionary<string, string> context = null);
        void Log(string message, LogSeverity severity = LogSeverity.Info);
        void SetUserId(string userId);
        void SetCustomKey(string key, string value);
    }

    public enum LogSeverity { Debug, Info, Warning, Error }

    // dev/Editor 스텁. Unity Console에 출력만, 외부 전송 X.
    public sealed class NullCrashReporter : ICrashReporter
    {
        public void Report(Exception e, IReadOnlyDictionary<string, string> context = null)
        {
            int n = context?.Count ?? 0;
            Debug.LogException(e);
            Debug.Log($"[CrashReporter:Null] non-fatal reported ({n} context keys)");
        }

        public void Log(string message, LogSeverity severity = LogSeverity.Info)
        {
            Debug.Log($"[CrashReporter:{severity}] {message}");
        }

        public void SetUserId(string userId) { }
        public void SetCustomKey(string key, string value) { }
    }

    // 여러 reporter에 동시 발송 — Firebase Crashlytics + Sentry 등 병행 가능.
    public sealed class CompositeCrashReporter : ICrashReporter
    {
        readonly ICrashReporter[] reporters;

        public CompositeCrashReporter(params ICrashReporter[] reporters)
        {
            this.reporters = reporters ?? new ICrashReporter[0];
        }

        public void Report(Exception e, IReadOnlyDictionary<string, string> context = null)
        {
            foreach (var r in reporters) r?.Report(e, context);
        }

        public void Log(string message, LogSeverity severity = LogSeverity.Info)
        {
            foreach (var r in reporters) r?.Log(message, severity);
        }

        public void SetUserId(string userId)
        {
            foreach (var r in reporters) r?.SetUserId(userId);
        }

        public void SetCustomKey(string key, string value)
        {
            foreach (var r in reporters) r?.SetCustomKey(key, value);
        }
    }

    // Firebase Crashlytics 래퍼.
    // SDK 통합 시 Player Settings → Scripting Define Symbols 에 "FIREBASE_CRASHLYTICS" 추가하면 실 호출 활성.
    // 미통합 시 Console 로그만.
    public sealed class FirebaseCrashlyticsProvider : ICrashReporter
    {
        public FirebaseCrashlyticsProvider()
        {
#if FIREBASE_CRASHLYTICS
            // Firebase 자동 초기화 (FirebaseApp.CheckAndFixDependenciesAsync 후) — 별도 셋업 X
            Debug.Log("[FirebaseCrashlyticsProvider] active");
#else
            Debug.LogWarning("[FirebaseCrashlyticsProvider] SDK 미통합 — Console 로그만. Define FIREBASE_CRASHLYTICS to activate.");
#endif
        }

        public void Report(Exception e, IReadOnlyDictionary<string, string> context = null)
        {
#if FIREBASE_CRASHLYTICS
            if (context != null)
            {
                foreach (var kv in context)
                    Firebase.Crashlytics.Crashlytics.SetCustomKey(kv.Key, kv.Value ?? "");
            }
            Firebase.Crashlytics.Crashlytics.LogException(e);
#else
            int n = context?.Count ?? 0;
            Debug.LogException(e);
            Debug.Log($"[Crashlytics] non-fatal reported ({n} context keys)");
#endif
        }

        public void Log(string message, LogSeverity severity = LogSeverity.Info)
        {
#if FIREBASE_CRASHLYTICS
            Firebase.Crashlytics.Crashlytics.Log($"[{severity}] {message}");
#else
            Debug.Log($"[Crashlytics:{severity}] {message}");
#endif
        }

        public void SetUserId(string userId)
        {
#if FIREBASE_CRASHLYTICS
            Firebase.Crashlytics.Crashlytics.SetUserId(userId ?? "");
#else
            Debug.Log($"[Crashlytics] SetUserId={userId}");
#endif
        }

        public void SetCustomKey(string key, string value)
        {
#if FIREBASE_CRASHLYTICS
            Firebase.Crashlytics.Crashlytics.SetCustomKey(key, value ?? "");
#else
            Debug.Log($"[Crashlytics] {key}={value}");
#endif
        }
    }
}
