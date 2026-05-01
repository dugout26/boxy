#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Boxy.App;
using Boxy.App.Audio;
using Boxy.App.UI;
using Boxy.App.Gameplay;
using Boxy.App.Gameplay.UI;
using Boxy.App.Levels;
using Mound.Core.Audio;

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

        // Editor에서 수동 호출 가능 — 4 씬 모두에 Main Camera + AudioListener 박음.
        // 기존 자동화가 EmptyScene으로 만들어 Camera 누락된 상태 정정용.
        [MenuItem("Boxy/Fix Missing Cameras (4 scenes)")]
        public static void FixMissingCameras()
        {
            string[] scenes = { "MainMenu", "LevelSelect", "Gameplay", "Settings" };
            foreach (var name in scenes)
            {
                string path = ScenesFolder + "/" + name + ".unity";
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                bool hasCamera = false;
                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go.GetComponent<Camera>() != null) { hasCamera = true; break; }
                }
                if (hasCamera) { Debug.Log($"[FixCameras] {name} — 이미 Camera 있음, skip"); continue; }

                var camGo = new GameObject("Main Camera");
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.97f, 0.94f, 0.86f);   // Cozy cream — UI 시작 배경
                cam.orthographic = true;
                cam.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[FixCameras] {name} — Camera + AudioListener 추가 완료");
            }
            Debug.Log("[FixCameras] DONE — MainMenu 씬 다시 열어 Play 시도");
        }

        public static void SetupAll()
        {
            try
            {
                Step1_GenerateLevels();
                var panel = Step2_CreatePanelSettings();   // ← panel 직접 반환 → Step 3에 전달 (LoadAssetAtPath batchmode 타이밍 이슈 회피)
                Step3_CreateAndWireScenes(panel);
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

        static PanelSettings Step2_CreatePanelSettings()
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
            return panel;
        }

        static void Step3_CreateAndWireScenes(PanelSettings _unused)
        {
            Debug.Log("[BoxyAutomation] Step 3/5 — Create 4 scenes + wire");
            EnsureFolder(ScenesFolder);
            // NewScene이 outer-scope PanelSettings reference를 invalidate (Unity null-equivalent).
            // 각 CreateScene 콜백 안에서 LoadAssetAtPath로 fresh panel 재취득 필수.

            CreateScene("MainMenu", () =>
            {
                AddBootstrap();
                var p = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
                var doc = AddUIDocument("MainMenuRoot", UiFolder + "/MainMenu.uxml", p);
                doc.gameObject.AddComponent<MainMenuController>();
            });

            CreateScene("LevelSelect", () =>
            {
                AddBootstrap();
                var p = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
                var doc = AddUIDocument("LevelSelectRoot", UiFolder + "/LevelSelect.uxml", p);
                var ctrl = doc.gameObject.AddComponent<LevelSelectController>();
                AssignLevelArray(ctrl, "levelAssets");
            });

            CreateScene("Gameplay", () =>
            {
                AddBootstrap();
                var p = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
                var ctrlGo = new GameObject("GameplayController");
                var ctrl = ctrlGo.AddComponent<GameplayController>();

                var uiGo = new GameObject("GameplayUI");
                var doc = uiGo.AddComponent<UIDocument>();
                doc.panelSettings = p;
                doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UiFolder + "/Gameplay.uxml");
                var soDoc = new SerializedObject(doc);
                soDoc.FindProperty("m_PanelSettings").objectReferenceValue = p;
                soDoc.FindProperty("sourceAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UiFolder + "/Gameplay.uxml");
                soDoc.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(doc);
                EditorUtility.SetDirty(uiGo);

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
                var p = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
                var doc = AddUIDocument("SettingsRoot", UiFolder + "/Settings.uxml", p);
                doc.gameObject.AddComponent<SettingsController>();
            });
        }

        // BoxyBootstrap을 4 씬 모두에 추가 — DontDestroyOnLoad라 첫 씬에서 만든 1개만 살아남고
        // 다른 씬의 중복은 Awake에서 self-destroy. AudioService 자식 + SfxLibrary 자동 wire.
        static void AddBootstrap()
        {
            var go = new GameObject("BoxyBootstrap");
            var bootstrap = go.AddComponent<BoxyBootstrap>();

            // AudioService 자식 GameObject — AudioSource 2개 (BGM + SFX)
            var audioGo = new GameObject("AudioService");
            audioGo.transform.SetParent(go.transform);
            var bgmSource = audioGo.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            var sfxSource = audioGo.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            var audioService = audioGo.AddComponent<AudioService>();

            // AudioService.bgmSource / sfxSource SerializeField wire
            var soAudio = new SerializedObject(audioService);
            SetReference(soAudio, "bgmSource", bgmSource);
            SetReference(soAudio, "sfxSource", sfxSource);
            soAudio.ApplyModifiedPropertiesWithoutUndo();

            // BoxyBootstrap의 audioService + sfxLibrary SerializeField wire
            var sfxLib = AssetDatabase.LoadAssetAtPath<SfxLibrary>("Assets/Boxy.App/Audio/SfxLibrary.asset");
            var soBoot = new SerializedObject(bootstrap);
            SetReference(soBoot, "audioService", audioService);
            SetReference(soBoot, "sfxLibrary", sfxLib);
            soBoot.ApplyModifiedPropertiesWithoutUndo();
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
            if (panel == null) Debug.LogError($"[BoxyAutomation] {goName}: panel arg is NULL!");

            var go = new GameObject(goName);
            var doc = go.AddComponent<UIDocument>();

            // panelSettings는 property setter로 set (Unity 6 UIDocument 내부 register 처리 필요).
            doc.panelSettings = panel;
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            // SerializedObject로 한 번 더 박아 SaveScene 시 직렬화 보장 (property setter가 EditMode dirty 누락하는 경우 대비).
            var so = new SerializedObject(doc);
            so.FindProperty("m_PanelSettings").objectReferenceValue = panel;
            so.FindProperty("sourceAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            so.ApplyModifiedPropertiesWithoutUndo();

            // 검증 — 실제 set됐는지 Re-read
            var verify = new SerializedObject(doc);
            var ps = verify.FindProperty("m_PanelSettings").objectReferenceValue;
            Debug.Log($"[BoxyAutomation] {goName}: panelSettings after set = {(ps != null ? ps.name : "NULL")}");

            EditorUtility.SetDirty(doc);
            EditorUtility.SetDirty(go);
            return doc;
        }

        static void CreateScene(string name, Action wireAction)
        {
            string path = ScenesFolder + "/" + name + ".unity";
            // DefaultGameObjects = Main Camera + Directional Light 자동 박힘.
            // UI Toolkit ScreenSpace-Overlay가 PanelSettings.RenderMode=0이라도 Camera/AudioListener 없으면
            // "No cameras rendering" + "No audio listeners" 경고 + 검정 화면.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
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
