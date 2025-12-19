using CodeStage.AntiCheat.ObscuredTypes;

public enum EDevicePrefs
{
  APP_FIRST_RUN, // 앱 최초 실행 여부

  LANGUAGE_CODE,
  LANGUAGE_CODE_CHANGED,

  // SOUND
  SOUND_MASTER_VOLUME,
  SOUND_MASTER_MUTE,

  SOUND_BGM_VOLUME,
  SOUND_BGM_MUTE,

  SOUND_EFFECT_VOLUME,
  SOUND_EFFECT_MUTE,

  ANDROID_NOTIFICATION_PERMISSION_CHECK,

  ANDROID_ANALYTICS_CONSENT, // 안드로이드 애널리틱스 데이터 수집 동의 여부

  MAX,
}

public class DevicePrefs
{
  const string FORMAT = "{0}_{1}";

  static int maxCount = -1;

  public static bool HasKey(EDevicePrefs eKey) { return ObscuredPrefs.HasKey(eKey.ToString()); }

  public static void DeleteKey(EDevicePrefs eKey)
  {
    ObscuredPrefs.DeleteKey(eKey.ToString());
  }

  public static void DeleteAll()
  {
    if (maxCount == -1)
    {
      maxCount = (int)EDevicePrefs.MAX;
    }

    for (int i = 0; i < maxCount; i++)
    {
      DeleteKey((EDevicePrefs)i);
    }
  }

  #region Bool
  public static bool GetBool(EDevicePrefs eKey)
  {
    return ObscuredPrefs.GetBool(eKey.ToString());
  }
  public static bool GetBool(EDevicePrefs eKey, bool defaultValue)
  {
    return ObscuredPrefs.GetBool(eKey.ToString(), defaultValue);
  }

  public static void SetBool(EDevicePrefs eKey, bool value)
  {
    ObscuredPrefs.SetBool(eKey.ToString(), value);
  }
  #endregion

  #region String
  public static string GetString(EDevicePrefs eKey)
  {
    return ObscuredPrefs.GetString(eKey.ToString());
  }
  public static string GetString(EDevicePrefs eKey, string defaultValue)
  {
    return ObscuredPrefs.GetString(eKey.ToString(), defaultValue);
  }

  public static void SetString(EDevicePrefs eKey, string value)
  {
    ObscuredPrefs.SetString(eKey.ToString(), value);
  }
  #endregion

  #region Int
  public static int GetInt(EDevicePrefs eKey)
  {
    return ObscuredPrefs.GetInt(eKey.ToString());
  }
  public static int GetInt(EDevicePrefs eKey, int defaultValue)
  {
    return ObscuredPrefs.GetInt(eKey.ToString(), defaultValue);
  }

  public static void SetInt(EDevicePrefs eKey, int value)
  {
    ObscuredPrefs.SetInt(eKey.ToString(), value);
  }
  #endregion

  #region UInt
  public static uint GetUInt(EDevicePrefs eKey)
  {
    return ObscuredPrefs.GetUInt(eKey.ToString());
  }
  public static uint GetUInt(EDevicePrefs eKey, uint defaultValue)
  {
    return ObscuredPrefs.GetUInt(eKey.ToString(), defaultValue);
  }

  public static void SetUInt(EDevicePrefs eKey, uint value)
  {
    ObscuredPrefs.SetUInt(eKey.ToString(), value);
  }
  #endregion

  #region Float
  public static float GetFloat(EDevicePrefs eKey)
  {
    return ObscuredPrefs.GetFloat(eKey.ToString());
  }
  public static float GetFloat(EDevicePrefs eKey, float defaultValue)
  {
    return ObscuredPrefs.GetFloat(eKey.ToString(), defaultValue);
  }

  public static void SetFloat(EDevicePrefs eKey, float value)
  {
    ObscuredPrefs.SetFloat(eKey.ToString(), value);
  }
  #endregion

  #region Double
  public static double GetDouble(EDevicePrefs eKey)
  {
    return ObscuredPrefs.GetDouble(eKey.ToString());
  }
  public static double GetDouble(EDevicePrefs eKey, double defaultValue)
  {
    return ObscuredPrefs.GetDouble(eKey.ToString(), defaultValue);
  }

  public static void SetDouble(EDevicePrefs eKey, double value)
  {
    ObscuredPrefs.SetDouble(eKey.ToString(), value);
  }
  #endregion

  #region Demical
  public static decimal GetDecimal(EDevicePrefs eKey)
  {
    return ObscuredPrefs.GetDecimal(eKey.ToString());
  }
  public static decimal GetDecimal(EDevicePrefs eKey, decimal defaultValue)
  {
    return ObscuredPrefs.GetDecimal(eKey.ToString(), defaultValue);
  }

  public static void SetDecimal(EDevicePrefs eKey, decimal value)
  {
    ObscuredPrefs.SetDecimal(eKey.ToString(), value);
  }
  #endregion

  #region ULong
  public static ulong GetULong(EDevicePrefs eKey)
  {
    return ObscuredPrefs.GetULong(eKey.ToString());
  }
  public static ulong GetULong(EDevicePrefs eKey, ulong defaultValue)
  {
    return ObscuredPrefs.GetULong(eKey.ToString(), defaultValue);
  }

  public static void SetULong(EDevicePrefs eKey, ulong value)
  {
    ObscuredPrefs.SetULong(eKey.ToString(), value);
  }
  #endregion

  #region Long
  public static long GetLong(EDevicePrefs eKey)
  {
    return ObscuredPrefs.GetLong(eKey.ToString());
  }
  public static long GetLong(EDevicePrefs eKey, long defaultValue)
  {
    return ObscuredPrefs.GetLong(eKey.ToString(), defaultValue);
  }

  public static void SetLong(EDevicePrefs eKey, long value)
  {
    ObscuredPrefs.SetLong(eKey.ToString(), value);
  }
  #endregion
}