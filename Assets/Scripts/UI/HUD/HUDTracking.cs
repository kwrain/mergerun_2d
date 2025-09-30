using UnityEngine;
using UnityEngine.UI;

/**
* HUDTracking.cs
* 작성자 : dev@fairstudios.kr
* 작성일 : 2022년 11월 30일 오후 5시 01분
*/
public class HUDTracking : MonoBehaviour
{
  protected enum EDirection
  {
    None,

    Top,
    Bottom,
    Left,
    Right,

    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
  }

  private RectTransform _rectTransform;

  [SerializeField] protected GameObject trackingTarget;
  [SerializeField] protected Vector3 trackingOffset;
  [SerializeField] protected Vector3 trackingSize;

  [SerializeField, Space] protected RectTransform rtDefault;
  [SerializeField] protected Vector2 defaultOffset;

  [SerializeField, Space] protected RectTransform rtNavigation;
  [SerializeField] protected Vector2 naviSpacing;

  [SerializeField, Space] protected Graphic[] graphics;

  protected Camera mainCamera { get; private set; }

  public RectTransform rectTransform
  {
    get
    {
      if (null == _rectTransform)
        _rectTransform = GetComponent<RectTransform>();

      return _rectTransform;
    }
  }

  protected virtual Vector3 TrackingPosition { get; set; }
  protected virtual Vector3 TrackingSize { get; set; } = Vector3.zero;

  public Vector2 DefaultOffset
  {
    get => defaultOffset;
    set => defaultOffset = value;
  }

  public virtual bool RaycastTarget 
  {
    get
    {
      if (graphics == null || graphics.Length == 0)
        return true;

      return graphics[0].raycastTarget;
    }
    set
    {
      if (graphics == null || graphics.Length == 0)
        return;

      foreach (var graphic in graphics)
      {
        graphic.raycastTarget = value;
      }
    }
  }

  protected virtual void Awake()
  {
  }

  protected virtual void OnEnable()
  {
    InitAnchor();
    
    if (trackingTarget != null)
    {
      TrackingPosition = trackingTarget.transform.position + trackingOffset;
    }

    if (mainCamera == null)
    {
      mainCamera = Camera.main;
    }

    rtNavigation.SetActive(false);
  }

  protected virtual void LateUpdate()
  {
    Tracking();
  }

  private void InitAnchor()
  {
    rectTransform.anchorMin = Vector2.zero;
    rectTransform.anchorMax = Vector2.one;
    rectTransform.anchoredPosition = Vector2.zero;
    rectTransform.offsetMin = rectTransform.offsetMax = rectTransform.sizeDelta = Vector2.zero;

    rtDefault.anchorMin = rtDefault.anchorMax = Vector2.zero;
    rtDefault.pivot = Vector2.one * 0.5f;
  }

  [ContextMenu("Reset Graphic Components")]
  protected void ResetGraphicComponent()
  {
    graphics = GetComponentsInChildren<Graphic>();
  }

  [ContextMenu("Tracking")]
  protected virtual void Tracking()
  {
    if (trackingTarget == null)
      return;

    if (mainCamera == null)
    {
      mainCamera = Camera.main;
    }

    if (mainCamera == null)
      return;

    var pos = RectTransformUtility.WorldToScreenPoint(mainCamera,
      new Vector3(TrackingPosition.x - TrackingSize.x, TrackingPosition.y - TrackingSize.y));
    var pos2 = RectTransformUtility.WorldToScreenPoint(mainCamera,
      new Vector3(TrackingPosition.x + TrackingSize.x, TrackingPosition.y + TrackingSize.y));

    // rtDefault.sizeDelta = new Vector2(pos2.x - pos.x, pos2.y - pos.y) / GameModel.DeviceInfoModel.ScaleFactor;
    // rtDefault.anchoredPosition = (pos + pos2) * 0.5f / GameModel.DeviceInfoModel.ScaleFactor + defaultOffset;

    // var eDirection = EDirection.None;
    // var vecNavi = rtNavigation.anchoredPosition;
    // // UI가 스크린에서 벗어나는지 확인한다.
    // if (rtDefault.anchoredPosition.y > UIManager.CanvasHeight + (rtDefault.sizeDelta.y * 0.5f))
    // {
    //   // 상
    //   vecNavi.y = UIManager.CanvasHeight * 0.5f - rtNavigation.sizeDelta.y * 0.5f - naviSpacing.y;
    //   eDirection = EDirection.Top;
    // }
    // else if (rtDefault.anchoredPosition.y < -rtDefault.sizeDelta.y * 0.5f)
    // {
    //   // 하
    //   vecNavi.y = -UIManager.CanvasHeight * 0.5f + rtNavigation.sizeDelta.y * 0.5f + naviSpacing.y;
    //   eDirection = EDirection.Bottom;
    // }
    //
    // if (rtDefault.anchoredPosition.x < -rtDefault.sizeDelta.x * 0.5f)
    // {
    //   // 좌
    //   vecNavi.x = -UIManager.CanvasWidth * 0.5f + rtNavigation.sizeDelta.x * 0.5f + naviSpacing.y;
    //   eDirection = EDirection.Left;
    // }
    // else if (rtDefault.anchoredPosition.x > UIManager.CanvasWidth + (rtDefault.sizeDelta.x * 0.5f))
    // {
    //   // 우
    //   vecNavi.x = UIManager.CanvasWidth * 0.5f - rtNavigation.sizeDelta.x * 0.5f - naviSpacing.x;
    //   eDirection = EDirection.Right;
    // }
    //
    // // 넘어감
    // if (eDirection != EDirection.None)
    // {
    //   if (Attribute.ShowNavigation)
    //   {
    //     SetNavigation(eDirection);
    //
    //     rtNavigation.anchoredPosition = vecNavi;
    //     rtNavigation.SetActive(true);
    //   }
    //   else
    //   {
    //     if (rtNavigation != null)
    //     {
    //       rtNavigation.SetActive(false);
    //     }
    //   }
    //
    //   rtDefault.SetActive(false);
    // }
    // else
    // {
    //   rtDefault.SetActive(true);
    //   rtNavigation.SetActive(false);
    // }
  }

  protected virtual void SetNavigation(EDirection eDirection)
  {
  }
}