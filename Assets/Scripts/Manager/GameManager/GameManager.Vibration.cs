using UnityEngine;

/**
* GameManager.Vibration.cs
* 진동 관리 기능
* 게임 내 진동 기능을 관리합니다.
*/
public partial class GameManager
{
  #region Vibration

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
      return;

#if UNITY_ANDROID && !UNITY_EDITOR
    // Android에서 진동
    Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
    // iOS에서 진동 (iOS는 duration 파라미터를 지원하지 않음)
    Handheld.Vibrate();
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
    Vibrate(50);
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
    // iOS는 패턴 진동을 지원하지 않으므로 기본 진동
    Handheld.Vibrate();
#elif UNITY_EDITOR
    Debug.Log($"[GameManager] 패턴 진동 발생: {string.Join(", ", pattern)}");
#else
    Handheld.Vibrate();
#endif
  }

  #endregion
}

