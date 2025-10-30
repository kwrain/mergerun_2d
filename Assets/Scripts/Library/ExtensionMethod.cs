using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using FAIRSTUDIOS.SODB.Property;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.U2D;
using Component = UnityEngine.Component;

public static class Enum<T> where T : struct, IComparable, IFormattable, IConvertible
{
  public static int Count
  {
    get
    {
      var type = typeof(T);
      if (!type.IsEnum)
        throw new ArgumentException();

      return Enum.GetValues(type).Length;
    }
  }
}

public static class ExtensionMethod
{
  #region Color

  public static string ToHex(this Color color)
  {
    return string.Format("{0}{1}{2}{3}",
      0 == color.r ? "00" : ((int) (color.r * 255f)).ToString("X2"),
      0 == color.g ? "00" : ((int) (color.g * 255f)).ToString("X2"),
      0 == color.b ? "00" : ((int) (color.b * 255f)).ToString("X2"),
      0 == color.a ? "00" : ((int) (color.a * 255f)).ToString("X2"));
  }

  #endregion

  #region Enum

  public static T GetCustomAttribute<T>(this Enum enumType) where T : Attribute
  {
    var type = enumType.GetType();
    if(type == null) return null;
    var name = Enum.GetName(type, enumType);
    if(string.IsNullOrEmpty(name)) return null;
    // lds - 결과적으로 T가 반환되므로 return 시 제네릭 메서드를 사용하여 반환하도록 수정
    //object obj = type.GetField(name).GetCustomAttribute(typeof(T), false);
    var field = type.GetField(name);
    if(field == null) return null;

    return field.GetCustomAttribute<T>(false);// (T) obj;
  }

  public static T[] GetCustomAttributes<T>(this Enum enumType) where T : Attribute
  {
    var type = enumType.GetType();
    if (type == null) return null;
    var name = Enum.GetName(type, enumType);
    if (string.IsNullOrEmpty(name)) return null;
    var field = type.GetField(name);
    if(field == null) return null;

    return (T[]) field.GetCustomAttributes(typeof(T), false);
  }


  /// <summary>
  /// Get the Description Attributes At Class Level
  /// </summary>
  /// <param name="en"></param>
  /// <returns></returns>
  static public string GetDescription(this Enum en)
  {
    descriptionCacheMap ??= new();
    string key = en.ToString();
    if (descriptionCacheMap.ContainsKey(key) == true)
      return descriptionCacheMap[key];

    var fi = en.GetType().GetField(key);
    if (fi == null)
      return string.Empty;

    var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
    if (attributes == null || attributes.Length == 0) return string.Empty;
    descriptionCacheMap[key] = attributes[0].Description;
    return descriptionCacheMap[key];
  }

  static public string GetDescriptionIconType(this Enum en)
  {
    descriptionCacheMap ??= new();
    string key = en.ToString();
    if (descriptionCacheMap.ContainsKey(key) == true)
      return descriptionCacheMap[key];

    var fi = en.GetType().GetField(key);
    if (fi == null)
      return string.Empty;

    var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
    if (attributes == null || attributes.Length == 0) return string.Empty;
    descriptionCacheMap[key] = attributes[0].Description;
    return descriptionCacheMap[key];
  }

  #endregion

  #region double

  public static string ToFileSize(this double byteLength)
  {
    string size = null;
    if (byteLength >= 1073741824)
    {
      size = $"{byteLength / 1073741824f:0.##}GB";
    }
    else if (byteLength >= 1048576)
    {
      size = $"{byteLength / 1048576f:0.##}MB";
    }
    else if (byteLength >= 1024)
    {
      size = $"{byteLength / 1024f:0.##}KB";
    }
    else if (byteLength > 0 && byteLength < 1024.0)
    {
      size = $"{byteLength:0.##}Byte";
    }

    return size;
  }

  #endregion

  #region Float

  public static string ToFileSize(this float byteLength)
  {
    string size = null;
    if (byteLength >= 1073741824)
    {
      size = $"{byteLength / 1073741824f:0.##}GB";
    }
    else if (byteLength >= 1048576)
    {
      size = $"{byteLength / 1048576f:0.##}MB";
    }
    else if (byteLength >= 1024)
    {
      size = $"{byteLength / 1024f:0.##}KB";
    }
    else if (byteLength > 0 && byteLength < 1024.0)
    {
      size = $"{byteLength:0.##}Byte";
    }

    return size;
  }

  #endregion

  #region Long

  public static string ToThousandSeparator(this ulong data) { return $"{data:#,##0}"; }

  public static string ToThousandSeparator(this long data) { return $"{data:#,##0}"; }

  #endregion

  #region Int

  public static string ToThousandSeparator(this uint data) { return $"{data:#,##0}"; }

  public static string ToThousandSeparator(this int data) { return $"{data:#,##0}"; }

  #endregion

  #region String

  /// <summary>
  /// Hex 형태의 string 을Color 로 변경
  /// </summary>
  /// <param name="value"></param>
  /// <param name="defaultColor"></param>
  /// <returns></returns>
  public static Color ToColorFromHex(this string value, Color? defaultColor = null)
  {
    try
    {
      value = value.Replace("#", string.Empty);

      var r = byte.Parse(value.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
      var g = byte.Parse(value.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
      var b = byte.Parse(value.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
      byte a = 255;

      if (value.Length == 8)
        a = byte.Parse(value.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

      return new Color32(r, g, b, a);
    }
    catch
    {
      Debug.LogFormat("[ERROR] hex : {0}, defaultColor : {1}", value, defaultColor);
    }

    return defaultColor.HasValue ? defaultColor.Value : Color.white;
  }

  public static T ToEnum<T>(this string value, T defaultValue = default) where T : struct, IConvertible
  {
    if (!typeof(T).IsEnum)
      throw new ArgumentException("T must be an enumerated type");
    try
    {
      return (T) Enum.Parse(typeof(T), value, true);
    }
    catch
    {
      return defaultValue;
    }
  }

  public static DateTime ToDateTime(this string value, int timeOffset = -9, char split = '-')
  {
    if (string.IsNullOrEmpty(value))
      return default;

    var temp = value.Split(split);
    if (temp.Length < 3)
      return default;

    DateTime dateTime;
    switch(temp.Length)
    {
      case 3:
        dateTime = new DateTime(int.Parse(temp[0]), int.Parse(temp[1]), int.Parse(temp[2]));
        break;

      case 4:
        dateTime = new DateTime(int.Parse(temp[0]), int.Parse(temp[1]), int.Parse(temp[2]), int.Parse(temp[3]), 0, 0);
        break;

      case 5:
        dateTime = new DateTime(int.Parse(temp[0]), int.Parse(temp[1]), int.Parse(temp[2]), int.Parse(temp[3]), int.Parse(temp[4]), 0);
        break;

      default:
        dateTime = default;
        break;
    }

    return dateTime.AddHours(timeOffset);
  }

  public static Rect ToRect(this string value, char split)
  {
    if (string.IsNullOrEmpty(value))
      return default;

    var arrRect = value.Split(split);
    if (4 != arrRect.Length) return default;
    var arrFloat = new float[arrRect.Length];
    for (var index = 0; index < arrRect.Length; index++)
    {
      if (!float.TryParse(arrRect[index].Trim(), out arrFloat[index]))
        return default;
    }

    return new Rect(arrFloat[0], arrFloat[1], arrFloat[2], arrFloat[3]);
  }
  public static Vector2 ToVector2(this string value, char split)
  {
    if (string.IsNullOrEmpty(value))
      return default;

    var temp = value.Split(split);
    if (temp.Length == 0)
      return default;

    if (!float.TryParse(temp[0], out var x)) return default;

    var y = 0f;
    if (temp.Length > 1)
    {
      if (!string.IsNullOrEmpty(temp[1]) && !float.TryParse(temp[1], out y))
      {
        return default;
      }
    }

    return new Vector2(x, y);
  }
  public static Vector3 ToVector3(this string value, char split)
  {
    if (string.IsNullOrEmpty(value))
      return default;

    var temp = value.Split(split);
    if (temp.Length == 0)
      return default;

    if (!float.TryParse(temp[0], out var x)) return default;

    var y = 0f;
    var z = 0f;
    if (temp.Length > 1)
    {
      if (float.TryParse(temp[1], out y))
      {
        if (temp.Length > 2)
        {
          if (!string.IsNullOrEmpty(temp[2]) && !float.TryParse(temp[2], out z))
          {
            return default;
          }
        }
      }
      else if(!string.IsNullOrEmpty(temp[1]))
      {
        return default;
      }
    }

    return new Vector3(x, y, z);
  }
  public static bool ToVector3(this string value, char split, out Vector3 result)
  {
    result = Vector3.zero;
    if (string.IsNullOrEmpty(value))
      return false;

    var temp = value.Split(split);
    if (temp.Length == 0)
      return false;

    for (var i = 0; i < temp.Length; i++)
    {
      if (temp[i].SpecificCount(".") > 1)
      {
        temp[i] = temp[i].Substring(0, temp[i].LastIndexOf("."));
      }
    }

    if (!float.TryParse(temp[0], out var x)) return false;

    var y = 0f;
    var z = 0f;
    if (temp.Length > 1)
    {
      if (float.TryParse(temp[1], out y))
      {
        if (temp.Length > 2)
        {
          if (!string.IsNullOrEmpty(temp[2]) && !float.TryParse(temp[2], out z))
          {
            return false;
          }
        }
      }
      else if(!string.IsNullOrEmpty(temp[1]))
      {
        return false;
      }
    }

    result = new Vector3(x, y, z);
    return true;
  }
  public static Vector4 ToVector4(this string value, char split)
  {
    if (string.IsNullOrEmpty(value))
      return default;

    var temp = value.Split(split);
    if (temp.Length == 0)
      return default;

    if (!float.TryParse(temp[0], out var x)) return default;

    var y = 0f;
    var z = 0f;
    var w = 0f;
    if (temp.Length > 1)
    {
      if (float.TryParse(temp[1], out y))
      {
        if (temp.Length > 2)
        {
          if (float.TryParse(temp[2], out z))
          {
            if(temp.Length > 3)
            {
              if (!string.IsNullOrEmpty(temp[3]) && !float.TryParse(temp[3], out w))
              {
                return default;
              }
            }
          }
          else if(!string.IsNullOrEmpty(temp[2]))
          {
            return default;
          }
        }
      }
      else if(!string.IsNullOrEmpty(temp[1]))
      {
        return default;
      }
    }

    return new Vector4(x, y, z, w);
  }
  public static bool ToVector4(this string value, char split, out Vector4 result)
  {
    result = Vector4.zero;
    if (string.IsNullOrEmpty(value))
      return false;

    var temp = value.Split(split);
    if (temp.Length == 0)
      return false;


    if (!float.TryParse(temp[0], out var x)) return false;

    var y = 0f;
    var z = 0f;
    var w = 0f;
    if (temp.Length > 1)
    {
      if (float.TryParse(temp[1], out y))
      {
        if (temp.Length > 2)
        {
          if (float.TryParse(temp[2], out z))
          {
            if (temp.Length > 3)
            {
              if (!string.IsNullOrEmpty(temp[3]) && !float.TryParse(temp[3], out w))
              {
                return false;
              }
            }
          }
          else if(!string.IsNullOrEmpty(temp[2]))
          {
            return false;
          }
        }
      }
      else if(!string.IsNullOrEmpty(temp[1]))
      {
        return false;
      }
    }

    result = new Vector4(x, y, z, w);
    return true;
  }

  /// <summary>
  /// 특정 문자 갯수 리턴
  /// </summary>
  /// <param name="value"></param>
  /// <param name="specific"></param>
  /// <returns></returns>
  public static int SpecificCount(this string value, string specific)
  {
    var Num = value.Length - value.Replace(specific, "").Length;
    Num /= specific.Length;

    return Num;
  }

  public static bool IsJson(this string value)
  {
    return value.Trim().Substring(0, 1).IndexOfAny(new[] {'[', '{'}) == 0;
  }

  public static bool IsXML(this string value)
  {
    return value.TrimStart().StartsWith("<");
  }

  public static string CleanInvalidXmlChars(this string StrInput)
  {
    //Returns same value if the value is empty.
    if (string.IsNullOrWhiteSpace(StrInput))
    {
      return StrInput;
    }

    StrInput = StrInput.Replace((char)0x0B, '\n');
    return StrInput;
    // From xml spec valid chars:
    // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
    // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
    // string RegularExp = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
    // return Regex.Replace(StrInput, RegularExp, string.Empty);
  }

  public static string CleanZeroSpace(this string input)
  {
    if (string.IsNullOrEmpty(input))
      return null;

    return Regex.Replace(input, @"[\u200B-\u200D\uFEFF]", "");
  }

  public static bool XmlCheckEscape(this string value)
  {
    if (string.IsNullOrEmpty(value)) return false;

    // XML escape 문자 목록
    string[] xmlEscapeChars = { "<", ">", "&", "\"", "'" };

    foreach (string escapeChar in xmlEscapeChars)
    {
      if (value.Contains(escapeChar))
      {
        return true;
      }
    }

    return false;
  }

  public static string XmlEscape(this string value)
  {
    return value
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;")
        .Replace("'", "&apos;");
  }

  public static string XmlUnescape(this string escaped)
  {
    return escaped
        .Replace("&lt;", "<")
        .Replace("&gt;", ">")
        .Replace("&apos;", "'")
        .Replace("&quot;", "\"")
        .Replace("&amp;", "&");
  }


  public static bool IsValidURL(this string value)
  {
    return Uri.TryCreate(value, UriKind.Absolute, out var uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
  }

  public static bool IsBase64(this string data, out string decodedString)
  {
    // 참고 : https://github.com/veler/DevToys/blob/ffa4c98eb642d076f56c1be23acaada7eeec42fc/src/dev/impl/DevToys/Helpers/Base64Helper.cs#L11
    decodedString = string.Empty;
    if (string.IsNullOrWhiteSpace(data))
    {
      return false;
    }

    data = data!.Trim();

    if (data.Length % 4 != 0)
    {
      return false;
    }

    data = data.Replace('-', '+').Replace('_', '/');
    if (new Regex(@"[^A-Z0-9+/=]", RegexOptions.IgnoreCase).IsMatch(data))
    {
      return false;
    }

    int equalIndex = data.IndexOf('=');
    int length = data.Length;

    if (!(equalIndex == -1 || equalIndex == length - 1 || (equalIndex == length - 2 && data[length - 1] == '=')))
    {
      return false;
    }

    string? decoded;
    try
    {
      byte[]? decodedData = Convert.FromBase64String(data);
      decoded = Encoding.UTF8.GetString(decodedData);
    }
    catch (Exception)
    {
      return false;
    }

    //check for special chars that you know should not be there
    char current;
    for (int i = 0; i < decoded.Length; i++)
    {
      current = decoded[i];
      if (current == 65533)
      {
        return false;
      }

#pragma warning disable IDE0078 // Use pattern matching
      if (!(current == 0x9
          || current == 0xA
          || current == 0xD
          || (current >= 0x20 && current <= 0xD7FF)
          || (current >= 0xE000 && current <= 0xFFFD)
          || (current >= 0x10000 && current <= 0x10FFFF)))
#pragma warning restore IDE0078 // Use pattern matching
      {
        return false;
      }
    }
    decodedString = decoded;

    return true;
  }

  public static bool IsVaildEmail(this string value)
  {
    return value.Contains("@") && value.Contains(".");
  }

  public static string EncodeBase64(this string value)
  {
    var encodeData = Encoding.UTF8.GetBytes(value);
    return encodeData != null ? Convert.ToBase64String(encodeData) : default;
  }

  public static string DecodeBase64(this string value)
  {
    if (string.IsNullOrEmpty(value))
      return default;

    if (value.IsBase64(out var decodedString))
    {
      return decodedString;
    }
    else
    {
      return value;
    }
  }

  public static bool CheckLimit(this string value, int limit, out string correct)
  {
    var size = 0;
    var sb = new StringBuilder();
    foreach (var c in value)
    {
      size += Encoding.UTF8.GetBytes(c.ToString()).Length;
      if (size <= limit)
      {
        sb.Append(c);
      }
      else
      {
        correct = sb.ToString();
        return false;
      }
    }

    correct = sb.ToString();
    return true;
  }
  public static bool CheckLimit(this string value, int limit, ref Dictionary<char, int> limitSpecific, out string correct)
  {
    // 문자 제한 체크 후, 전체 길이 확인.
    var result = true;
    var sb = new StringBuilder();
    var ints = limitSpecific;
    if (ints != null)
    {
      Dictionary<char, int> specificCount = new();
      foreach (var c in value)
      {
        if (CheckSpecific(c, specificCount))
        {
          sb.Append(c);
        }
        else if(result)
        {
          ints[c] = -1;
          result = false;
        }
      }

      value = sb.ToString();
      sb.Clear();
    }

    var totalSize = 0;
    foreach (var c in value)
    {
      var size = Encoding.UTF8.GetBytes(c.ToString()).Length;
      totalSize += size;

      if (totalSize <= limit)
      {
        sb.Append(c);
      }
      else
      {
        result = false;
        break;
      }
    }

    correct = sb.ToString();
    return result;

    bool CheckSpecific(char c, Dictionary<char, int> specificCount)
    {
      foreach (var specific in ints)
      {
        if (specific.Key != c)
          continue;

        if (!specificCount.ContainsKey(specific.Key))
        {
          specificCount[specific.Key] = 1;
        }

        if (specificCount[specific.Key] >= specific.Value)
          return false;

        specificCount[specific.Key]++;
      }

      return true;
    }
  }

  #endregion

  #region DateTime

  public static long ToUnixTimestamp(this DateTime date)
  {
    var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    var diff = date - origin;

    return (long)diff.TotalSeconds;
  }

  public static ulong ToUlongUnixTimestamp(this DateTime date)
  {
    var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    var diff = date - origin;

    return (ulong)diff.TotalSeconds;
  }

  #endregion

  #region Unity

  public static void RemoveEvent(this AnimationClip animationClip, string functionName)
  {
    var animationEvents = animationClip.events;
    var updatedAnimationEvents = new List<AnimationEvent>();

    foreach (var animationEvent in animationEvents)
    {
      if (animationEvent.functionName == functionName)
        continue;

      updatedAnimationEvents.Add(animationEvent);
    }

    animationClip.events = updatedAnimationEvents.ToArray();
  }

  public static T CopyComponent<T>(this GameObject go, T original) where T : UnityEngine.Component
  {
    var type = original.GetType();
    var copy = go.AddComponent(type);
    var fields = type.GetFields();
    foreach (var field in fields)
    {
      field.SetValue(copy, field.GetValue(original));
    }

    var props = type.GetProperties();
    foreach (var prop in props)
    {
      if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name")
        continue;
      prop.SetValue(copy, prop.GetValue(original, null), null);
    }

    return copy as T;
  }

  public static T Find<T>(this GameObject transform, string n)
  {
    var go = GameObject.Find(n);
    if (go != null)
    {
      return go.GetComponent<T>();
    }
    else
    {
      return default;
    }
  }

  public static T FindChild<T>(this GameObject go, string childName)
  {
    var target = FindChild(go, childName);
    if (null == target)
      return default(T);

    return target.GetComponent<T>();
  }

  public static T FindChild<T>(this Component component, string childName)
  {
    var target = FindChild(component.gameObject, childName);
    if (null == target)
      return default(T);

    return target.GetComponent<T>();
  }

  public static GameObject FindChild(this GameObject go, string childName)
  {
    GameObject result = null;
    for (var i = 0; i < go.transform.childCount; ++i)
    {
      var child = go.transform.GetChild(i);
      if (child.name == childName)
      {
        result = child.gameObject;
        break;
      }
      else
      {
        if (child.childCount > 0)
          result = child.gameObject.FindChild(childName);

        if (null != result)
          break;
      }
    }

    return result;
  }

  public static Transform FindChildEx(this Transform transform, string childName)
  {
    Transform result = null;
    for (var i = 0; i < transform.childCount; ++i)
    {
      var child = transform.GetChild(i);
      if (child.name == childName)
      {
        result = child;
        break;
      }
      else if (child.childCount > 0)
      {
        result = child.FindChildEx(childName);
        if (result != null)
          break;
      }
    }

    return result;
  }

  public static bool IsActiveSelf(this Component component)
  {
    return component && component.gameObject.activeSelf;
  }
  public static bool IsActiveInHierarchy(this Component component)
  {
    return component && component.gameObject.activeInHierarchy;
  }

  public static void SetActive(this Component component, bool bActive)
  {
    if (component == null || component.gameObject == null)
      return;

    component.gameObject.SetActive(bActive);
  }
  /// <summary>
  /// 자신을 제외한 ChildObject 들의 Active 설정을 하는 함수
  /// </summary>
  /// <param name="go"></param>
  /// <param name="bActive"></param>
  /// <param name="arrIgnoreName"></param>
  public static void SetActiveInChildren(this GameObject go, bool bActive, params string[] arrIgnoreName)
  {
    if (go.transform.childCount == 0)
      return;

    List<string> ltIgnoreName = null;
    if (null != arrIgnoreName && arrIgnoreName.Length > 0)
      ltIgnoreName = new List<string>(arrIgnoreName);

    Transform trChild = null;
    for (var i = 0; i < go.transform.childCount; i++)
    {
      trChild = go.transform.GetChild(i);
      if (null != ltIgnoreName && ltIgnoreName.Contains(trChild.name))
        continue;

      trChild.SetActive(bActive);
    }
  }
  public static void SetParent(this GameObject go, GameObject goParent, Vector3? localPosition = null)
  {
    if (null == goParent)
      return;

    go.transform.SetParent(goParent.transform);

    var rectTransform = go.GetComponent<RectTransform>();
    if (null != rectTransform)
    {
      go.transform.localPosition = Vector3.zero;
      rectTransform.anchoredPosition = localPosition ?? Vector3.zero;
    }
    else
    {
      go.transform.localPosition = localPosition ?? Vector3.zero;
    }

    go.transform.localRotation = Quaternion.identity;
    go.transform.localScale = Vector3.one;
  }
  public static void SetParent(this GameObject go, Component component, Vector3? localPosition = null)
  {
    if (null == component)
      return;

    go.transform.SetParent(component.transform);

    var rectTransform = go.GetComponent<RectTransform>();
    if (null != rectTransform)
    {
      go.transform.localPosition = Vector3.zero;
      rectTransform.anchoredPosition = localPosition.HasValue ? localPosition.Value : Vector3.zero;
    }
    else
    {
      go.transform.localPosition = localPosition.HasValue ? localPosition.Value : Vector3.zero;
    }
    go.transform.localRotation = Quaternion.identity;
    go.transform.localScale = Vector3.one;
  }

  /// <summary>
  /// Checks if a GameObject has been destroyed.
  /// </summary>
  /// <param name="gameObject">GameObject reference to check for destructedness</param>
  /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
  public static bool IsDestroyed(this GameObject gameObject)
  {
    // UnityEngine overloads the == opeator for the GameObject type
    // and returns null when the object has been destroyed, but
    // actually the object is still there but has not been cleaned up yet
    // if we test both we can determine if the object has been destroyed.
    return gameObject == null && !ReferenceEquals(gameObject, null);
  }

  public static void SetLayer(this GameObject go, string name, bool applyChild = true)
  {
    go.layer = LayerMask.NameToLayer(name);

    if (!applyChild) return;
    foreach (Transform child in go.transform)
    {
      SetLayer(child.gameObject, name);
    }
  }

  #region UI

  public static void ChangePivot(this GameObject go, Vector2 pivot)
  {
    var rectTransform = go.GetComponent<RectTransform>();
    rectTransform.ChangePivot(pivot);
  }

  public static void ChangePivot(this RectTransform rectTransform, Vector2 pivot)
  {
    var deltaPivot = rectTransform.pivot - pivot;

    var deltaX = deltaPivot.x * rectTransform.sizeDelta.x * rectTransform.localScale.x;
    var deltaY = deltaPivot.y * rectTransform.sizeDelta.y * rectTransform.localScale.y;

    var rot = rectTransform.rotation.eulerAngles.z * Mathf.PI / 180;
    var deltaPosition = new Vector3(Mathf.Cos(rot) * deltaX - Mathf.Sin(rot) * deltaY, Mathf.Sin(rot) * deltaX + Mathf.Cos(rot) * deltaY);

    rectTransform.pivot = pivot;
    rectTransform.localPosition -= deltaPosition;
  }

  public static void SetSizeDelta(this GameObject go, Vector2 sizeDelta)
  {
    var rectTransform = go.GetComponent<RectTransform>();
    if (null == rectTransform)
      return;

    rectTransform.sizeDelta = sizeDelta;
  }

  public static Rect GetWorldRect(this RectTransform rect)
  {
    var corners = new Vector3[4];
    rect.GetWorldCorners(corners);

    return new Rect(
        corners[0].x,
        corners[0].y,
        corners[2].x - corners[0].x,
        corners[2].y - corners[0].y
    );
  }

  /// <summary>
  /// 캔버스 그룹 설명 : https://wergia.tistory.com/177 , https://docs.unity3d.com/kr/2019.3/Manual/class-CanvasGroup.html
  /// </summary>
  /// <param name="cg">Canvas Group</param>
  /// <param name="active">true : 켜기, false : 끄기</param>
  /// <param name="ignoreParentGroups"></param>
  public static void SetActiveUseCanvasGroup(this CanvasGroup cg, bool active, bool ignoreParentGroups = false)
  {
    if (cg == null) return;

    SetActiveUseCanvasGroup(cg, active, active, active, ignoreParentGroups);
  }

  public static void SetActiveUseCanvasGroup(this CanvasGroup cg, bool active, bool interactable, bool blocksRaycasts, bool ignoreParentGroups = false)
  {
    if (cg == null) return;

    cg.alpha = active == true ? 1 : 0;          // UI 숨기기
    cg.interactable = interactable;             // UI 상호작용 => false일 때 토글, 버튼, 슬라이드 등이 회색으로 변하고 상호작용이 불가능해짐
    cg.blocksRaycasts = blocksRaycasts;         // UI 레이케스트 => false일 때 토글, 버튼, 슬라이드 등이 회색으로 변하지않으며 레이케스트 대상에서 무시됨.
    cg.ignoreParentGroups = ignoreParentGroups;
  }

  #endregion

  #endregion

  #region Do Tween

  #endregion

  // lds - 22.7.8
  // 리플렉션을 통한 Description 결과를 캐싱 ( 최적화 )
  private static Dictionary<string, string> descriptionCacheMap;

  static public void LoadFromJsonEx<T>(this PropertyPlayerPrefsString property, string key, out T output)
  {
    if(PlayerPrefs.HasKey(key) == false)
    {
      output = default;
      return;
    }
    var json = PlayerPrefs.GetString(key);
    property.RuntimeValue[key] = json;
    output = JsonConvert.DeserializeObject<T>(json);
  }

  static public void SaveToJsonEx<T>(this PropertyPlayerPrefsString property, string key, T input)
  {
    var json = JsonConvert.SerializeObject(input);
    property.RuntimeValue[key] = json;
    PlayerPrefs.SetString(key, json);
  }


#if UNITY_EDITOR
  /// <summary>
  /// SpriteAtlas 에셋에 적용된 스프라이트들의 texture에셋들에 라벨을 붙이는 함수<br/>
  /// 사용 순서<br/>
  /// 1. SpriteAtals 생성 <br/>
  /// 2. SpriteAtlas 리소스 추가 및 패킹 <br/>
  /// 2. 프로젝트뷰에서 SpriteAtals 에셋선택 후 우클릭 하여 SpriteAtals/Validate Labels 메뉴 클릭 <br/>
  /// 3. 자동으로 SpriteAtlas의 guid값이 해당 texture2D 에셋 라벨로 지정된다. <br/>
  /// 이후에는 리소스 이동 또는 추가때마다 패킹 후 2번을 해주면된다. <br/>
  /// 아래의 함수에서는 선택된 SpriteAtlas의 guid로 라벨링된 에셋폴더 내 모든 texture2D를 가져오고 (1) allTextures <br/>
  /// 그리고나서 선택된 SpriteAtlas에서 GetSprites()를 통해 연결된 texture2D 에셋들의 라벨을 SpriteAtals의 guid로 지정한다. (2) outTextures <br/>
  /// 그 후 (1)과 (2)를 합친 리스트에서 (2)를 제외시켜준다. (3) combineAndDistinct <br/>
  /// (3)의 개수가 0보다 크다면 (3)의 texture2D의 라벨을 null로 지정해준다.
  /// </summary>
  [MenuItem("Assets/SpriteAtlas/Validate Labels")]
  public static void ValidateSpriteAtlasLabels()
  {
    var spriteAtals = Selection.activeObject as SpriteAtlas;
    if (spriteAtals == null) { Debug.LogError("Sprite Atlas 에셋에서 사용해주세요."); }
    var spriteAtlasPath = AssetDatabase.GetAssetPath(Selection.activeObject);
    var guid = AssetDatabase.AssetPathToGUID(spriteAtlasPath);
    var allGuids = AssetDatabase.FindAssets("l:" + guid, null);
    Texture2D[] allTextures = new Texture2D[allGuids.Length];
    for (int i = 0; i < allGuids.Length; i++)
    {
      var path = AssetDatabase.GUIDToAssetPath(allGuids[i]);
      allTextures[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    Sprite[] outSprites = new Sprite[spriteAtals.spriteCount];
    var spPackedSprites = spriteAtals.GetSprites(outSprites);
    Texture2D[] outTextures = new Texture2D[outSprites.Length];
    for (int i = 0; i < outSprites.Length; i++)
    {
      outTextures[i] = outSprites[i].texture;
    }
    foreach (var s in outTextures)
    {
      // 현재 선택된 아틀라스의 스프라이트에 달려있는 라벨을 가져와서
      var s1Labels = AssetDatabase.GetLabels(s);
      // 해당 라벨의 개수가 한개일 때
      if (s1Labels.Length == 1)
      {
        // 지정된 라별이 같다면 제외
        if (s1Labels[0] == guid) continue;

        // 지정된 라벨이 다르면 현재의 guid를 라벨로 지정
        AssetDatabase.SetLabels(s, new[] { guid });
      }
      // 해당 라벨의 개수가 0보다 작거나 1보다 큰 경우

      // 라벨지정
      AssetDatabase.SetLabels(s, new[] { guid });
    }

    var combineAndDistinct = new List<Texture2D>();
    combineAndDistinct.AddRange(allTextures);
    combineAndDistinct.AddRange(outTextures);

    combineAndDistinct = combineAndDistinct.Except(outTextures).ToList();
    if (combineAndDistinct.Count == 0) return;
    foreach (var s in combineAndDistinct)
    {
      AssetDatabase.SetLabels(s, null);
    }

  }
#endif

  public static void ApplicationQuit()
  {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.ExitPlaymode();
#else
    Application.Quit();
#endif
  }
}