using System;
using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;

/**
* BuiltinLocalizeAssetCollection.cs
* 작성자 : doseon@fairsutdios.kr (LEE DO SEON)
* 작성일 : 2023년 11월 21일 오후 3시 03분
*/

[CreateAssetMenu(fileName = "LocalizeTextCollection", menuName = "Data/Localize/LocalizeTextCollection")]
public class LocalizeTextDataCollection : ScriptableObject
{
  [SerializeField] private GenericDictionary<ELanguageCode, LocalizeTextData> tables;

  [NonSerialized] private ELanguageCode languageCode = ELanguageCode.None;

  private List<ELanguageCode> supportLanguages = new List<ELanguageCode>();

  public ELanguageCode ELanguageCode
  {
    set
    {
      if(languageCode == value)
        return;

      languageCode = value;
    }
    get
    {
      if(languageCode == ELanguageCode.None)
      {
        languageCode = DevicePrefs.GetString(EDevicePrefs.LANGUAGE_CODE, "KO").ToEnum<ELanguageCode>();
      }
      return languageCode;
    }
  }

  public bool IsSupportLanguage(ELanguageCode eLanguageCode)
  {
    return supportLanguages.Contains(eLanguageCode);
  }

  public bool ContainsKey(string key)
  {
    if (ELanguageCode == ELanguageCode.None)
      return false;

    if (tables.TryGetValue(ELanguageCode, out var table) == false)
      return false;

    return table.ContainsKey(key);
  }

  public string GetValue(string key)
  {
    if (ELanguageCode == ELanguageCode.None)
      return key;
    if (tables.TryGetValue(ELanguageCode, out var table) == false)
      return key;

    return table.GetValue(key).Replace("\\n", "\n");
  }

  public string GetValueFormat(string key, params object[] args)
  {
    if (args != null)
      return string.Format(GetValue(key), args);
    else
      return GetValue(key);
  }
}