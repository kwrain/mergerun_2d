using UnityEngine;
using System.Runtime.InteropServices;

/**
* GameManager.Vibration.cs
* 진동 관리 기능
* 게임 내 진동 기능을 관리합니다.
*/
public partial class GameManager
{
  #region Vibration

#if UNITY_IOS && !UNITY_EDITOR
  // iOS CoreHaptics 네이티브 함수 선언
  [DllImport("__Internal")]
  private static extern void InitializeCoreHaptics();

  [DllImport("__Internal")]
  private static extern bool StartCoreHapticsEngine();

  [DllImport("__Internal")]
  private static extern void VibrateWithDuration(float duration);

  [DllImport("__Internal")]
  private static extern void VibrateWithIntensityAndDuration(float intensity, float duration);

  [DllImport("__Internal")]
  private static extern void VibrateWithPattern(long[] pattern, int patternLength);

  [DllImport("__Internal")]
  private static extern void StopCoreHapticsEngine();

  private static bool s_iOSHapticsInitialized = false;
#endif

  /// <summary>
  /// 진동이 활성화되어 있는지 확인합니다.
  /// </summary>
  public bool IsVibrationEnabled
  {
    get
    {
      if (SOManager.Instance == null || SOManager.Instance.PlayerPrefsModel == null)
        return true; // 기본값은 활성화

      return SOManager.Instance.PlayerPrefsModel.VibrationEnabled;
    }
    set
    {
      if (SOManager.Instance == null || SOManager.Instance.PlayerPrefsModel == null)
        return;

      SOManager.Instance.PlayerPrefsModel.VibrationEnabled = value;
    }
  }

  /// <summary>
  /// 진동을 발생시킵니다.
  /// 설정이 활성화되어 있을 때만 동작합니다.
  /// </summary>
  /// <param name="duration">진동 지속 시간 (밀리초). 기본값은 100ms입니다.</param>
  public void Vibrate(int duration = 100)
  {
    if (!IsVibrationEnabled)
    {
#if UNITY_EDITOR
      Debug.Log($"[GameManager] 진동 비활성화 상태로 인해 진동을 발생시키지 않습니다.");
#endif
      return;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // Android에서 Vibrator 서비스를 직접 사용하여 duration 지원
    try
    {
      using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
      {
        using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
          if (currentActivity == null)
          {
#if UNITY_EDITOR
            Debug.LogError("[GameManager] currentActivity가 null입니다.");
#endif
            return;
          }

          // Android API 레벨 확인
          int sdkVersion = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
#if UNITY_EDITOR
          Debug.Log($"[GameManager] Android SDK 버전: {sdkVersion}, 진동 지속시간: {duration}ms");
#endif

          AndroidJavaObject vibratorService = null;

          // Android 13 (API 33) 이상에서는 VibratorManager 사용
          if (sdkVersion >= 33)
          {
            try
            {
              var vibratorManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator_manager");
              if (vibratorManager != null)
              {
                vibratorService = vibratorManager.Call<AndroidJavaObject>("getDefaultVibrator");
#if UNITY_EDITOR
                Debug.Log("[GameManager] VibratorManager를 통해 Vibrator를 가져왔습니다.");
#endif
              }
            }
            catch (System.Exception e)
            {
#if UNITY_EDITOR
              Debug.LogWarning($"[GameManager] VibratorManager 사용 실패, 기본 방법 시도: {e.Message}");
#endif
            }
          }

          // VibratorManager가 실패했거나 API 33 미만인 경우 기본 방법 사용
          if (vibratorService == null)
          {
            vibratorService = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
          }

          if (vibratorService == null)
          {
#if UNITY_EDITOR
            Debug.LogError("[GameManager] Vibrator 서비스를 가져올 수 없습니다.");
#endif
            return;
          }

          // 진동 가능 여부 확인
          bool hasVibrator = vibratorService.Call<bool>("hasVibrator");
          if (!hasVibrator)
          {
#if UNITY_EDITOR
            Debug.LogWarning("[GameManager] 디바이스가 진동을 지원하지 않습니다.");
#endif
            return;
          }

          // duration 최소값 보장 (Android는 최소 1ms 이상 필요, 하지만 실제로는 50ms 이상이 감지 가능)
          // 너무 짧은 duration은 사용자가 느끼기 어려우므로 최소 50ms로 제한
          long vibrationDuration = System.Math.Max(50, duration);
          if (duration < 50)
          {
#if UNITY_EDITOR
            Debug.LogWarning($"[GameManager] duration이 너무 짧습니다 ({duration}ms). 최소 50ms로 조정합니다.");
#endif
          }
          
          // Android API 26 이상에서는 VibrationEffect 사용
          if (sdkVersion >= 26)
          {
            using (var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
            {
              // DEFAULT_AMPLITUDE는 -1이며, 시스템 기본 강도 사용
              var defaultAmplitude = vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE");
              
              // createOneShot은 밀리초 단위를 받습니다
              var vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", vibrationDuration, defaultAmplitude);
              
              if (vibrationEffect != null)
              {
                // VibrationEffect를 사용하여 진동 발생
                // Android 13 이상에서도 VibrationEffect만으로 작동 가능
                vibratorService.Call("vibrate", vibrationEffect);
#if UNITY_EDITOR
                Debug.Log($"[GameManager] 진동 발생 성공 (VibrationEffect 사용, duration: {vibrationDuration}ms, amplitude: {defaultAmplitude})");
#endif
              }
              else
              {
#if UNITY_EDITOR
                Debug.LogError("[GameManager] VibrationEffect 생성 실패");
#endif
                // 대체 방법: 직접 duration 전달 시도
                vibratorService.Call("vibrate", vibrationDuration);
#if UNITY_EDITOR
                Debug.Log($"[GameManager] 대체 방법으로 진동 발생 (직접 duration 전달, {vibrationDuration}ms)");
#endif
              }
            }
          }
          else
          {
            // Android API 26 미만에서는 직접 duration 전달 (밀리초 단위)
            vibratorService.Call("vibrate", vibrationDuration);
#if UNITY_EDITOR
            Debug.Log($"[GameManager] 진동 발생 성공 (직접 duration 전달, {vibrationDuration}ms)");
#endif
          }
        }
      }
    }
    catch (System.Exception e)
    {
#if UNITY_EDITOR
      Debug.LogError($"[GameManager] 안드로이드 진동 발생 실패: {e.Message}\n스택 트레이스: {e.StackTrace}");
#endif
      // 실패 시 기본 진동 시도
      try
      {
        Handheld.Vibrate();
#if UNITY_EDITOR
        Debug.Log("[GameManager] Handheld.Vibrate()로 대체 진동 시도");
#endif
      }
      catch (System.Exception fallbackException)
      {
#if UNITY_EDITOR
        Debug.LogError($"[GameManager] 대체 진동도 실패: {fallbackException.Message}");
#endif
      }
    }
#elif UNITY_IOS && !UNITY_EDITOR
    // iOS에서 CoreHaptics 사용
    if (!s_iOSHapticsInitialized)
    {
      InitializeCoreHaptics();
      s_iOSHapticsInitialized = true;
    }
    VibrateWithIntensityAndDuration(1f, duration);
#elif UNITY_EDITOR
    // 에디터에서는 로그만 출력
    Debug.Log($"[GameManager] 진동 발생 (설정: 활성화, 지속시간: {duration}ms)");
#else
    // 기타 플랫폼
    Handheld.Vibrate();
#endif
  }

  /// <summary>
  /// 짧은 진동을 발생시킵니다. (50ms)
  /// </summary>
  public void VibrateShort()
  {
    Vibrate(50); // 최소 50ms로 변경 (10ms는 너무 짧아 감지하기 어려움)
  }

  /// <summary>
  /// 중간 길이의 진동을 발생시킵니다. (100ms)
  /// </summary>
  public void VibrateMedium()
  {
    Vibrate(100);
  }

  /// <summary>
  /// 긴 진동을 발생시킵니다. (200ms)
  /// </summary>
  public void VibrateLong()
  {
    Vibrate(200);
  }

  /// <summary>
  /// 패턴 진동을 발생시킵니다.
  /// </summary>
  /// <param name="pattern">진동 패턴 배열 (진동 시간, 대기 시간, 진동 시간, 대기 시간...)</param>
  public void VibratePattern(long[] pattern)
  {
    if (!IsVibrationEnabled)
      return;

    if (pattern == null || pattern.Length == 0)
      return;

#if UNITY_ANDROID && !UNITY_EDITOR
    // Android에서 패턴 진동 지원
    using (var vibrator = new AndroidJavaClass("android.os.Vibrator"))
    {
      using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
      {
        using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
          var vibratorService = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
          vibratorService.Call("vibrate", pattern, -1);
        }
      }
    }
#elif UNITY_IOS && !UNITY_EDITOR
    // iOS에서 CoreHaptics를 사용한 패턴 진동 지원
    if (!s_iOSHapticsInitialized)
    {
      InitializeCoreHaptics();
      s_iOSHapticsInitialized = true;
    }
    VibrateWithPattern(pattern, pattern.Length);
#elif UNITY_EDITOR
    Debug.Log($"[GameManager] 패턴 진동 발생: {string.Join(", ", pattern)}");
#else
    // 기타 플랫폼
    Handheld.Vibrate();
#endif
  }

  #endregion
}

