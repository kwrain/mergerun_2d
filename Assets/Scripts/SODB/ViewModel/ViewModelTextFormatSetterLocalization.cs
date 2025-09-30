using System;
using System.Collections.Generic;
using System.Text;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.SODB.ViewModel;
using UnityEngine;
using UnityEngine.UI;

public class ViewModelTextFormatSetterLocalization : ViewModelTextSetter
{
  [SerializeField] protected string format = "{0}";

  [SerializeField] protected List<LocalizeData> localize = new List<LocalizeData>();

  private object[] _args;

  [Serializable]
  public class LocalizeData
  {
    public string id;
    public List<string> args;
  }

  protected virtual void Awake()
  {
    if (_args == null) InitArgs();
  }

  protected void InitArgs()
  {
    _args = new object[localize.Count + 1];
    for (var i = 0; i < localize.Count; i++)
    {
      string value;
      if (localize[i].args.Count == 0)
      {
        value = Localize.GetValue(localize[i].id);
      }
      else
      {
        if (localize[i].args.Count == 1)
        {
          value= Localize.GetValueFormat(localize[i].id, localize[i].args[0]);
        }
        else
        {
          var args = new object[localize[i].args.Count + 1];
          for (var j = 0; j < localize[i].args.Count; j++)
          {
            args[j] = localize[i].args[j];
          }

          value = Localize.GetValueFormat(localize[i].id, args);
        }
      }

      _args[i + 1] = value;
    }
  }


  public override void OnPropertyChanged(PropertyBase property)
  {
    switch (propertyType)
    {
      case PropertyType.Single:
      {
        if (targets == null) return;
        var newValue = property as IPropertyToString;
        _args[0] = newValue.StringRuntimeValue;
      }
        break;
      case PropertyType.List:
      {
        if (targets == null) return;
        var newValue = property as IPropertyToString<int>;
        if (newValue.Count == 0) return;
        if (index == -1) { targets.text = ""; return; }
        _args[0]  = newValue[index];
      }
        break;
      case PropertyType.Dictionary:
      {
        if (targets == null) return;
        var newValue = property as IPropertyToString<string>;
        if (newValue.ContainsKey(key) == false) return;
        _args[0] = newValue[key];
      }
        break;
    }

    targets.text = Localize.ContainsKey(format) ? Localize.GetValueFormat(format, _args) : string.Format(format, _args);
  }
}