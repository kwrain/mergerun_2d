using System;
using FAIRSTUDIOS.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[PopupUI(Type = typeof(PopupAlert), PrefabPath = "Common/PopupAlert", DontCloseOnInput = true, IsBlockMoveClose = true, CanvasType = UICanvasTypes.Focus, DontCloseOnLoad = true)]
public class PopupAlert : PopupUIBehaviour
{
  [Header("[Alert]")]
  public Text txtTitle;
  /// <summary>
  /// lds - 추가
  /// </summary>
  public GameObject titleLayoutGroup;
  /// <summary>
  public RectTransform iconRT;  // UI 아이콘 프리팹 사용시
  private GameObject goIcon;    // UI 아이콘 풀링

  public Text txtMainText;

  public Text txtButton;

  public Action onAfterPushToPool;

  #region Show
  public static PopupAlert Show(string title, string mainText, string button = "TXT_YES", UnityAction buttonAction = null)
  {
    return Show<PopupAlert>(title, mainText, button, buttonAction);
  }

  public static PopupAlert Show(string mainText, string button = "321", UnityAction buttonAction = null)
  {
    return Show(null, mainText, button, buttonAction);
  }
  #endregion Show

  #region Show<T>
  // lds - 23.4.1, Show<T> 추가
  // 완전 동일한 팝업이면서 다른 목적으로 사용해야되는 경우가 있을 수 있다.
  // ex) 구매처리에 의해 다른 PopupAlert를 내리는 경우 해당 PopupAlert의 Callback 처리가 안되는 상황이 발생함
  // 따라서 구매처리에 대한 PopupAlertPurchase를 추가, 해당 팝업은 PopupAlert와는 완전히 동일하면서 다른 인스턴스.
  // 사용법 예시 PopupAlert.Show<PopupAlertPurchase>();
  public static PopupAlert Show<T>(string title, string mainText, string button = "TXT_YES", UnityAction buttonAction = null) where T : PopupAlert
  {
    var popup = UIManager.Instance.ShowPopup<T>();
    MakePopup(popup, title, mainText, button, buttonAction);
    return popup;
  }

  public static PopupAlert Show<T>(string mainText, string button = "321", UnityAction buttonAction = null) where T : PopupAlert
  {
    return Show<T>(null, mainText, button, buttonAction);
  }
  // lds - 23.4.1, Show<T> 추가
  #endregion Show<T>

  #region MakePopup<T>
  // lds - 23.4.1, MakePopup<T> 추가
  // ShowPopup, ShowPopup<T> 모두 동일하게 세팅되야 하므로 관리 측면에서 추가함.
  private static void MakePopup<T>(T popup, string title, string mainText, string button = "TXT_YES", UnityAction buttonAction = null) where T : PopupAlert
  {
    popup.SetTitle(title);
    popup.SetMainText(mainText);
    popup.SetButton(button, buttonAction);
  }

  // lds - 23.4.1, MakePopup<T> 추가
  #endregion MakePopup<T>

  protected virtual void SetTitle(string title)
  {
    if (string.IsNullOrEmpty(title))
    {
      txtTitle.SetActive(false);
    }
    else
    {
      txtTitle.text = Localize.GetValue(title);
      txtTitle.SetActive(true);
    }
    iconRT.SetActive(false);
  }

  protected virtual void SetMainText(string mainText)
  {
    txtMainText.text = Localize.GetValue(mainText);
  }

  protected virtual void SetButton(string button, UnityAction buttonAction)
  {
    txtButton.text = Localize.GetValue(button);

    if(buttonAction != null)
    {
      if (onButtonOK == null)
      {
        onButtonOK = new UnityEvent();
      }

      onButtonOK.AddListener(buttonAction);
    }
  }

  public override void Show(object obj = null, bool bUpdate = false, Action complete = null)
  {
    base.Show(obj, bUpdate, complete);

    // 버튼 이벤트 클리어
    ClearButtonEvent();
    onAfterPushToPool = null;

    if (onButtonOK == null)
    {
      onButtonOK = new UnityEvent();
    }
    onButtonOK.AddListener(() => { Hide(); });
  }

  public override void OnAfterPushToPool()
  {
    base.OnAfterPushToPool();
    onAfterPushToPool?.Invoke();
    onAfterPushToPool = null;
  }
}
