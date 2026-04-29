// iOS App Tracking Transparency (ATT) — iOS 14.5+ 필수.
// AdMob / AppLovin이 IDFA 사용 시 사전 권한 받기 위해 호출.
// Info.plist에 NSUserTrackingUsageDescription 키 필요 (BoxyBuilder PostProcess에서 자동 삽입).

#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>

extern "C" {

// 권한 요청 (비동기). callback으로 결과 전달.
// 결과 코드: 0=NotDetermined, 1=Restricted, 2=Denied, 3=Authorized
typedef void (*BoxyATTCallback)(int status);

void _BoxyRequestATT(BoxyATTCallback callback) {
    if (@available(iOS 14, *)) {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            if (callback != NULL) {
                callback((int)status);
            }
        }];
    } else {
        // iOS 14 미만은 ATT 다이얼로그 없음 — Authorized 간주.
        if (callback != NULL) callback(3);
    }
}

int _BoxyGetATTStatus() {
    if (@available(iOS 14, *)) {
        return (int)[ATTrackingManager trackingAuthorizationStatus];
    }
    return 3;  // Authorized
}

}
