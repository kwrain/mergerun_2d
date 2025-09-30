using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.UI;

[BindProperty(typeof(PropertyBoolean))]
[BindProperty(typeof(PropertyListBoolean))]
[BindProperty(typeof(PropertyMapStringBoolean))]
public class ViewModelKButtonInteractableSetter : ViewModelBase<KButton>
{
  private void Awake()
  {
    targets = targets == null ? GetComponent<KButton>() : targets;
  }
  public override void OnPropertyChanged(PropertyBase property)
    => RTSViewModelCommonHelper.SetKButtonInteractable(targets, property, propertyType, index, key);
}