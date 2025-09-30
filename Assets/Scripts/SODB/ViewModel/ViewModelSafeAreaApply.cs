using System.Collections;
using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[BindProperty(typeof(PropertyVector2))]
public class ViewModelSafeAreaApply : ViewModelBase<RectTransform>
{
  public enum SafeAreaHorizontalType
  {
    None = -1,
    Both = 0,
    Left,
    Right,
  }
  public enum SafeAreaVerticalType
  {
    None = -1,
    Both = 0,
    Top,
    Bottom,
  }

  public enum MirrorHorizontalType
  {
    None = -1,
    LeftToRight,
    RightToLeft
  }

  public enum MirrorVerticalType
  {
    None = -1,
    BottomToTop,
    TopToBottom,
  }

  public enum ApplyType
  {
    StretchToRight,
    StretchToLeft,
    StretchToBottom,
    StretchToTop,
    Anchor,
    Anchor_StretchToRight,
    Anchor_StretchToLeft,
    Anchor_StretchToBottom,
    Anchor_StretchToTop,
  }

  [Tooltip("Manipulate Type이 Anchor일 때만 적용됩니다.")]
  [ShowIf(nameof(IsAdjustableAnchorH)), SerializeField] private SafeAreaHorizontalType safeAreaType_H = SafeAreaHorizontalType.Both;
  [Tooltip("Manipulate Type이 Anchor일 때만 적용됩니다.")]
  [ShowIf(nameof(IsAdjustableAnchorV)), SerializeField] private SafeAreaVerticalType safeAreaType_V = SafeAreaVerticalType.Both;
  [Tooltip("Manipulate Type이 Anchor이면서 Safe AreaType_H가 Both일 때만 적용됩니다..")]
  [ShowIf(nameof(IsHAnchorBoth)), SerializeField] private MirrorHorizontalType mirrorType_H = MirrorHorizontalType.None;
  [Tooltip("Manipulate Type이 Anchor이면서 Safe AreaType_V가 Both일 때만 적용됩니다..")]
  [ShowIf(nameof(IsVAnchorBoth)), SerializeField] private MirrorVerticalType mirrorType_V = MirrorVerticalType.None;
  [SerializeField] private ApplyType manipulateType;
  [Tooltip("Manipulate Type이 Anchor일 때만 적용됩니다.")]
  [ShowIf(nameof(IsAnchor)),SerializeField]
  private Vector2 anchorMinCorrection = Vector2.zero;
  [Tooltip("Manipulate Type이 Anchor일 때만 적용됩니다.")]
  [ShowIf(nameof(IsAnchor)), SerializeField]
  private Vector2 anchorMaxCorrection = Vector2.zero;
  [ShowIf(nameof(IsNotOnlyAnchor)), SerializeField]
  private float defaultSize = 100f;
  private bool IsStretch => manipulateType == ApplyType.StretchToRight || manipulateType == ApplyType.StretchToLeft ||
                            manipulateType == ApplyType.StretchToBottom || manipulateType == ApplyType.StretchToTop;
  private bool IsNotOnlyAnchor => manipulateType != ApplyType.Anchor;
  private bool IsAnchor => !IsStretch;
  private bool IsAnchor_StretchH => manipulateType == ApplyType.Anchor_StretchToRight || manipulateType == ApplyType.Anchor_StretchToLeft;
  private bool IsAnchor_StretchV => manipulateType == ApplyType.Anchor_StretchToBottom || manipulateType == ApplyType.Anchor_StretchToTop;
  /// <summary>
  /// 좌우 앵커를 조정할 수 있는 방식
  /// </summary>
  private bool IsAdjustableAnchorH => manipulateType == ApplyType.Anchor || IsAnchor_StretchV;
  /// <summary>
  /// 상하 앵커를 조정할 수 있는 방식
  /// </summary>
  private bool IsAdjustableAnchorV => manipulateType == ApplyType.Anchor || IsAnchor_StretchH;
  private bool IsHAnchorBoth => IsAnchor && safeAreaType_H == SafeAreaHorizontalType.Both;
  private bool IsVAnchorBoth => IsAnchor && safeAreaType_V == SafeAreaVerticalType.Both;

  private Vector2 originAnchoredPosition;
  private Vector2 originSizeDelta;
  private Vector2 originAnchorMin;
  private Vector2 originAnchorMax;
  private Camera mainCam;
  private void Awake()
  {
    targets = targets != null ? targets : GetComponent<RectTransform>();
    originAnchoredPosition = targets.anchoredPosition;
    originSizeDelta = targets.sizeDelta;
    originAnchorMin = targets.anchorMin;
    originAnchorMax = targets.anchorMax;
  }

  protected override void OnEnable()
  {
    mainCam = Camera.main;
    base.OnEnable();
  }

  public override void OnPropertyChanged(PropertyBase property)
  {
    if (targets == null) return;
    var newValue = (property as PropertyVector2).NewValue;
    if (newValue == null) return;

    var safeArea = UnityEngine.Device.Screen.safeArea;
    var isEditor = UnityEngine.Device.Application.platform == RuntimePlatform.OSXEditor ||
                  UnityEngine.Device.Application.platform == RuntimePlatform.WindowsEditor;
    var isStandalone = UnityEngine.Device.Application.platform == RuntimePlatform.OSXPlayer ||
                      UnityEngine.Device.Application.platform == RuntimePlatform.WindowsPlayer;
    if (isEditor == true || isStandalone == true)
    {
      safeArea.width = mainCam.pixelWidth;
      safeArea.height = mainCam.pixelHeight;
    }
    var width = mainCam.pixelWidth;
    var height = mainCam.pixelHeight;

    var canvasSizeDelta = newValue;
    var screenSize = new Vector2(width, height);
    var anchorMin = safeArea.position;
    var anchorMax = anchorMin + safeArea.size;

    anchorMin.x /= width;
    anchorMax.x /= width;

    anchorMin.y /= height;
    anchorMax.y /= height;

    var leftGap = canvasSizeDelta.x * anchorMin.x;
    var rightGap = canvasSizeDelta.x * (1 - anchorMax.x);

    var bottomGap = canvasSizeDelta.y * anchorMin.y;
    var topGap = canvasSizeDelta.y * (1 - anchorMax.y);

    switch (manipulateType)
    {
      case ApplyType.StretchToRight:
      case ApplyType.Anchor_StretchToRight:
        // 오른쪽으로 늘려야되는 경우
        targets.sizeDelta = originSizeDelta + new Vector2(leftGap, 0);
        break;
      case ApplyType.StretchToLeft:
      case ApplyType.Anchor_StretchToLeft:
        // 왼쪽으로 늘려야되는 경우
        targets.sizeDelta = originSizeDelta + new Vector2(rightGap, 0);
        break;
      case ApplyType.StretchToBottom:
      case ApplyType.Anchor_StretchToBottom:
        // 아래쪽으로 늘려야되는 경우
        targets.sizeDelta = originSizeDelta + new Vector2(0, topGap);
        break;
      case ApplyType.StretchToTop:
      case ApplyType.Anchor_StretchToTop:
        // 위쪽으로 늘려야되는 경우
        targets.sizeDelta = originSizeDelta + new Vector2(0, bottomGap);
        break;
    }

    switch (manipulateType)
    {
      case ApplyType.Anchor_StretchToRight:
      case ApplyType.Anchor_StretchToLeft:
      case ApplyType.Anchor_StretchToBottom:
      case ApplyType.Anchor_StretchToTop:
      case ApplyType.Anchor:
        CalculateMirrorAnchorH(ref anchorMin, ref anchorMax);
        CalculateMirrorAnchorV(ref anchorMin, ref anchorMax);

        CalculateAnchorCorrection(ref anchorMin, ref anchorMax);

        if (manipulateType == ApplyType.Anchor)
        {
          // 최소 0, 최대 1로 제한
          anchorMin.x = Mathf.Clamp01(anchorMin.x);
          anchorMax.x = Mathf.Clamp01(anchorMax.x);
          anchorMin.y = Mathf.Clamp01(anchorMin.y);
          anchorMax.y = Mathf.Clamp01(anchorMax.y);
        }

        var resultMin = originAnchorMin;
        var resultMax = originAnchorMax;

        if (IsAdjustableAnchorH)
        {
          if (safeAreaType_H == SafeAreaHorizontalType.Both || safeAreaType_H == SafeAreaHorizontalType.Left)
            resultMin.x = anchorMin.x;

          if (safeAreaType_H == SafeAreaHorizontalType.Both || safeAreaType_H == SafeAreaHorizontalType.Right)
            resultMax.x = anchorMax.x;
        }

        if (IsAdjustableAnchorV)
        {
          if (safeAreaType_V == SafeAreaVerticalType.Both || safeAreaType_V == SafeAreaVerticalType.Bottom)
            resultMin.y = anchorMin.y;

          if (safeAreaType_V == SafeAreaVerticalType.Both || safeAreaType_V == SafeAreaVerticalType.Top)
            resultMax.y = anchorMax.y;
        }

        targets.anchorMin = resultMin;
        targets.anchorMax = resultMax;
        break;
    }
  }

  private void CalculateMirrorAnchorH(ref Vector2 anchorMin, ref Vector2 anchorMax)
  {
    if (IsAdjustableAnchorH == false)
    {
      return;
    }

    if (safeAreaType_H != SafeAreaHorizontalType.Both)
    {
      return;
    }

    if (mirrorType_H == MirrorHorizontalType.LeftToRight)
    {
      if (UnityEngine.Device.Screen.orientation != ScreenOrientation.LandscapeRight)
      {
        // 디바이스 카메라가 우측에 있지 않을 때
        anchorMax.x = 1f - anchorMin.x;
      }
      else
      {
        // 디바이스 카메라가 우측에 있을 때
        anchorMin.x = 1f - anchorMax.x;
      }
    }
    else if (mirrorType_H == MirrorHorizontalType.RightToLeft)
    {
      if (UnityEngine.Device.Screen.orientation != ScreenOrientation.LandscapeRight)
      {
        // 디바이스 카메라가 우측에 있지 않을 때
        anchorMin.x = anchorMax.x - 1f;
      }
      else
      {
        // 디바이스 카메라가 우측에 있을 때
        anchorMax.x = anchorMin.x - 1f;
      }
    }
  }

  private void CalculateMirrorAnchorV(ref Vector2 anchorMin, ref Vector2 anchorMax)
  {
    if (IsAdjustableAnchorV == false)
    {
      return;
    }

    if (safeAreaType_V != SafeAreaVerticalType.Both)
    {
      return;
    }

    if (mirrorType_V == MirrorVerticalType.BottomToTop)
    {
      if (UnityEngine.Device.Screen.orientation != ScreenOrientation.PortraitUpsideDown)
      {
        // 디바이스 카메라가 하단에 있지 않을 때
        anchorMax.y = 1f - anchorMin.y;
      }
      else
      {
        // 디바이스 카메라가 하단에 있을 때
        anchorMin.y = 1f - anchorMax.y;
      }
    }
    else if (mirrorType_V == MirrorVerticalType.TopToBottom)
    {
      if (UnityEngine.Device.Screen.orientation != ScreenOrientation.PortraitUpsideDown)
      {
        // 디바이스 카메라가 하단에 있지 않을 때
        anchorMin.y = anchorMax.y - 1f;
      }
      else
      {
        // 디바이스 카메라가 하단에 있을 때
        anchorMax.y = anchorMin.y - 1f;
      }
    }
  }

  private void CalculateAnchorCorrection(ref Vector2 anchorMin, ref Vector2 anchorMax)
  {
    // 안쪽으로 모으는 경우, 너무 안쪽으로 들어오는것을 보정값으로 보정한다.

    if (UnityEngine.Device.Screen.orientation != ScreenOrientation.LandscapeRight)
    {
      // 디바이스 카메라가 우측에 있지 않을 때
      anchorMin.x += anchorMinCorrection.x; // anchorMin.x가 줄어들수록 UI가 세이프 영역의 왼쪽으로 빠져나옴
      anchorMax.x += anchorMaxCorrection.x; // anchorMax.x가 늘어날수록 UI가 세이프 영역의 오른쪽으로 빠져나옴
    }
    else
    {
      // 디바이스 카메라가 우측에 있을 때만 보정값을 반대로 적용.
      anchorMin.x += anchorMaxCorrection.x * -1f; // anchorMin.x가 줄어들수록 UI가 세이프 영역의 왼쪽으로 빠져나옴
      anchorMax.x += anchorMinCorrection.x * -1f; // anchorMax.x가 늘어날수록 UI가 세이프 영역의 오른쪽으로 빠져나옴
    }


    if (UnityEngine.Device.Screen.orientation != ScreenOrientation.PortraitUpsideDown)
    {
      // 디바이스 카메라가 하단에 있지 않을 때
      anchorMin.y += anchorMinCorrection.y; // anchorMin.y가 줄어들수록 UI가 세이프 영역의 위쪽으로 빠져나옴
      anchorMax.y += anchorMaxCorrection.y; // anchorMax.y가 늘어날수록 UI가 세이프 영역의 아래쪽으로 빠져나옴
    }
    else
    {
      // 디바이스 카메라가 하단에 있을 때만 보정값을 반대로 적용
      anchorMin.y += anchorMinCorrection.y * -1f; // anchorMin.y가 줄어들수록 UI가 세이프 영역의 위쪽으로 빠져나옴
      anchorMax.y += anchorMaxCorrection.y * -1f; // anchorMax.y가 늘어날수록 UI가 세이프 영역의 아래쪽으로 빠져나옴
    }
  }

  [Button("프리셋 초기화")]
  public void RectTransformPresetInitialize()
  {
    targets = targets != null ? targets : GetComponent<RectTransform>();
    switch (manipulateType)
    {
      case ApplyType.StretchToRight:
        targets.anchorMin = new Vector2(0f, 0f);
        targets.anchorMax = new Vector2(0f, 1f);
        targets.pivot = new Vector2(0f, .5f);
        targets.sizeDelta = new Vector2(defaultSize, 0f);
        break;
      case ApplyType.StretchToLeft:
        targets.anchorMin = new Vector2(1f, 0f);
        targets.anchorMax = new Vector2(1f, 1f);
        targets.pivot = new Vector2(1f, .5f);
        targets.sizeDelta = new Vector2(defaultSize, 0f);
        break;
      case ApplyType.StretchToBottom:
        targets.anchorMin = new Vector2(0f, 1f);
        targets.anchorMax = new Vector2(1f, 1f);
        targets.pivot = new Vector2(.5f, 1f);
        targets.sizeDelta = new Vector2(0f, defaultSize);
        break;
      case ApplyType.StretchToTop:
        targets.anchorMin = new Vector2(0f, 0f);
        targets.anchorMax = new Vector2(1f, 0f);
        targets.pivot = new Vector2(.5f, 0f);
        targets.sizeDelta = new Vector2(0f, defaultSize);
        break;
      case ApplyType.Anchor:
        targets.anchorMin = new Vector2(0f, 0f);
        targets.anchorMax = new Vector2(1f, 1f);
        targets.pivot = new Vector2(.5f, .5f);
        break;
    }
  }
}
