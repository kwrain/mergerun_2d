using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.SODB.Utils;
using RedBlueGames.Tools.TextTyper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
* VmTmpTextSetter.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 07월 26일 오후 6시 04분
*/

[AddComponentMenu("Vm/[Vm]TmpTextTyperSetter")]
[DisallowMultipleComponent]
[SupportedProperty(null)]
public class VmTmpTextTyperSetter : VmBase<TextTyper, VmTmpTextTyperSetter.Param>
{
  private object[] args = null;
  [SerializeField] private string format;
  [SerializeField] [Range(-10, 10)] private float delay = -1; 
  [SerializeField] private bool fit = true; 
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
      args[i] = pInfo.Param.GetValue(pInfo);
    }

    view.TypeText(string.Format(format, args));
  }

  public override void UpdateView(string context)
  {
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      if (pInfo.Context != context) continue;
      var arg = pInfo.Param.GetValue(pInfo);
      if (arg == args[i]) return;
      args[i] = arg;
    }
    
    view.TypeText(string.Format(format, args));
  }

  [ContextMenu("Format 출력")]
  public void CreateFormat()
  {
    var sb = new StringBuilder();
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      if (Application.isPlaying == true)
        pInfo.Param.Create(pInfo);

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

    private GenericClassValue classValue;
    public GenericClassValue ClassValue => classValue;

    public void Create(PropertyInfoBase<Param> pInfo)
    {
      string propertyName;
      object target;
      switch (pInfo.Property)
      {
        case PropertyContextClass pClass:
          {
            target = pClass.NewValue;
            propertyName = pInfo.PropertyName;
          }
          break;
        default:
          {
            target = pInfo.Property;
            propertyName = "NewValue";
          }
          break;
      }
      if (target == null) return;

      var propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
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
          else if (args[0] == typeof(int))
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
      if (newType == null || typeArguments == null) return;
      var constructedType = newType.MakeGenericType(typeArguments);
      var construct = constructedType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[] { typeof(object), typeof(string) }, null);
      classValue = GenericClassValueManager.GetActivator(construct)(target, propertyName);
    }

    public object GetValue(PropertyInfoBase<Param> pInfo)
    {
      return classValue switch
      {
        GenericClassValueList c => c.GetToObject(pInfo.Index),
        GenericClassValueContextList c => c.GetToObject(pInfo.Index),
        GenericClassValueGenericDictionary c => c.KeyType switch
        {
          GenericClassValueGenericDictionary.KeyTypes.INTEGER => c.GetToObject(pInfo.Index),
          GenericClassValueGenericDictionary.KeyTypes.STRING => c.GetToObject(pInfo.StringKey),
          _ => null
        },
        GenericClassValueContextDictionary c => c.GetToObject(pInfo.StringKey),
        _ => classValue.GetToObject(),
      };
    }
  }
}