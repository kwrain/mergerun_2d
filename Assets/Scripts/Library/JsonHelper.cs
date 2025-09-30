using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// - Require : JsonFx.Json.dll
/// - Dictionary 지원 (Class안에 Member변수)
/// Json Library Wrapper Class
/// </summary>
public class JsonHelper
{
  /// <summary>
  /// UTF-8의 BOM 및 특정 문자는 무시하도록한다.
  /// </summary>
  /// <param name="json"></param>
  /// <returns></returns>
  static private string PreProcess(string json)
  {
    if (json[0] == '\uFEFF')
      json = json.Trim(new char[] { '\uFEFF', '\u200B' });
    return json;
  }

  static public Dictionary<string, string> Deserialize(string json)
  {
    return Deserialize<Dictionary<string, string>>(json);
  }

  static public object Deserialize(string json, Type type)
  {
    return JsonConvert.DeserializeObject(json, type);
  }

  static public T Deserialize<T>(string json)
  {
    return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
    {
      Error = (sender, args) =>
      {
        Console.WriteLine(args.ErrorContext.Error.Message);
        args.ErrorContext.Handled = true;
      }
    });
  }

  static public T DeserializeFromFile<T>(string path)
  {
    if (!File.Exists(path))
      return default;

    using (StreamReader reader = new StreamReader(path))
    {
      return Deserialize<T>(reader.ReadToEnd());
    }
  }

  static public string Serialize(object obj)
  {
    return JsonConvert.SerializeObject(obj);
    //return JsonUtility.ToJson(obj);
  }

  static public string SerializeToPath(object obj, string path)
  {
    string dirPath = Path.GetDirectoryName(path);
    if (!Directory.Exists(dirPath))
      Directory.CreateDirectory(dirPath);

    string json = Serialize(obj);
    StreamWriter sw = new StreamWriter(path);
    sw.Write(json);
    sw.Close();

    return json;
  }

  //[SerializeField]
  //private class Wrapper<T>
  //{
  //  public T[] items;
  //}
}

