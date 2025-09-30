using System;
using System.Threading.Tasks;
using FAIRSTUDIOS.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[PopupUI(Type = typeof(PopupConfirm), PrefabPath = "Common/PopupConfirm", DontCloseOnInput = true, IsBlockMoveClose = true, CanvasType = UICanvasTypes.Focus, DontCloseOnLoad = true)]
public class PopupConfirm : PopupUIBehaviour
{
  [Header("[Confirm]")]
  public Text txtTitle;

  public Text txtMainText;

  public Text txtLeft;
  public Text txtRight;

  public RectTransform iconRoot;  // UI 아이콘 프리팹 사용시
  public RectTransform iconRT;  // UI 아이콘 프리팹 사용시
  private GameObject goIcon;    // UI 아이콘 풀링

  public AtlasImage buttonRightBG;
  public Color normalRightButtonTextShadowColor;
  public Color disableRightButtonTextShadowColor;

  private const string DISCOUNT_TEXT_FORMAT = "<s>{0}</s>";

  public Action OnHideCompleteAction
  {
    get => onHideCompleteAction;
    set => onHideCompleteAction = value;
  }


  // public static void Show(string title, string mainText, string left = "TXT_NO", UnityAction leftAction = null, string right = "TXT_YES", UnityAction rightAction = null)
  // {
  //   PopupConfirm popup = UIManager.Instance.ShowPopup<PopupConfirm>();
  //   popup.SetTitle(title);
  //   popup.SetMainText(mainText);
  //   popup.SetLeftButton(left, leftAction);
  //   popup.SetRightButton(right, rightAction);
  // }

  public static PopupConfirm Show(string mainText, string left = "335", UnityAction leftAction = null, string right = "336", UnityAction rightAction = null, PopupUIAttribute attribute = null)
  {
    // Show(null, mainText, left, leftAction, right, rightAction);
    var popup = attribute == null ? UIManager.Instance.ShowPopup<PopupConfirm>() : UIManager.Instance.ShowPopup(attribute) as PopupConfirm;
    popup.SetTitle(string.Empty);
    popup.SetMainText(mainText);
    popup.SetLeftButton(left, leftAction);
    popup.SetRightButton(right, rightAction);
    return popup;
  }

  public void SetTitle(string title)
  {
    if(string.IsNullOrEmpty(title))
    {
      txtTitle.SetActive(false);
    }
    else
    {
      txtTitle.text = Localize.GetValue(title);
      txtTitle.SetActive(true);
    }
  }

  public void SetMainText(string mainText)
  {
    txtMainText.text = Localize.GetValue(mainText);
  }

  public void SetLeftButton(string left, UnityAction leftAction)
  {
    txtLeft.text = Localize.GetValue(left);

    if(leftAction != null)
    {
      if (onButtonCancel == null)
      {
        onButtonCancel = new UnityEvent();
      }

      onButtonCancel.AddListener(leftAction);
    }
  }

  public void SetRightButton(string right, UnityAction rightAction, bool useLocalize = true)
  {
    txtRight.text = useLocalize == true ? Localize.GetValue(right) : right;

    if(rightAction != null)
    {
      if (onButtonOK == null)
      {
        onButtonOK = new UnityEvent();
      }

      onButtonOK.AddListener(rightAction);
    }
  }

  public void SetLeftText(string left, bool useLocalize = true)
  {
    txtLeft.text = useLocalize == true ? Localize.GetValue(left) : left;
  }

  public void SetLeftButtonAction(UnityAction leftAction)
  {
    if (leftAction != null)
    {
      if (onButtonOK == null)
      {
        onButtonOK = new UnityEvent();
      }

      onButtonOK.AddListener(leftAction);
    }
  }

  public void SetRightText(string right, bool useLocalize = true)
  {
    txtRight.text = useLocalize == true ? Localize.GetValue(right) : right;
  }

  public void SetRightButtonAction(UnityAction rightAction)
  {
    if (rightAction != null)
    {
      if (onButtonOK == null)
      {
        onButtonOK = new UnityEvent();
      }

      onButtonOK.AddListener(rightAction);
    }
  }

  public override void Show(object obj = null, bool bUpdate = false, Action complete = null)
  {
    base.Show(obj, bUpdate, complete);

    // 버튼 이벤트 클리어
    ClearButtonEvent();

    if (onButtonCancel == null)
    {
      onButtonCancel = new UnityEvent();
    }
    onButtonCancel.AddListener(() => { Hide(); });

    if (onButtonOK == null)
    {
      onButtonOK = new UnityEvent();
    }
    onButtonOK.AddListener(()=> { Hide(); });
  }
}