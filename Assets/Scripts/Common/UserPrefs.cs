using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;

public enum EUserPrefs
{
  ID,
  CERT_KEY,

	MAX
}

public class UserPrefs
{
  const string ACHIEVEEMENT_FORMAT = "{0}_{1}";

  static Dictionary<string, int> dtAchievemenComplete;

  static int maxCount = -1;

  public static bool HasKey(EUserPrefs eKey) { return ObscuredPrefs.HasKey(eKey.ToString()); }

  public static void DeleteKey(EUserPrefs eKey)
  {
    ObscuredPrefs.DeleteKey(eKey.ToString());
  }

  public static void DeleteAll()
  {
    if(maxCount == -1)
    {
      maxCount = (int)EUserPrefs.MAX;
    }

    for (int i = 0; i < maxCount; i++)
    {
      DeleteKey((EUserPrefs)i);
    }
  }

  #region Bool
  public static bool GetBool(EUserPrefs eKey)
  {
    return ObscuredPrefs.GetBool(eKey.ToString());
  }
  public static bool GetBool(EUserPrefs eKey, bool defaultValue)
  {
    return ObscuredPrefs.GetBool(eKey.ToString(), defaultValue);
  }

  public static void SetBool(EUserPrefs eKey, bool value)
  {
    ObscuredPrefs.SetBool(eKey.ToString(), value);
  }
  #endregion

  #region String
  public static string GetString(EUserPrefs eKey)
  {
    return ObscuredPrefs.GetString(eKey.ToString());
  }
  public static string GetString(EUserPrefs eKey, string defaultValue)
  {
    return ObscuredPrefs.GetString(eKey.ToString(), defaultValue);
  }

  public static void SetString(EUserPrefs eKey, string value)
  {
    ObscuredPrefs.SetString(eKey.ToString(), value);
  }
  #endregion

  #region Int
  public static int GetInt(EUserPrefs eKey)
  {
    return ObscuredPrefs.GetInt(eKey.ToString());
  }
  public static int GetInt(EUserPrefs eKey, int defaultValue)
  {
    return ObscuredPrefs.GetInt(eKey.ToString(), defaultValue);
  }

  public static void SetInt(EUserPrefs eKey, int value)
  {
    ObscuredPrefs.SetInt(eKey.ToString(), value);
  }
  #endregion

  #region UInt
  public static uint GetUInt(EUserPrefs eKey)
  {
    return ObscuredPrefs.GetUInt(eKey.ToString());
  }
  public static uint GetUInt(EUserPrefs eKey, uint defaultValue)
  {
    return ObscuredPrefs.GetUInt(eKey.ToString(), defaultValue);
  }

  public static void SetUInt(EUserPrefs eKey, uint value)
  {
    ObscuredPrefs.SetUInt(eKey.ToString(), value);
  }
  #endregion

  #region Float
  public static float GetFloat(EUserPrefs eKey)
  {
    return ObscuredPrefs.GetFloat(eKey.ToString());
  }
  public static float GetFloat(EUserPrefs eKey, float defaultValue)
  {
    return ObscuredPrefs.GetFloat(eKey.ToString(), defaultValue);
  }

  public static void SetFloat(EUserPrefs eKey, float value)
  {
    ObscuredPrefs.SetFloat(eKey.ToString(), value);
  }
  #endregion

  #region Double
  public static double GetDouble(EUserPrefs eKey)
  {
    return ObscuredPrefs.GetDouble(eKey.ToString());
  }
  public static double GetDouble(EUserPrefs eKey, double defaultValue)
  {
    return ObscuredPrefs.GetDouble(eKey.ToString(), defaultValue);
  }

  public static void SetDouble(EUserPrefs eKey, double value)
  {
    ObscuredPrefs.SetDouble(eKey.ToString(), value);
  }
  #endregion

  #region Demical
  public static decimal GetDecimal(EUserPrefs eKey)
  {
    return ObscuredPrefs.GetDecimal(eKey.ToString());
  }
  public static decimal GetDecimal(EUserPrefs eKey, decimal defaultValue)
  {
    return ObscuredPrefs.GetDecimal(eKey.ToString(), defaultValue);
  }

  public static void SetDecimal(EUserPrefs eKey, decimal value)
  {
    ObscuredPrefs.SetDecimal(eKey.ToString(), value);
  }
  #endregion

  #region ULong
  public static ulong GetULong(EUserPrefs eKey)
  {
    return ObscuredPrefs.GetULong(eKey.ToString());
  }
  public static ulong GetULong(EUserPrefs eKey, ulong defaultValue)
  {
    return ObscuredPrefs.GetULong(eKey.ToString(), defaultValue);
  }

  public static void SetULong(EUserPrefs eKey, ulong value)
  {
    ObscuredPrefs.SetULong(eKey.ToString(), value);
  }
  #endregion

  #region Long
  public static long GetLong(EUserPrefs eKey)
  {
    return ObscuredPrefs.GetLong(eKey.ToString());
  }
  public static long GetLong(EUserPrefs eKey, long defaultValue)
  {
    return ObscuredPrefs.GetLong(eKey.ToString(), defaultValue);
  }

  public static void SetLong(EUserPrefs eKey, long value)
  {
    ObscuredPrefs.SetLong(eKey.ToString(), value);
  }
  #endregion
}
