#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Boxy.App.UI;
using Boxy.App.Gameplay;
using Boxy.App.Gameplay.UI;

namespace Boxy.Editor
{
    // 4 씬 (MainMenu / LevelSelect / Gameplay / Settings) 활성 씬에 자동 wire.
    // boxy-unity-setup-guide.md §D 30분 작업을 30초로 단축.
    //
    // 사용:
    //   1. File → New Scene → Empty 생성 후 저장 (예: MainMenu.unity)
    //   2. 메뉴 → Boxy → Scene Setup → Wire Current Scene as [씬 종류]
    //   3. Inspector에서 누락된 SerializeField (Panel Settings, LevelData 등) 수동 채움
    public static class SceneSetupHelper
    {
        const string MainMenuUxmlPath = "Assets/Boxy.App/UI/MainMenu.uxml";
        const string LevelSelectUxmlPath = "Assets/Boxy.App/UI/LevelSelect.uxml";
        const string GameplayUxmlPath = "Assets/Boxy.App/UI/Gameplay.uxml";
        const string SettingsUxmlPath = "Assets/Boxy.App/UI/Settings.uxml";

        [MenuItem("Boxy/Scene Setup/Wire as MainMenu")]
        public static void WireMainMenu()
        {
            var go = new GameObject("MainMenuRoot");
            var doc = go.AddComponent<UIDocument>();
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainMenuUxmlPath);
            go.AddComponent<MainMenuController>();
            WarnPanelSettings();
        }

        [MenuItem("Boxy/Scene Setup/Wire as LevelSelect")]
        public static void WireLevelSelect()
        {
            var go = new GameObject("LevelSelectRoot");
            var doc = go.AddComponent<UIDocument>();
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LevelSelectUxmlPath);
            go.AddComponent<LevelSelectController>();
            WarnPanelSettings();
            Debug.LogWarning("[SceneSetup] LevelSelectController.levelAssets 인스펙터에서 LevelData 50개 드래그 필요");
        }

        [MenuItem("Boxy/Scene Setup/Wire as Gameplay")]
        public static void WireGameplay()
        {
            var ctrlGo = new GameObject("GameplayController");
            ctrlGo.AddComponent<GameplayController>();

            var uiGo = new GameObject("GameplayUI");
            var doc = uiGo.AddComponent<UIDocument>();
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GameplayUxmlPath);

            var ui = uiGo.AddComponent<GameplayUI>();
            // SerializeField는 Editor SerializedObject로 설정해야 — 정수가 인스펙터에서 직접 드래그 권장
            uiGo.AddComponent<OnboardingController>();

            WarnPanelSettings();
            Debug.LogWarning("[SceneSetup] GameplayUI 인스펙터에서 controller (GameplayController GameObject 드래그) + defaultLevel + resultPopupAsset + levelAssets 연결 필요");
        }

        [MenuItem("Boxy/Scene Setup/Wire as Settings")]
        public static void WireSettings()
        {
            var go = new GameObject("SettingsRoot");
            var doc = go.AddComponent<UIDocument>();
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingsUxmlPath);
            go.AddComponent<SettingsController>();
            WarnPanelSettings();
        }

        static void WarnPanelSettings()
        {
            Debug.LogWarning("[SceneSetup] UIDocument.panelSettings 인스펙터에 BoxyPanelSettings asset 드래그 필요. " +
                "(없으면 Project → Create → UI Toolkit → Panel Settings Asset 으로 생성)");
        }
    }
}
#endif
