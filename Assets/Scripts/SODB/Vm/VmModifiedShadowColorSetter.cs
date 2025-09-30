using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using UnityEngine;

/**
* VmModifiedShadowColorSetter.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 08월 04일 오후 1시 52분
*/
[AddComponentMenu("Vm/[Vm]ModifiedShadowColorSetter")]
[DisallowMultipleComponent]
[SupportedProperty(typeof(PropertyColor), typeof(PropertyListColor), typeof(PropertyMapStringColor))]
public class VmModifiedShadowColorSetter : VmBase<ModifiedShadow, VmModifiedShadowColorSetter.Param>
{

  public override void UpdateViewActivate()
  {
    view.effectColor = GetValue(pInfos[0]);
  }

  public override void UpdateView(string context)
  {
    if (pInfos[0].Context != context) return;
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