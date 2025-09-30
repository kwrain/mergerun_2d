using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;

/**
* VmObjectSetActive.cs
* 작성자 : doseon@fairsutdios.kr (LEE DO SEON)
* 작성일 : 2022년 07월 13일 오후 11시 53분
*/
[AddComponentMenu("Vm/[Vm]SetActive")]
[DisallowMultipleComponent]
[SupportedProperty
  (
    typeof(PropertyInteger), typeof(PropertyListInteger), typeof(PropertyMapIntegerInteger), typeof(PropertyMapStringInteger), typeof(PropertyPlayerPrefsInteger),
    typeof(PropertyUInteger), typeof(PropertyListUInteger), typeof(PropertyMapStringUInteger),
    typeof(PropertyFloat), typeof(PropertyListFloat), typeof(PropertyMapStringFloat),
    typeof(PropertyLong), typeof(PropertyListLong), typeof(PropertyMapStringLong),
    typeof(PropertyBoolean), typeof(PropertyListBoolean), typeof(PropertyMapStringBoolean), typeof(PropertyPlayerPrefsBoolean),
    typeof(PropertyString), typeof(PropertyListString), typeof(PropertyMapStringString), typeof(PropertyPlayerPrefsString),
    typeof(PropertyContextClass), typeof(int), typeof(long), typeof(bool), typeof(string)
  )
]
public class VmSetActive : VmBase<GameObject, VmSetActive.Param>
{
  private bool[] args = null;
  public override void SetView() => view = gameObject;
  /// <summary>
  /// 프로퍼티가 변경되어 View가 활성화되었는지 여부 <br/>
  /// SetActive는 SetActive가 호출될 때 OnEnable이 호출되는데  <br/>
  /// 이로인해 <seealso cref="UpdateView"/> 이후 <seealso cref="UpdateViewActivate"/>가 이중 호출되는 경우가 있음
  /// 따라서 UpdateView에 의해 View가 활성화되는 경우 이를 막기 위한 플래그값 <br/>
  /// <br/>
  /// cf. <seealso cref="VmSetInteractable"/>는 게임오브젝트가 활성화되는 처리가 아니기 때문에 필요없음
  /// </summary>
  private bool isActivatedByProperty = false;
  protected override void Initialize()
  {
    base.Initialize();
    foreach (var pInfo in pInfos)
      pInfo.Param.CreateExpectedValue(pInfo.Property, pInfo.PropertyName);

    args = new bool[pInfos.Length];
  }

  public override void UpdateViewActivate()
  {
    if (isActivatedByProperty == true)
    {
      isActivatedByProperty = false;
      return;
    }
    bool result = CheckArgs();
    view.SetActive(result);
  }

  public override void UpdateView(string context)
  {
    bool result = CheckArgs(context);
    if (result == true)
    {
      isActivatedByProperty = true;
    }
    view.SetActive(result);
  }

  private bool CheckArgs(string context = null)
  {
    // context 유무 => context가 존재한다는 것은 프로퍼티의 변경에 의해 UpdateView가 호출되는것
    bool hasContext = !string.IsNullOrEmpty(context);
    bool result = view.activeSelf;
    // 루프 결과가 반드시 false인지에 대한 여부
    bool isResultMustBeFalse = false;
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      var param = pInfo.Param;

      // context가 없으면 모든 args를 업데이트한다
      // context가 있으면 해당하는 arg만 업데이트 한다.
      bool updateArg = hasContext == false || (hasContext == true && pInfo.Context == context);
      if (updateArg)
      {
        args[i] = param.GetCompareResult(pInfo.Property, pInfo.Index, pInfo.StringKey);
      }

      // 첫 번째는 로직 비교 없이 바로 result가 되고 다음을 비교하기 위해 넘어간다
      if (i == 0)
      {
        result = args[i];
        continue;
      }

      // 이 루프의 결과가 반드시 false라도 루프를 빠져나오지 않고, 다음으로 넘겨 updateArgs 처리를 마저 진행한다.
      // 만약에 루프를 빠져나오게되면 상황에 따라 args의 특정 요소가 업데이트되지 않는다.
      if (isResultMustBeFalse == true)
      {
        continue;
      }

      param.Logic(ref result, args[i]);
      if (param.ConditionalLogic == ConditionalLogicType.AND
      && result == false)
      {
        // 현재 parm의 조건로직이 AND이지만 현재까지의 결과가 false라면 이후 루프에 의한 결과는 항상 false이다.
        isResultMustBeFalse = true;
      }
    }
    return result;
  }

  public void GetPropertyGetter<T, Out>(T target, Out outt, string propertyName, out Func<T, Out> getter)
  {
    var method = target.GetType().GetProperty(propertyName).GetMethod;
    var instanceParameter = Expression.Parameter(typeof(T));
    var convertExp = Expression.Convert(instanceParameter, method.DeclaringType);
    var body = Expression.Call(convertExp, method);
    var lambda = Expression.Lambda<Func<T, Out>>(body, instanceParameter);
    getter = lambda.Compile();
  }

  protected override void OnDisable() { }

  [System.Serializable]
  public class Param : PropertyInfoParamBase
  {
    [Tooltip("조건 로직 타입, 0번 프로퍼티는 무시됨")]
    [SerializeField] private ConditionalLogicType conditionalLogic;
    public ConditionalLogicType ConditionalLogic => conditionalLogic;
    [Tooltip("비교 타입, 지원하지않는 비교는 무시됨")]
    [SerializeField] private ComparisonType comparison;
    public ComparisonType Comparison => comparison;
    [Tooltip("예상되는 프로퍼티값.\nbool의 경우 true 또는 false 입력\n그 외 각 프로퍼티 타입별 값을 입력")]
    [SerializeField] private string expected;
    public string Expected => expected;
    [Tooltip("비교를 스트링 값으로 할경우 체크, 기본 체크 해제")]
    [SerializeField] private bool isExpectedString;
    public bool IsExpectedString => isExpectedString;


    private ExpectedValue expectedValue;
    private GenericClassValue classValue;
    public ExpectedValue ExpectedValue => expectedValue;
    public GenericClassValue ClassValue => classValue;
    public void CreateExpectedValue(PropertyBase property, string propertyName)
    {
      switch (property)
      {
        case PropertyInteger pInt:
        case PropertyListInteger pListInt:
        case PropertyMapIntegerInteger pMapIntInt:
        case PropertyMapStringInteger pMapStringInt:
        case PropertyPlayerPrefsInteger pPlayerPrefsInt:
          {
            if (int.TryParse(expected, out int value) == true)
              expectedValue = new ExpectedValue<int>(value);
          }
          break;
        case PropertyUInteger pUInt:
        case PropertyListUInteger pListUInt:
        case PropertyMapStringUInteger pMapStringUInt:
          {
            if (uint.TryParse(expected, out uint value) == true)
              expectedValue = new ExpectedValue<uint>(value);
          }
          break;
        case PropertyFloat pFloat:
        case PropertyListFloat pListFloat:
        case PropertyMapStringFloat pMapStringFloat:
          {
            if (float.TryParse(expected, out float value) == true)
              expectedValue = new ExpectedValue<float>(value);
          }
          break;
        case PropertyLong pLong:
        case PropertyListLong pListLong:
        case PropertyMapStringLong pMapStringLong:
          {
            if(long.TryParse(expected, out long value) == true)
              expectedValue = new ExpectedValue<long>(value);
          }
          break;
        case PropertyBoolean pBool:
        case PropertyListBoolean pListBool:
        case PropertyMapStringBoolean pMapStringBool:
        case PropertyPlayerPrefsBoolean pPlayerPrefsBool:
          {
            if (bool.TryParse(expected, out bool value) == true)
              expectedValue = new ExpectedValue<bool>(value);
          }
          break;
        case PropertyString pString:
        case PropertyListString pListString:
        case PropertyMapStringString pListStringString:
        case PropertyPlayerPrefsString pPlayerPrefsString:
          {
            expectedValue = new ExpectedValue<string>(expected);
          }
          break;
        case PropertyContextClass pClass:
          {
            var type = pClass.NewValue.GetType();
            var propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            var propertyType = propertyInfo.PropertyType;
            if(propertyType == typeof(int))
            {
              if (int.TryParse(expected, out int value) == true)
              {
                expectedValue = new ExpectedValue<int>(value);
                classValue = new GenericClassValue<int>(pClass.NewValue, propertyName);
              }
            }
            else if (propertyType == typeof(float))
            {
              if (float.TryParse(expected, out float value) == true)
              {
                expectedValue = new ExpectedValue<float>(value);
                classValue = new GenericClassValue<float>(pClass.NewValue, propertyName);
              }
            }
            else if(propertyType == typeof(long))
            {
              if (long.TryParse(expected, out long value) == true)
              {
                expectedValue = new ExpectedValue<long>(value);
                classValue = new GenericClassValue<long>(pClass.NewValue, propertyName);
              }
            }
            else if(propertyType == typeof(bool))
            {
              if (bool.TryParse(expected, out bool value) == true)
              {
                expectedValue = new ExpectedValue<bool>(value);
                classValue = new GenericClassValue<bool>(pClass.NewValue, propertyName);
              }
            }
            else if(propertyType == typeof(string))
            {
              expectedValue = new ExpectedValue<string>(expected);
              classValue = new GenericClassValue<string>(pClass.NewValue, propertyName);
            }
          }
          break;
      }
    }

    public void Logic(ref bool result, bool arg)
    {
      switch (conditionalLogic)
      {
        case ConditionalLogicType.AND: result = result && arg; break;
        case ConditionalLogicType.OR: result = result || arg; break;
      }
    }

    #region Compare
    public bool GetCompareResult(PropertyBase property, int index, string key)
    {
      return expectedValue switch
      {
        ExpectedValue<int> intValue => CompareInt(property, index, key, intValue.Expected),
        ExpectedValue<uint> uintValue => CompareUInt(property, index, key, uintValue.Expected),
        ExpectedValue<float> floatValue => CompareFloat(property, index, key, floatValue.Expected),
        ExpectedValue<long> longValue => CompareLong(property, index, key, longValue.Expected),
        ExpectedValue<bool> boolValue => CompareBool(property, index, key, boolValue.Expected),
        ExpectedValue<string> stringValue => CompareString(property, index, key, stringValue.Expected),
        _ => false,
      };
    }

    private bool CompareInt(PropertyBase property, int index, string key, int rhs)
    {
      int lhs = GetInt(property, index, key);
      if (isExpectedString == true)
        return CompareString(lhs.ToString(), expected);

      return CompareInt(lhs, rhs);
    }

    private bool CompareInt(int lhs, int rhs)
    {
      return comparison switch
      {
        ComparisonType.EqualTo => lhs == rhs,
        ComparisonType.NotEqual => lhs != rhs,
        ComparisonType.GreaterThan => lhs > rhs,
        ComparisonType.LessThan => lhs < rhs,
        ComparisonType.GreaterThanOrEqualTo => lhs >= rhs,
        ComparisonType.LessThenOrEqualTo => lhs <= rhs,
        _ => false,
      };
    }

    private int GetInt(PropertyBase property, int index, string key) => property switch
    {
      PropertyInteger pInt => pInt.NewValue,
      PropertyListInteger pListInteger => pListInteger[index],
      PropertyMapIntegerInteger pMapIntegerInteger => pMapIntegerInteger[index],
      PropertyMapStringInteger pMapStringInteger => pMapStringInteger[key],
      PropertyPlayerPrefsInteger pPlayerPrefsInt => pPlayerPrefsInt[key],
      PropertyContextClass pClass => (ClassValue as GenericClassValue<int>).GetValue(),
      _ => 0,
    };

    private bool CompareUInt(PropertyBase property, int index, string key, uint rhs)
    {
      uint lhs = GetUInt(property, index, key);
      if (isExpectedString == true)
        return CompareString(lhs.ToString(), expected);

      return CompareUInt(lhs, rhs);
    }

    private bool CompareUInt(uint lhs, uint rhs)
    {
      return comparison switch
      {
        ComparisonType.EqualTo => lhs == rhs,
        ComparisonType.NotEqual => lhs != rhs,
        ComparisonType.GreaterThan => lhs > rhs,
        ComparisonType.LessThan => lhs < rhs,
        ComparisonType.GreaterThanOrEqualTo => lhs >= rhs,
        ComparisonType.LessThenOrEqualTo => lhs <= rhs,
        _ => false,
      };
    }

    private uint GetUInt(PropertyBase property, int index, string key) => property switch
    {
      PropertyUInteger pInt => pInt.NewValue,
      PropertyListUInteger pListInteger => pListInteger[index],
      PropertyMapStringUInteger pMapStringInteger => pMapStringInteger[key],
      PropertyContextClass pClass => (ClassValue as GenericClassValue<uint>).GetValue(),
      _ => 0,
    };

    private bool CompareFloat(PropertyBase property, int index, string key, float rhs)
    {
      float lhs = GetFloat(property, index, key);
      if (isExpectedString == true)
        return CompareString(lhs.ToString(), expected);

      return CompareFloat(lhs, rhs);
    }

    private bool CompareFloat(float lhs, float rhs)
    {
      return comparison switch
      {
        ComparisonType.EqualTo => lhs == rhs,
        ComparisonType.NotEqual => lhs != rhs,
        ComparisonType.GreaterThan => lhs > rhs,
        ComparisonType.LessThan => lhs < rhs,
        ComparisonType.GreaterThanOrEqualTo => lhs >= rhs,
        ComparisonType.LessThenOrEqualTo => lhs <= rhs,
        _ => false,
      };
    }

    private float GetFloat(PropertyBase property, int index, string key) => property switch
    {
      PropertyFloat pInt => pInt.NewValue,
      PropertyListFloat pListInteger => pListInteger[index],
      PropertyMapStringFloat pMapStringInteger => pMapStringInteger[key],
      //PropertyPlayerPrefsInteger pPlayerPrefsInt => pPlayerPrefsInt[pInfo.StringKey],
      PropertyContextClass pClass => (ClassValue as GenericClassValue<float>).GetValue(),
      _ => 0,
    };

    private bool CompareLong(PropertyBase property, int index, string key, long rhs)
    {
      long lhs = GetLong(property, index, key);
      if (isExpectedString == true)
        return CompareString(lhs.ToString(), expected);

      return CompareLong(lhs, rhs);
    }

    private bool CompareLong(long lhs, long rhs)
    {
      return comparison switch
      {
        ComparisonType.EqualTo => lhs == rhs,
        ComparisonType.NotEqual => lhs != rhs,
        ComparisonType.GreaterThan => lhs > rhs,
        ComparisonType.LessThan => lhs < rhs,
        ComparisonType.GreaterThanOrEqualTo => lhs >= rhs,
        ComparisonType.LessThenOrEqualTo => lhs <= rhs,
        _ => false,
      };
    }

    private long GetLong(PropertyBase property, int index, string key) => property switch
    {
      PropertyLong pInt => pInt.NewValue,
      PropertyListLong pListInteger => pListInteger[index],
      PropertyMapStringLong pMapStringInteger => pMapStringInteger[key],
      //PropertyPlayerPrefsInteger pPlayerPrefsInt => pPlayerPrefsInt[pInfo.StringKey],
      PropertyContextClass pClass => (ClassValue as GenericClassValue<long>).GetValue(),
      _ => 0,
    };

    private bool CompareBool(PropertyBase property, int index, string key, bool rhs)
    {
      bool lhs = GetBool(property, index, key);
      if (isExpectedString == true)
        return CompareString(lhs.ToString(), expected);

      return CompareBool(lhs, rhs);
    }

    private bool CompareBool(bool lhs, bool rhs)
    {
      return comparison switch
      {
        ComparisonType.EqualTo => lhs == rhs,
        ComparisonType.NotEqual => lhs != rhs,
        _ => false,
      };
    }

    private bool GetBool(PropertyBase property, int index, string key) => property switch
    {
      PropertyBoolean pBool => pBool.NewValue,
      PropertyListBoolean pListBool => pListBool[index],
      PropertyMapStringBoolean pMapStringBool => pMapStringBool[key],
      PropertyPlayerPrefsBoolean pPlayerPrefsBool => pPlayerPrefsBool[key],
      PropertyContextClass pClass => (ClassValue as GenericClassValue<bool>).GetValue(),
      _ => false,
    };

    private bool CompareString(PropertyBase property, int index, string key, string rhs)
    {
      string lhs = GetString(property, index, key);
      if (lhs == null) return false;
      return CompareString(lhs, rhs);
    }

    private bool CompareString(string lhs, string rhs)
    {
      // expected 가 null or empty 일 때
      if(string.IsNullOrEmpty(rhs) == true)
      {
        return comparison switch
        {
          ComparisonType.EqualTo => string.IsNullOrEmpty(lhs), // lhs 가 null or empty 면 true
          ComparisonType.NotEqual => !string.IsNullOrEmpty(lhs), // lhs 가 null or empty 가 아니면 false
          _ => false,
        };
      }
      else
      {
        return comparison switch
        {
          ComparisonType.EqualTo => lhs == rhs,
          ComparisonType.NotEqual => lhs != rhs,
          _ => false,
        };
      }
    }

    private string GetString(PropertyBase property, int index, string key) => property switch
    {
      PropertyString pString => pString.NewValue,
      PropertyListString pListString => pListString[index],
      PropertyMapStringString pMapStringString => pMapStringString[key],
      PropertyPlayerPrefsString pPlayerPrefsString => pPlayerPrefsString[key],
      PropertyContextClass pClass => (ClassValue as GenericClassValue<string>).GetValue(),
      _ => null,
    };
    #endregion
  }

  public abstract class ExpectedValue
  {
  }

  public class ExpectedValue<TValue> : ExpectedValue
  {
    private TValue expected;
    public TValue Expected { get => expected; private set => expected = value; }
    public ExpectedValue(TValue expected)
    {
      this.expected = expected;
    }
  }

  public enum ComparisonType
  {
    EqualTo = 0,
    NotEqual = 1,
    GreaterThan = 2,
    LessThan = 3,
    GreaterThanOrEqualTo = 4,
    LessThenOrEqualTo = 5,
  }
  public enum ConditionalLogicType
  {
    AND = 0,
    OR = 1,
  }
}