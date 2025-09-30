using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using UnityEngine;

[BindProperty(typeof(PropertyColor))]
[BindProperty(typeof(PropertyListColor))]
[BindProperty(typeof(PropertyMapStringColor))]
public class ViewModelModifiedEffectColorChanger : ViewModelBase<ModifiedShadow>
{
  //[SerializeField] protected ModifiedShadow modifiedShadow;
  private void Awake()
  {
    targets = targets != null ? targets : GetComponent<ModifiedShadow>();
  }

  public override void OnPropertyChanged(PropertyBase property)
    => RTSViewModelCommonHelper.SetModifiedShadowEffectColor(targets, property, propertyType, index, key);
}