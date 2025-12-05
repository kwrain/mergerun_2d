using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace FAIRSTUDIOS.Manager
{
    /// <summary>
    /// iOS App Tracking Transparency (ATT) 브릿지
    /// iOS 14.5+ 광고 추적 권한 요청을 처리합니다.
    /// </summary>
    public class AppTrackingTransparencyBridge : MonoBehaviour
    {
        public enum TrackingAuthorizationStatus
        {
            NotDetermined = 0,  // 아직 요청하지 않음
            Restricted = 1,     // 제한됨 (부모 제어 등)
            Denied = 2,         // 거부됨
            Authorized = 3      // 허용됨
        }

        private static AppTrackingTransparencyBridge instance;
        private Action<TrackingAuthorizationStatus> onAuthorizationResult;

        public static AppTrackingTransparencyBridge Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("AppTrackingTransparencyBridge");
                    instance = go.AddComponent<AppTrackingTransparencyBridge>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int GetTrackingAuthorizationStatus();

        [DllImport("__Internal")]
        private static extern void RequestTrackingAuthorization();

        [DllImport("__Internal")]
        private static extern IntPtr GetIDFANative();

        [DllImport("__Internal")]
        private static extern void FreeIDFANative(IntPtr ptr);
#else
        // 에디터나 다른 플랫폼에서는 빈 구현
        private static int GetTrackingAuthorizationStatus() { return 3; }
        private static void RequestTrackingAuthorization() { }
        private static IntPtr GetIDFANative() { return IntPtr.Zero; }
        private static void FreeIDFANative(IntPtr ptr) { }
#endif

        /// <summary>
        /// 현재 추적 권한 상태를 가져옵니다.
        /// </summary>
        public TrackingAuthorizationStatus GetStatus()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return (TrackingAuthorizationStatus)GetTrackingAuthorizationStatus();
#else
            return TrackingAuthorizationStatus.Authorized; // 에디터나 다른 플랫폼은 항상 허용
#endif
        }

        /// <summary>
        /// 추적 권한을 요청합니다.
        /// </summary>
        /// <param name="onResult">권한 요청 결과 콜백</param>
        public void RequestAuthorization(Action<TrackingAuthorizationStatus> onResult = null)
        {
#if UNITY_IOS && !UNITY_EDITOR
            onAuthorizationResult = onResult;
            RequestTrackingAuthorization();
#else
            Debug.Log("[ATT] Editor 또는 iOS가 아닌 플랫폼에서는 권한 요청을 건너뜁니다.");
            onResult?.Invoke(TrackingAuthorizationStatus.Authorized);
#endif
        }

        /// <summary>
        /// 네이티브에서 호출되는 콜백 (UnitySendMessage로 호출됨)
        /// </summary>
        private void OnTrackingAuthorizationResult(string statusString)
        {
            if (int.TryParse(statusString, out int status))
            {
                TrackingAuthorizationStatus authStatus = (TrackingAuthorizationStatus)status;
                Debug.Log($"[ATT] 추적 권한 요청 결과: {authStatus}");
                
                onAuthorizationResult?.Invoke(authStatus);
                onAuthorizationResult = null;
            }
        }

        /// <summary>
        /// IDFA (광고 식별자)를 가져옵니다. (권한이 허용된 경우에만)
        /// </summary>
        public string GetIDFA()
        {
#if UNITY_IOS && !UNITY_EDITOR
            IntPtr idfaPtr = GetIDFANative();
            if (idfaPtr != IntPtr.Zero)
            {
                string idfa = Marshal.PtrToStringAnsi(idfaPtr);
                FreeIDFANative(idfaPtr);
                return idfa ?? "";
            }
            return "";
#else
            return ""; // 에디터나 다른 플랫폼에서는 빈 문자열 반환
#endif
        }
    }
}

