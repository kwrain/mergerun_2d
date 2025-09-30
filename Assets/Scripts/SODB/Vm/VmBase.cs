using FAIRSTUDIOS.SODB.Core;
using UnityEngine;
using UnityEngine.Events;

/**
* VmBase.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 07월 13일 오후 11시 31분
*/

public abstract class VmBase<TView, TParam> : VmBase
where TView : Object
where TParam : PropertyInfoParamBase
{
  [SerializeField] private bool isViewThis;
  [SerializeField] protected TView view;
  [SerializeField] protected PropertyInfoBase<TParam>[] pInfos;

  [SerializeField, Tooltip("Start에서만 발생")]
  private bool useOnStart = false;
  [SerializeField] protected UnityEvent<TView> onStart;

  [SerializeField, Tooltip("Start 또는 OnEnable에서 발생 ( 처음 생성시 Start에서만 발생 )")]
  private bool useOnActivate = false;
  [SerializeField] protected UnityEvent<TView> onActivate;

  [SerializeField, Tooltip("UpdateView이후에 항상 발생")]
  private bool useOnViewChanged = false;
  [SerializeField] protected UnityEvent<TView> onViewChanged;

  [SerializeField] private bool foldoutEvents = false;

  /// <summary>
  /// 항상 스크립트 상 마지막에 선언할 것.
  /// </summary>
  /// <typeparam name="TView"></typeparam>
  [SerializeField] private bool lastPropertyForInspector;

  public override void SetView() => view ??= GetComponent<TView>();
  public override Object GetView() => view;
  public sealed override void ReleaseView() => view = null;

  private void Reset()
  {
    SetView();
  }

  protected override void Start()
  {
    isStarted = true;
    UpdateViewActivate();
    if (useOnStart == true)
      onStart?.Invoke(view);
    if (useOnActivate == true)
      onActivate?.Invoke(view);
  }

  protected override void OnEnable()
  {
    foreach (var pInfo in pInfos)
      Bind(pInfo.Context);
    if (isStarted == false) return;
    UpdateViewActivate();
    if (useOnActivate == true)
      onActivate?.Invoke(view);
  }

  protected override void OnDisable()
  {
    foreach (var pInfo in pInfos)
      UnBind(pInfo.Context);
  }

  protected void Bind(string context)
  {
    if (Binding.valueChangedList.ContainsKey(context) == false)
      Binding.valueChangedList.Add(context, new());

    if (Binding.valueChangedList[context].Contains(this) == true)
      return;
    Binding.valueChangedList[context].Add(this);
  }

  protected void UnBind(string context)
  {
    if (Binding.valueChangedList.ContainsKey(context) == false)
      return;
    if (Binding.valueChangedList[context].Contains(this) == false)
      return;
    Binding.valueChangedList[context].Remove(this);
    if (Binding.valueChangedList[context].Count == 0)
      Binding.valueChangedList.Remove(context);
  }

  public override void NotifyViewChanged()
  {
    if (useOnViewChanged == true)
      onViewChanged?.Invoke(view);
  }
}

public abstract class VmBase : MonoBehaviour, IPropertyChanged
{
  protected bool isStarted = false;

  public void OnPropertyChanged(PropertyBase property)
    => OnPropertyChanged(property.ContextKey, property);

  public void OnPropertyChanged(string context, PropertyBase property)
  {
    if(isStarted == false) return;
    if(GetView() == null) return;
    UpdateView(context);
    NotifyViewChanged();
  }

  public void OnViewChanged(PropertyBase property)
    => OnViewChanged(property.ContextKey, property);

  public void OnViewChanged(string context, PropertyBase property) { }

  /// <summary>
  /// Start 또는 OnEnable 시 현재 프로퍼티 값으로 업데이트
  /// </summary>
  public abstract void UpdateViewActivate();
  /// <summary>
  /// 프로퍼티 감지 시 업데이트
  /// </summary>
  /// <param name="context"></param>
  public abstract void UpdateView(string context);
  public abstract void NotifyViewChanged();
  public abstract void SetView();
  public abstract Object GetView();
  public abstract void ReleaseView();

  protected virtual void Awake()
  {
    SetView();
    Initialize();
  }
  /// <summary>
  /// 초기화, Awake 시점에 호출됨.
  /// </summary>
  protected virtual void Initialize() { }
  protected abstract void Start();
  protected abstract void OnEnable();
  protected abstract void OnDisable();
}

[System.Serializable]
public abstract class PropertyInfoBase
{
  [SerializeField] private PropertyBase property;
  [SerializeField] private PropertyInfoContextType contextType;
  private string context = string.Empty;
  [SerializeField] private int index = -1;
  [SerializeField] private string stringKey = string.Empty;

  [SerializeField] private PropertyNameContextType nameContextType;
  [SerializeField] private string propertyName = string.Empty;
  [SerializeField] private int propertyIndex = -1;
  [SerializeField] private string propertyStringKey = string.Empty;
  public string Context
  {
    get
    {
      if (string.IsNullOrEmpty(context) == true)
      {
        context = contextType switch
        {
          PropertyInfoContextType.Mono => context = property.ContextKey,
          PropertyInfoContextType.Index => context = property.ContextKey + index,
          PropertyInfoContextType.StringKey => context = property.ContextKey + stringKey,
          PropertyInfoContextType.PropertyName => context = property.ContextKey + PropertyContext,
          _ => context = property.ContextKey,
        };
      }
      return context;
    }
  }
  public PropertyBase Property => property;
  public PropertyInfoContextType ContextType => contextType;
  public PropertyNameContextType NameContextType => nameContextType;
  public int Index
  {
    get
    {
      if (contextType == PropertyInfoContextType.Index)
        return index;
      else if (contextType == PropertyInfoContextType.PropertyName)
        return propertyIndex;
      return -1;
    }
  }
  public string StringKey
  {
    get
    {
      if (contextType == PropertyInfoContextType.StringKey)
        return stringKey;
      else if (contextType == PropertyInfoContextType.PropertyName)
        return propertyStringKey;
      return string.Empty;
    }
  }
  public string PropertyName => propertyName;

  public string PropertyContext
  {
    get
    {
      return nameContextType switch
      {
        PropertyNameContextType.Mono => propertyName,
        PropertyNameContextType.Index => propertyName + propertyIndex,
        PropertyNameContextType.StringKey => propertyName + propertyStringKey,
        _ => string.Empty,
      };
    }
  }
}

[System.Serializable]
public class PropertyInfoBase<TParam> : PropertyInfoBase
where TParam : PropertyInfoParamBase
{
  [SerializeField] protected TParam param;
  public TParam Param => param;
}

[System.Serializable]
public abstract class PropertyInfoParamBase
{

}

public enum PropertyInfoContextType
{
  Mono = 0,
  Index = 1,
  StringKey = 2,
  PropertyName = 3, // 신규 타입, 속성 이름으로 접근
}

public enum PropertyNameContextType
{
  Mono = 0,
  Index = 1,
  StringKey = 2,
}