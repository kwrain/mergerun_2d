using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;

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
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        // NSUserTrackingUsageDescription 추가
        string trackingDescription = "더 나은 광고 경험을 위해 광고 추적을 허용해주세요.";
        plist.root.SetString("NSUserTrackingUsageDescription", trackingDescription);

        // ITSAppUsesNonExemptEncryption 설정
        // false: 면제되지 않은 암호화를 사용하지 않음 (기본값)
        // true: 면제되지 않은 암호화를 사용함
        bool usesNonExemptEncryption = false;
        plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", usesNonExemptEncryption);

        // SKAdNetworkItems 추가 (iOS 14+ 광고 추적에 필수)
        PlistElementArray skAdNetworkItems = plist.root.CreateArray("SKAdNetworkItems");

        // Level Play/IronSource 주요 광고 네트워크의 SKAdNetwork ID 추가
        string[] skAdNetworkIds = new string[]
        {
                    // IronSource
                    "su67r6k2v3.skadnetwork",
                    // Google AdMob
                    "cstr6suwn9.skadnetwork",
                    // Meta (Facebook)
                    "v9wttpbfk9.skadnetwork",
                    "n38lu8286q.skadnetwork",
                    "c6k4g5qg8m.skadnetwork",
                    "s39g8k73mm.skadnetwork",
                    "3qy4746246.skadnetwork",
                    "f38h382jlk.skadnetwork",
                    "hs6bdukanm.skadnetwork",
                    "v72qych5uu.skadnetwork",
                    "wzmmz9fp6w.skadnetwork",
                    "yclnxrl5pm.skadnetwork",
                    "t38b2kh725.skadnetwork",
                    "7ug5zh24hu.skadnetwork",
                    "gta9lk7p23.skadnetwork",
                    "vutu7akeur.skadnetwork",
                    "y5ghdn5j9q.skadnetwork",
                    "n6fk4nfna4.skadnetwork",
                    "v79kvwwj4g.skadnetwork",
                    "2u9pt9hc89.skadnetwork",
                    "8s468mfl3y.skadnetwork",
                    "klf5c3l5u5.skadnetwork",
                    "ppxm28t8ap.skadnetwork",
                    "ecpz2srf59.skadnetwork",
                    "uw77j35x4d.skadnetwork",
                    "p78axxw29g.skadnetwork",
                    "v4nxqhlyqp.skadnetwork",
                    "w9q455wk68.skadnetwork",
                    "y45688jllp.skadnetwork",
                    "t38b2kh725.skadnetwork",
                    "prcb7njmu6.skadnetwork",
                    "v52xf8jfm7.skadnetwork",
                    "ludvb6z3bs.skadnetwork",
                    "cp8zw746q7.skadnetwork",
                    "3sh42y64q3.skadnetwork",
                    "c6k4g5qg8m.skadnetwork",
                    "kbd757ywx3.skadnetwork",
                    "9t245vhmpl.skadnetwork",
                    "eh6m2bh4zr.skadnetwork",
                    "a2p9lx4jpn.skadnetwork",
                    "22mmun2rn5.skadnetwork",
                    "4468km3ulz.skadnetwork",
                    "2u9pt9hc89.skadnetwork",
                    "8s468mfl3y.skadnetwork",
                    "klf5c3l5u5.skadnetwork",
                    "ppxm28t8ap.skadnetwork",
                    "ecpz2srf59.skadnetwork",
                    "uw77j35x4d.skadnetwork",
                    "p78axxw29g.skadnetwork",
                    "v4nxqhlyqp.skadnetwork",
                    "w9q455wk68.skadnetwork",
                    "y45688jllp.skadnetwork",
                    // Unity Ads
                    "4dzt52r2t5.skadnetwork",
                    // AppLovin
                    "ludvb6z3bs.skadnetwork",
                    "cp8zw746q7.skadnetwork",
                    "3sh42y64q3.skadnetwork",
                    "c6k4g5qg8m.skadnetwork",
                    "kbd757ywx3.skadnetwork",
                    "9t245vhmpl.skadnetwork",
                    "eh6m2bh4zr.skadnetwork",
                    "a2p9lx4jpn.skadnetwork",
                    "22mmun2rn5.skadnetwork",
                    "4468km3ulz.skadnetwork",
                    // Vungle
                    "gta9lk7p23.skadnetwork",
                    "vutu7akeur.skadnetwork",
                    "y5ghdn5j9q.skadnetwork",
                    "n6fk4nfna4.skadnetwork",
                    "v79kvwwj4g.skadnetwork",
                    // AdColony
                    "4fzdc2evr5.skadnetwork",
                    "t38b2kh725.skadnetwork",
                    // Chartboost
                    "yclnxrl5pm.skadnetwork",
                    "t38b2kh725.skadnetwork",
                    // Tapjoy
                    "wzmmz9fp6w.skadnetwork",
                    "yclnxrl5pm.skadnetwork",
                    // InMobi
                    "s39g8k73mm.skadnetwork",
                    "3qy4746246.skadnetwork",
                    // TikTok
                    "22mmun2rn5.skadnetwork",
                    // Snapchat
                    "22mmun2rn5.skadnetwork",
                    // 기타 주요 네트워크
                    "f73kdq92p3.skadnetwork",
                    "hdw39hrw9y.skadnetwork",
                    "prcb7njmu6.skadnetwork",
                    "v52xf8jfm7.skadnetwork",
                    "wzmmz9fp6w.skadnetwork",
                    "yclnxrl5pm.skadnetwork",
                    "t38b2kh725.skadnetwork",
                    "7ug5zh24hu.skadnetwork",
                    "gta9lk7p23.skadnetwork",
                    "vutu7akeur.skadnetwork",
                    "y5ghdn5j9q.skadnetwork",
                    "n6fk4nfna4.skadnetwork",
                    "v79kvwwj4g.skadnetwork",
                    "2u9pt9hc89.skadnetwork",
                    "8s468mfl3y.skadnetwork",
                    "klf5c3l5u5.skadnetwork",
                    "ppxm28t8ap.skadnetwork",
                    "ecpz2srf59.skadnetwork",
                    "uw77j35x4d.skadnetwork",
                    "p78axxw29g.skadnetwork",
                    "v4nxqhlyqp.skadnetwork",
                    "w9q455wk68.skadnetwork",
                    "y45688jllp.skadnetwork"
        };

        // 중복 제거 및 SKAdNetworkItems에 추가
        System.Collections.Generic.HashSet<string> addedIds = new System.Collections.Generic.HashSet<string>();
        foreach (string networkId in skAdNetworkIds)
        {
          if (!addedIds.Contains(networkId))
          {
            PlistElementDict networkDict = skAdNetworkItems.AddDict();
            networkDict.SetString("SKAdNetworkIdentifier", networkId);
            addedIds.Add(networkId);
          }
        }

        plist.WriteToFile(plistPath);

        Debug.Log($"[iOS PostProcess] NSUserTrackingUsageDescription이 Info.plist에 추가되었습니다: {trackingDescription}");
        Debug.Log($"[iOS PostProcess] ITSAppUsesNonExemptEncryption이 Info.plist에 설정되었습니다: {usesNonExemptEncryption}");
        Debug.Log($"[iOS PostProcess] SKAdNetworkItems가 Info.plist에 추가되었습니다. (총 {addedIds.Count}개 네트워크)");
      }
      else
      {
        Debug.LogWarning($"[iOS PostProcess] Info.plist를 찾을 수 없습니다: {plistPath}");
      }
    }
  }
}

#endif