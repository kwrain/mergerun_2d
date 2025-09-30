using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using UnityEngine;
using UnityEngine.UI;

/**
* VmTileImageSetter.cs
* 작성자 : dev@fairstudios.kr
* 작성일 : 2025년 03월 14일 오전 9시 55분
*/

[AddComponentMenu("Vm/[Vm]SliderSetter")]
[DisallowMultipleComponent]
[SupportedProperty
  (
    typeof(PropertyInteger), typeof(PropertyListInteger), typeof(PropertyMapIntegerInteger), typeof(PropertyMapStringInteger), typeof(PropertyPlayerPrefsInteger),
    typeof(PropertyUInteger), typeof(PropertyListUInteger), typeof(PropertyMapStringUInteger),
    typeof(PropertyFloat), typeof(PropertyListFloat), typeof(PropertyMapStringFloat),
    typeof(PropertyLong), typeof(PropertyListLong), typeof(PropertyMapStringLong),
    typeof(PropertyString), typeof(PropertyListString), typeof(PropertyMapStringString), typeof(PropertyPlayerPrefsString),
    typeof(PropertyContextClass), typeof(int), typeof(long), typeof(bool), typeof(string)
  )
]
public class VmSliderSetter : VmBase<Slider, VmSliderSetter.Param>
{
  public override void UpdateViewActivate()
  {
    if (pInfos.Length == 0)
      return;

    var value = 0f;
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      value = GetValue(pInfo.Property, pInfo.Index, pInfo.StringKey);
    }

    view.value = value;
  }

  public override void UpdateView(string context)
  {
    if (pInfos.Length == 0)
      return;

    var value = 0f;
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      if (pInfo.Context != context) continue;
      value = GetValue(pInfo.Property, pInfo.Index, pInfo.StringKey);
    }

    view.value = value;
  }

  private int GetInt(PropertyBase property, int index, string key) => property switch
  {
    PropertyInteger pInt => pInt.NewValue,
    PropertyListInteger pListInteger => pListInteger[index],
    PropertyMapIntegerInteger pMapIntegerInteger => pMapIntegerInteger[index],
    PropertyMapStringInteger pMapStringInteger => pMapStringInteger[key],
    _ => 0,
  };

  private uint GetUInt(PropertyBase property, int index, string key) => property switch
  {
    PropertyUInteger pInt => pInt.NewValue,
    PropertyListUInteger pListInteger => pListInteger[index],
    PropertyMapStringUInteger pMapStringInteger => pMapStringInteger[key],
    _ => 0,
  };

  private float GetFloat(PropertyBase property, int index, string key) => property switch
  {
    PropertyFloat pInt => pInt.NewValue,
    PropertyListFloat pListInteger => pListInteger[index],
    PropertyMapStringFloat pMapStringInteger => pMapStringInteger[key],
    _ => 0,
  };

  private long GetLong(PropertyBase property, int index, string key) => property switch
  {
    PropertyLong pInt => pInt.NewValue,
    PropertyListLong pListInteger => pListInteger[index],
    PropertyMapStringLong pMapStringInteger => pMapStringInteger[key],
    _ => 0,
  };

  private string GetString(PropertyBase property, int index, string key) => property switch
  {
    PropertyString pString => pString.NewValue,
    PropertyListString pListString => pListString[index],
    PropertyMapStringString pMapStringString => pMapStringString[key],
    _ => null,
  };

  private float GetValue(PropertyBase property, int index, string key)
  {
    switch(property)
    {
      case PropertyInteger:
      case PropertyListInteger:
      case PropertyMapIntegerInteger:
      case PropertyMapStringInteger:
        return GetInt(property, index, key);

      case PropertyUInteger:
      case PropertyListUInteger:
      case PropertyMapStringUInteger:
        return GetUInt(property, index, key);

      case PropertyFloat:
      case PropertyListFloat:
      case PropertyMapStringFloat:
        return GetFloat(property, index, key);

      case PropertyLong:
      case PropertyListLong:
      case PropertyMapStringLong:
        return GetLong(property, index, key);

      case PropertyString:
      case PropertyListString:
      case PropertyMapStringString:
        var str = GetString(property, index, key);
        float.TryParse(str, out var value);
        return value;
    }

    return 0;
  }

  [System.Serializable]
  public class Param : PropertyInfoParamBase
  {

  }
}