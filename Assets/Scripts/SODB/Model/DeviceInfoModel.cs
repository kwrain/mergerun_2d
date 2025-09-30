using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[CreateAssetMenu(fileName = "DeviceInfoModel", menuName = "Model/DeviceInfoModel")]
public class DeviceInfoModel : ModelBase
{
  [SerializeField] private PropertyVector2 canvasSizeDelta;
  public Vector2 CanvasSizeDelta { get => canvasSizeDelta.RuntimeValue; set => canvasSizeDelta.RuntimeValue = value; }

  [field: System.NonSerialized] ScreenOrientation LastOrientation { get; set; } = ScreenOrientation.Unknown;
  [field: System.NonSerialized] string LastDeviceModel { get; set; } = string.Empty;
  [field: System.NonSerialized] Rect LastSafeArea { get; set; } = Rect.zero;
  [System.NonSerialized] private Camera mainCam = null;


  [SerializeField] private CanvasScaler.ScreenMatchMode screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
  public CanvasScaler.ScreenMatchMode ScreenMatchMode { get => screenMatchMode; set => screenMatchMode = value; }


  [SerializeField] private Vector2 refereneceScreenSize;
  public Vector2 RefereneceScreenSize { get => refereneceScreenSize; set => refereneceScreenSize = value; }

  public float ScaleFactor
  {
    get
    {
      float scaleFactor = 0;
      switch (screenMatchMode)
      {
        case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
          {
            // todo
          }
          break;
        case CanvasScaler.ScreenMatchMode.Expand:
          {
            scaleFactor = Mathf.Min(CurrentResolution.width / RefereneceScreenSize.x, CurrentResolution.height / RefereneceScreenSize.y);
          }
          break;
        case CanvasScaler.ScreenMatchMode.Shrink:
          {
            // todo
          }
          break;
      }
      return scaleFactor;
    }
  }

  public Resolution CurrentResolution
  {
    get
    {
      if(ReferenceEquals(mainCam, Camera.main) == false)
        mainCam = Camera.main;

      return new Resolution() { width = mainCam.pixelWidth, height = mainCam.pixelHeight };
      //return Application.isMobilePlatform == true ? Screen.currentResolution : new Resolution() { width = Screen.width, height = Screen.height };
    }
  }

  public bool IsScreenInfoChanged
  {
    get
    {
      bool result = UnityEngine.Device.Application.isMobilePlatform && LastOrientation != UnityEngine.Device.Screen.orientation;
      result = result || LastDeviceModel != UnityEngine.Device.SystemInfo.deviceModel;
      result = result || LastSafeArea != UnityEngine.Device.Screen.safeArea;
      return result;
    }
  }

  public async void OnScreenInfoChanged()
  {
    if (IsScreenInfoChanged == false) return;
    SOManager.Instance.StartCoroutine(WaitForCameraPixelSizeChanged());
  }

  IEnumerator WaitForCameraPixelSizeChanged()
  {
    yield return new WaitForEndOfFrame();
    // Debug.Log(Screen.currentResolution);
    // Debug.Log($"{Camera.main.pixelWidth}/{Camera.main.pixelHeight}");
    //CanvasSizeDelta = canvasSizeDelta;
    LastOrientation = UnityEngine.Device.Screen.orientation;
    LastDeviceModel = UnityEngine.Device.SystemInfo.deviceModel;
    LastSafeArea = UnityEngine.Device.Screen.safeArea;
    CanvasSizeDelta = new Vector2(CurrentResolution.width, CurrentResolution.height) / ScaleFactor;
  }
}
