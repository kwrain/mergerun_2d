using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

/**
* PropertyClass.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 07월 15일 오전 12시 57분
*/

[CreateAssetMenu]
public class PropertyContextClass : PropertyBase<ContextClass>
{
  [HideInInspector] public new ContextClass defaultValue; // defaultValue 사용 금지.
  [SerializeReference] public new ContextClass runtimeValue;
  public new ContextClass RuntimeValue
  {
    get
    {
      if (model.isInit == false) model.Init();
      return runtimeValue;
    }
    set
    {
      if (model.isInit == false) model.Init();
      runtimeValue = value;
      runtimeValue.Notify();      
    }
  }
  public new ContextClass NewValue => RuntimeValue;
  [Tooltip(nameof(ContextClass) + "를 상속 받는 클래스 이름, Nested Class인 경우 '.'으로 구분")]
  [SerializeField] private string assemblyName = "Assembly-CSharp";
  public string AssemblyName => assemblyName;
  [SerializeField] private string contextClassName;
  public string ContextClassName => contextClassName;
  public override void InitValue()
  {
#if UNITY_EDITOR
    if(isResetting == false) return;
    isResetting = false;
#endif
    runtimeValue = Activator.CreateInstance(Type.GetType(contextClassName), new[] { this }) as ContextClass;
  }

  public override void Resetting()
  {
    runtimeValue = null;
#if UNITY_EDITOR
    isResetting = true;
#endif
  }

  public override void ResetRuntimeValue()
  {
    runtimeValue = Activator.CreateInstance(Type.GetType(contextClassName), new[] { this }) as ContextClass;
  }
}

[System.Serializable]
public abstract class ContextClass
{
  private List<string> fieldNameList;
  protected PropertyContextClass propertyClass;
  public ContextClass(PropertyContextClass propertyClass)
  {
    this.propertyClass = propertyClass;

    var fieldInfos = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
    fieldNameList = new();
    foreach (var fieldInfo in fieldInfos)
    {
      if (fieldInfo.FieldType == typeof(PropertyContextClass)) continue;
      fieldNameList.Add(fieldInfo.Name);
    }
  }

  public void Notify()
  {
    if(fieldNameList == null || fieldNameList.Count == 0)
      return;
    foreach(var fieldName in fieldNameList)
      Notify(fieldName);
  }

  public void Notify(string fieldName)
  {
    string context = propertyClass.ContextKey + fieldName;
    if(Binding.valueChangedList.ContainsKey(context) == false) return;
    for (int i = 0; i < Binding.valueChangedList[context].Count; i++)
      Binding.valueChangedList[context][i].OnPropertyChanged(context, propertyClass);
  }
}

public static class PropertyClassDataExtensionMethod
{
}

// 예시
/// <summary>
/// PropertyClass의 typeName을 "ExamplePropertyClassData"를 입력 <br/>
/// Vm에서는 PropertyInfoContextType을 PropertyName로 선택, stringKey에 속성 필드 이름 지정 <br/>
/// 예시에서는 a의 값 변경을 알리고 Vm에서 받기 위해선, stringKey를 "A"로 지정한다. <br/>
/// Model에서 접근하기 위해서 PropertyClass.RuntimeValue as ExamplePropertyClassData 를 속성필드로 만든 후 사용. <br/>
/// 아래 TestModel class 참고
/// </summary>
public class ExamplePropertyClassData : ContextClass
{
  [SerializeField] private ContextList<int> list;
  [Context] public ContextList<int> List => list;

  public enum StringMapKey
  {
    Good,
    Bad,
    UU,
  }
  [SerializeField] private ContextDictionary<int> dictionary;
  [Context][EnumKey(typeof(StringMapKey))] public ContextDictionary<int> Dictionary => dictionary;
  public ExamplePropertyClassData(PropertyContextClass propertyClass) : base(propertyClass)
  {
    list = new(this, nameof(List));
    list.Add(0);
    list[0]++;
    dictionary = new(this, nameof(Dictionary));
    dictionary[nameof(StringMapKey.Good)] = 714;
  }

  [SerializeField] private int a;
  [Context]
  public int A
  {
    get => a;
    set
    {
      if (a == value) return;
      a = value;
      Notify(nameof(A));
    }
  }
  [SerializeField] private int b;
  [Context]
  public int B
  {
    get => b;
    set
    {
      if (b == value) return;
      b = value;
      Notify(nameof(B));
    }
  }
  private int c;
  [Context]
  public int C
  {
    get => c;
    set
    {
      if (c == value) return;
      c = value;
      Notify(nameof(C));
    }
  }
}

/// <summary>
/// 예시
/// </summary>
public class TestModel /* : ModelBase */
{
  [SerializeField] private PropertyContextClass propertyClass;
  public ExamplePropertyClassData ExamplePropertyClassData => propertyClass.RuntimeValue as ExamplePropertyClassData;

  public void Test()
  {
    ExamplePropertyClassData.A++;
    ExamplePropertyClassData.List.Add(10);
    ExamplePropertyClassData.List[0] = 20;
    ExamplePropertyClassData.Dictionary["Good"] = 100;
  }
}