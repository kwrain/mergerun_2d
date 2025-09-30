using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;

/**
* ContextDictionary.cs
* 작성자 : doseon@fairsutdios.kr (LEE DO SEON)
* 작성일 : 2022년 07월 18일 오전 2시 14분
*/

[System.Serializable]
public class ContextDictionary<TValue>
{
  private string context;
  private ContextClass propertyData;
  [SerializeField] public GenericDictionary<string, TValue> dictionary = new();
  public ContextDictionary(ContextClass propertyData, string context)
  {
    this.propertyData = propertyData;
    this.context = context;
  }

  public TValue this[string key]
  {
    get
    {
      if(dictionary.ContainsKey(key) == false)
      {
        Debug.LogError($"{key} is not in dictionary");
        return default;
      }
      return dictionary[key];
    }
    set
    {
      if (dictionary.ContainsKey(key) == true 
        && EqualityComparer<TValue>.Default.Equals(dictionary[key], value) == true)
        return;
      dictionary[key] = value;
      propertyData.Notify(context + key);
    }
  }
  public int Count => dictionary.Count;
  public bool ContainsKey(string key) => dictionary.ContainsKey(key);
  public ICollection<string> Keys => dictionary.Keys;
  public ICollection<TValue> Values => dictionary.Values;

  public void Add(string key, TValue value)
  {
    if(dictionary.ContainsKey(key) == true)
    {
      Debug.LogError($"{key} already exists in dictionary");
      return;
    }
    dictionary.Add(key, value);
    propertyData.Notify(context + key);
  }
}