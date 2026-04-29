using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Monetization
{
    // iOS 14.5+ App Tracking Transparency (ATT) 권한 요청 — IDFA 사용 동의.
    // BoxyATTBridge.mm을 통해 ATTrackingManager 호출.
    // §11-3 플랫폼 분기 — Android는 NullConsentProvider 사용.
    //
    // Info.plist에 NSUserTrackingUsageDescription 키 필수 (BoxyBuilder PostProcess에서 자동 삽입).
    public sealed class IosAttConsentProvider : IConsentProvider
    {
        public ConsentStatus Status { get; private set; } = ConsentStatus.Unknown;

#if UNITY_IOS && !UNITY_EDITOR
        delegate void AttCallback(int status);

        [DllImport("__Internal")]
        static extern void _BoxyRequestATT(AttCallback callback);

        [DllImport("__Internal")]
        static extern int _BoxyGetATTStatus();

        static TaskCompletionSource<int> pending;

        [AOT.MonoPInvokeCallback(typeof(AttCallback))]
        static void OnAttResult(int status)
        {
            pending?.TrySetResult(status);
            pending = null;
        }
#endif

        public async Task<ConsentStatus> RequestAsync(CancellationToken ct)
        {
#if UNITY_IOS && !UNITY_EDITOR
            int currentRaw = _BoxyGetATTStatus();
            // 0=NotDetermined, 1=Restricted, 2=Denied, 3=Authorized
            if (currentRaw != 0)
            {
                Status = MapStatus(currentRaw);
                return Status;
            }

            pending = new TaskCompletionSource<int>();
            ct.Register(() => pending?.TrySetCanceled());
            _BoxyRequestATT(OnAttResult);
            int result = await pending.Task;
            Status = MapStatus(result);
            return Status;
#else
            // Android / Editor — ATT 불필요.
            await Task.Yield();
            Status = ConsentStatus.NotRequired;
            return Status;
#endif
        }

        public bool CanShowAds() => Status != ConsentStatus.Denied;
        public bool CanShowPersonalizedAds() => Status == ConsentStatus.Obtained;

        static ConsentStatus MapStatus(int raw) => raw switch
        {
            0 => ConsentStatus.Required,        // NotDetermined
            1 => ConsentStatus.Denied,          // Restricted (parental control)
            2 => ConsentStatus.Denied,          // Denied
            3 => ConsentStatus.Obtained,        // Authorized
            _ => ConsentStatus.Unknown
        };
    }
}
