using UnityEngine;
using UnityEngine.UIElements;
using Mound.Core;
using Mound.Core.Audio;
using Mound.Core.Diagnostics;
using Mound.Core.Events;
using Mound.Core.Haptic;
using Mound.Core.Save;
using Mound.Core.Scenes;
using Mound.Localization;
using Mound.Monetization;
using Mound.Analytics;
using Mound.UI;
using Boxy.App.Audio;
using Boxy.App.Save;

namespace Boxy.App
{
    // 앱 전역 단일 진입점 — 모든 Provider 인스턴스화 + Service Locator 역할.
    // DontDestroyOnLoad로 씬 전환 시에도 유지.
    // Phase 7+ 컨트롤러들이 사용하는 의존성 (광고/IAP/분석/저장/씬/i18n) 일관 제공.
    [DefaultExecutionOrder(-100)]
    public sealed class BoxyBootstrap : MonoBehaviour
    {
        [Tooltip("dev/Editor에서는 Stub Provider, Production은 실제 SDK 래퍼로 교체 (Week 3)")]
        [SerializeField] bool useStubProviders = true;

        public static BoxyBootstrap Instance { get; private set; }

        public ISaveSystem SaveSystem { get; private set; }
        public ISceneLoader SceneLoader { get; private set; }
        public IAdProvider AdProvider { get; private set; }
        public IIapProvider IapProvider { get; private set; }
        public IReceiptValidator ReceiptValidator { get; private set; }
        public IAnalyticsProvider Analytics { get; private set; }
        public IStringTable Strings { get; private set; }
        public IEventBus EventBus { get; private set; }
        public IConsentProvider Consent { get; private set; }
        public ICrashReporter CrashReporter { get; private set; }
        public IRemoteConfigProvider RemoteConfig { get; private set; }
        public IAssetProvider AssetProvider { get; private set; }
        public AdSsvClient AdSsv { get; private set; }    // null이면 SSV 비활성 (dev/Editor)

        public ProgressionService Progression { get; private set; }
        public ToastManager Toast { get; private set; }
        public IAudioService Audio { get; private set; }
        public GameAudioBindings AudioBindings { get; private set; }
        public IHapticService Haptic { get; private set; }

        [SerializeField] EnvironmentConfig environmentConfig;
        [SerializeField] UIDocument globalUIDocument;   // 토스트/팝업 호스트
        [SerializeField] AudioService audioService;     // bgmSource + sfxSource 자식 GameObject 컴포넌트
        [SerializeField] SfxLibrary sfxLibrary;          // SfxLibrary asset

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // CrashReporter 먼저 — 다른 Provider 초기화 실패 시 보고 가능하도록.
            CrashReporter = useStubProviders
                ? new NullCrashReporter()
                : new CompositeCrashReporter(new FirebaseCrashlyticsProvider());

            // CLAUDE.md §11-4 P0-2: 세이브 HMAC seed = SystemInfo.deviceUniqueIdentifier + 앱 상수.
            // 디바이스 변경 시 seed 변경되어 backup load → 기본값 — 의도된 동작 (다른 디바이스 변조 차단).
            string saveHmacSeed = $"boxy-mound-v1::{SystemInfo.deviceUniqueIdentifier}";
            SaveSystem = new PlayerPrefsSaveSystem(SaveMigrator.MigrateJsonIfNeeded, saveHmacSeed);
            SceneLoader = new SceneLoader();
            EventBus = new EventBus();
            Strings = new KoEnStringTable();   // Day 18 ~ Week 3: Unity Localization Package로 교체 가능
            // 저장된 로케일 복원 (정수가 Settings에서 한↔영 전환 후 재시작해도 유지)
            string savedLocale = PlayerPrefs.GetString(PrefsKey.LanguageCode, "");
            if (!string.IsNullOrEmpty(savedLocale)) Strings.SetLocale(savedLocale);

            // CLAUDE.md §11-4 P0-1: IAP 영수증 검증기 wire-up.
            // dev/Editor: NullReceiptValidator. Production: Supabase Edge Function (cloud) 우선 → 실패 시 클라 직검증 fallback.
            if (useStubProviders || environmentConfig == null)
            {
                ReceiptValidator = new NullReceiptValidator();
            }
            else if (!string.IsNullOrEmpty(environmentConfig.Supabase.Url)
                     && !string.IsNullOrEmpty(environmentConfig.Supabase.AnonKey))
            {
                // Cloud 우선 (transactions 테이블 UNIQUE 중복 차단 + 서버 직검증)
                ReceiptValidator = new CloudReceiptValidator(
                    environmentConfig.Supabase.ValidateIapUrl,
                    environmentConfig.Supabase.AnonKey);
            }
            else
            {
                // Cloud 미설정 시 클라 직검증으로 fallback (보안 약함, 경고)
                Debug.LogWarning("[BoxyBootstrap] Supabase 미설정 — IAP 클라 직검증 fallback. v1.0 출시 전 anon key 입력 필수.");
                ReceiptValidator = new CompositeReceiptValidator(
                    apple: new AppleReceiptValidator(environmentConfig.AppleReceipt),
                    google: new GoogleReceiptValidator(environmentConfig.GoogleReceipt));
            }

            // 광고 SSV — Supabase 설정된 경우만 활성. 미설정 시 null (HintAdController 등이 null 체크).
            AdSsv = (!useStubProviders && environmentConfig != null
                     && !string.IsNullOrEmpty(environmentConfig.Supabase.Url)
                     && !string.IsNullOrEmpty(environmentConfig.Supabase.AnonKey))
                ? new AdSsvClient(environmentConfig.Supabase.AdSsvUrl, environmentConfig.Supabase.AnonKey)
                : null;
            // CLAUDE.md §11-4 P0-5: Remote Config — Firebase 통합 후 FirebaseRemoteConfigProvider로 교체.
            RemoteConfig = useStubProviders ? new NullRemoteConfigProvider() : new FirebaseRemoteConfigProvider();
            // CLAUDE.md §20-2 (v1.1 백로그): Addressables 도입 시 AddressablesAssetProvider로 교체.
            AssetProvider = new ResourcesAssetProvider();

            if (useStubProviders || environmentConfig == null)
            {
                AdProvider = new NullAdProvider();
                Analytics = new NullAnalyticsProvider();
                Consent = new NullConsentProvider();
                IapProvider = new UnityIapProvider(ReceiptValidator);
            }
            else
            {
                AdProvider = new AppLovinAdProvider(environmentConfig);
                // Firebase + AppsFlyer 동시 발송 — ARPDAU(Firebase) + 출처 추적(AppsFlyer) 분리 도구.
                Analytics = new CompositeAnalyticsProvider(
                    new FirebaseAnalyticsProvider(),
                    new AppsFlyerAnalyticsProvider(environmentConfig.AppsFlyerDevKey)
                );
                // iOS는 ATT 우선, 그 외는 Google UMP (GDPR/CCPA).
#if UNITY_IOS && !UNITY_EDITOR
                Consent = new IosAttConsentProvider();
#else
                Consent = new GoogleUmpConsentProvider();
#endif
                IapProvider = new UnityIapProvider(ReceiptValidator);
            }

            // 광고 제거 IAP 보유 시 자동 제거 — boxy-plan §B-4 비소비성 IAP 영구 효과
            Progression = new ProgressionService(SaveSystem);
            // Remote Config fetch — 비동기 fire-and-forget. CLAUDE.md §11-2 명시적 _ =.
            _ = RemoteConfig.FetchAsync(System.Threading.CancellationToken.None);

            if (globalUIDocument != null)
            {
                Toast = new ToastManager(globalUIDocument.rootVisualElement);
            }

            // 오디오 — Day 13 UX 폴리싱. AudioService + SfxLibrary 인스펙터 미연결 시 silent.
            Audio = audioService;
            // 햅틱 — Android 단순 진동만, iOS는 Day 13 후반 native 플러그인 후 교체.
            Haptic = new StandardHapticService();
            if (audioService != null && sfxLibrary != null)
            {
                AudioBindings = new GameAudioBindings(audioService, sfxLibrary, EventBus, Haptic);
                AudioBindings.Bind();
            }

            // 저장된 볼륨/햅틱 설정을 적용 — 앱 재시작 후에도 마지막 값 유지.
            ApplySavedAudioSettings();

            // ATT 권한 요청 — iOS만, 첫 실행 시 1회. 첫 실행 후 잠깐 (1초) 딜레이로 사용자가 화면 본 뒤 표시.
            _ = RequestConsentDelayedAsync();
        }

        async System.Threading.Tasks.Task RequestConsentDelayedAsync()
        {
            if (Consent == null) return;
            await System.Threading.Tasks.Task.Delay(1000);
            try
            {
                await Consent.RequestAsync(System.Threading.CancellationToken.None);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BoxyBootstrap] Consent.RequestAsync 실패 — 광고는 비개인화로 폴백. {e.Message}");
                CrashReporter?.Report(e, new System.Collections.Generic.Dictionary<string, string> { { "stage", "consent_request" } });
            }
        }

        void ApplySavedAudioSettings()
        {
            float bgm = PlayerPrefs.GetFloat(PrefsKey.SettingsBgmVolume, 50f) / 100f;
            float sfx = PlayerPrefs.GetFloat(PrefsKey.SettingsSfxVolume, 80f) / 100f;
            bool hapticEnabled = PlayerPrefs.GetInt(PrefsKey.SettingsHapticEnabled, 1) == 1;
            Audio?.SetVolume(AudioCategory.Bgm, bgm);
            Audio?.SetVolume(AudioCategory.Sfx, sfx);
            Haptic?.SetEnabled(hapticEnabled);
        }

        void OnDestroy()
        {
            AudioBindings?.Unbind();
            if (Instance == this) Instance = null;
        }
    }
}
