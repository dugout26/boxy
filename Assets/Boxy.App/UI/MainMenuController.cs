using UnityEngine;
using UnityEngine.UIElements;
using Mound.Core.Scenes;
using Mound.Localization;

namespace Boxy.App.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class MainMenuController : MonoBehaviour
    {
        UIDocument uiDocument;
        ISceneLoader sceneLoader;
        Button playButton;
        Button settingsButton;
        Button soundButton;
        Button shopButton;
        VisualElement boxyMascot;
        IVisualElementScheduledItem bobAnimation;

        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            sceneLoader = new SceneLoader();

            var root = uiDocument.rootVisualElement;
            playButton = root.Q<Button>("play-button");
            settingsButton = root.Q<Button>("settings-button");
            soundButton = root.Q<Button>("sound-button");
            shopButton = root.Q<Button>("shop-button");
            boxyMascot = root.Q<VisualElement>("boxy-mascot");

            if (playButton != null) playButton.clicked += OnPlay;
            if (settingsButton != null) settingsButton.clicked += OnSettings;
            if (soundButton != null) soundButton.clicked += OnSound;
            if (shopButton != null) shopButton.clicked += OnShop;

            ApplyLocalization();
            StartIdleBob();
        }

        void ApplyLocalization()
        {
            var strings = BoxyBootstrap.Instance?.Strings;
            if (strings == null) return;
            var subtitle = uiDocument.rootVisualElement.Q<Label>("subtitle");
            if (subtitle != null) subtitle.text = strings.Get(StringKey.AppSubtitle);
            if (playButton != null) playButton.text = strings.Get(StringKey.MenuPlay);
        }

        void OnDestroy()
        {
            bobAnimation?.Pause();
            if (playButton != null) playButton.clicked -= OnPlay;
            if (settingsButton != null) settingsButton.clicked -= OnSettings;
            if (soundButton != null) soundButton.clicked -= OnSound;
            if (shopButton != null) shopButton.clicked -= OnShop;
        }

        // 마스코트 idle bob — 위아래로 부드럽게 +-8px 움직임. 1.6초 주기.
        // sin 곡선으로 자연스러운 호흡 느낌.
        void StartIdleBob()
        {
            if (boxyMascot == null) return;
            float startTime = Time.realtimeSinceStartup;
            bobAnimation = boxyMascot.schedule.Execute(() =>
            {
                if (boxyMascot == null) return;
                float t = Time.realtimeSinceStartup - startTime;
                float offset = Mathf.Sin(t * Mathf.PI / 0.8f) * 8f;  // 1.6s 주기, ±8px
                boxyMascot.style.translate = new StyleTranslate(new Translate(0, offset, 0));
            }).Every(33);  // ~30fps
        }

        void OnPlay() => sceneLoader.LoadScene(BoxySceneNames.LevelSelect);
        void OnSettings() => sceneLoader.LoadScene(BoxySceneNames.Settings);

        // v1.0: BGM 토글은 Settings에 위임. 메인 메뉴 sound 버튼은 Settings 진입의 빠른 단축
        void OnSound() => sceneLoader.LoadScene(BoxySceneNames.Settings);

        // IAP 상점은 v1.0 보류 — boxy-plan §B-4 IAP 3종은 광고 보상 모달 + Settings에서 트리거
        void OnShop() => Debug.Log("[MainMenu] Shop은 v1.1 백로그 (boxy-plan §B-6 일정 외)");
    }
}
