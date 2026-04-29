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
    // SDK 통합: Week 2 Day 13 — Asset Store에서 Lofelt Nice Vibrations 임포트 후
    //   Player Settings → Scripting Define Symbols 에 "LOFELT_NICE_VIBRATIONS" 추가.
    //
    // SDK 미통합 시: Debug.Log만, 진동 없음. 출시 빌드는 SDK 통합 필수.
    public static class HapticService
    {
        public static bool Enabled { get; set; } = true;

        public static void Play(HapticType type)
        {
            if (!Enabled) return;

#if LOFELT_NICE_VIBRATIONS
            // TODO Week 2 Day 13: Lofelt 임포트 후 활성
            // Lofelt.NiceVibrations.HapticController.Play(MapToLofelt(type));
#else
            Debug.Log($"[HapticService] {type} (SDK 미통합 — Week 2 Day 13 통합 후 활성)");
#endif
        }

#if LOFELT_NICE_VIBRATIONS
        // TODO Week 2 Day 13:
        // static Lofelt.NiceVibrations.HapticPatterns.PresetType MapToLofelt(HapticType type) => type switch
        // {
        //     HapticType.Light   => HapticPatterns.PresetType.LightImpact,
        //     HapticType.Medium  => HapticPatterns.PresetType.MediumImpact,
        //     HapticType.Heavy   => HapticPatterns.PresetType.HeavyImpact,
        //     HapticType.Success => HapticPatterns.PresetType.Success,
        //     HapticType.Failure => HapticPatterns.PresetType.Failure,
        //     _ => HapticPatterns.PresetType.Selection
        // };
#endif
    }
}
