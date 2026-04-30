using UnityEngine;

namespace Mound.UI
{
    public enum HapticType
    {
        Light,      // 일반 탭, 드래그
        Medium,     // 성공 / 회전
        Heavy,      // 클리어 / 별 3개 / 주요 보상
        Success,    // 명시적 성공
        Failure     // 명시적 실패
    }

    // mound-design-system v2 §6 햅틱 매핑.
    // SDK 통합 가이드: decisions/2026-04-29-17-firebase-ios-setup-guide.md (또는 별도 SDK 가이드 문서).
    // Lofelt Nice Vibrations 임포트 후 Player Settings → Scripting Define Symbols 에 "LOFELT_NICE_VIBRATIONS" 추가.
    public static class HapticService
    {
        public static bool Enabled { get; set; } = true;

        public static void Play(HapticType type)
        {
            if (!Enabled) return;

#if LOFELT_NICE_VIBRATIONS
            Lofelt.NiceVibrations.HapticController.Play(MapToLofelt(type));
#else
            Debug.Log($"[HapticService] {type} (SDK 미통합)");
#endif
        }

#if LOFELT_NICE_VIBRATIONS
        static Lofelt.NiceVibrations.HapticPatterns.PresetType MapToLofelt(HapticType type) => type switch
        {
            HapticType.Light   => Lofelt.NiceVibrations.HapticPatterns.PresetType.LightImpact,
            HapticType.Medium  => Lofelt.NiceVibrations.HapticPatterns.PresetType.MediumImpact,
            HapticType.Heavy   => Lofelt.NiceVibrations.HapticPatterns.PresetType.HeavyImpact,
            HapticType.Success => Lofelt.NiceVibrations.HapticPatterns.PresetType.Success,
            HapticType.Failure => Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure,
            _ => Lofelt.NiceVibrations.HapticPatterns.PresetType.Selection
        };
#endif
    }
}
