using FAIRSTUDIOS.SODB.Core;
using NaughtyAttributes;
using UnityEngine.UI;

[BindProperty(typeof(PropertyUnityImage))]
public class ViewModelUnityImage : ViewModelBase<Image>
{
  private void Awake()
  {
    targets = targets != null ? targets : GetComponent<Image>();
  }

  public override void OnPropertyChanged(PropertyBase property)
  {
    targets = targets != null ? targets : GetComponent<Image>();
    var newProperty = property as PropertyUnityImage;
    var runtimeValue = newProperty.NewValue;
    targets.type = runtimeValue.ImageType;
    targets.raycastTarget = runtimeValue.RaycastTarget;
    targets.maskable = runtimeValue.Maskable;
    switch (targets.type)
    {
      case Image.Type.Simple:
        targets.preserveAspect = runtimeValue.PreserveAspect;
        targets.useSpriteMesh = runtimeValue.UseSpriteMesh;
        break;
      case Image.Type.Sliced:
        targets.pixelsPerUnitMultiplier = runtimeValue.PixelsPerUnitMultiplier;
        break;
      case Image.Type.Tiled:
        targets.pixelsPerUnitMultiplier = runtimeValue.PixelsPerUnitMultiplier;
        break;
      case Image.Type.Filled:
        targets.fillAmount = runtimeValue.FillAmount;
        targets.fillMethod = runtimeValue.FillMethod;
        targets.preserveAspect = runtimeValue.PreserveAspect;
        switch (targets.fillMethod)
        {
          case Image.FillMethod.Horizontal:
            targets.fillOrigin = (int)runtimeValue.OriginHorizontal;
            break;
          case Image.FillMethod.Vertical:
            targets.fillOrigin = (int)runtimeValue.OriginVertical;
            break;
          case Image.FillMethod.Radial90:
            targets.fillOrigin = (int)runtimeValue.Origin90;
            targets.fillClockwise = runtimeValue.FillClockwise;
            break;
          case Image.FillMethod.Radial180:
            targets.fillOrigin = (int)runtimeValue.Origin180;
            targets.fillClockwise = runtimeValue.FillClockwise;
            break;
          case Image.FillMethod.Radial360:
            targets.fillOrigin = (int)runtimeValue.Origin360;
            targets.fillClockwise = runtimeValue.FillClockwise;
            break;
        }
        break;
    }
  }

}