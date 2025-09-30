using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* ContextList.cs
* 작성자 : anonymous
* 작성일 : 2022년 07월 18일 오전 2시 12분
*/

[System.Serializable]
public class ContextList<TValue>
{
  private string context;
  private ContextClass propertyData;
  [SerializeField] private List<TValue> list = new();
  public ContextList(ContextClass propertyData, string context)
  {
    this.propertyData = propertyData;
    this.context = context;
  }
  [SerializeField]
  public TValue this[int index]
  {
    get
    {
      if (index >= list.Count)
      {
        Debug.Log($"{index} out of bind index");
        return default;
      }
      return list[index];
    }
    set
    {
      if (index >= list.Count)
      {
        Debug.Log($"{index} out of bind index");
        return;
      }
      if(EqualityComparer<TValue>.Default.Equals(list[index], value) == true)
        return;
      list[index] = value;
      propertyData.Notify(context + index);
    }
  }

  public int Count => list.Count;
  public bool Contains(TValue value) => list.Contains(value);

  public void Add(TValue item)
  {
    list.Add(item);
    propertyData.Notify(context + (list.Count - 1));
  }

  public void Remove(TValue item)
  {
    list.Remove(item);
  }
  // 리스트 메서드 중 추가적으로 필요한 메서드는 추가할것.
}