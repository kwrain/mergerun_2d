using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class iOSPostProcess
{
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string plistPath = Path.Combine(path, "Info.plist");
            
            if (File.Exists(plistPath))
            {
                // PlistDocument를 사용하여 Info.plist 수정
                UnityEditor.iOS.Xcode.PlistDocument plist = new UnityEditor.iOS.Xcode.PlistDocument();
                plist.ReadFromFile(plistPath);
                
                // NSUserTrackingUsageDescription 추가
                string trackingDescription = "더 나은 광고 경험을 위해 광고 추적을 허용해주세요.";
                plist.root.SetString("NSUserTrackingUsageDescription", trackingDescription);
                
                // ITSAppUsesNonExemptEncryption 설정
                // false: 면제되지 않은 암호화를 사용하지 않음 (기본값)
                // true: 면제되지 않은 암호화를 사용함
                bool usesNonExemptEncryption = false;
                plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", usesNonExemptEncryption);
                
                plist.WriteToFile(plistPath);
                
                Debug.Log($"[iOS PostProcess] NSUserTrackingUsageDescription이 Info.plist에 추가되었습니다: {trackingDescription}");
                Debug.Log($"[iOS PostProcess] ITSAppUsesNonExemptEncryption이 Info.plist에 설정되었습니다: {usesNonExemptEncryption}");
            }
            else
            {
                Debug.LogWarning($"[iOS PostProcess] Info.plist를 찾을 수 없습니다: {plistPath}");
            }
        }
    }
}

