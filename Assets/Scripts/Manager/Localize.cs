using UnityEngine;

public class Localize : Singleton<Localize>
{
  public enum ELetterType
  {
    Normal,
    Lower,
    Upper
  }

#if LOCALIZATION_CHECK
  public const string LOCALIZATION_CHECK_STRING = "<color=red>#</color>";
#endif 

  public delegate void ChangedLaguageCode();
  public static event ChangedLaguageCode OnChangedLanguageCode;

  private static LocalizeTextDataCollection localizeAssetCollection;

  public static ELanguageCode ELanguageCode
  {
    get { return LanguageCode.ToEnum<ELanguageCode>(); }
    set
    {
      if(ELanguageCode == value)
        return;

      LanguageCode = value.ToString();
    }
  }

  public static string LanguageCode
  {
    get { return DevicePrefs.GetString(EDevicePrefs.LANGUAGE_CODE, "EN"); }
    set
    {
      if (LanguageCode == value)
        return;

      // 지원하지 않는 언어일 경우 ?
      DevicePrefs.SetString(EDevicePrefs.LANGUAGE_CODE, value);
      DevicePrefs.SetBool(EDevicePrefs.LANGUAGE_CODE_CHANGED, true);

      // 언어 변경 시 다시 로드
      OnChangedLanguageCode?.Invoke();
    }
  }

  protected override void Awake()
  {
    base.Awake();

    ELanguageCode = localizeAssetCollection.ELanguageCode;

    if (Application.isPlaying)
    {
      localizeAssetCollection = SOManager.Instance.LocalizeTextAssetCollection;
    }
  }

  public static bool IsSupportLanguage(ELanguageCode eLanguageCode) { return localizeAssetCollection.IsSupportLanguage(eLanguageCode); }

  public static bool ContainsKey(string key)
  {
    if (localizeAssetCollection == null)
    {
      if (Application.isPlaying)
      {
        localizeAssetCollection = SOManager.Instance.LocalizeTextAssetCollection;
      }
      else
      {
#if UNITY_EDITOR
        localizeAssetCollection = GlobalDataAccessor.Instance.LocalizeTextAssetCollection;
#endif
      }
    }

    return localizeAssetCollection.ContainsKey(key);
  }

  public static string GetValue(string key, bool bShowError = false)
  {
    if (string.IsNullOrEmpty(key))
    {
      return string.Empty;
    }

    if (localizeAssetCollection == null)
    {
      if (Application.isPlaying)
      {
        localizeAssetCollection = SOManager.Instance.LocalizeTextAssetCollection;
      }
      else
      {
#if UNITY_EDITOR
        localizeAssetCollection = GlobalDataAccessor.Instance.LocalizeTextAssetCollection;
#endif
      }
      return key;
    }

    if (localizeAssetCollection.ContainsKey(key))
    {
      // lds - 안드로이드에서 value가 null인 경우 NullException 발생하므로 예외처리
      // lds - 존재하지않는 localizationID라면 해당 localizationID가 반환
      var value = localizeAssetCollection.GetValue(key);
      if (string.IsNullOrEmpty(value))
        return key;

#if LOCALIZATION_CHECK
      return LOCALIZATION_CHECK_STRING + dtLocalization[localizationID].Replace("\\n", "\n");
#else
      return value;
#endif
    }
    else
    {
#if LOCALIZATION_CHECK
      return LOCALIZATION_CHECK_STRING + localizationID;
#else
      return key;
#endif
    }
  }

  /// <summary>
  /// 번역 된 값에 데이터 포맷 적용
  /// </summary>
  /// <param name="localizationID"></param>
  /// <param name="args"></param>
  /// <returns></returns>
  public static string GetValueFormat(string localizationID, params object[] args)
  {
    if (args != null)
    {
      return string.Format(GetValue(localizationID), args);
    }
    else
      return GetValue(localizationID);
  }
}
