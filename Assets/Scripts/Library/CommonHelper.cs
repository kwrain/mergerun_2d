using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.SODB.ViewModel;
using FAIRSTUDIOS.UI;
using UnityEngine;
using UnityEngine.U2D;
using Random = System.Random;

public static class CommonHelper
{
  #region Color

  /// <summary>
  ///  Text에 포함된 마크업 포맷 형태의 컬러의 Alpha 값을 변경
  /// </summary>
  /// <param name="text"></param>
  /// <param name="alpha"></param>
  /// <returns></returns>
  public static string AlphaTagChangeInText(string text, float alpha)
  {
    if (string.IsNullOrEmpty(text))
      return string.Empty;

    if (alpha < 0)
    {
      alpha = 0;
    }
    else if (alpha > 1)
    {
      alpha = 1;
    }

    var alphaHex = (alpha == 0.0f) ? "00" : ((int)(alpha * 255.0f)).ToString("x02");

    var regex = new Regex(@"=#[A-Fa-f0-9]{6,8}>");
    var v = regex.Matches(text);
    foreach (Match match in v)
    {
      if (match.Success && (match.ToString().Length <= 11))
      {
        // alpha을 찾아서 교체만 하면은 됨
        var buffer = $"{match.ToString().Substring(0, 8)}{alphaHex}>";
        text = text.Replace(match.ToString(), buffer);
      }
    }
    return text;
  }
  #endregion

  #region Time
  static public TimeSpan? GetTimeSpan(string szDateTime, DateTime dateTime)
  {
    DateTime tmpDateTime;
    if (!DateTime.TryParse(szDateTime, out tmpDateTime))
      return null;

    return dateTime - tmpDateTime;
  }

  static public TimeSpan? GetTimeSpan(DateTime dateTime, string szDateTime)
  {
    DateTime tmpDateTime;
    if (!DateTime.TryParse(szDateTime, out tmpDateTime))
      return null;

    return tmpDateTime - dateTime;
  }

  /// <summary>
  /// 두 날짜를 비교해서 초 차이 값을 반환한다.
  /// </summary>
  /// <param name="dateTime"></param>
  /// <param name="szDateTime">문자열 날짜</param>
  /// <returns></returns>
  static public int GetDiffSecondDateTime(string szDateTime, DateTime dateTime)
  {
    var ts = GetTimeSpan(szDateTime, dateTime);
    if (!ts.HasValue)
      return 0;

    return Convert.ToInt32(ts.Value.TotalSeconds);
  }

  /// <summary>
  /// 두 날짜를 비교해서 분 차이 값을 반환한다.
  /// </summary>
  /// <param name="szDateTime">문자열 날짜</param>
  /// <param name="dateTime"></param>
  /// <returns></returns>
  static public int? GetDiffMinuteDateTime(string szDateTime, DateTime dateTime)
  {
    var ts = GetTimeSpan(szDateTime, dateTime);

    if (!ts.HasValue)
      return null;

    return Convert.ToInt32(ts.Value.TotalMinutes);
  }

  /// <summary>
  /// 두 날짜를 비교해서 시간 차이 값을 반환한다.
  /// </summary>
  /// <param name="szDateTime">문자열 날짜</param>
  /// <param name="dateTime"></param>
  /// <returns></returns>
  static public int? GetDiffHourDateTime(string szDateTime, DateTime dateTime)
  {
    var ts = GetTimeSpan(szDateTime, dateTime);

    if (!ts.HasValue)
      return null;

    return Convert.ToInt32(ts.Value.TotalHours);
  }

  /// <summary>
  /// 두 날짜를 비교해서 날짜 차이 값을 반환한다.
  /// </summary>
  /// <param name="szDateTime">문자열 날짜</param>
  /// <param name="dateTime"></param>
  /// <returns></returns>
  static public int? GetDiffDayDateTime(string szDateTime, DateTime dateTime)
  {
    var ts = GetTimeSpan(szDateTime, dateTime);

    if (!ts.HasValue)
      return null;

    return Convert.ToInt32(ts.Value.TotalDays);
  }

  /// <summary>
  /// 두 날짜를 비교해서 초 차이 값을 반환한다.
  /// </summary>
  /// <param name="dateTime"></param>
  /// <param name="szDateTime">문자열 날짜</param>
  /// <returns></returns>
  static public long? GetDiffSecondDateTime(DateTime dateTime, string szDateTime)
  {
    var ts = GetTimeSpan(dateTime, szDateTime);

    if (!ts.HasValue)
      return null;

    return Convert.ToInt64(ts.Value.TotalSeconds);
  }

  /// <summary>
  /// 두 날짜를 비교해서 분 차이 값을 반환한다.
  /// </summary>
  /// <param name="dateTime"></param>
  /// <param name="szDateTime">문자열 날짜</param>
  /// <returns></returns>
  static public int? GetDiffMinuteDateTime(DateTime dateTime, string szDateTime)
  {
    var ts = GetTimeSpan(dateTime, szDateTime);

    if (!ts.HasValue)
      return null;

    return Convert.ToInt32(ts.Value.TotalMinutes);
  }

  /// <summary>
  /// 두 날짜를 비교해서 시간 차이 값을 반환한다.
  /// </summary>
  /// <param name="dateTime"></param>
  /// <param name="szDateTime">문자열 날짜</param>
  /// <returns></returns>
  static public int? GetDiffHourDateTime(DateTime dateTime, string szDateTime)
  {
    var ts = GetTimeSpan(dateTime, szDateTime);

    if (!ts.HasValue)
      return null;

    return Convert.ToInt32(ts.Value.TotalHours);
  }

  /// <summary>
  /// 두 날짜를 비교해서 날짜 차이 값을 반환한다.
  /// </summary>
  /// <param name="dateTime"></param>
  /// <param name="szDateTime">문자열 날짜</param>
  /// <returns></returns>
  static public int? GetDiffDayDateTime(DateTime dateTime, string szDateTime)
  {
    var ts = GetTimeSpan(dateTime, szDateTime);

    if (!ts.HasValue)
      return null;

    return Convert.ToInt32(ts.Value.TotalDays);
  }

  /// <summary>
  /// 두 날짜를 비교해서 선택한 시간의 차이 값을 반환한다.
  /// </summary>
  static public int GetSelectDiffDateTime(ETimeTable eType, DateTime dateTime, string szDateTime)
  {
    var ts = GetTimeSpan(dateTime, szDateTime);

    var iDate = -1;

    if (!ts.HasValue)
      return iDate;

    switch (eType)
    {
      case ETimeTable.SECONDS:
        iDate = Convert.ToInt32(ts.Value.TotalSeconds);
        break;
      case ETimeTable.MINUTES:
        iDate = Convert.ToInt32(ts.Value.TotalMinutes);
        break;
      case ETimeTable.MILLISECONDS:
        iDate = Convert.ToInt32(ts.Value.TotalMilliseconds);
        break;
      case ETimeTable.HOURS:
        iDate = Convert.ToInt32(ts.Value.TotalHours);
        break;
      case ETimeTable.DAYS:
        iDate = Convert.ToInt32(ts.Value.TotalDays);
        break;
    }

    return iDate;
  }

  /// <summary>
  /// 두 날짜를 비교해서 선택한 시간의 차이 값을 반환한다.
  /// </summary>
  static public int GetSelectDiffDateTime(ETimeTable eType, string szDateTime, DateTime dateTime)
  {
    var ts = GetTimeSpan(szDateTime, dateTime);

    var iDate = 0;

    if (!ts.HasValue)
      return iDate;

    switch (eType)
    {
      case ETimeTable.SECONDS:
        iDate = Convert.ToInt32(ts.Value.TotalSeconds);
        break;
      case ETimeTable.MINUTES:
        iDate = Convert.ToInt32(ts.Value.TotalMinutes);
        break;
      case ETimeTable.MILLISECONDS:
        iDate = Convert.ToInt32(ts.Value.TotalMilliseconds);
        break;
      case ETimeTable.HOURS:
        iDate = Convert.ToInt32(ts.Value.TotalHours);
        break;
      case ETimeTable.DAYS:
        iDate = Convert.ToInt32(ts.Value.TotalDays);
        break;
    }

    return iDate;
  }

  /// <summary>
  /// 데이트타입 형식인지 체크한다.
  /// </summary>
  /// <param name="szDateTime"></param>
  /// <returns></returns>
  static public bool CheckDateType(string szDateTime)
  {
    return DateTime.TryParse(szDateTime, out _);
  }

  public static DateTime ConvertFromUnixTimestamp(double timestamp)
  {
    var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    return origin.AddSeconds(timestamp);
  }

  public static long ConvertToUnixTimestamp(DateTime date)
  {
    var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    var diff = date - origin;

    return (long) diff.TotalSeconds;
  }

  public static ulong ConvertToUlongUnixTimestamp(DateTime date)
  {
    var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    var diff = date - origin;
    return (ulong)diff.TotalSeconds;
  }

  /// <summary>
  /// 유닉스타임을 (일 시), (시 분) (분 초) 로 변환
  /// </summary>
  /// <param name="unixTime"></param>
  /// <returns></returns>
  public static string ConvertTimeToString(int unixTime)
  {
    var time = string.Empty;
    if (unixTime < 60)
    {
      time = $"{unixTime % 60}{Localize.GetValue("2000418")}";//초
    }
    else if (unixTime < 3600)
    {
      if (unixTime % 60 == 0)
      {
        time = $"{unixTime / 60}{Localize.GetValue("326")}";//{0}분
      }
      else
      {
        time =
          $"{unixTime / 60}{Localize.GetValue("326")} {unixTime % 60}{Localize.GetValue("2000418")}";//{0}분 {1}초
      }
    }
    else if (unixTime < 86400)
    {
      if (unixTime % 3600 == 0)
      {
        time = $"{unixTime / 3600}{Localize.GetValue("325")}";//{0}시간
      }
      else
      {
        time =
          $"{unixTime / 3600}{Localize.GetValue("325")} {(unixTime % 3600) / 60}{Localize.GetValue("326")}";//{0}시간 {1}분
      }
    }
    else
    {
      if (unixTime % 86400 == 0)
      {
        time = $"{unixTime / 86400}{Localize.GetValue("375")}";//{0}일
      }
      else
      {
        time =
          $"{unixTime / 86400}{Localize.GetValue("375")} {(unixTime % 86400) / 3600}{Localize.GetValue("325")}";//{0}일 {1}시간
      }
    }

    return time;
  }
  public static string ConvertTimeToString(DateTime date)
  {
    return ConvertTimeToString((int)ConvertToUnixTimestamp(date));
  }

  public static string ConvertTimeToShortString(int unixTime, bool isLocalize = true)
  {
    if (unixTime < 0)
      unixTime = 0;

    return ConvertTimeToShortString((ulong) unixTime, isLocalize);
  }
  public static string ConvertTimeToShortString(long unixTime, bool isLocalize = true)
  {
    if (unixTime < 0)
      unixTime = 0;

    return ConvertTimeToShortString((ulong)unixTime, isLocalize);
  }
  /// <summary>
  /// 유닉스타임을 시, 분, 초 로 변환
  /// </summary>
  /// <param name="unixTime"></param>
  /// <returns></returns>
  public static string ConvertTimeToShortString(ulong unixTime, bool isLocalize = true)
  {
    var time = string.Empty;
    if (unixTime < 60)
    {
      time = $"{unixTime % 60}";
      if(isLocalize)
        time += Localize.GetValue("2000418");//초
    }
    else if (unixTime < 3600)
    {
      time = $"{unixTime / 60}";
      if(isLocalize)
        time += Localize.GetValue("326");//분
    }
    else if (unixTime < 86400)
    {
      time = $"{unixTime / 3600}";
      if(isLocalize)
        time += Localize.GetValue("325");//시간
    }
    else
    {
      time = $"{unixTime / 86400}";
      if(isLocalize)
        time += Localize.GetValue("375");//일
    }

    return time;
  }
  /// <summary>
  /// 유닉스타임을 일, 시, 분, 초 배열로 변환 <br/>
  /// 0-days <br/>
  /// 1-hours <br/>
  /// 2-minutes <br/>
  /// 3-seconds <br/>
  /// </summary>//
  /// <param name="unixTime"></param>
  /// <returns></returns>
  public static string[] ConvertTimeToElementString(int unixTime)
  {
    var result = new string[4]{"0","0","0","0"};
    var times = new int[4];
    if(unixTime < 0)
      return result;
    times[0] = unixTime;          // 전체 초 ex) 2587650초
    times[1] = times[0] % 86400;  // 일 뺀 나머지 초 ex) 2587650 % 86400 => 82050초
    times[2] = times[1] % 3600;   // 일, 시 뺀 나머지 초 ex) 82050 % 3600 => 2850초
    times[3] = times[2] % 60;     // 일, 시, 분 뺀 나머지 초 ex) 2850 % 60 => 30

    result[0] = $"{times[0] / 86400}{Localize.GetValue("375")}";//일
    result[1] = $"{times[1] / 3600}{Localize.GetValue("325")}";//시
    result[2] = $"{times[2] / 60}{Localize.GetValue("326")}";//분
    result[3] = $"{times[3]}{Localize.GetValue("2000418")}";//초
    //Debug.Log($"{unixTime},{result[0]},{result[1]},{result[2]},{result[3]}");
    return result;
  }
  public static string ConvertTimeToShortString(DateTime date)
  {
    return ConvertTimeToShortString(ConvertToUnixTimestamp(date));
  }

  #endregion

  #region Random
  public static List<int> RandomShuffle(int min, int max)
  {
    var rand = new Random((int)DateTime.Now.Ticks);
    var list = Enumerable.Range(min, max).ToList();
    int idx, old;
    for (var i = min; i < max; i++)
    {
      idx = rand.Next(max);
      old = list[i];
      list[i] = list[idx];
      list[idx] = old;
    }

    return list;
  }

  public static void Shuffle<T>(this IList<T> list)
  {
    var rand = new Random();
    var n = list.Count;
    while (n > 1)
    {
      n--;
      var k = rand.Next(n + 1);
      var value = list[k];
      list[k] = list[n];
      list[n] = value;
    }
  }

  public static List<T> ShuffleAndCopy<T>(this IList<T> src)
  {
    var rand = new Random();
    var list = src.ToList();
    var n = list.Count;
    while (n > 1)
    {
      n--;
      var k = rand.Next(n + 1);
      var value = list[k];
      list[k] = list[n];
      list[n] = value;
    }

    return list;
  }

  /// <summary>
  /// 지정 범위에서 일부값을 제외한 랜덤값을 개수만큼 얻어옴.
  /// </summary>
  /// <param name="min"></param>
  /// <param name="max"></param>
  /// <param name="count"></param>
  /// <param name="ignoreList"></param>
  /// <returns></returns>
  public static List<int> RandomRangeList(int min, int max, int count, List<int> ignoreList)
  {
    if (min >= max)
      return null;

    var randomValues = new List<int>();
    while (randomValues.Count < count)
    {
      var value = UnityEngine.Random.Range(min, max);
      if (!randomValues.Contains(value) && 0 == ignoreList.Find(x => x == value))
        randomValues.Add(value);
    }

    return randomValues;
  }

  public static int RandomRange(int min, int max, List<int> ignoreList = null)
  {
    if (min >= max)
      return 0;

    for (var i = 0; i < max - min; i++)
    {
      var value = UnityEngine.Random.Range(min, max);
      if (ignoreList != null && ignoreList.FindIndex(x => x == value) < 0)
        return value;
    }

    return 0;
  }

  public static int RandomRange(List<int> list, List<int> ignore = null)
  {
    if (list == null || list.Count == 0)
      return 0;

    var random = new List<int>(list);
    if (ignore != null)
    {
      random.RemoveAll(value => ignore.Contains(value));
    }

    var index = UnityEngine.Random.Range(0, random.Count);
    return random[index];
  }

  public static List<int> RandomList(List<int> list, int count, List<int> ignore = null)
  {
    if (list == null || list.Count == 0)
      return null;

    var random = new List<int>(list);
    if (ignore != null)
    {
      random.RemoveAll(value => ignore.Contains(value));
    }

    var r = new System.Random();
    return random.OrderBy(x => r.Next()).Take(count).ToList();
  }
  #endregion

  public static bool NumberInRange(int num, int start, int end)
  {
    return num >= start && num <= end;
  }

  public static bool NumberInRange(float num, float start, float end)
  {
    return num >= start && num <= end;
  }

  public static T[,] MatrixRotate<T>(T[,] matrix, bool clockwise)
  {
    var lengthY = matrix.GetLength(0);
    var lengthX = matrix.GetLength(1);

    var result = new T[lengthX, lengthY];
    for (var y = 0; y < lengthY; y++)
    {
      for (var x = 0; x < lengthX; x++)
      {
        if (clockwise)
        {
          result[x, y] = matrix[y, lengthX - 1 - x];
        }
        else
        {
          result[x, y] = matrix[lengthY - 1 - y, x];
        }
      }
    }

    return result;
  }

  /// <summary>
  /// '.' 들어간 숫자로 이루어진 문자열 크기 비교
  /// ex) 1.0.0 과 1.0.1 비교
  /// </summary>
  /// <param name="szSource">비교할 원본</param>
  /// <param name="szTarget">비교할 대상</param>
  /// <returns></returns>
  public static bool IsBigNumberPeriodString(string szSource, string szTarget)
  {
    var nSourceNumber = int.Parse(szSource.Replace(".", ""));
    var nTargetNumber = int.Parse(szTarget.Replace(".", ""));

    return nSourceNumber > nTargetNumber;
  }

  public static bool IsOtherLetter(string character)
  {
    return char.GetUnicodeCategory(character[0]) == System.Globalization.UnicodeCategory.OtherLetter;
  }

  public static float GetNearValue(float target, List<float> data)
  {
    float near = 0;
    var min = float.MaxValue;

    //[2] Process
    for (var i = 0; i < data.Count; i++)
    {
      if (Mathf.Abs(data[i] - target) < min)
      {
        min = Mathf.Abs(data[i] - target); //최소값 알고리즘
        near = data[i]; //최종적으로 가까운 값
      }
    }

    return near;
  }

  public static string GetCountingNumber(int eventCash)
  {
    return GetCountingNumber((long) eventCash);
  }
  public static string GetCountingNumber(long value)
  {
    if (Localize.ELanguageCode == ELanguageCode.KO)
    {
      if (value < 1000000)
      {
        return $"{value:#,##0}";
      }
      else if (value < 100000000)
      {
        // 100만 = 1000000
        return $"{value / 10000}만";
      }
      else
      {
        return $"{value / 100000000}억";
      }
    }
    else
    {
      if (value < 1000000)
      {
        return $"{value:#,##0}";
      }
      else if (value < 1000000000)
      {
        var a = value / 1000000; // 백만으로 나눴을 때 몫
        var b = value % 1000000; // 백만으로 나눴을 때 나머지
        var c = b / 10000; // 나머지를 만으로 나눴을 때 몫
        return $"{a}.{c:D2}M";
      }
      else
      {
        var a = value / 1000000000; // 10억으로 나눴을 때 몫
        var b = value % 1000000000; // 10억으로 나눴을 때 나머지
        var c = b / 10000000; // 나머지를 천만으로 나눴을 때 몫
        return $"{a}.{c:D2}B";
      }
    }
  }

  public static List<T> RemoveDuplicateValue<T>(List<T> list)
  {
    var newList = new List<T>();

    for (var i = 0; i < list.Count; i++)
    {
      if (newList.Contains(list[i]))
      {
        continue;
      }

      newList.Add(list[i]);
    }

    return newList;
  }
  public static T DictionaryValuesToGenericList<T>() where T : new()
  {
    return default(T);
  }

  public static T ObjectToGenericType<T>(object obj) where T : new()
  {
    return (T)ObjectToGenericType(obj, typeof(T));
  }

  static object ObjectToGenericType(object obj, Type type)
  {
    object newObj = null;

    if (obj.GetType() == typeof(Dictionary<string, object>))
    {
      newObj = Activator.CreateInstance(type);
      var dtData = (Dictionary<string, object>)obj;
      if (dtData.Count > 0)
      {
        var key = dtData.Keys.ToArray()[0];
        var nKey = 0;
        if (Int32.TryParse(key, out nKey))
        {
          newObj = ValueToDictionary(type, obj);

          return newObj;
        }
      }

      foreach (var kv in dtData)
      {
        SetObjectPropertyAndField(newObj, kv.Key, kv.Value);
      }
    }
    else if (obj.GetType().IsArray)
    {
      if (type.IsArray)
      {
        newObj = ValueToArray(type, obj);
      }
      else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
      {
        newObj = ValueToList(type, obj);
      }
      else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
      {
        newObj = ValueToDictionary(type, obj);
      }
    }

    return newObj;
  }

  static object ValueToDictionary(Type type, object obj)
  {
    var keyType = type.GetGenericArguments()[0];
    var valueType = type.GetGenericArguments()[1];
    var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
    var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

    var objType = obj.GetType();
    if (objType == typeof(Dictionary<string, object>))
    {
      var dtData = (Dictionary<string, object>)obj;
      foreach (var kv in dtData)
      {
        if (kv.Value.GetType().IsClass)
        {
          object value = null;
          if (kv.Value.GetType().IsArray)
          {
            if (valueType.IsGenericType)
            {
              value = ValueToList(kv.Value.GetType(), kv.Value);
            }
            else
            {
              value = ValueToArray(kv.Value.GetType(), kv.Value);
            }
          }
          else
          {
            value = Activator.CreateInstance(valueType);
          }

          SetObjectPropertyAndField(value, kv.Key, kv.Value);
          dictionary.Add(kv.Key, value);
        }
        else
        {
          dictionary.Add(kv.Key, kv.Value);
        }
      }
    }
    else if (objType == typeof(Dictionary<string, object>[]))
    {
      var arrDictionaryData = (Dictionary<string, object>[])obj;
      for (var i = 0; i < arrDictionaryData.Length; i++)
      {
        var value = Activator.CreateInstance(valueType);
        foreach (var kv in arrDictionaryData[i])
        {
          SetObjectPropertyAndField(value, kv.Key, kv.Value);
        }

        var properties = valueType.GetProperties();
        var property = properties.Where(item => item.Name == "Key").FirstOrDefault();
        var key = property.GetValue(value, null);

        dictionary.Add(key, value);
      }
    }

    return dictionary;
  }
  static object ValueToList(Type type, object obj)
  {
    Type elementType = null;
    if (type.IsGenericType)
    {
      elementType = type.GetGenericArguments()[0];
    }
    else
    {
      elementType = type.GetElementType();
    }
    var listType = typeof(List<>).MakeGenericType(elementType);
    var list = (IList)Activator.CreateInstance(listType);

    if (obj.GetType() == typeof(Dictionary<string, object>[]))
    {
      var arrDictionaryData = (Dictionary<string, object>[])obj;
      for (var i = 0; i < arrDictionaryData.Length; i++)
      {
        var element = Activator.CreateInstance(elementType);
        foreach (var kv in arrDictionaryData[i])
        {

          SetObjectPropertyAndField(element, kv.Key, kv.Value);
        }

        list.Add(element);
      }
    }
    else
    {
      var arrData = ((Array)obj).Cast<object>().ToArray();
      for (var i = 0; i < arrData.Length; i++)
      {
        list.Add(arrData[i]);
      }
    }

    return list;
  }
  static object ValueToArray(Type type, object obj)
  {
    Array array = null;
    var objType = obj.GetType();
    if (objType == typeof(Dictionary<string, object>[]))
    {
      var arrDictionaryData = (Dictionary<string, object>[])obj;
      array = Array.CreateInstance(type.GetElementType(), arrDictionaryData.Length);

      for (var i = 0; i < arrDictionaryData.Length; i++)
      {
        var element = Activator.CreateInstance(type.GetElementType());
        foreach (var kv in arrDictionaryData[i])
        {
          SetObjectPropertyAndField(element, kv.Key, kv.Value);
        }

        array.SetValue(element, i);
      }
    }
    else
    {
      var arrData = ((IEnumerable)obj).OfType<object>().ToArray();
      array = Array.CreateInstance(objType.GetElementType(), arrData.Length);

      for (var i = 0; i < arrData.Length; i++)
      {
        array.SetValue(arrData[i], i);
      }
    }

    return array;
  }

  public static void SetObjectPropertyAndField(object newObj, string name, object value)
  {
    if (value == null)
      return;

    var valueType = value.GetType();
    var properties = newObj.GetType().GetProperties();
    var fileds = newObj.GetType().GetFields();

    var property = properties.Where(item => item.Name == name).FirstOrDefault();
    var field = fileds.Where(item => item.Name == name).FirstOrDefault();
    if (null != property)
    {
      if (valueType.IsArray || valueType == typeof(Dictionary<string, object>) || valueType == typeof(Dictionary<string, object>[]))
      {
        value = ObjectToGenericType(value, property.PropertyType);
      }

      property.SetValue(newObj, value, null);
    }
    else if (null != field)
    {
      if (valueType.IsArray || valueType == typeof(Dictionary<string, object>) || valueType == typeof(Dictionary<string, object>[]))
      {
        value = ObjectToGenericType(value, field.FieldType);
      }

      try
      {
        field.SetValue(newObj, value);
      }
      catch
      {
        Debug.LogErrorFormat("Type Error : Name : {0}, valueType : {1}, value : {2}", name, valueType, value);
      }
    }
  }

  /// <summary>
  /// 오브젝트의 크기가 변경될 때 마다 자동으로 CircleCollider2D의 크기를 재설정해주는 함수
  /// </summary>
  /// <param name="parentObject"></param>
  public static void FitCircleCollider2DToChildren(GameObject parentObject)
  {
    var bc = parentObject.GetComponent<CircleCollider2D>();
    if (bc == null)
    {
      bc = parentObject.AddComponent<CircleCollider2D>();
    }

    var bounds = new Bounds(Vector3.zero, Vector3.zero);
    var hasBounds = false;
    var renderers = parentObject.GetComponentsInChildren<Renderer>();

    foreach (var render in renderers)
    {
      if (hasBounds)
      {
        bounds.Encapsulate(render.bounds);
      }
      else
      {
        bounds = render.bounds;
        hasBounds = true;
      }
    }
    if (hasBounds)
    {
      bc.offset = bounds.center - parentObject.transform.position;
      bc.radius = bounds.extents.x;
    }
    else
    {
      bc.offset = Vector3.zero;
      bc.radius = 0;
    }
  }

  /// <summary>
  /// 오브젝트의 크기가 변경될 때 마다 자동으로 BoxCollider2D의 크기를 재설정해주는 함수
  /// </summary>
  /// <param name="parentObject"></param>
  public static void FitBoxCollider2DToChildren(GameObject parentObject)
  {
    var bc = parentObject.GetComponent<BoxCollider2D>();
    if (bc == null) { bc = parentObject.AddComponent<BoxCollider2D>(); }
    var bounds = new Bounds(Vector3.zero, Vector3.zero);
    var hasBounds = false;
    var renderers = parentObject.GetComponentsInChildren<Renderer>();
    foreach (var render in renderers)
    {
      if (hasBounds)
      {
        bounds.Encapsulate(render.bounds);
      }
      else
      {
        bounds = render.bounds;
        hasBounds = true;
      }
    }
    if (hasBounds)
    {
      bc.offset = bounds.center - parentObject.transform.position;
      bc.size = bounds.size;
    }
    else
    {
      bc.size = bc.offset = Vector3.zero;
      bc.size = Vector3.zero;
    }
  }

  /// <summary>
  /// 정규 표현식에 해당되는 문자인지 확인
  /// </summary>
  /// <param name="strCheckMsg"></param>
  /// <param name="pattern"></param>
  /// <returns></returns>
  public static bool CheckInputPattern(string strCheckMsg, string strPattern = "^\\S+[a-zA-Z0-9가-힣\u0020-\u007E]$")
  {
    var match = Regex.Match(strCheckMsg, strPattern);
    if (match.Success)
      return true;
    else
      return false;
  }

  public static string EncodeSHA256(string data)
  {
    SHA256 sha = new SHA256Managed();
    byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
    var stringBuilder = new StringBuilder();
    foreach (byte b in hash)
    {
      stringBuilder.AppendFormat("{0:x2}", b);
    }
    return stringBuilder.ToString();
  }
}

/// <summary>
/// 한글 헬퍼
/// </summary>
public class HangulHelper
{
  //////////////////////////////////////////////////////////////////////////////////////////////////// Field
  ////////////////////////////////////////////////////////////////////////////////////////// Private

  #region Field

  /// <summary>
  /// 초성 수
  /// </summary>
  public const int INITIAL_COUNT = 19;

  /// <summary>
  /// 중성 수
  /// </summary>
  public const int MEDIAL_COUNT = 21;

  /// <summary>
  /// 종성 수
  /// </summary>
  public const int FINAL_COUNT = 28;

  /// <summary>
  /// 한글 유니코드 시작 인덱스
  /// </summary>
  public const int HANGUL_UNICODE_START_INDEX = 0xac00;

  /// <summary>
  /// 한글 유니코드 종료 인덱스
  /// </summary>
  public const int HANGUL_UNICODE_END_INDEX = 0xD7A3;

  /// <summary>
  /// 초성 시작 인덱스
  /// </summary>
  public const int INITIAL_START_INDEX = 0x1100;

  /// <summary>
  /// 중성 시작 인덱스
  /// </summary>
  public const int MEDIAL_START_INDEX = 0x1161;

  /// <summary>
  /// 종성 시작 인덱스
  /// </summary>
  public const int FINAL_START_INDEX = 0x11a7;

  #endregion

  //////////////////////////////////////////////////////////////////////////////////////////////////// Method
  ////////////////////////////////////////////////////////////////////////////////////////// Static
  //////////////////////////////////////////////////////////////////////////////// Public

  #region 한글 여부 구하기 - IsHangul(char source)

  /// <summary>
  /// 한글 여부 구하기
  /// </summary>
  /// <param name="source">소스 문자</param>
  /// <returns>한글 여부</returns>
  public static bool IsHangul(char source)
  {
    if (HANGUL_UNICODE_START_INDEX <= source && source <= HANGUL_UNICODE_END_INDEX)
    {
      return true;
    }

    return false;
  }

  #endregion
  #region 한글 여부 구하기 - IsHangul(string source)

  /// <summary>
  /// 한글 여부 구하기
  /// </summary>
  /// <param name="source">소스 문자열</param>
  /// <returns>한글 여부</returns>
  public static bool IsHangul(string source)
  {
    bool result = false;

    for (int i = 0; i < source.Length; i++)
    {
      if (HANGUL_UNICODE_START_INDEX <= source[i] && source[i] <= HANGUL_UNICODE_END_INDEX)
      {
        result = true;
      }
      else
      {
        result = false;

        break;
      }
    }

    return result;
  }

  #endregion
  #region 한글 나누기 - DivideHangul(source)

  /// <summary>
  /// 한글 나누기
  /// </summary>
  /// <param name="source">소스 한글 문자</param>
  /// <returns>분리된 자소 배열</returns>
  public static char[] DivideHangul(char source)
  {
    char[] elementArray = null;

    if (IsHangul(source))
    {
      int index = source - HANGUL_UNICODE_START_INDEX;

      int initial = INITIAL_START_INDEX + index / (MEDIAL_COUNT * FINAL_COUNT);
      int medial = MEDIAL_START_INDEX + (index % (MEDIAL_COUNT * FINAL_COUNT)) / FINAL_COUNT;
      int final = FINAL_START_INDEX + index % FINAL_COUNT;

      if (final == 4519)
      {
        elementArray = new char[2];

        elementArray[0] = (char)initial;
        elementArray[1] = (char)medial;
      }
      else
      {
        elementArray = new char[3];

        elementArray[0] = (char)initial;
        elementArray[1] = (char)medial;
        elementArray[2] = (char)final;
      }
    }

    return elementArray;
  }

  public static string ExtractChoList(string source, string chosung = null)
  {
    string choList = string.Empty;

    for (int i = 0; i < source.Length; i++)
    {
      if (IsHangul(source[i]) == false) continue;
      int index = source[i] - HANGUL_UNICODE_START_INDEX;

      int jong = index % 28;
      index = (index - jong) / 28;

      int jung = index % 21;
      index = (index - jung) / 21;

      int cho = index;
      int initial = INITIAL_START_INDEX + index / (MEDIAL_COUNT * FINAL_COUNT);
      //Debug.Log(initial);
      choList += chosung[cho];
    }
    return choList;
  }
  //public static string chosung = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";

  #endregion

  /// <summary>
  /// 초성인지 체크하는 함수
  /// </summary>
  /// <param name="inputWord">입력 스트링</param>
  /// <returns></returns>
  public static bool CheckCho(string inputWord)
  {
    bool result = true;
    for (int i = 0; i < inputWord.Length; i++)
    {
      // inputWord[i]가 아스키 코드상 ㄱ과 ㅎ 사이의 값이면 isCho는 true 유지
      if (inputWord[i] >= 'ㄱ' && inputWord[i] <= 'ㅎ') continue;

      // 그렇지 않다면 초성이 아닌경우이므로 false
      result = false;
      break;
    }
    return result;
  }

  /// <summary>
  /// 존재여부 확인 <br/>
  /// isCho 파라미터는 <seealso cref="CheckCho(string)"/>로 먼저 체크한 후 해당 값을 파라미터로 사용 <br/>
  /// case1) <br/>
  /// isCho가 false이고<br/>
  /// inputWord가 사과나무 <br/>
  /// target이 감나무라면 isExist는 false를 리턴 <br/>
  /// target이 사과나무라면 isExist는 true를 리턴 <br/>
  /// target이 감사과나무라면 isExist는 true를 리턴 <br/>
  /// case2) <br/>
  /// isCho가 true이고 <br/>
  /// inputWord가 ㅅㄱㄴㅁ <br/>
  /// target이 감나무라면 isExist는 false를 리턴 <br/>
  /// target이 사과나무라면 isExist는 true를 리턴 <br/>
  /// target이 생강나무라면 isExist는 true를 리턴 <br/>
  /// </summary>
  /// <param name="isCho"><seealso cref="CheckCho(string)"/>로 먼저 체크한 후 해당 값을 파라미터로 사용</param>
  /// <param name="inputWord">입력 스트링</param>
  /// <param name="target">타겟</param>
  /// <returns></returns>
  public static bool CheckExist(bool isCho, string inputWord, string target)
  {
    bool isExist = true;
    // 초성일 때
    if (isCho == true)
    {
      // target의 초성 추출
      var targetCho = ExtractChoList(target, "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ");
      // inputWord가 targetCho에 포함되는지 판단
      if (targetCho.Contains(inputWord) == false)
        isExist = false;
    }
    // 초성이 아닐때 때
    else
    {
      // inputWord가 영어든 아니든 대소문자 구분, 빈칸 구분 없이 taget에 포함되는지 체크
      if (target.IndexOf(inputWord, 0, StringComparison.OrdinalIgnoreCase) < 0
       && target.Replace(" ", string.Empty).IndexOf(inputWord, 0, StringComparison.OrdinalIgnoreCase) < 0)
        isExist = false;
    }
    return isExist;

  }
}

public class RTSViewModelCommonHelper
{
  [Obsolete]
  public enum ViewModelExKToggleSetType
  {
    None = -1,
    IsOn,
    Set,
    SetWithoutCallback,
    SetWithoutCallbackAndTransition,
  }
  [Obsolete]
  public static void SetKToggleSet(PropertyType propertyType, KToggle targets, PropertyBase property, int index, string key, ViewModelExKToggleSetType setType)
    => SetKToggleSet(propertyType, targets, property, index, key, setType, out _);
  [Obsolete]
  public static void SetKToggleSet(PropertyType propertyType, KToggle targets, PropertyBase property, int index, string key, ViewModelExKToggleSetType setType, out bool value)
  {
    value = default;
    switch (propertyType)
    {
      case PropertyType.Single:
        {
          var newValue = property as PropertyBoolean;
          if (newValue == null) return;
          value = newValue.NewValue;
        }
        break;
      case PropertyType.List:
        {
          var newValue = property as PropertyListBoolean;
          if (newValue == null) return;
          if (newValue.Count == 0 || newValue.Count <= index) return;
          value = newValue[index];
        }
        break;
      case PropertyType.Dictionary:
        {
          var newValue = property as PropertyMapStringBoolean;
          if (newValue == null) return;
          if (newValue.ContainsKey(key) == false) return;
          value = newValue[key];
        }
        break;
    }
    Set(value);
    void Set(bool value)
    {
      switch (setType)
      {
        case ViewModelExKToggleSetType.IsOn:
          targets.isOn = value;
          break;
        case ViewModelExKToggleSetType.Set:
          targets.Set(value, true);
          break;
        case ViewModelExKToggleSetType.SetWithoutCallback:
          targets.Set(value, false);
          break;
        case ViewModelExKToggleSetType.SetWithoutCallbackAndTransition:
          targets.Set(value, false, false);
          break;
      }
    }
  }

  #region KButton

  public static void SetKButtonInteractable(KButton targets, PropertyBase property, PropertyType propertyType, int index, string key)
    => SetKButtonInteractable(targets, property, propertyType, index, key, out _);
  public static void SetKButtonInteractable(KButton targets, PropertyBase property, PropertyType propertyType, int index, string key, out bool interactable)
  {
    switch (propertyType)
    {
      case PropertyType.Single:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyBoolean, out interactable) == false) return;
        break;
      case PropertyType.List:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyListBoolean, index, out interactable) == false) return;
        break;
      case PropertyType.Dictionary:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyMapStringBoolean, key, out interactable) == false && ViewModelCommonHelper.ValidCheck(property as PropertyMapIntegerBoolean, key, out interactable) == false) return;
        break;
      default: interactable = false; break;
    }
    if (targets == null) return;
    targets.interactable = interactable;
  }
  #endregion

  #region SpriteAtlas
  public static void SetSpriteAtlas(AtlasImage targets, PropertyBase property, PropertyType propertyType, int index, string key)
    => SetSpriteAtlas(targets, property, propertyType, index, key, out _);
  public static void SetSpriteAtlas(AtlasImage targets, PropertyBase property, PropertyType propertyType, int index, string key, out SpriteAtlas spriteAtlas)
  {
    switch (propertyType)
    {
      case PropertyType.Single:
        if (ViewModelCommonHelper.ValidCheck(property as PropertySpriteAtlas, out spriteAtlas) == false) return;
        break;
      case PropertyType.List:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyListSpriteAtlas, index, out spriteAtlas) == false) return;
        break;
      case PropertyType.Dictionary:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyMapStringSpriteAtlas, key, out spriteAtlas) == false) return;
        break;
      default:
        spriteAtlas = default;
        break;
    }
    if (targets == null) return;
    targets.spriteAtlas = spriteAtlas;
  }
  #endregion

  #region AtlasImage
  public static void SetAtlasImageSpriteName(AtlasImage targets, PropertyBase property, PropertyType propertyType, int index, string key, string prefix, string suffix, bool bNativeSize)
    => SetAtlasImageSpriteName(targets, property, propertyType, index, key, prefix, suffix, bNativeSize, out _);
  public static void SetAtlasImageSpriteName(AtlasImage targets, PropertyBase property, PropertyType propertyType, int index, string key, string prefix, string suffix, bool bNativeSize, out string spriteName)
  {
    switch (propertyType)
    {
      case PropertyType.Single:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyString, out spriteName) == false) return;
        break;
      case PropertyType.List:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyListString, index, out spriteName) == false) return;
        break;
      case PropertyType.Dictionary:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyMapStringString, key, out spriteName) == false) return;
        break;
      default:
        spriteName = default;
        break;
    }
    if (targets == null) return;
    targets.spriteName = prefix + spriteName + suffix;
    if (bNativeSize == true)
      targets.SetNativeSize();
  }
  #endregion

  #region ModifiedShadow
  public static void SetModifiedShadowEffectColor(ModifiedShadow targets, PropertyBase property, PropertyType propertyType, int index, string key)
    => SetModifiedShadowEffectColor(targets, property, propertyType, index, key, out _);
  public static void SetModifiedShadowEffectColor(ModifiedShadow targets, PropertyBase property, PropertyType propertyType, int index, string key, out Color effectColor)
  {
    switch (propertyType)
    {
      case PropertyType.Single:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyColor, out effectColor) == false) return;
        break;
      case PropertyType.List:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyListColor, index, out effectColor) == false) return;
        break;
      case PropertyType.Dictionary:
        if (ViewModelCommonHelper.ValidCheck(property as PropertyMapStringColor, key, out effectColor) == false) return;
        break;
      default:
        effectColor = default;
        break;
    }
    if (targets == null) return;
    targets.effectColor = effectColor;
  }
  #endregion
}