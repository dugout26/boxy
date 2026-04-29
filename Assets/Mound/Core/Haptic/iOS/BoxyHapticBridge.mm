// iOS native bridge — UIImpactFeedbackGenerator (iOS 10+).
// Unity C#에서 DllImport "__Internal"로 호출.
//
// 빌드 시 Unity가 자동으로 Xcode 프로젝트에 포함 (UnityEditor.iOS.Xcode.PBXProject).
// 별도 import 설정 X — .mm 확장자만으로 iOS 빌드 시 자동 컴파일.

#import <UIKit/UIKit.h>

extern "C" {

// intensity: 0=Light, 1=Medium, 2=Heavy
void _BoxyHapticImpact(int intensity) {
    if (@available(iOS 10.0, *)) {
        UIImpactFeedbackStyle style;
        switch (intensity) {
            case 0: style = UIImpactFeedbackStyleLight; break;
            case 1: style = UIImpactFeedbackStyleMedium; break;
            case 2: style = UIImpactFeedbackStyleHeavy; break;
            default: style = UIImpactFeedbackStyleMedium;
        }
        UIImpactFeedbackGenerator* generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
        [generator prepare];
        [generator impactOccurred];
    }
    // iOS 10 미만 — 무진동 (silent fail)
}

}
