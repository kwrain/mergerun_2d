using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public interface ITouchEvent
{
  void OnTouchBegan(Vector3 pos, bool isFirstTouchedUI);
  void OnTouchStationary(Vector3 pos, float time, bool isFirstTouchedUI);
  void OnTouchMoved(Vector3 lastPos, Vector3 newPos, bool isFirstTouchedUI);
  void OnTouchEnded(Vector3 pos, bool isFirstTouchedUI, bool isMoved);
  void OnTouchCanceled(Vector3 pos, bool isFirstTouchedUI, bool isMoved);
  void OnLongTouched(Vector3 pos, bool isFirstTouchedUI);
  void OnClicked(Vector3 pos, bool isFirstTouchedUI);
  void OnPinchUpdated(float offset, float zoomSpeed, bool isFirstTouchedUI);
  void OnPinchEnded();
  void OnChangeTouchEventState(bool state);
}

public class TouchManager : Singleton<TouchManager>
{
  private class TouchListener
  {
    public ITouchEvent Listener { get; }
    public int Priority { get; }

    public TouchListener(ITouchEvent listener, int priority)
    {
      Listener = listener;
      Priority = priority;
    }
  }

  private readonly List<TouchListener> listeners = new();

  #region Delegate

  delegate void TouchBeganDelegate(Vector3 pos, bool isFirstTouchedUI);
  delegate void TouchStationaryDelegate(Vector3 pos, float time, bool isFirstTouchedUI);
  delegate void TouchMovedDelegate(Vector3 lastPos, Vector3 newPos, bool isFirstTouchedUI);
  delegate void TouchEndedDelegate(Vector3 pos, bool isFirstTouchedUI, bool isMoved);
  delegate void TouchCanceledDelegate(Vector3 pos, bool isFirstTouchedUI, bool isMoved);
  delegate void LongTouchedDelegate(Vector3 pos, bool isFirstTouchedUI);
  delegate void ClickedDelegate(Vector3 pos, bool isFirstTouchedUI);
  delegate void PinchUpdatedDelegate(float offset, float zoomSpeed, bool isFirstTouchedUI);
  delegate void PinchEndedDelegate();

  delegate void OnChangeTouchEventStateDelegate(bool status);

  private event TouchBeganDelegate OnTouchBegan;
  private event TouchStationaryDelegate OnTouchStationary;
  private event TouchMovedDelegate OnTouchMoved;

  private event TouchEndedDelegate OnTouchEnded;
  private event TouchCanceledDelegate OnTouchCanceled;
  private event LongTouchedDelegate OnLongTouched;
  private event ClickedDelegate OnClicked;
  private event PinchUpdatedDelegate OnPinchUpdated;
  private event PinchEndedDelegate OnPinchEnded;

  private event OnChangeTouchEventStateDelegate OnChangeTouchEventState;

  #endregion

  private Vector2 vTouchRange = Vector2.one;
  private Vector3 vStartPos = Vector3.zero;
  private Vector3 vLastPos = Vector3.zero;
  private Vector2 vFirstPinchPosToScreen = Vector3.zero; //pinch zoom에서 사용할 중심
  private Vector2[] vLastPinchPos;

  private float fLongTouchSpeed = 5.0f;
  private float fLongTouchCnt = 0;
  private float fPinchSpeedToTouch = 1.0f;
  private float fPinchSpeedToMouse = 30.0f;

  private int nFingerID = -1;

  private bool isTouch = false;
  private bool isMoved = false;
  private bool isLongTouch = false;
  private bool isPinch = false;
  private bool isFirstTouchedUI = false;

  private int prevTouchCount;
  private TouchPhase prevTouchPhase;

  private Vector3 vecOldTouchPosition = Vector3.zero;
  private int iOldFingerId = 0;

  private bool bEnableTouchEvent = true;

  private GameObject cursorFollowGameObject;
  public static GameObject CursorFollowGameObject => Instance.cursorFollowGameObject;

  public static Vector2 FirstPinchPosToScreen => Instance.vFirstPinchPosToScreen;
  public static Vector2 LastTouchPosition => Instance.vLastPos;
  public static bool IsTouch => Instance.isTouch;
  public static bool IsMoved => Instance.isMoved;
  public static bool IsLongTouch => Instance.isLongTouch;
  public static bool IsPinch => Instance.isPinch;

  public static bool IsFirstTouchedUI
  {
    get => Instance.isFirstTouchedUI;
    set => Instance.isFirstTouchedUI = value;
  }

  // 터치 이벤트 컨트롤
  public static bool IsEnableTouch => Instance.bEnableTouchEvent;

  public static void EnableTouchEvent()
  {
    Instance.bEnableTouchEvent = true;
    Instance.OnChangeTouchEventState?.Invoke(Instance.bEnableTouchEvent);
  }

  public static void DisableTouchEvent()
  {
    Instance.bEnableTouchEvent = false;
    Instance.OnChangeTouchEventState?.Invoke(Instance.bEnableTouchEvent);
  }

  protected override void Awake()
  {
    base.Awake();

    touchMoveSensitive = 10f;
    // style.normal.textColor = Color.red;
    // style.fontSize = 60;

    var pixelDragThreshold = (0.25f * Screen.dpi / 2.54f);
    vTouchRange = new Vector2(pixelDragThreshold, pixelDragThreshold);
  }

  private void Update()
  {
    if (!IsEnableTouch)
      return;

    if(UnityEngine.Device.Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer)
    {
      TouchEvent();
    }
    else
    {
      MouseEvent();
    }
  }

  private void OnApplicationPause(bool pause)
  {
    if (pause)
    {
      InitializeData();
    }
  }

  protected override void ScenePreloadEvent(Scene currScene)
  {
    base.ScenePreloadEvent(currScene);

    OnTouchBegan = null;
    OnTouchStationary = null;
    OnTouchMoved = null;
    OnTouchEnded = null;
    OnTouchCanceled = null;
    OnLongTouched = null;
    OnClicked = null;
    OnPinchUpdated = null;
    OnPinchEnded = null;

    OnChangeTouchEventState = null;
  }

  /// <summary>
  /// 리스너 등록 (priority 낮을수록 먼저 실행)
  /// </summary>
  public static void AddListenerTouchEvent(ITouchEvent listener, int priority = 0)
  {
    if (Instance == null || listener == null)
      return;

    // 중복 방지
    Instance.listeners.RemoveAll(l => l.Listener == listener);

    Instance.listeners.Add(new TouchListener(listener, priority));
    // 우선순위 정렬
    Instance.listeners.Sort((a, b) => a.Priority.CompareTo(b.Priority));

    Instance.BindEvents();
  }

  /// <summary>
  /// 리스너 제거
  /// </summary>
  public static void RemoveListenerTouchEvent(ITouchEvent listener)
  {
    if (Instance == null || listener == null)
      return;

    Instance.listeners.RemoveAll(l => l.Listener == listener);
    Instance.BindEvents();
  }

  /// <summary>
  /// 현재 등록된 리스너 기준으로 델리게이트 바인딩
  /// </summary>
  private void BindEvents()
  {
    OnTouchBegan = null;
    OnTouchStationary = null;
    OnTouchMoved = null;
    OnTouchEnded = null;
    OnTouchCanceled = null;
    OnLongTouched = null;
    OnClicked = null;
    OnPinchUpdated = null;
    OnPinchEnded = null;
    OnChangeTouchEventState = null;

    foreach (var l in listeners)
    {
      OnTouchBegan += l.Listener.OnTouchBegan;
      OnTouchStationary += l.Listener.OnTouchStationary;
      OnTouchMoved += l.Listener.OnTouchMoved;
      OnTouchEnded += l.Listener.OnTouchEnded;
      OnTouchCanceled += l.Listener.OnTouchCanceled;
      OnLongTouched += l.Listener.OnLongTouched;
      OnClicked += l.Listener.OnClicked;
      OnPinchUpdated += l.Listener.OnPinchUpdated;
      OnPinchEnded += l.Listener.OnPinchEnded;
      OnChangeTouchEventState += l.Listener.OnChangeTouchEventState;
    }
  }

  private bool IsTouchedUI()
  {
    if (EventSystem.current == null)
      return false;

    if (UnityEngine.Device.Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer)
    {
      return EventSystem.current.IsPointerOverGameObject(0);
    }
    else
    {
      return EventSystem.current.IsPointerOverGameObject();
    }
  }
  private bool IsTouchInRange(Vector3 stdPos, Vector3 touchPos)
  {
    if (touchPos.x >= stdPos.x - vTouchRange.x && touchPos.x <= stdPos.x + vTouchRange.x
       && touchPos.y >= stdPos.y - vTouchRange.y && touchPos.y <= stdPos.y + vTouchRange.y)
    {
      return true;
    }
    else
    {
      return false;
    }
  }

  public void InitializeData()
  {
    vStartPos = Vector3.zero;

    isTouch = isMoved = isLongTouch = false;
    fLongTouchCnt = 0;
  }

  private void TouchEvent()
  {
    var touchCount = Input.touchCount;
    if (touchCount == 1)
    {
      if (isPinch) // 줌
      {
        return;
      }

      var touch = Input.GetTouch(0);

      vecOldTouchPosition = touch.position;
      iOldFingerId = touch.fingerId;

      switch (touch.phase)
      {
        case TouchPhase.Began: //터치 시작
          TouchBegan(touch.position, touch.fingerId);
          break;

        case TouchPhase.Moved: //터치 후 이동 중
          if (isTouch)
          {
            TouchMoved(touch.position, touch.fingerId);
          }
          else
          {
            TouchBegan(touch.position, touch.fingerId);
          }
          break;

        case TouchPhase.Stationary: //터치 후 이동 중에서 멈쳤을 경우
          TouchStationary(touch.position, touch.fingerId);
          break;

        case TouchPhase.Ended: //화면에서 손을 뗀 상태
          TouchEnded(touch.position, touch.fingerId);
          isPinch = false;
          break;

        case TouchPhase.Canceled: //시스템에 의해 터치가 종료 되었을 경우
          TouchCanceled(touch.position, touch.fingerId);
          break;
      }
    }
    else if(touchCount > 1)
    {
      // 멀티터치 시 TouchCanceled 호출되므로, 문제 발생 시 확인 필요함.
      if (prevTouchCount != touchCount)
      {
        TouchCanceled(vecOldTouchPosition, iOldFingerId);
      }

      var newZoomPos = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };

      if (isPinch)
      {
        //이벤트 마다 두 포인트의 중심(평균)을 갱신 => MapCamera.UpdateZoom에서 사용
        vFirstPinchPosToScreen.Set((newZoomPos[0].x + newZoomPos[1].x) * 0.5f, (newZoomPos[0].y + newZoomPos[1].y) * 0.5f);

        var lastDistance = Vector2.Distance(vLastPinchPos[0], vLastPinchPos[1]);
        var newDistance = Vector2.Distance(newZoomPos[0], newZoomPos[1]);
        var offset = newDistance - lastDistance;

        OnPinchUpdated?.Invoke(offset, fPinchSpeedToTouch, false);

        vLastPinchPos = newZoomPos;
        vLastPos = Input.GetTouch(0).position;
      }
      else
      {
        isPinch = true;
        vLastPinchPos = newZoomPos;
      }
    }
    else
    {
      isPinch = false;
      OnPinchEnded?.Invoke();
    }
    prevTouchCount = touchCount;
  }
  private void MouseEvent()
  {
    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
    {
      TouchBegan(Input.mousePosition);
    }
    else if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
    {
      if (isTouch)
      {
        if(vStartPos != Input.mousePosition)
        {
          TouchMoved(Input.mousePosition);
        }
      }
      else
      {
        TouchBegan(Input.mousePosition);
      }
    }
    else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
    {
      TouchEnded(Input.mousePosition);
    }
    else
    {
      var scroll = Input.GetAxis("Mouse ScrollWheel");
      if (scroll != 0)
      {
        isPinch = true;
        OnPinchUpdated?.Invoke(scroll, fPinchSpeedToMouse, false);
      }
      else if(isPinch)
      {
        isPinch = false;
        OnPinchEnded?.Invoke();
      }
    }
  }

  private void TouchBegan(Vector3 touchPos, int fingerID = -1)
  {
    // Debug.LogFormat("OnTouchBegan / Pos : {0}", touchPos);

    fLongTouchCnt = 0;
    vStartPos = touchPos;
    vLastPos = touchPos;
    nFingerID = fingerID;
    isFirstTouchedUI = IsTouchedUI();

    OnTouchBegan?.Invoke(vLastPos, isFirstTouchedUI);

    isTouch = true;
  }

  // GUIStyle style = new();
  // private void OnGUI()
  // {
  //   touchSensitive = GUI.HorizontalSlider(new Rect(100, 100, 1200, 60), touchSensitive, 0.0F, 20.0F);
  //   GUI.Label(new Rect(25, 200, 200, 60), touchSensitive.ToString(), style);
  // }

  /// <summary>
  /// 터치 무브 민감도. 클수록 더 많이 움직여야 무브로 판단. <br/>
  /// 2024.1.4 기준 10정도가 적당하여 10 고정.
  /// </summary>
  [SerializeField] private float touchMoveSensitive = 10f;
  private void TouchMoved(Vector3 touchPos, int fingerID = -1)
  {
    if (nFingerID == fingerID)
    {
      var lastPos = vLastPos;
      vLastPos = touchPos;

      var dist = Vector2.Distance(vStartPos, vLastPos);
      //Debug.Log($"거리 : {dist}");

      isMoved = dist >= touchMoveSensitive;

      if (IsTouchInRange(vStartPos, touchPos))
      {
        fLongTouchCnt += 0.1f;

        OnTouchStationary?.Invoke(touchPos, fLongTouchCnt, isFirstTouchedUI);
      }
      else
      {
        fLongTouchCnt = 0;

        OnTouchMoved?.Invoke(lastPos, touchPos, isFirstTouchedUI);

        // Debug.Log("X : " + touchPos.x + ",Y : " + touchPos.y);
        // Debug.LogFormat("OnTouchMove / Pos : {0}, isFirstTouchedUI {1}", touchPos, isFirstTouchedUI);
      }

      // 롱터치
      if (!isLongTouch && fLongTouchCnt > fLongTouchSpeed)
      {
        if (IsTouchInRange(vStartPos, touchPos))
        {
          isLongTouch = true;

          OnLongTouched?.Invoke(touchPos, isFirstTouchedUI);
        }
      }
    }
  }
  private void TouchStationary(Vector3 touchPos, int fingerID = -1)
  {
    if (IsTouchInRange(vStartPos, touchPos))
    {
      fLongTouchCnt += 0.1f;

      OnTouchStationary?.Invoke(touchPos, fLongTouchCnt, isFirstTouchedUI);
    }
    else
    {
      fLongTouchCnt = 0;
    }

    // 롱터치
    if (!isLongTouch && fLongTouchCnt > fLongTouchSpeed)
    {
      if (IsTouchInRange(vStartPos, touchPos))
      {
        isLongTouch = true;

        OnLongTouched?.Invoke(touchPos, isFirstTouchedUI);
      }
    }
  }
  private void TouchEnded(Vector3 touchPos, int fingerID = -1)
  {
    if (nFingerID == fingerID)
    {
      // 클릭 (마우스 클릭 후 뗐을 때 발생되는데 안에 아무것도 안들어 있다.)
      if (!isLongTouch && IsTouchInRange(vStartPos, touchPos))
      {
        OnClicked?.Invoke(touchPos, isFirstTouchedUI);
      }

      // 터치 (마우스와 터치 둘다 이 이벤트를 발생 시킨.)
      OnTouchEnded?.Invoke(touchPos, isFirstTouchedUI, isMoved);
    }

    InitializeData();
  }

  private void TouchCanceled(Vector3 touchPos, int fingerID = -1)
  {
    if (nFingerID == fingerID)
    {
      OnTouchCanceled?.Invoke(touchPos, isFirstTouchedUI, isMoved);
    }

    InitializeData();
  }
}
