using FAIRSTUDIOS.SODB.Property;
using UnityEngine;
using UnityEngine.UI;

/**
* VmGraphicColorSetter.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 08월 18일 오전 9시 26분
*/
[AddComponentMenu("Vm/[Vm]GraphicColorSetter")]
[DisallowMultipleComponent]
[SupportedProperty(typeof(PropertyColor), typeof(PropertyListColor), typeof(PropertyMapStringColor))]
public class VmGraphicColorSetter : VmBase<Graphic, VmGraphicColorSetter.Param>
{
  public override void UpdateViewActivate()
  {
    view.color = GetValue(pInfos[0]);
  }

  public override void UpdateView(string context)
  {
    if(pInfos[0].Context != context) return;
    UpdateViewActivate();
  }

  private Color GetValue(PropertyInfoBase pInfo) => pInfo.Property switch
  {
    PropertyColor p => p.NewValue,
    PropertyListColor p => p[pInfo.Index],
    PropertyMapStringColor p => p[pInfo.StringKey],
    _ => Color.white,
  };

  public class Param : PropertyInfoParamBase
  {
    
  }
}