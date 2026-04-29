#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mound.Core.Audio;
using Boxy.App;
using Boxy.App.Audio;

namespace Boxy.Editor
{
    // 오디오 시스템 자동 셋업 — batchmode + Editor 메뉴 둘 다 사용 가능.
    // 1. Assets/Boxy.App/Audio/*.wav 파일 import 트리거
    // 2. SfxLibrary.asset 생성 + AudioClip 5종 매핑
    // 3. MainMenu.unity 씬에 BoxyBootstrap GameObject 추가 + AudioService 자식 + Inspector 와이어링
    public static class AudioSetupHelper
    {
        const string SfxLibraryPath = "Assets/Boxy.App/Audio/SfxLibrary.asset";
        const string AudioFolder = "Assets/Boxy.App/Audio";
        const string MainScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Boxy/Audio/Setup All")]
        public static void SetupAll()
        {
            AssetDatabase.Refresh();
            CreateSfxLibrary();
            WireBootstrapScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[AudioSetup] 완료 — SfxLibrary.asset + BoxyBootstrap 씬 wiring");
        }

        public static void CreateSfxLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<SfxLibrary>(SfxLibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<SfxLibrary>();
                AssetDatabase.CreateAsset(lib, SfxLibraryPath);
            }
            lib.uiTap = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/sfx_ui_tap.wav");
            lib.dragStart = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/sfx_drag_start.wav");
            lib.itemPlaced = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/sfx_item_placed.wav");
            lib.invalid = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/sfx_invalid.wav");
            lib.levelCleared = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/sfx_level_cleared.wav");
            lib.rotate = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/sfx_rotate.wav");
            lib.dropHover = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/sfx_drop_hover.wav");
            lib.starCount = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/sfx_star_count.wav");
            lib.bgmCozyLoop = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/bgm_cozy_loop.wav");
            EditorUtility.SetDirty(lib);
            Debug.Log("[AudioSetup] SfxLibrary.asset 생성/갱신 완료 — 8 SFX + 1 BGM");
        }

        public static void WireBootstrapScene()
        {
            var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            // BoxyBootstrap GameObject가 없으면 생성
            var bootstrapGo = GameObject.Find("BoxyBootstrap");
            BoxyBootstrap bootstrap;
            if (bootstrapGo == null)
            {
                bootstrapGo = new GameObject("BoxyBootstrap");
                bootstrap = bootstrapGo.AddComponent<BoxyBootstrap>();
            }
            else
            {
                bootstrap = bootstrapGo.GetComponent<BoxyBootstrap>() ?? bootstrapGo.AddComponent<BoxyBootstrap>();
            }

            // AudioService 자식 GameObject + AudioSource 2개 (BGM + SFX)
            var audioGo = bootstrapGo.transform.Find("AudioService")?.gameObject;
            if (audioGo == null)
            {
                audioGo = new GameObject("AudioService");
                audioGo.transform.SetParent(bootstrapGo.transform);
            }
            var audioService = audioGo.GetComponent<AudioService>() ?? audioGo.AddComponent<AudioService>();

            var bgmSrc = audioGo.transform.Find("BGM")?.GetComponent<AudioSource>();
            if (bgmSrc == null)
            {
                var bgmGo = new GameObject("BGM");
                bgmGo.transform.SetParent(audioGo.transform);
                bgmSrc = bgmGo.AddComponent<AudioSource>();
                bgmSrc.playOnAwake = false;
            }
            var sfxSrc = audioGo.transform.Find("SFX")?.GetComponent<AudioSource>();
            if (sfxSrc == null)
            {
                var sfxGo = new GameObject("SFX");
                sfxGo.transform.SetParent(audioGo.transform);
                sfxSrc = sfxGo.AddComponent<AudioSource>();
                sfxSrc.playOnAwake = false;
            }

            // SerializedObject로 AudioService의 bgmSource/sfxSource 와이어링
            var asSo = new SerializedObject(audioService);
            asSo.FindProperty("bgmSource").objectReferenceValue = bgmSrc;
            asSo.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
            asSo.ApplyModifiedProperties();

            // Bootstrap의 audioService + sfxLibrary 와이어링
            var bsSo = new SerializedObject(bootstrap);
            bsSo.FindProperty("audioService").objectReferenceValue = audioService;
            bsSo.FindProperty("sfxLibrary").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<SfxLibrary>(SfxLibraryPath);
            bsSo.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[AudioSetup] BoxyBootstrap + AudioService + SfxLibrary 씬 wiring 완료");
        }
    }
}
#endif
