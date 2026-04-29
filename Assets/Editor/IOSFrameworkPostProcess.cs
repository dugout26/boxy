#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace Boxy.Editor
{
    // iOS Xcode 프로젝트 빌드 후 BoxyATTBridge.mm + BoxyHapticBridge.mm가 사용하는
    // 시스템 프레임워크 자동 링크. 없으면 linker error 발생.
    public static class IOSFrameworkPostProcess
    {
        [PostProcessBuild(45)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            // Unity 6.x: GetUnityFrameworkTargetGuid + GetUnityMainTargetGuid (둘 다 framework 추가 필요).
            string mainTarget = proj.GetUnityMainTargetGuid();
            string frameworkTarget = proj.GetUnityFrameworkTargetGuid();

            // ATT (iOS 14.5+)
            proj.AddFrameworkToProject(frameworkTarget, "AppTrackingTransparency.framework", false);
            proj.AddFrameworkToProject(frameworkTarget, "AdSupport.framework", false);
            // UIKit (햅틱 + ATT 모두 사용)
            proj.AddFrameworkToProject(frameworkTarget, "UIKit.framework", false);

            proj.WriteToFile(projPath);
            UnityEngine.Debug.Log("[IOSFrameworkPostProcess] AppTrackingTransparency, AdSupport, UIKit frameworks 추가 완료");
        }
    }
}
#endif
