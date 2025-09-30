using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;

/**
* GenericClassValue.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 07월 16일 오전 7시 13분
*/


#region GenericClassValue
public class GenericClassValueManager
{
  private static GenericClassValueManager instance;
  public static GenericClassValueManager Instance
  {
    get
    {
      if(instance == null) instance = new GenericClassValueManager();
      return instance;
    }
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Init()
  {
    instance = null;
  }

  public Dictionary<Type, ObjectActivator> cachedObjectActivatorMap = new();
  public Dictionary<Type, Delegate> getValueFuncMap = new();

  public delegate GenericClassValue ObjectActivator(object target, string propertyName);
  public static ObjectActivator GetActivator(ConstructorInfo ctor)
  {
    var type = ctor.DeclaringType;
    Instance.cachedObjectActivatorMap ??= new();
    if (Instance.cachedObjectActivatorMap.ContainsKey(type) == true)
      return Instance.cachedObjectActivatorMap[type];

    var target = Expression.Parameter(typeof(object), "target");
    var propertyName = Expression.Parameter(typeof(string), "propertyName");
    var newExp = Expression.New(ctor, target, propertyName);
    var lambda = Expression.Lambda(typeof(ObjectActivator), newExp, target, propertyName);
    var compiled = lambda.Compile() as ObjectActivator;

    Instance.cachedObjectActivatorMap.Add(type, compiled);
    return compiled;
  }

  public static TDelegate CreateFunc<TDelegate>(Type type, Func<TDelegate> createCallback) where TDelegate : Delegate
  {
    if(Instance.getValueFuncMap.ContainsKey(type) == false)
      Instance.getValueFuncMap.Add(type, createCallback());
    return Instance.getValueFuncMap[type] as TDelegate;
  }

  public static Delegate GetFunc(Type type)
  {
    if(Instance.getValueFuncMap.ContainsKey(type) == false)
    {
      Debug.LogError($"{type}으로 생성된 인스턴스가 존재하지 않습니다. 반드시 먼저 생성이되도록 해주세요");
      return null;
    }
    return Instance.getValueFuncMap[type];
  }

}
public abstract class GenericClassValue
{
  protected object target;
  protected Type cachedType;
  public Type CachedType => cachedType;
  public abstract object GetToObject();
}

public class GenericClassValue<TValue> : GenericClassValue
{
  public Func<object, TValue> GetValueFunc { get; private set; }
  public object cachedObj;

  public GenericClassValue(object obj, string propertyName)
  {
    cachedType = typeof(GenericClassValue<TValue>);
    target = obj;
    GetValueFunc = GenericClassValueManager.CreateFunc(cachedType, () => CratePropertyGetter(target, propertyName));
  }

  public Func<T, TValue> CratePropertyGetter<T>(T target, string propertyName)
  {
    var method = target.GetType().GetProperty(propertyName).GetMethod;
    var instanceParameter = Expression.Parameter(typeof(T));
    var convertExp = Expression.Convert(instanceParameter, method.DeclaringType);
    var body = Expression.Call(convertExp, method);
    var lambda = Expression.Lambda<Func<T, TValue>>(body, instanceParameter);
    return lambda.Compile();
  }

  public TValue GetValue() => GetValueFunc(target);
  public override object GetToObject() => GetValueFunc(target);
}
#endregion

#region GenericClassValueList
public abstract class GenericClassValueList : GenericClassValue
{
  public abstract object GetToObject(int index);
}

public class GenericClassValueList<TValue> : GenericClassValueList
{
  public Func<object, List<TValue>> GetValueFunc { get; private set; }

  public GenericClassValueList(object obj, string propertyName)
  {
    cachedType = typeof(GenericClassValueList<TValue>);
    target = obj;
    GetValueFunc = GenericClassValueManager.CreateFunc(cachedType, () => CratePropertyGetter(target, propertyName));
  }

  public Func<T, List<TValue>> CratePropertyGetter<T>(T target, string propertyName)
  {
    var method = target.GetType().GetProperty(propertyName).GetMethod;
    var instanceParameter = Expression.Parameter(typeof(T));
    var convertExp = Expression.Convert(instanceParameter, method.DeclaringType);
    var body = Expression.Call(convertExp, method);
    var lambda = Expression.Lambda<Func<T, List<TValue>>>(body, instanceParameter);
    return lambda.Compile();
  }
  public TValue GetValue(int index) => GetValueFunc(target)[index];
  public override object GetToObject() => GetValueFunc(target);
  public override object GetToObject(int index) => GetValueFunc(target)[index];
}
#endregion

#region GenericClassValueContextList
public abstract class GenericClassValueContextList : GenericClassValue
{
  public abstract object GetToObject(int index);
}

public class GenericClassValueContextList<TValue> : GenericClassValueContextList
{
  public Func<object, ContextList<TValue>> GetValueFunc { get; private set; }

  public GenericClassValueContextList(object obj, string propertyName)
  {
    cachedType = typeof(GenericClassValueContextList<TValue>);
    target = obj;
    GetValueFunc = GenericClassValueManager.CreateFunc(cachedType, () => CratePropertyGetter(target, propertyName));
  }

  public Func<T, ContextList<TValue>> CratePropertyGetter<T>(T target, string propertyName)
  {
    var method = target.GetType().GetProperty(propertyName).GetMethod;
    var instanceParameter = Expression.Parameter(typeof(T));
    var convertExp = Expression.Convert(instanceParameter, method.DeclaringType);
    var body = Expression.Call(convertExp, method);
    var lambda = Expression.Lambda<Func<T, ContextList<TValue>>>(body, instanceParameter);
    return lambda.Compile();
  }

  public TValue GetValue(int index) => GetValueFunc(target)[index];
  public override object GetToObject() => GetValueFunc(target);
  public override object GetToObject(int index) => GetValueFunc(target)[index];
}
#endregion

#region GenericClassValueGenericDictionary
public abstract class GenericClassValueGenericDictionary : GenericClassValue
{
  public virtual KeyTypes KeyType { get; protected set; }
  public virtual object GetToObject(string key) => null;
  public virtual object GetToObject(int key) => null;

  public enum KeyTypes
  {
    INTEGER,
    STRING,
  }
}

public abstract class GenericClassValueGenericDictionary<TKey, TValue> : GenericClassValueGenericDictionary
{
  public Func<object, GenericDictionary<TKey, TValue>> GetValueFunc { get; private set; }

  public GenericClassValueGenericDictionary(object obj, string propertyName)
  {
    cachedType = typeof(GenericClassValueGenericDictionary<TKey, TValue>);
    target = obj;
    GetValueFunc = GenericClassValueManager.CreateFunc(cachedType, () => CratePropertyGetter(target, propertyName));
  }

  public Func<T, GenericDictionary<TKey, TValue>> CratePropertyGetter<T>(T target, string propertyName)
  {
    var method = target.GetType().GetProperty(propertyName).GetMethod;
    var instanceParameter = Expression.Parameter(typeof(T));
    var convertExp = Expression.Convert(instanceParameter, method.DeclaringType);
    var body = Expression.Call(convertExp, method);
    var lambda = Expression.Lambda<Func<T, GenericDictionary<TKey, TValue>>>(body, instanceParameter);
    return lambda.Compile();
  }
  public override object GetToObject() => GetValueFunc(target);
}

public class GenericClassValueGenericDictionaryStringKey<TValue> : GenericClassValueGenericDictionary<string, TValue>
{
  public override KeyTypes KeyType => KeyTypes.STRING;
  public GenericClassValueGenericDictionaryStringKey(object obj, string propertyName) : base(obj, propertyName) { }

  public TValue GetValue(string key) => GetValueFunc(target)[key];
  public override object GetToObject(string key)
  {
    base.GetToObject(key);
    var result = GetValueFunc(target);
    if(result.ContainsKey(key) == false) return null;
    return result[key];
  }
}
public class GenericClassValueGenericDictionaryIntegerKey<TValue> : GenericClassValueGenericDictionary<int, TValue>
{
  public override KeyTypes KeyType => KeyTypes.INTEGER;
  public GenericClassValueGenericDictionaryIntegerKey(object obj, string propertyName) : base(obj, propertyName) { }

  public TValue GetValue(int key) => GetValueFunc(target)[key];

  public override object GetToObject(int key)
  {
    base.GetToObject(key);
    var result = GetValueFunc(target);
    if (result.ContainsKey(key) == false) return null;
    return result[key];
  }
}
#endregion

#region GenericClassValueContextDictionary
public abstract class GenericClassValueContextDictionary : GenericClassValue
{
  public abstract object GetToObject(string key);
}

public class GenericClassValueContextDictionary<TValue> : GenericClassValueContextDictionary
{
  public Func<object, ContextDictionary<TValue>> GetValueFunc { get; private set; }

  public GenericClassValueContextDictionary(object obj, string propertyName)
  {
    cachedType = typeof(GenericClassValueContextDictionary<TValue>);
    target = obj;
    GetValueFunc = GenericClassValueManager.CreateFunc(cachedType, () => CratePropertyGetter(target, propertyName));
  }

  public Func<T, ContextDictionary<TValue>> CratePropertyGetter<T>(T target, string propertyName)
  {
    var method = target.GetType().GetProperty(propertyName).GetMethod;
    var instanceParameter = Expression.Parameter(typeof(T));
    var convertExp = Expression.Convert(instanceParameter, method.DeclaringType);
    var body = Expression.Call(convertExp, method);
    var lambda = Expression.Lambda<Func<T, ContextDictionary<TValue>>>(body, instanceParameter);
    return lambda.Compile();
  }
  public TValue GetValue(string key) => GetValueFunc(target)[key];
  public override object GetToObject() => GetValueFunc(target);
  public override object GetToObject(string key)
  {
    var result = GetValueFunc(target);
    if (result.ContainsKey(key) == false) return null;
    return result[key];
  }
}
#endregion