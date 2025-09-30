using FAIRSTUDIOS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[HUD(Type = typeof(HUDSample), PrefabPath = "HUDSample", ShowNavigation = true)]
public class HUDSample : HUDBehaviour
{
  private float maxAmount;

  public AtlasImage imgScore;
  public Text txtScore;

  public AtlasImage imgAbility;
  public AtlasImage imgAbilityIcon;
  public Text txtName;

  public Text txtHP;
  public AtlasImage imgHP;
  public AtlasImage imgHPProgress;

  public GameObject goUIWeapon;
  public AtlasImage[] imgPacks;

  public Text txtNaviName;
  public RectTransform rtNaviName;

  public AtlasImage imgNavi;
  public RectTransform rtNavi;

  public float this[int index]
  {
    get { return imgPacks[index].fillAmount; }
    set
    {
      imgPacks[index].fillAmount = value * maxAmount;
    }
  }

  protected override void SetNavigation(EDirection eDirection)
  {
    base.SetNavigation(eDirection);

    switch (eDirection)
    {
      case EDirection.Top:
        rtNavi.localEulerAngles = new Vector3(0, 0, 180);
        rtNaviName.anchoredPosition = new Vector2(0, rtNavi.sizeDelta.y + rtNaviName.sizeDelta.y) * -0.5f;
        break;

      case EDirection.Bottom:
        rtNavi.localEulerAngles = Vector3.zero;
        rtNaviName.anchoredPosition = new Vector2(0, rtNavi.sizeDelta.y + rtNaviName.sizeDelta.y) * 0.5f;
        break;

      case EDirection.Left:
        rtNavi.localEulerAngles = new Vector3(0, 0, 270);
        rtNaviName.anchoredPosition = new Vector2(rtNavi.sizeDelta.x + rtNaviName.sizeDelta.x, 0) * 0.5f;
        break;

      case EDirection.Right:
        rtNavi.localEulerAngles = new Vector3(0, 0, 90);
        rtNaviName.anchoredPosition = new Vector2(rtNavi.sizeDelta.x + rtNaviName.sizeDelta.x, 0) * -0.5f;
        break;
    }
  }

}
