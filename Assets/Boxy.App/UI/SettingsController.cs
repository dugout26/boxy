using UnityEngine;
using UnityEngine.UIElements;
using Mound.Core.Audio;
using Mound.Core.Scenes;
using Boxy.App.Save;

namespace Boxy.App.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class SettingsController : MonoBehaviour
    {
        UIDocument uiDocument;
        ISceneLoader sceneLoader;

        Button backButton;
        Slider bgmSlider;
        Slider sfxSlider;
        Toggle hapticToggle;
        Button resetButton;
        Button privacyButton;
        Button termsButton;
        Button contactButton;
        Button restoreButton;
        Button languageButton;
        Label languageValue;

        const float DefaultBgm = 50f;
        const float DefaultSfx = 80f;
        const string PrivacyUrl = "https://mound.example.com/privacy";  // v1.0 출시 전 정수가 실 URL로 교체
        const string TermsUrl = "https://mound.example.com/terms";
        const string ContactEmailUrl = "mailto:dugout26.gm@gmail.com";

        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            sceneLoader = new SceneLoader();

            var root = uiDocument.rootVisualElement;
            backButton = root.Q<Button>("back-button");
            bgmSlider = root.Q<Slider>("bgm-slider");
            sfxSlider = root.Q<Slider>("sfx-slider");
            hapticToggle = root.Q<Toggle>("haptic-toggle");
            resetButton = root.Q<Button>("reset-button");
            privacyButton = root.Q<Button>("privacy-button");
            termsButton = root.Q<Button>("terms-button");
            contactButton = root.Q<Button>("contact-button");
            restoreButton = root.Q<Button>("restore-button");
            languageButton = root.Q<Button>("language-button");
            languageValue = root.Q<Label>("language-value");
            UpdateLanguageLabel();
            UpdateVersionLabel();
            ApplyLocalization();
        }

        void ApplyLocalization()
        {
            var strings = BoxyBootstrap.Instance?.Strings;
            if (strings == null) return;
            // header title (Settings) — UXML 첫 Label
            var root = uiDocument.rootVisualElement;
            // Settings 섹션 라벨들은 UXML에 직접 있어 Q<Label> 못 함 — 텍스트 매칭으로 swap
            // 간단히 핵심 버튼만 i18n: 뒤로가기는 ← 유지, reset/privacy/terms/contact/restore 텍스트만
            var resetBtn = root.Q<Button>("reset-button");
            var privacyBtn = root.Q<Button>("privacy-button");
            var termsBtn = root.Q<Button>("terms-button");
            var contactBtn = root.Q<Button>("contact-button");
            var restoreBtn = root.Q<Button>("restore-button");
            if (strings.CurrentLocale == "en-US")
            {
                if (resetBtn != null) resetBtn.text = "Reset";
                if (privacyBtn != null) privacyBtn.text = "Privacy Policy ›";
                if (termsBtn != null) termsBtn.text = "Terms of Service ›";
                if (contactBtn != null) contactBtn.text = "Contact ›";
                if (restoreBtn != null) restoreBtn.text = "Restore";
            }

            LoadSettings();
            BindHandlers();
        }

        void OnDestroy()
        {
            UnbindHandlers();
        }

        void LoadSettings()
        {
            if (bgmSlider != null) bgmSlider.value = PlayerPrefs.GetFloat(PrefsKey.SettingsBgmVolume, DefaultBgm);
            if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat(PrefsKey.SettingsSfxVolume, DefaultSfx);
            if (hapticToggle != null) hapticToggle.value = PlayerPrefs.GetInt(PrefsKey.SettingsHapticEnabled, 1) == 1;
        }

        void BindHandlers()
        {
            if (bgmSlider != null) bgmSlider.RegisterValueChangedCallback(OnBgmChanged);
            if (sfxSlider != null) sfxSlider.RegisterValueChangedCallback(OnSfxChanged);
            if (hapticToggle != null) hapticToggle.RegisterValueChangedCallback(OnHapticChanged);
            if (backButton != null) backButton.clicked += OnBack;
            if (resetButton != null) resetButton.clicked += OnResetProgress;
            if (privacyButton != null) privacyButton.clicked += OnPrivacy;
            if (termsButton != null) termsButton.clicked += OnTerms;
            if (contactButton != null) contactButton.clicked += OnContact;
            if (restoreButton != null) restoreButton.clicked += OnRestore;
            if (languageButton != null) languageButton.clicked += OnLanguageToggle;
        }

        void UnbindHandlers()
        {
            if (bgmSlider != null) bgmSlider.UnregisterValueChangedCallback(OnBgmChanged);
            if (sfxSlider != null) sfxSlider.UnregisterValueChangedCallback(OnSfxChanged);
            if (hapticToggle != null) hapticToggle.UnregisterValueChangedCallback(OnHapticChanged);
            if (backButton != null) backButton.clicked -= OnBack;
            if (resetButton != null) resetButton.clicked -= OnResetProgress;
            if (privacyButton != null) privacyButton.clicked -= OnPrivacy;
            if (termsButton != null) termsButton.clicked -= OnTerms;
            if (contactButton != null) contactButton.clicked -= OnContact;
            if (restoreButton != null) restoreButton.clicked -= OnRestore;
            if (languageButton != null) languageButton.clicked -= OnLanguageToggle;
        }

        void OnLanguageToggle()
        {
            var strings = BoxyBootstrap.Instance?.Strings;
            if (strings == null) return;
            string newLocale = strings.CurrentLocale == "ko-KR" ? "en-US" : "ko-KR";
            strings.SetLocale(newLocale);
            PlayerPrefs.SetString(PrefsKey.LanguageCode, newLocale);
            PlayerPrefs.Save();
            UpdateLanguageLabel();
        }

        void UpdateLanguageLabel()
        {
            if (languageValue == null) return;
            var locale = BoxyBootstrap.Instance?.Strings?.CurrentLocale ?? "ko-KR";
            languageValue.text = locale == "ko-KR" ? "한국어" : "English";
        }

        // 버전 표시 — UXML 하드코딩 "1.0.0" 대신 PlayerSettings.bundleVersion 동적 반영.
        void UpdateVersionLabel()
        {
            var versionLbl = uiDocument.rootVisualElement.Q<Label>("version-value");
            if (versionLbl != null) versionLbl.text = Application.version;
        }

        void OnBgmChanged(ChangeEvent<float> e)
        {
            PlayerPrefs.SetFloat(PrefsKey.SettingsBgmVolume, e.newValue);
            PlayerPrefs.Save();
            // 실시간 적용 — AudioService에 즉시 push (Bootstrap이 살아 있어야 함, 없으면 silent)
            BoxyBootstrap.Instance?.Audio?.SetVolume(AudioCategory.Bgm, e.newValue / 100f);
        }

        void OnSfxChanged(ChangeEvent<float> e)
        {
            PlayerPrefs.SetFloat(PrefsKey.SettingsSfxVolume, e.newValue);
            PlayerPrefs.Save();
            BoxyBootstrap.Instance?.Audio?.SetVolume(AudioCategory.Sfx, e.newValue / 100f);
        }

        void OnHapticChanged(ChangeEvent<bool> e)
        {
            PlayerPrefs.SetInt(PrefsKey.SettingsHapticEnabled, e.newValue ? 1 : 0);
            PlayerPrefs.Save();
            BoxyBootstrap.Instance?.Haptic?.SetEnabled(e.newValue);
        }

        void OnResetProgress()
        {
            // 확인 다이얼로그 — 즉시 삭제 X. 사용자 실수 방지.
            ShowConfirmDialog(
                title: "진행도 초기화",
                body: "정말 모든 진행도를 초기화하시겠어요? 별 ★, 잠금 해제 레벨이 모두 사라집니다.",
                confirmText: "초기화",
                cancelText: "취소",
                onConfirm: () =>
                {
                    PlayerPrefs.DeleteKey(PrefsKey.Progression);
                    PlayerPrefs.Save();
                    Debug.Log("[Settings] 진행도 초기화 완료");
                });
        }

        // 간단한 모달 확인 다이얼로그 — UIDocument의 root에 overlay 추가.
        void ShowConfirmDialog(string title, string body, string confirmText, string cancelText,
            System.Action onConfirm)
        {
            var root = uiDocument.rootVisualElement;
            var overlay = new VisualElement { name = "confirm-overlay" };
            overlay.AddToClassList("popup-overlay");
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0; overlay.style.top = 0;
            overlay.style.right = 0; overlay.style.bottom = 0;

            var card = new VisualElement { name = "confirm-card" };
            card.AddToClassList("popup-card");
            card.style.width = new Length(86f, LengthUnit.Percent);
            card.style.maxWidth = 440f;
            card.style.alignItems = Align.Center;
            card.style.paddingTop = 32f;
            card.style.paddingBottom = 32f;
            card.style.paddingLeft = 24f;
            card.style.paddingRight = 24f;

            var titleLbl = new Label(title);
            titleLbl.AddToClassList("popup-title");
            titleLbl.style.marginBottom = 12f;
            card.Add(titleLbl);

            var bodyLbl = new Label(body);
            bodyLbl.AddToClassList("popup-body");
            bodyLbl.style.whiteSpace = WhiteSpace.Normal;
            bodyLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            bodyLbl.style.marginBottom = 24f;
            card.Add(bodyLbl);

            var btnRow = new VisualElement();
            btnRow.style.flexDirection = FlexDirection.Row;
            btnRow.style.justifyContent = Justify.Center;
            btnRow.style.alignSelf = Align.Stretch;

            var cancelBtn = new Button { text = cancelText };
            cancelBtn.AddToClassList("btn-secondary");
            cancelBtn.style.flexGrow = 1; cancelBtn.style.height = 48; cancelBtn.style.marginRight = 8;
            cancelBtn.clicked += () => overlay.RemoveFromHierarchy();
            btnRow.Add(cancelBtn);

            var confirmBtn = new Button { text = confirmText };
            confirmBtn.AddToClassList("btn-primary");
            confirmBtn.style.flexGrow = 1; confirmBtn.style.height = 48; confirmBtn.style.marginLeft = 8;
            // confirm은 빨강 톤으로 destructive 강조
            confirmBtn.style.backgroundColor = new Color(0.86f, 0.15f, 0.15f, 1f);
            confirmBtn.style.color = Color.white;
            confirmBtn.clicked += () => { overlay.RemoveFromHierarchy(); onConfirm?.Invoke(); };
            btnRow.Add(confirmBtn);

            card.Add(btnRow);
            overlay.Add(card);
            root.Add(overlay);
        }

        void OnPrivacy() => Application.OpenURL(PrivacyUrl);
        void OnTerms() => Application.OpenURL(TermsUrl);
        void OnContact() => Application.OpenURL(ContactEmailUrl);

        // v1.0: IAP 미통합 stub. Week 3 UnityIapProvider 통합 후 실제 호출.
        void OnRestore() => Debug.Log("[Settings] Restore purchases (v1.0 stub — Week 3 IAP 통합)");

        void OnBack() => sceneLoader.LoadScene(BoxySceneNames.MainMenu);
    }
}
