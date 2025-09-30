using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;
using UnityEngine.UI;
using static VmSetActive;

/**
* VmSetInteractable.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 08월 26일 오전 10시 21분
*/

[AddComponentMenu("Vm/[Vm]SetInteractable")]
[DisallowMultipleComponent]
[SupportedProperty
  (
    typeof(PropertyInteger), typeof(PropertyListInteger), typeof(PropertyMapStringInteger), typeof(PropertyPlayerPrefsInteger),
    typeof(PropertyUInteger), typeof(PropertyListUInteger), typeof(PropertyMapStringUInteger),
    typeof(PropertyLong), typeof(PropertyListLong), typeof(PropertyMapStringLong),
    typeof(PropertyBoolean), typeof(PropertyListBoolean), typeof(PropertyMapStringBoolean), typeof(PropertyPlayerPrefsBoolean),
    typeof(PropertyString), typeof(PropertyListString), typeof(PropertyMapStringString), typeof(PropertyPlayerPrefsString),
    typeof(PropertyContextClass), typeof(int), typeof(long), typeof(bool), typeof(string)
  )
]
public class VmSetInteractable : VmBase<Selectable, VmSetInteractable.Param>
{
  private Action<Selectable, bool> setter;
  private Func<Selectable, bool> getter;
  private bool[] args = null;
  protected override void Initialize()
  {
    base.Initialize();
    foreach (var pInfo in pInfos)
      pInfo.Param.CreateExpectedValue(pInfo.Property, pInfo.PropertyName);

    GetPropertySetter(view, "interactable", out setter);
    GetPropertyGetter(view, "interactable", out getter);
    args = new bool[pInfos.Length];
  }

  public override void UpdateViewActivate()
  {
    bool result = CheckArgs();
    setter(view, result);
  }

  public override void UpdateView(string context)
  {
    bool result = CheckArgs(context);
    setter(view, result);
  }

  private bool CheckArgs(string context = null)
  {
    // context 유무 => context가 존재한다는 것은 프로퍼티의 변경에 의해 UpdateView가 호출되는것
    bool hasContext = !string.IsNullOrEmpty(context);
    bool result = getter(view);
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

  public void GetPropertySetter<T, In>(T target, string propertyName, out Action<T, In> setter)
  {
    var method = target.GetType().GetProperty(propertyName).SetMethod;
    var instanceParameter = Expression.Parameter(typeof(T));
    var argParameter = Expression.Parameter(typeof(In));
    var convertExp = Expression.Convert(instanceParameter, method.DeclaringType);
    var body = Expression.Call(convertExp, method, argParameter);
    var lambda = Expression.Lambda<Action<T, In>>(body, instanceParameter, argParameter);
    setter = lambda.Compile();
  }
  public void GetPropertyGetter<T, Out>(T target, string propertyName, out Func<T, Out> getter)
  {
    var method = target.GetType().GetProperty(propertyName).GetMethod;
    var instanceParameter = Expression.Parameter(typeof(T));
    var convertExp = Expression.Convert(instanceParameter, method.DeclaringType);
    var body = Expression.Call(convertExp, method);
    var lambda = Expression.Lambda<Func<T, Out>>(body, instanceParameter);
    getter = lambda.Compile();
  }

  [System.Serializable]
  public class Param : VmSetActive.Param
  {

  }
}