using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.UI;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[BindProperty(typeof(PropertyString))]
[BindProperty(typeof(PropertyListString))]
[BindProperty(typeof(PropertyMapStringString))]
public class ViewModelAtlasImageSpriteNameSetter : ViewModelBase<AtlasImage>
{
  [SerializeField] private string prefix = string.Empty;
  [SerializeField] private string suffix = string.Empty;
  [SerializeField] private bool bNativeSize = false;
  private void Awake()
  {
    targets = targets != null ? targets : GetComponent<AtlasImage>();
  }

  public override void OnPropertyChanged(PropertyBase property)
    => RTSViewModelCommonHelper.SetAtlasImageSpriteName(targets, property, propertyType, index, key, prefix, suffix, bNativeSize);
}
