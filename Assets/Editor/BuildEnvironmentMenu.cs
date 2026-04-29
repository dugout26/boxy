#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Boxy.Editor
{
    // CLAUDE.md §8 빌드 환경 분리 (2-tier, decisions/2026-04-29-09-app-id-strategy-2tier.md). 메뉴 한 번 클릭으로:
    //   1. secrets/google-services-{env}.json → Assets/google-services.json 복사
    //   2. Scripting Define Symbol 갱신 (BOXY_DEV / BOXY_PROD 중 하나)
    //   3. Application Bundle ID 변경 (Android + iOS 동시)
    //
    // 사용 절차 (Week 3 통합 후):
    //   Boxy → Environment → Switch to Dev/Production → 빌드
    //   AdMob/AppLovin은 BOXY_DEV에서 자동으로 테스트 ID 사용 (EnvironmentConfig.useTestAdIds)
    public static class BuildEnvironmentMenu
    {
        const string SecretsFolder = "secrets";
        const string TargetServicesPath = "Assets/google-services.json";

        [MenuItem("Boxy/Environment/Switch to Dev")]
        public static void SwitchToDev() => Switch("dev", "BOXY_DEV", "com.mound.boxy.dev");

        [MenuItem("Boxy/Environment/Switch to Production")]
        public static void SwitchToProduction() => Switch("prod", "BOXY_PROD", "com.mound.boxy");

        static void Switch(string env, string defineSymbol, string bundleId)
        {
            // 1. Firebase config 복사
            string sourcePath = $"{SecretsFolder}/google-services-{env}.json";
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, TargetServicesPath, true);
                AssetDatabase.Refresh();
                Debug.Log($"[BuildEnvironment] {sourcePath} → {TargetServicesPath}");
            }
            else
            {
                Debug.LogWarning($"[BuildEnvironment] {sourcePath} 없음 — Firebase config 미복사. (Day 1에 firebase apps:sdkconfig로 생성)");
            }

            // 2. ScriptingDefineSymbols 갱신 (BOXY_* 중 하나만 활성, Android+iOS 동시)
            string allEnvSymbols = "BOXY_DEV;BOXY_PROD";
            ApplyDefineSymbols(NamedBuildTarget.Android, allEnvSymbols, defineSymbol);
            ApplyDefineSymbols(NamedBuildTarget.iOS, allEnvSymbols, defineSymbol);

            // 3. Bundle ID (Android + iOS 동시)
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, bundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleId);

            EditorUtility.DisplayDialog("Environment Switched",
                $"Environment: {env}\nBundle ID: {bundleId}\nScripting Define: {defineSymbol}\n" +
                "(Player Settings 확인하여 적용 검증 권장)", "OK");
        }

        static void ApplyDefineSymbols(NamedBuildTarget target, string allEnvSymbols, string defineSymbol)
        {
            string current = PlayerSettings.GetScriptingDefineSymbols(target);
            foreach (var sym in allEnvSymbols.Split(';'))
            {
                current = current.Replace(sym + ";", "").Replace(";" + sym, "").Replace(sym, "");
            }
            current = current.Trim(';');
            if (!string.IsNullOrEmpty(current)) current += ";";
            current += defineSymbol;
            PlayerSettings.SetScriptingDefineSymbols(target, current);
        }
    }
}
#endif
