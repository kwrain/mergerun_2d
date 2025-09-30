using System.Text;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.UI;
using UnityEngine;
using UnityEngine.UI;

/**
* VmAtlasImageSpriteNameSetter.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 08월 30일 오전 10시 01분
*/

[AddComponentMenu("Vm/[Vm]AtlasImageSpriteNameSetter")]
[DisallowMultipleComponent]
[SupportedProperty(null)]
public class VmAtlasImageSpriteNameSetter : VmBase<AtlasImage, VmAtlasImageSpriteNameSetter.Param>
{
  private object[] args = null;
  [SerializeField, Tooltip("런타임 확인용")] private string format;
  [SerializeField, Tooltip("스프라이트 교체시 SetNativeSize 메서드 호출 여부")] 
  private bool applyNativeSize = true;

  protected override void Initialize()
  {
    base.Initialize();
    CreateFormat();
    args = new object[pInfos.Length];
  }
  public override void UpdateViewActivate()
  {
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      args[i] = pInfo.Param.GetValue(pInfo.Index, pInfo.StringKey);
    }
    view.spriteName = string.Format(format, args);
    if(applyNativeSize == true) view.SetNativeSize();
  }

  public override void UpdateView(string context)
  {
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      if (pInfo.Context != context) continue;
      var arg = pInfo.Param.GetValue(pInfo.Index, pInfo.StringKey);
      if (arg == args[i]) return;
      args[i] = arg;
    }
    view.spriteName = string.Format(format, args);
    if (applyNativeSize == true) view.SetNativeSize();
  }

  [ContextMenu("Format 출력")]
  public void CreateFormat()
  {
    var sb = new StringBuilder();
    for (int i = 0; i < pInfos.Length; i++)
    {
      var pInfo = pInfos[i];
      if (Application.isPlaying == true)
        pInfo.Param.Create(pInfo.Property, pInfo.PropertyName);

      var prefix = pInfo.Param.PreFix; // 접두사
      var argFormat = pInfo.Param.ArgFormat; // 형식 매개값
      var suffix = pInfo.Param.Suffix; // 접미사

      if (string.IsNullOrEmpty(prefix) == false)
        sb.Append(prefix.Replace("\\n", "\n")); // 개행 문자열 변환

      if (string.IsNullOrEmpty(argFormat) == true)
        sb.Append($"{{{i}}}"); // null 또는 빈값이면 해당 index가 기본 형식 매개값으로 지정
      else
        sb.Append(argFormat); // 그러지않으면 지정한 형식 매개값으로 지정

      if (string.IsNullOrEmpty(suffix) == false)
        sb.Append(suffix.Replace("\\n", "\n")); // 개행 문자열 변환
    }
    if (Application.isPlaying == true)
      format = sb.ToString();
    else
      Debug.Log(sb.ToString());
  }

  [System.Serializable]
  public class Param : VmTextTextSetter.Param
  {

  }
}