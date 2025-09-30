using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using System;
using System.Collections.Generic;
using UnityEngine;

[BindProperty(typeof(PropertyBoolean))]
[Obsolete("삭제 예정")]
public class ViewModelBehavioursActivator : ViewModelBase<List<Behaviour>>
{
  public override void OnPropertyChanged(PropertyBase property)
  {
    if (targets.Count == 0) return;
    var newValue = property as PropertyBoolean;
    for (int i = 0; i < targets.Count; i++)
    {
      targets[i].enabled = newValue.NewValue;
    }
  }
}