using System.Runtime.InteropServices;
using UnityEngine;

namespace Mound.Core.Haptic
{
    public enum HapticIntensity
    {
        Light,    // 가벼운 탭, 토글, 항목 선택
        Medium,   // 아이템 정확 배치, 일반 액션 확정
        Heavy     // 레벨 클리어, 큰 보상
    }

    public interface IHapticService
    {
        void Trigger(HapticIntensity intensity);
        void SetEnabled(bool enabled);
    }

    public sealed class NullHapticService : IHapticService
    {
        public void Trigger(HapticIntensity intensity) { }
        public void SetEnabled(bool enabled) { }
    }

    // 기본 구현 — Android Handheld.Vibrate + iOS UIImpactFeedbackGenerator (네이티브 .mm).
    // §11-3 플랫폼 분기. Editor에선 silent.
    public sealed class StandardHapticService : IHapticService
    {
        bool enabled = true;

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void _BoxyHapticImpact(int intensity);
#endif

        public void SetEnabled(bool value) => enabled = value;

        public void Trigger(HapticIntensity intensity)
        {
            if (!enabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            // iOS — UIImpactFeedbackGenerator. Light/Medium/Heavy 매핑.
            _BoxyHapticImpact((int)intensity);
#elif UNITY_ANDROID && !UNITY_EDITOR
            // Android — 단순 진동 (Handheld.Vibrate는 intensity 무시). v1.1에서 Vibrator API로 세밀 제어.
            Handheld.Vibrate();
#endif
            // Editor — silent.
        }
    }
}
