#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Boxy.App;
using Boxy.App.UI;
using Boxy.App.Gameplay;
using Boxy.App.Gameplay.UI;
using Boxy.App.Levels;

namespace Boxy.Editor
{
    // batchmode -executeMethod Boxy.Editor.BoxyAutomation.SetupAll 진입점.
    // HANDOFF.md §3 정수 차단 8개 GUI 작업을 일괄 자동화 (Editor 종료 상태에서 실행).
    public static class BoxyAutomation
    {
        const string ScenesFolder = "Assets/Scenes";
        const string LevelsFolder = "Assets/Levels";
        const string UiFolder = "Assets/Boxy.App/UI";
        const string PanelSettingsPath = UiFolder + "/BoxyPanelSettings.asset";
        const string ThemePath = UiFolder + "/BoxyTheme.tss";

        public static void SetupAll()
        {
            try
            {
                Step1_GenerateLevels();
                Step2_CreatePanelSettings();
                Step3_CreateAndWireScenes();
                Step4_RegisterBuildSettings();
                Step5_SwitchToAndroid();
                AssetDatabase.SaveAssets();
                Debug.Log("[BoxyAutomation] SUCCESS — all 5 steps completed");
            }
            catch (Exception e)
            {
                Debug.LogError("[BoxyAutomation] FAILED: " + e);
                throw;
            }
        }

        static void Step1_GenerateLevels()
        {
            Debug.Log("[BoxyAutomation] Step 1/5 — Generate 50 Levels");
            LevelGeneratorMenu.GenerateAllLevels();
        }

        static void Step2_CreatePanelSettings()
        {
            Debug.Log("[BoxyAutomation] Step 2/5 — Create BoxyPanelSettings + connect BoxyTheme.tss");
            EnsureFolder(UiFolder);

            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            }

            // BoxyTheme.tss는 unity-theme default + tokens.uss + accent-boxy.uss + components.uss 합본.
            // PanelSettings.themeStyleSheet 미연결 시 var(--accent) 등 inline style 변수가 resolve 안 되어 RecreateUI에서 NullRef 발생.
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            if (theme == null)
            {
                Debug.LogWarning("[BoxyAutomation] BoxyTheme.tss를 찾을 수 없음 — " + ThemePath);
            }
            else
            {
                panel.themeStyleSheet = theme;
                EditorUtility.SetDirty(panel);
            }

            AssetDatabase.SaveAssets();
        }

        static void Step3_CreateAndWireScenes()
        {
            Debug.Log("[BoxyAutomation] Step 3/5 — Create 4 scenes + wire");
            EnsureFolder(ScenesFolder);
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);

            CreateScene("MainMenu", () =>
            {
                AddBootstrap();
                var doc = AddUIDocument("MainMenuRoot", UiFolder + "/MainMenu.uxml", panel);
                doc.gameObject.AddComponent<MainMenuController>();
            });

            CreateScene("LevelSelect", () =>
            {
                AddBootstrap();
                var doc = AddUIDocument("LevelSelectRoot", UiFolder + "/LevelSelect.uxml", panel);
                var ctrl = doc.gameObject.AddComponent<LevelSelectController>();
                AssignLevelArray(ctrl, "levelAssets");
            });

            CreateScene("Gameplay", () =>
            {
                AddBootstrap();
                var ctrlGo = new GameObject("GameplayController");
                var ctrl = ctrlGo.AddComponent<GameplayController>();

                var uiGo = new GameObject("GameplayUI");
                var doc = uiGo.AddComponent<UIDocument>();
                doc.panelSettings = panel;
                doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UiFolder + "/Gameplay.uxml");

                var ui = uiGo.AddComponent<GameplayUI>();
                var soUi = new SerializedObject(ui);
                SetReference(soUi, "controller", ctrl);
                SetReference(soUi, "resultPopupAsset", AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UiFolder + "/ResultPopup.uxml"));
                SetReference(soUi, "defaultLevel", AssetDatabase.LoadAssetAtPath<LevelData>(LevelsFolder + "/Level_01.asset"));
                AssignLevelArrayToProperty(soUi, "levelAssets");
                soUi.ApplyModifiedPropertiesWithoutUndo();

                var onb = uiGo.AddComponent<OnboardingController>();
                var soOnb = new SerializedObject(onb);
                SetReference(soOnb, "onboardingAsset", AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UiFolder + "/Onboarding.uxml"));
                soOnb.ApplyModifiedPropertiesWithoutUndo();
            });

            CreateScene("Settings", () =>
            {
                AddBootstrap();
                var doc = AddUIDocument("SettingsRoot", UiFolder + "/Settings.uxml", panel);
                doc.gameObject.AddComponent<SettingsController>();
            });
        }

        // BoxyBootstrap을 4 씬 모두에 추가 — DontDestroyOnLoad라 첫 씬에서 만든 1개만 살아남고
        // 다른 씬의 중복은 Awake에서 self-destroy. SerializeField는 비워둠 (useStubProviders=true가 기본값이라 dev 모드 동작).
        static void AddBootstrap()
        {
            var go = new GameObject("BoxyBootstrap");
            go.AddComponent<BoxyBootstrap>();
        }

        static void Step4_RegisterBuildSettings()
        {
            Debug.Log("[BoxyAutomation] Step 4/5 — Register Build Settings");
            string[] order = { "MainMenu", "LevelSelect", "Gameplay", "Settings" };
            var list = new EditorBuildSettingsScene[order.Length];
            for (int i = 0; i < order.Length; i++)
            {
                list[i] = new EditorBuildSettingsScene(ScenesFolder + "/" + order[i] + ".unity", true);
            }
            EditorBuildSettings.scenes = list;
        }

        static void Step5_SwitchToAndroid()
        {
            Debug.Log("[BoxyAutomation] Step 5/5 — Switch active build target to Android");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        // 헬퍼 ─────────────────────────────────────────────

        static UIDocument AddUIDocument(string goName, string uxmlPath, PanelSettings panel)
        {
            var go = new GameObject(goName);
            var doc = go.AddComponent<UIDocument>();
            // panelSettings를 먼저 set — visualTreeAsset setter가 RecreateUI 호출 시점에 PanelSettings.themeStyleSheet의 var() 변수가 resolve 가능해야 NullRef 회피.
            doc.panelSettings = panel;
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            return doc;
        }

        static void CreateScene(string name, Action wireAction)
        {
            string path = ScenesFolder + "/" + name + ".unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            wireAction();
            EditorSceneManager.SaveScene(scene, path);
        }

        static void AssignLevelArray(MonoBehaviour ctrl, string fieldName)
        {
            var so = new SerializedObject(ctrl);
            AssignLevelArrayToProperty(so, fieldName);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AssignLevelArrayToProperty(SerializedObject so, string fieldName)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null) { Debug.LogWarning("[BoxyAutomation] Field not found: " + fieldName); return; }

            var guids = AssetDatabase.FindAssets("t:LevelData", new[] { LevelsFolder });
            Array.Sort(guids, (a, b) => AssetDatabase.GUIDToAssetPath(a).CompareTo(AssetDatabase.GUIDToAssetPath(b)));

            prop.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guids[i]));
                prop.GetArrayElementAtIndex(i).objectReferenceValue = asset;
            }
        }

        static void SetReference(SerializedObject so, string fieldName, UnityEngine.Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null) { Debug.LogWarning("[BoxyAutomation] Field not found: " + fieldName); return; }
            prop.objectReferenceValue = value;
        }

        static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif
