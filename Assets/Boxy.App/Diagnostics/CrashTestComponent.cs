#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Boxy.App.Diagnostics
{
    // boxy-plan §C Day 21 출시 직전 검증 — Firebase Crashlytics 대시보드 도착 확인용.
    // CLAUDE.md §12-3: 출시 직전 강제 크래시 1회 → 대시보드 도착 확인.
    //
    // 사용법:
    //   - Editor: 메뉴 → "Boxy → Diagnostics → Trigger Test Crash"
    //   - 빌드 in dev/staging: 컴포넌트.TriggerNativeCrashAfterDelay() 호출
    //   - Production 빌드 출시 시 이 컴포넌트는 씬에서 제거 (또는 #if 분리)
    public sealed class CrashTestComponent : MonoBehaviour
    {
        public void TriggerNativeCrashAfterDelay(float seconds = 0.5f)
        {
            Debug.LogError($"[CrashTest] Triggering crash in {seconds}s — Firebase Crashlytics 검증");
            Invoke(nameof(ForceCrashInternal), seconds);
        }

        void ForceCrashInternal()
        {
            // 의도적 NullReferenceException — Firebase Crashlytics가 캡처해야 함
            throw new System.NullReferenceException(
                "[CrashTest] intentional crash — Firebase Crashlytics dashboard 도착 검증용");
        }

#if UNITY_EDITOR
        [MenuItem("Boxy/Diagnostics/Trigger Test Crash (NullRef)")]
        public static void TriggerCrashFromMenu()
        {
            Debug.LogError("[CrashTest] Editor menu — Crashlytics 검증용 즉시 크래시");
            throw new System.NullReferenceException(
                "[CrashTest] menu trigger — Crashlytics dashboard 도착 검증용");
        }
#endif
    }
}
