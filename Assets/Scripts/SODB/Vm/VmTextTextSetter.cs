using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;
using UnityEngine.UI;
using static VmSetActive;

/**
* VmTextTextSetter.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 07월 15일 오전 8시 33분
*/
[AddComponentMenu("Vm/[Vm]TextTextSetter")]
[DisallowMultipleComponent]
[SupportedProperty(null)]
public class VmTextTextSetter : VmBase<Text, VmTextTextSetter.Param>
{
  private object[] args = null;
  [SerializeField, Tooltip("런타임 확인용")] private string format;

  [SerializeField] private string localizeID;
  [SerializeField] private bool useLocalizeFormat = false;
  protected override void Initialize()
  {
    base.Initialize();
    CreateFormat();
    args = new object[pInfos.Length];
  }
  public override void UpdateViewActivate()
  {
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      args[i] = pInfo.Param.GetValue(pInfo.Index, pInfo.StringKey);
      if (pInfo.Param.IsLocalize)
      {
        args[i] = Localize.GetValue(args[i].ToString());
      }
    }
    UpdateView();
  }

  public override void UpdateView(string context)
  {
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      if(pInfo.Context != context)
      {
        continue;
      }

      var arg = pInfo.Param.GetValue(pInfo.Index, pInfo.StringKey);
      if(pInfo.Param.IsLocalize)
      {
        arg = Localize.GetValue(arg.ToString());
      }

      if (arg == args[i])
      {
        return;
      }

      args[i] = arg;
    }
    UpdateView();

  }

  private void UpdateView()
  {
    if (string.IsNullOrEmpty(localizeID))
    {
      view.text = string.Format(format, args);
    }
    else
    {
      if (useLocalizeFormat)
      {
        view.text = string.Format(Localize.GetValue(localizeID), args);
      }
      else
      {
        view.text = string.Format(Localize.GetValue(localizeID), string.Format(format, args));
      }

      // 아래 코드 안됨
      // view.text = string.Format(Localize.GetValue(localizeID), useLocalizeFormat ? args : string.Format(format, args));
    }
  }

  [ContextMenu("Format 출력")]
  public void CreateFormat()
  {
    var sb = new StringBuilder();
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      if(Application.isPlaying == true)
        pInfo.Param.Create(pInfo.Property, pInfo.PropertyName);

      var prefix = pInfo.Param.PreFix; // 접두사
      var argFormat = pInfo.Param.ArgFormat; // 형식 매개값
      var suffix = pInfo.Param.Suffix; // 접미사

      if (string.IsNullOrEmpty(prefix) == false)
        sb.Append(prefix.Replace("\\n", "\n")); // 개행 문자열 변환

      if (string.IsNullOrEmpty(argFormat) == true)
        sb.Append($"{{{i}}}"); // null 또는 빈값이면 해당 index가 기본 형식 매개값으로 지정
      else
        sb.Append(argFormat); // 그러지않으면 지정한 형식 매개값으로 지정

      if (string.IsNullOrEmpty(suffix) == false)
        sb.Append(suffix.Replace("\\n", "\n")); // 개행 문자열 변환
    }
    if (Application.isPlaying == true)
      format = sb.ToString();
    else
      Debug.Log(sb.ToString());
  }

  [System.Serializable]
  public class Param : PropertyInfoParamBase
  {
    [SerializeField] private string prefix;
    public string PreFix => prefix;
    [SerializeField] private string argFormat;
    public string ArgFormat => argFormat;
    [SerializeField] private string suffix;
    public string Suffix => suffix;
    [SerializeField] private bool isLocalize;
    public bool IsLocalize => isLocalize;

    private GenericClassValue classValue;
    public GenericClassValue ClassValue => classValue;

    public void Create(PropertyBase property, string propertyName)
    {
      string targetPropertyName;
      object target;
      switch (property)
      {
        case PropertyContextClass pClass:
          {
            target = pClass.NewValue;
            targetPropertyName = propertyName;
          }
          break;
        default:
          {
            target = property;
            targetPropertyName = "NewValue";
          }
          break;
      }
      if (target == null) return;

      var propertyInfo = target.GetType().GetProperty(targetPropertyName, BindingFlags.Instance | BindingFlags.Public);
      var propertyType = propertyInfo.PropertyType;
      Type newType = null;
      Type[] typeArguments = null;
      if (propertyType.IsGenericType == false)
      {
        newType = typeof(GenericClassValue<>);
        typeArguments = new[] { propertyType };
      }
      else
      {
        var genericDefinition = propertyType.GetGenericTypeDefinition();
        // List
        if (genericDefinition == typeof(List<>))
        {
          newType = typeof(GenericClassValueList<>);
          typeArguments = propertyType.GetGenericArguments();
        }
        // ContextList
        else if (genericDefinition == typeof(ContextList<>))
        {
          newType = typeof(GenericClassValueContextList<>);
          typeArguments = propertyType.GetGenericArguments();
        }
        // Generic Dictionary
        else if (genericDefinition == typeof(GenericDictionary<,>))
        {
          var args = propertyType.GetGenericArguments();

          if (args[0] == typeof(string))
            newType = typeof(GenericClassValueGenericDictionaryStringKey<>);
          else if(args[0] == typeof(int))
            newType = typeof(GenericClassValueGenericDictionaryIntegerKey<>);

          typeArguments = new Type[] { args[1] };
        }
        // ContextDictionary
        else if (genericDefinition == typeof(ContextDictionary<>))
        {
          newType = typeof(GenericClassValueContextDictionary<>);
          typeArguments = propertyType.GetGenericArguments();
        }
      }
      if(newType == null || typeArguments == null) return;
      var constructedType = newType.MakeGenericType(typeArguments);
      var construct = constructedType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[]{typeof(object), typeof(string)}, null);
      classValue = GenericClassValueManager.GetActivator(construct)(target, targetPropertyName);
      //classValue = Activator.CreateInstance(constructedType, new object[] { target, propertyName }) as GenericClassValue;

      // 리플렉션 최적화 테스트
      // int count = 1000;

      // var activator = GenericClassValueManager.GetActivator(construct);
      // var sw2 = new System.Diagnostics.Stopwatch();
      // sw2.Start();
      // for (int i = 0; i < count; i++)
      // {
      //   var t = activator(target, propertyName);
      // }
      // sw2.Stop();
      // Debug.Log($"Cache : {constructedType}-{sw2.Elapsed.TotalMilliseconds}");

      // var sw = new System.Diagnostics.Stopwatch();
      // sw.Start();
      // for (int i = 0; i < count; i++)
      // {
      //   var t = Activator.CreateInstance(constructedType, new object[] { target, propertyName }) as GenericClassValue;
      // }
      // sw.Stop();
      // Debug.Log($"Activator : {constructedType}-{sw.Elapsed.TotalMilliseconds}");
    }

    public object GetValue(int index, string key)
    {
      return classValue switch
      {
        GenericClassValueList c => c.GetToObject(index),
        GenericClassValueContextList c => c.GetToObject(index),
        GenericClassValueGenericDictionary c => c.KeyType switch
        {
          GenericClassValueGenericDictionary.KeyTypes.INTEGER => c.GetToObject(index),
          GenericClassValueGenericDictionary.KeyTypes.STRING => c.GetToObject(key),
          _ => null
        },
        GenericClassValueContextDictionary c => c.GetToObject(key),
        _ => classValue.GetToObject(),
      };
    }
  }
}