using UnityEngine;
using System;

namespace FAIRSTUDIOS.Manager
{
    /// <summary>
    /// 안드로이드 네이티브 다이얼로그 매니저
    /// 안드로이드 시스템 AlertDialog를 표시합니다.
    /// </summary>
    public class AndroidDialogManager : MonoBehaviour
    {
        private static AndroidDialogManager instance;
        
        // 현재 대기 중인 콜백 저장
        private Action pendingPositiveCallback;
        private Action pendingNegativeCallback;

        public static AndroidDialogManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("AndroidDialogManager");
                    instance = go.AddComponent<AndroidDialogManager>();
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

        /// <summary>
        /// 안드로이드 시스템 2버튼 다이얼로그를 표시합니다.
        /// </summary>
        /// <param name="title">다이얼로그 제목</param>
        /// <param name="message">다이얼로그 메시지</param>
        /// <param name="positiveButtonText">긍정 버튼 텍스트 (예: "확인", "예")</param>
        /// <param name="negativeButtonText">부정 버튼 텍스트 (예: "취소", "아니오")</param>
        /// <param name="onPositiveClick">긍정 버튼 클릭 시 콜백</param>
        /// <param name="onNegativeClick">부정 버튼 클릭 시 콜백</param>
        public void ShowDialog(
            string title,
            string message,
            string positiveButtonText = "확인",
            string negativeButtonText = "취소",
            Action onPositiveClick = null,
            Action onNegativeClick = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ShowAndroidDialog(title, message, positiveButtonText, negativeButtonText, onPositiveClick, onNegativeClick);
#else
            // 에디터나 다른 플랫폼에서는 로그만 출력하고 콜백 호출
            Debug.Log($"[AndroidDialog] {title}: {message}");
            Debug.Log($"[AndroidDialog] 긍정: {positiveButtonText}, 부정: {negativeButtonText}");
            
            // 에디터에서는 테스트를 위해 긍정 버튼 콜백을 자동 호출
            // 실제 안드로이드 빌드에서는 사용자 선택에 따라 호출됩니다.
            onPositiveClick?.Invoke();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void ShowAndroidDialog(
            string title,
            string message,
            string positiveButtonText,
            string negativeButtonText,
            Action onPositiveClick,
            Action onNegativeClick)
        {
            try
            {
                // 콜백을 인스턴스 변수에 저장 (가비지 컬렉션 방지)
                pendingPositiveCallback = onPositiveClick;
                pendingNegativeCallback = onNegativeClick;
                
                // 현재 Activity 가져오기
                AndroidJavaObject currentActivity = null;
                try
                {
                    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    }
                    
                    if (currentActivity == null)
                    {
                        Debug.LogError("[AndroidDialog] currentActivity is null.");
                        ClearCallbacks();
                        return;
                    }

                    // UI 스레드에서 실행되도록 RunOnUiThread 사용
                    // AndroidJavaRunnable 내부에서 새로운 참조를 가져와야 함
                    currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        AndroidJavaObject activityRef = null;
                        try
                        {
                            // UI 스레드 내에서 새로운 참조 가져오기
                            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                            {
                                activityRef = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                            }
                            
                            if (activityRef == null)
                            {
                                Debug.LogError("[AndroidDialog] currentActivity is null in UI thread.");
                                ClearCallbacks();
                                return;
                            }
                            
                            using (var alertDialogBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", activityRef))
                            {
                                // 제목 설정
                                alertDialogBuilder.Call<AndroidJavaObject>("setTitle", title);
                                
                                // 메시지 설정
                                alertDialogBuilder.Call<AndroidJavaObject>("setMessage", message);
                                
                                // 긍정 버튼 설정
                                if (!string.IsNullOrEmpty(positiveButtonText))
                                {
                                    alertDialogBuilder.Call<AndroidJavaObject>("setPositiveButton", positiveButtonText, new AndroidDialogOnClickListener(this, true));
                                }
                                
                                // 부정 버튼 설정
                                if (!string.IsNullOrEmpty(negativeButtonText))
                                {
                                    alertDialogBuilder.Call<AndroidJavaObject>("setNegativeButton", negativeButtonText, new AndroidDialogOnClickListener(this, false));
                                }
                                
                                // 다이얼로그 표시
                                using (var dialog = alertDialogBuilder.Call<AndroidJavaObject>("create"))
                                {
                                    if (dialog != null)
                                    {
                                        dialog.Call("show");
                                    }
                                    else
                                    {
                                        Debug.LogError("[AndroidDialog] Failed to create dialog.");
                                        ClearCallbacks();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[AndroidDialog] Error occurred while creating dialog: {e.Message}\nStackTrace: {e.StackTrace}");
                            ClearCallbacks();
                        }
                        finally
                        {
                            if (activityRef != null)
                            {
                                activityRef.Dispose();
                            }
                        }
                    }));
                }
                finally
                {
                    if (currentActivity != null)
                    {
                        currentActivity.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AndroidDialog] Failed to show Android dialog: {e.Message}\nStackTrace: {e.StackTrace}");
                ClearCallbacks();
            }
        }

        /// <summary>
        /// 콜백 초기화
        /// </summary>
        private void ClearCallbacks()
        {
            pendingPositiveCallback = null;
            pendingNegativeCallback = null;
        }

        /// <summary>
        /// 긍정 버튼 클릭 콜백 (Unity 메인 스레드에서 호출)
        /// </summary>
        private void OnPositiveButtonClicked()
        {
            if (pendingPositiveCallback != null)
            {
                var callback = pendingPositiveCallback;
                ClearCallbacks();
                callback.Invoke();
            }
        }

        /// <summary>
        /// 부정 버튼 클릭 콜백 (Unity 메인 스레드에서 호출)
        /// </summary>
        private void OnNegativeButtonClicked()
        {
            if (pendingNegativeCallback != null)
            {
                var callback = pendingNegativeCallback;
                ClearCallbacks();
                callback.Invoke();
            }
        }

        /// <summary>
        /// 안드로이드 DialogInterface.OnClickListener를 구현하는 클래스
        /// </summary>
        private class AndroidDialogOnClickListener : AndroidJavaProxy
        {
            private AndroidDialogManager manager;
            private bool isPositive;

            public AndroidDialogOnClickListener(AndroidDialogManager mgr, bool positive) : base("android.content.DialogInterface$OnClickListener")
            {
                manager = mgr;
                isPositive = positive;
            }

            public void onClick(AndroidJavaObject dialog, int which)
            {
                // Unity 메인 스레드에서 콜백 실행
                if (manager != null)
                {
                    if (isPositive)
                    {
                        manager.OnPositiveButtonClicked();
                    }
                    else
                    {
                        manager.OnNegativeButtonClicked();
                    }
                }
            }
        }
#endif

        /// <summary>
        /// 간단한 확인 다이얼로그 (1버튼)
        /// </summary>
        public void ShowAlert(string title, string message, Action onOkClick = null)
        {
            ShowDialog(title, message, "확인", null, onOkClick, null);
        }
    }
}

