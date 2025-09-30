using System;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using System.Collections.Generic;
using UnityEngine;

[BindProperty(typeof(PropertyBoolean))]
[Obsolete("삭제 예정")]
public class ViewModelGameObjectsActivator : ViewModelBase<List<GameObject>>
{
  [SerializeField] protected bool inverse;
  
  //[SerializeField] protected List<GameObject> gameObjects = new List<GameObject>();
  
  protected override void OnDisable() { }

  public override void OnPropertyChanged(PropertyBase property)
  {
    if (targets.Count == 0) return;
    var newValue = property as PropertyBoolean;
    foreach (var go in targets)
    {
      go.SetActive(inverse ? !newValue.NewValue : newValue.NewValue);
    }
  }
}