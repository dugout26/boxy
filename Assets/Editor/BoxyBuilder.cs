#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Boxy.Editor
{
    // batchmode 빌드 진입점.
    // 실행: Unity -batchmode -projectPath ... -executeMethod Boxy.Editor.BoxyBuilder.BuildDevAndroid -quit
    public static class BoxyBuilder
    {
        const string OutputDir = "Builds";

        static readonly string[] Scenes =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/LevelSelect.unity",
            "Assets/Scenes/Gameplay.unity",
            "Assets/Scenes/Settings.unity"
        };

        // 2-tier 환경 전략 (decisions/2026-04-29-09-app-id-strategy-2tier.md):
        //   dev:  Development build, Bundle `com.mound.boxy.dev`, 정수 디바이스 매일 검증
        //   prod: Release build, Bundle `com.mound.boxy`, TestFlight Internal/External + App Store
        // staging 폐지 — closed test는 prod 앱의 TestFlight External 트랙으로 대체.

        public static void BuildDevAndroid()
        {
            BuildAndroid("dev", BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        public static void BuildProductionAndroid()
        {
            BuildAndroid("prod", BuildOptions.None);
        }

        // CI/CD entry — 환경변수 BUILD_NUMBER_OVERRIDE 적용 후 빌드.
        public static void BumpAndBuildDevAndroid()
        {
            ApplyBuildNumberFromEnvOrIncrement();
            BuildDevAndroid();
        }

        public static void BumpAndBuildProductionAndroid()
        {
            ApplyBuildNumberFromEnvOrIncrement();
            BuildProductionAndroid();
        }

        // CI/CD: 컴파일 + 자산 import만 검증. 빌드 산출물 X.
        // game-ci/unity-builder가 -executeMethod 호출 시 그냥 종료 = 컴파일 OK.
        public static void CompileCheck()
        {
            UnityEngine.Debug.Log("[BoxyBuilder] CompileCheck: 컴파일/자산 import 통과 — 즉시 종료.");
        }

        public static void BuildDevIOS()
        {
            BuildIOS("dev", "com.mound.boxy.dev", BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        public static void BuildProductionIOS()
        {
            BuildIOS("prod", "com.mound.boxy", BuildOptions.None);
        }

        // 업로드 직전에만 호출 — buildNumber +1 후 BuildDevIOS. 로컬 rebuild는 BuildDevIOS만.
        // CI에서는 BUILD_NUMBER_OVERRIDE 환경변수로 명시 주입 (GITHUB_RUN_NUMBER 또는 git rev-list count).
        public static void BumpAndBuildDevIOS()
        {
            ApplyBuildNumberFromEnvOrIncrement();
            BuildDevIOS();
        }

        public static void BumpAndBuildProductionIOS()
        {
            ApplyBuildNumberFromEnvOrIncrement();
            BuildProductionIOS();
        }

        // 환경별 EnvironmentConfig.asset 자동 스왑 — secrets/EnvironmentConfig-{env}.asset → Assets/Mound/Monetization/EnvironmentConfig.asset
        // §8-2 빌드 환경 분리 + §9-2 시크릿 보호. asset 파일은 .gitignore. CI는 GitHub Secret(base64) → 로컬 파일로 디코드 → 빌드.
        // 파일 없으면 경고만 — Editor에서 수동 SO 만들어 쓰는 로컬 개발 흐름은 차단하지 않음.
        static void ApplyEnvironmentConfig(string envTag)
        {
            string src = $"secrets/EnvironmentConfig-{envTag}.asset";
            string target = "Assets/Mound/Monetization/EnvironmentConfig.asset";
            if (File.Exists(src))
            {
                File.Copy(src, target, true);
                string srcMeta = src + ".meta";
                string targetMeta = target + ".meta";
                if (File.Exists(srcMeta))
                {
                    File.Copy(srcMeta, targetMeta, true);
                }
                AssetDatabase.Refresh();
                Debug.Log($"[BoxyBuilder] EnvironmentConfig: {src} → {target}");
            }
            else if (!File.Exists(target))
            {
                Debug.LogWarning($"[BoxyBuilder] {src} 없음 + 로컬 EnvironmentConfig.asset도 없음 — Provider Null fallback 동작.");
            }
        }

        // CI/CD: BUILD_NUMBER_OVERRIDE 환경변수 우선 → 없으면 +1 fallback.
        // CI에서 GITHUB_RUN_NUMBER + base offset 주입하면 ProjectSettings.asset 변경 없이 빌드별 단조 증가.
        static void ApplyBuildNumberFromEnvOrIncrement()
        {
            string envOverride = System.Environment.GetEnvironmentVariable("BUILD_NUMBER_OVERRIDE");
            if (!string.IsNullOrEmpty(envOverride) && int.TryParse(envOverride, out int forced))
            {
                PlayerSettings.iOS.buildNumber = forced.ToString();
                PlayerSettings.Android.bundleVersionCode = forced;
                Debug.Log($"[BoxyBuilder] BUILD_NUMBER_OVERRIDE={forced} 적용 (iOS + Android)");
                return;
            }

            int next = int.TryParse(PlayerSettings.iOS.buildNumber, out var b) ? b + 1 : 1;
            PlayerSettings.iOS.buildNumber = next.ToString();
            PlayerSettings.Android.bundleVersionCode = next;
            Debug.Log($"[BoxyBuilder] buildNumber bumped → {next} (iOS + Android)");
        }

        // 로컬 검증용 — iOS Simulator (Apple Silicon Mac은 arm64) 빌드.
        // TestFlight 업로드 없이 즉시 실행 가능. 시각·인터랙션 검증의 90%를 여기서 끝낼 수 있음.
        public static void BuildSimulatorIOS()
        {
            BuildSimulatorWithFirstScene(0);  // 0: MainMenu
        }

        // UI 검증용 — 특정 씬을 첫 씬으로 빌드. simctl 자동 탭 미지원 우회.
        public static void BuildSimulatorIOS_LevelSelect() { BuildSimulatorWithFirstScene(1); }
        public static void BuildSimulatorIOS_Gameplay() { BuildSimulatorWithFirstScene(2); }
        public static void BuildSimulatorIOS_Settings() { BuildSimulatorWithFirstScene(3); }

        static void BuildSimulatorWithFirstScene(int firstSceneIndex)
        {
            var prevSdk = PlayerSettings.iOS.sdkVersion;
            int prevSimArch = GetSimulatorArchitecture();
            string[] originalScenes = (string[])Scenes.Clone();
            try
            {
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
                SetSimulatorArchitecture(1);

                // 씬 순서 임시 swap — firstSceneIndex번째 씬을 첫 씬으로
                if (firstSceneIndex > 0 && firstSceneIndex < originalScenes.Length)
                {
                    var swapped = (string[])originalScenes.Clone();
                    var tmp = swapped[0];
                    swapped[0] = swapped[firstSceneIndex];
                    swapped[firstSceneIndex] = tmp;
                    System.Array.Copy(swapped, Scenes, swapped.Length);
                }

                BuildIOS("simulator", "com.mound.boxy.dev", BuildOptions.Development);
            }
            finally
            {
                PlayerSettings.iOS.sdkVersion = prevSdk;
                SetSimulatorArchitecture(prevSimArch);
                System.Array.Copy(originalScenes, Scenes, originalScenes.Length);
            }
        }

        // PlayerSettings.iOSSimulatorArchitecture는 Unity 6에 public setter 없어서 SerializedObject로 직접 접근.
        static int GetSimulatorArchitecture()
        {
            var so = new SerializedObject(Resources.FindObjectsOfTypeAll<PlayerSettings>()[0]);
            var prop = so.FindProperty("iOSSimulatorArchitecture");
            return prop != null ? prop.intValue : 0;
        }

        static void SetSimulatorArchitecture(int value)
        {
            var so = new SerializedObject(Resources.FindObjectsOfTypeAll<PlayerSettings>()[0]);
            var prop = so.FindProperty("iOSSimulatorArchitecture");
            if (prop != null)
            {
                prop.intValue = value;
                so.ApplyModifiedProperties();
            }
        }

        static void BuildIOS(string envTag, string bundleId, BuildOptions options)
        {
            string xcodePath = $"{OutputDir}/ios-{envTag}";
            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }

            // PanelSettings — decisions/2026-04-29-11-responsive-layout.md 표준 (Expand 모드).
            // Expand: 참조 해상도 1080×1920이 항상 보장, 더 긴 화면(iPhone 17 등)은 위/아래 추가 공간.
            // 그리드 게임(Boxy 6×6) 정사각형 보장에 결정적.
            var panelSettingsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>(
                "Assets/Boxy.App/UI/BoxyPanelSettings.asset");
            if (panelSettingsAsset != null)
            {
                panelSettingsAsset.scaleMode = UnityEngine.UIElements.PanelScaleMode.ScaleWithScreenSize;
                panelSettingsAsset.screenMatchMode = UnityEngine.UIElements.PanelScreenMatchMode.Expand;
                panelSettingsAsset.referenceResolution = new Vector2Int(390, 844);
                EditorUtility.SetDirty(panelSettingsAsset);
                AssetDatabase.SaveAssets();
                Debug.Log("[BoxyBuilder] PanelSettings: ScaleWithScreenSize, matchMode=Expand, refRes=390×844 (iPhone pt — handoff base)");
            }

            // Bundle ID, productName, Version 강제 설정 — 환경별 분기.
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleId);
            // App Store 표시명 (decisions/2026-04-29-10-app-store-name.md). dev는 구분 위해 suffix.
            PlayerSettings.productName = envTag == "dev" ? "Boxy Dev" : "Boxy Pack";

            // Firebase iOS GoogleService-Info.plist 환경별 자동 복사 (정수가 Firebase Console에서 다운받아 secrets/에 저장 후 작동).
            string srcPlist = $"secrets/GoogleService-Info-{envTag}.plist";
            string targetPlist = "Assets/GoogleService-Info.plist";
            if (File.Exists(srcPlist))
            {
                File.Copy(srcPlist, targetPlist, true);
                AssetDatabase.Refresh();
                Debug.Log($"[BoxyBuilder] Firebase iOS config: {srcPlist} → {targetPlist}");
            }
            else
            {
                Debug.Log($"[BoxyBuilder] {srcPlist} 없음 — iOS Firebase 미설정. (Firebase Console에서 iOS 앱 추가 후 다운로드)");
            }

            ApplyEnvironmentConfig(envTag);
            PlayerSettings.bundleVersion = "1.0.0";
            // 빌드 번호는 업로드 스크립트(scripts/ios-archive-upload.sh)에서 BumpAndBuildDevIOS로 관리.
            // BuildDevIOS 직접 호출은 bump 안 함 → 로컬 rebuild 시 번호 안 튀게.

            // Splash 화면 — bg-light 색 + 로고 스타일 (Personal License는 Unity 로고 강제, Pro는 숨김 가능).
            PlayerSettings.SplashScreen.backgroundColor = new Color(0.972f, 0.976f, 0.980f);  // #F8F9FA bg-light
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;  // Pro만 적용 (Personal은 무시됨)
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
            PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;

            // Portrait Only — 가방 퍼즐 게임은 세로 모드 강제. AutoRotation 시 UI Toolkit 레이아웃 망가짐.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            // 카메라/마이크/위치 권한 미요청 — Boxy는 권한 0으로 설계 (§B-5 UX 핵심).

            var buildOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = xcodePath,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = options
            };

            Debug.Log($"[BoxyBuilder] iOS build start: {envTag} ({bundleId}) → {xcodePath}");
            var report = BuildPipeline.BuildPlayer(buildOptions);
            var summary = report.summary;

            // Info.plist 수출 규제 면제 키 자동 삽입 (App Store Connect "암호화 문서" 다이얼로그 우회).
            // Boxy는 iOS 표준 HTTPS/TLS만 사용 → exempt. 매 빌드마다 수동 답변 안 하도록 박음.
            if (summary.result == BuildResult.Succeeded)
            {
                string plistPath = $"{xcodePath}/Info.plist";
                if (System.IO.File.Exists(plistPath))
                {
                    string plist = System.IO.File.ReadAllText(plistPath);
                    if (!plist.Contains("ITSAppUsesNonExemptEncryption"))
                    {
                        plist = plist.Replace("</dict>\n</plist>",
                            "\t<key>ITSAppUsesNonExemptEncryption</key>\n\t<false/>\n</dict>\n</plist>");
                        System.IO.File.WriteAllText(plistPath, plist);
                        Debug.Log("[BoxyBuilder] Info.plist에 ITSAppUsesNonExemptEncryption=false 삽입");
                    }

                    // ATT(App Tracking Transparency) 권한 요청 시 표시될 메시지 — iOS 14.5+ 필수.
                    if (!plist.Contains("NSUserTrackingUsageDescription"))
                    {
                        plist = plist.Replace("</dict>\n</plist>",
                            "\t<key>NSUserTrackingUsageDescription</key>\n\t<string>맞춤형 광고 표시를 위해 광고 식별자 사용 동의가 필요해요. 거부해도 게임은 정상 작동합니다.</string>\n</dict>\n</plist>");
                        System.IO.File.WriteAllText(plistPath, plist);
                        Debug.Log("[BoxyBuilder] Info.plist에 NSUserTrackingUsageDescription 삽입");
                    }
                }

                // 1024 App Store 아이콘 보강 — Unity가 매 빌드 Asset Catalog 재생성하며 1024 슬롯을 비움.
                // App Store Connect 업로드 시 91111 에러(아이콘 누락) 회피를 위해 placeholder 박음.
                // 정수 마스코트 그림(§A-6) 들어오면 Assets/Boxy.App/Icons/AppStore-1024.png로 교체.
                string iconsetDir = $"{xcodePath}/Unity-iPhone/Images.xcassets/AppIcon.appiconset";
                string customIconSrc = "Assets/Boxy.App/Icons/AppStore-1024.png";
                string targetIcon = $"{iconsetDir}/Icon-AppStore-1024.png";
                if (System.IO.Directory.Exists(iconsetDir))
                {
                    if (System.IO.File.Exists(customIconSrc))
                    {
                        System.IO.File.Copy(customIconSrc, targetIcon, true);
                    }
                    else
                    {
                        // placeholder 노란 사각형 1024×1024
                        var tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
                        var yellow = new Color32(255, 214, 10, 255);
                        var black = new Color32(26, 26, 26, 255);
                        var pixels = new Color32[1024 * 1024];
                        for (int y = 0; y < 1024; y++)
                        for (int x = 0; x < 1024; x++)
                        {
                            bool border = x < 16 || x >= 1008 || y < 16 || y >= 1008;
                            pixels[y * 1024 + x] = border ? black : yellow;
                        }
                        tex.SetPixels32(pixels);
                        tex.Apply();
                        System.IO.File.WriteAllBytes(targetIcon, tex.EncodeToPNG());
                        Object.DestroyImmediate(tex);
                    }

                    // Contents.json에 ios-marketing 1024 항목 추가
                    string contentsPath = $"{iconsetDir}/Contents.json";
                    string contents = System.IO.File.ReadAllText(contentsPath);
                    if (!contents.Contains("ios-marketing"))
                    {
                        contents = contents.Replace("\"images\" : [",
                            "\"images\" : [\n\t\t{\n\t\t\t\"filename\" : \"Icon-AppStore-1024.png\",\n\t\t\t\"idiom\" : \"ios-marketing\",\n\t\t\t\"scale\" : \"1x\",\n\t\t\t\"size\" : \"1024x1024\"\n\t\t},");
                        System.IO.File.WriteAllText(contentsPath, contents);
                    }
                    Debug.Log("[BoxyBuilder] AppStore 1024 아이콘 + Contents.json 갱신 완료");
                }
            }

            Debug.Log($"[BoxyBuilder] Result: {summary.result}");
            Debug.Log($"[BoxyBuilder] Total time: {summary.totalTime}");
            Debug.Log($"[BoxyBuilder] Total errors: {summary.totalErrors}");
            Debug.Log($"[BoxyBuilder] Total warnings: {summary.totalWarnings}");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new System.Exception($"[BoxyBuilder] iOS Xcode project gen FAILED: {summary.result}");
            }
        }

        static void BuildAndroid(string envTag, BuildOptions options)
        {
            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }

            string bundleId = envTag == "dev" ? "com.mound.boxy.dev" : "com.mound.boxy";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, bundleId);
            // Play Store 표시명 (decisions/2026-04-29-10-app-store-name.md).
            PlayerSettings.productName = envTag == "dev" ? "Boxy Dev" : "Boxy Pack";

            // Firebase google-services.json 환경별 자동 복사 (Day 18 통합 후 자동 사용).
            string srcServices = $"secrets/google-services-{envTag}.json";
            string targetServices = "Assets/google-services.json";
            if (File.Exists(srcServices))
            {
                File.Copy(srcServices, targetServices, true);
                AssetDatabase.Refresh();
                Debug.Log($"[BoxyBuilder] Firebase config: {srcServices} → {targetServices}");
            }
            else
            {
                Debug.LogWarning($"[BoxyBuilder] {srcServices} 없음 — Firebase 미설정. (firebase apps:sdkconfig으로 다운로드)");
            }

            ApplyEnvironmentConfig(envTag);

            string apkPath = $"{OutputDir}/boxy-{envTag}.apk";

            var buildOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = options
            };

            Debug.Log($"[BoxyBuilder] Build start: {envTag} → {apkPath}");
            var report = BuildPipeline.BuildPlayer(buildOptions);
            var summary = report.summary;

            Debug.Log($"[BoxyBuilder] Result: {summary.result}");
            Debug.Log($"[BoxyBuilder] Total time: {summary.totalTime}");
            Debug.Log($"[BoxyBuilder] Total size: {summary.totalSize / 1024 / 1024} MB");
            Debug.Log($"[BoxyBuilder] Total errors: {summary.totalErrors}");
            Debug.Log($"[BoxyBuilder] Total warnings: {summary.totalWarnings}");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new System.Exception($"[BoxyBuilder] Build FAILED: {summary.result} — see log for details");
            }
        }
    }
}
#endif
