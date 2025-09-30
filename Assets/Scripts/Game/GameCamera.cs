using System;
using System.Collections;
using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Utils;
using FAIRSTUDIOS.UI;
using UnityEngine;

public class GameCamera : MonoBehaviour, ITouchEvent
{
  public enum CameraTypes
  {
    Ground,
    Levitation,
    Main,
    Sub,
    OverUI,
  }

  private const int TOUCH_PRIORITY = 1000;

  [SerializeField] private GenericDictionary<CameraTypes, Camera> cameras;
  [SerializeField] private Dictionary<CameraTypes, int> cameraCullingMasks = new();

  private Vector3 autoDragPos;
  private Coroutine coAutoDrag;
  private Coroutine coMoveOrZoom;

  private float cameraDefaultSize = 5.0f;

  private bool startMoveOrZoom;
  private bool isDragLock;

  private bool TouchedUIIsHUD;

  [SerializeField]
  private float dragSpeed = 0f;
  [SerializeField]
  private float autoDragSpeed = 0.1f;
  [SerializeField]
  private float autoDragRatio = 0.3f;
  [SerializeField]
  private float zoomSpeed = 0.01f;

  //줌 했을 때 카메라 최소 사이즈
  [SerializeField]
  private float zoomMinSize = 1f;

  //줌 했을 때 카메라 최대 사이즈
  [SerializeField]
  private float zoomMaxSize = 17;

  [SerializeField] private float edgeRatio = 0.1f;

  public Vector2 mapSizeWidth = new(-45, 45);
  public Vector2 mapSizeHeight = new(-30, 30);
  public float MapWidth => Mathf.Abs(mapSizeWidth.x) + mapSizeWidth.y;
  public float MapHeight => Mathf.Abs(mapSizeHeight.x) + mapSizeHeight.y;

  private Vector2 cameraSize;

  private Action<Vector3> moveUpdateAction;
  private Action<float> zoomUpdateAction;

  public bool IsRendering { get; private set; }

  public bool IsLock { get; set; }

  public bool IsDragLock
  {
    get => isDragLock;
    set
    {
      isDragLock = value;
      if (!value)
      {
        if (coAutoDrag != null)
        {
          StopCoroutine(coAutoDrag);
        }
      }
    }
  }

  public Camera ground => cameras[CameraTypes.Ground];
  public Camera levitation => cameras[CameraTypes.Levitation];
  public Camera main => cameras[CameraTypes.Main];
  public Camera sub => cameras[CameraTypes.Sub];
  public Camera overUI => cameras[CameraTypes.OverUI];

  public float orthographicSize
  {
    get => main.orthographicSize;
    set
    {
      foreach (var kv in cameras)
      {
        kv.Value.orthographicSize = value;
      }
    }
  }

  public float ScreenWidth => main.pixelWidth;
  public float ScreenHeight => main.pixelHeight;

  public float CameraMinSize
  {
    get => zoomMinSize;
    set => zoomMinSize = value;
  }

  public float CameraMaxSize
  {
    get => zoomMaxSize;
    set => zoomMaxSize = value;
  }

  /// <summary>
  /// 카메라 뷰 영역
  /// </summary>
  public Vector2 CameraSize
  {
    get => cameraSize;
    set
    {
      cameraSize = value;
      var pos = transform.position;
      CameraArea = new Rect(pos.x - cameraSize.x * 0.5f, pos.y - cameraSize.y * 0.5f, cameraSize.x, cameraSize.y);
    }
  }

  /// <summary>
  /// 카메라 뷰 영역
  /// </summary>
  public Rect CameraArea { get; private set; }

  private void OnEnable()
  {
    var ratio = orthographicSize / cameraDefaultSize;
    CameraSize = new Vector2(ScreenWidth * ratio, ScreenHeight * ratio);

    TouchManager.AddListenerTouchEvent(this, TOUCH_PRIORITY);
  }
  private void OnDisable()
  {
    StopAllCoroutines();
    TouchManager.RemoveListenerTouchEvent(this);
  }
  private void Awake()
  {
    foreach (var kv in cameras)
    {
      cameraCullingMasks[kv.Key] = kv.Value.cullingMask;
    }

    cameraDefaultSize = ScreenHeight * 0.5f;
    orthographicSize = CameraMaxSize; //최대 줌 사이즈에서 시작

    //SoundManager.ZoomEffectAmvienceVolume();
  }

  private void OnApplicationPause(bool pause)
  {
    // 자유 선택 이슈 있음.
    // if (pause)
    // {
    //   IsDragLock = false;
    // }
  }

  /// <summary>
  /// 외곽 영역에 커서에 있을 경우 해당 방향으로 이동하는 함수
  /// </summary>
  /// <param name="pos"></param>
  /// <returns></returns>
  private Vector3 GetEdgeMovePosition(Vector3 pos)
  {
    var rtTop = new Rect(0, ScreenHeight - ScreenHeight * edgeRatio, ScreenWidth, ScreenHeight * edgeRatio);
    var rtBottom = new Rect(0, 0, ScreenWidth, ScreenHeight * edgeRatio);
    var rtLeft = new Rect(0, 0, ScreenWidth * edgeRatio, ScreenHeight);
    var rtRight = new Rect(ScreenWidth - ScreenWidth * edgeRatio, 0, ScreenWidth * edgeRatio, ScreenHeight);

    // 외곽 영역에 해당되는지 확인
    // 점점 더 외곽으로 이동할수록 이동속도가 빨라진다.
    var movePos = Vector3.zero;
    if (rtTop.Contains(pos))
    {
      movePos += new Vector3(0, (rtTop.yMin - pos.y) / (ScreenHeight * edgeRatio * 10) * -2, 0);
    }
    if (rtBottom.Contains(pos))
    {
      movePos += new Vector3(0, (rtBottom.yMax - pos.y) / (ScreenHeight * edgeRatio * 10) * -2, 0);
    }
    if (rtLeft.Contains(pos))
    {
      movePos += new Vector3((rtLeft.xMax - pos.x) / (ScreenWidth * edgeRatio * 10) * -2, 0, 0);
    }
    if (rtRight.Contains(pos))
    {
      movePos += new Vector3((rtRight.xMin - pos.x) / (ScreenWidth * edgeRatio * 10) * -2, 0, 0);
    }

    return movePos;
  }

  private IEnumerator UpdateAutoMove()
  {
    while (IsValidAutoMoved())
    {
      autoDragPos -= autoDragPos * autoDragSpeed;
      transform.position = RivisionPos(transform.position + autoDragPos);
      moveUpdateAction?.Invoke(transform.position);

      yield return null;
    }

    coAutoDrag = null;
  }
  private void UpdateMove(Vector3 lastPos, Vector3 newPos, bool isDragSpeed)
  {
    Vector3 dragPos;
    if (IsDragLock)
    {
      // 커서의 위치에 따라 카메라 이동
      dragPos = GetEdgeMovePosition(newPos);
      transform.position = RivisionPos(transform.position + dragPos);
    }
    else
    {
    var ratio = orthographicSize / cameraDefaultSize;
    var offset = lastPos - newPos;
    if (isDragSpeed && dragSpeed > 0)
      offset *= dragSpeed;

    dragPos = offset * ratio;
    autoDragPos = dragPos * autoDragRatio;
    transform.position = RivisionPos(transform.position + dragPos);
    moveUpdateAction?.Invoke(transform.position);
    }
  }
  private void UpdateZoom(float offset, float zoomSpeed, bool positionHold = false)
  {
    // 사이즈 수정
    var size = orthographicSize - offset * (zoomSpeed * this.zoomSpeed);
    var repSize = Mathf.Clamp(size, CameraMinSize, CameraMaxSize);

    // var mapSizeH = -mapSizeHeight.x + mapSizeHeight.y;
    // var mapSizeW = -mapSizeWidth.x + mapSizeWidth.y;

    // var cameraH = repSize * 2.0f;
    // var cameraW = main.aspect * cameraH;

    // var subH = mapSizeH - cameraH;
    // var subW = mapSizeW - cameraW;

    // if (subH < subW)
    // {
    //   // H 비교
    //   if (subH < 0)
    //   {
    //     repSize = mapSizeH * 0.5f;
    //   }
    // }
    // else
    // {
    //   // W 비교
    //   if (subW < 0)
    //   {
    //     var repCameraH = mapSizeW / main.aspect;
    //     repSize = repCameraH * 0.5f;
    //   }
    // }

    //최대or최소 Size에서 카메라만 이동되는 상황 예외처리
    var doZoom = !(orthographicSize == repSize);

    orthographicSize = repSize;  //줌 실행
    var ratio = orthographicSize / cameraDefaultSize;
    CameraSize = new Vector2(ScreenWidth * ratio, ScreenHeight * ratio);

    if (positionHold)
      return;

    var pos = transform.position;
    //카메라 이동: TouchManager.firstPinchPos_scr => Pinch의 중심점
    if (offset > 0.1f || offset < -0.1f)
    {
      if (doZoom)
      {
        //pos = Vector2.MoveTowards(transform.position, baseCamera.ScreenToWorldPoint(TouchManager.FirstPinchPosToScreen), offset * (zoomSpeed * ZoomSpeed));
      }
    }

    // 위치 보정 > 터치를 받은데에서만 처리하기
    pos = RivisionPos(pos);
    pos.z = transform.position.z;
    transform.position = pos;
    moveUpdateAction?.Invoke(transform.position);
    zoomUpdateAction?.Invoke(orthographicSize);
  }

  /// <summary>
  /// 카메라 오브젝트으 위치가 최대 드로우 범위를 벗어난 경우 보정함
  /// </summary>
  /// <param name="pos"></param>
  /// <returns></returns>
  private Vector3 RivisionPos(Vector3 pos)
  {
    var ratio = orthographicSize / cameraDefaultSize;
    CameraSize = new Vector2(ScreenWidth * ratio, ScreenHeight * ratio);

    // var halfSize = CameraSize * 0.5f;
    // revPos.x = Mathf.Clamp(revPos.x, mapSizeWidth.x + halfSize.x, mapSizeWidth.y - halfSize.x);
    // revPos.y = Mathf.Clamp(revPos.y, mapSizeHeight.x + halfSize.y, mapSizeHeight.y - halfSize.y);

    var revPos = pos;
    if (mapSizeWidth.x > revPos.x)
    {
      revPos.x = mapSizeWidth.x;
    }
    else if (mapSizeWidth.y < revPos.x)
    {
      revPos.x = mapSizeWidth.y;
    }

    if (mapSizeHeight.x > revPos.y)
    {
      revPos.y = mapSizeHeight.x;
    }
    else if (mapSizeHeight.y < revPos.y)
    {
      revPos.y = mapSizeHeight.y;
    }

    return revPos;
  }

  // 자동 움직임 체크
  private bool IsValidAutoMoved()
  {
    var chkRange = 0.01f;
    if (autoDragPos.x < -chkRange || autoDragPos.x > chkRange || autoDragPos.y < -chkRange || autoDragPos.y > chkRange)
    {
      return true;
    }
    else
    {
      return false;
    }
  }

  private IEnumerator _MoveZoom(Vector3 pos, float startZoom, float endZoom, float duration, float delay = 0, Action callback = null)
  {
    if (delay > 0)
      yield return new WaitForSeconds(delay);

    float amount = 0;
    while (amount < 1)
    {
      amount += Time.deltaTime / duration;

      var newPos = Vector3.Lerp(transform.position, pos, amount);
      orthographicSize = Mathf.Lerp(startZoom, endZoom, amount);
      transform.position = RivisionPos(newPos);
      moveUpdateAction?.Invoke(transform.position);
      zoomUpdateAction?.Invoke(orthographicSize);


      yield return null;
    }

    callback?.Invoke();
  }

  private IEnumerator _Move(Vector3 movePos, float duration, float delay = 0, Action callback = null)
  {
    if (delay > 0)
      yield return new WaitForSeconds(delay);

    float amount = 0;
    while (amount < 1)
    {
      amount += Time.deltaTime / duration;

      // 이동
      var position = transform.position;
      var newPos = RivisionPos(Vector3.Lerp(position, movePos, amount));
      newPos.z = position.z;
      position = newPos;
      transform.position = position;
      moveUpdateAction?.Invoke(transform.position);

      yield return null;
    }

    callback?.Invoke();
  }
  /// <summary>
  /// 줌 시키기 위한 함수 정의
  /// </summary>
  /// <param name="endZoom"> 최소 사이즈와 최대 사이즈를 기준으로 0~1의 비율로 변경한다 </param>
  /// <param name="duration"></param>
  /// <param name="callback"></param>
  /// <returns></returns>
  private IEnumerator _Zoom(float startZoom, float endZoom, float duration, float delay = 0, Action callback = null)
  {
    if (delay > 0)
      yield return new WaitForSeconds(delay);

    float amount = 0;
    while (amount < 1)
    {
      amount += Time.deltaTime / duration;
      orthographicSize = Mathf.Lerp(startZoom, endZoom, amount);
      transform.position = RivisionPos(transform.position);

      zoomUpdateAction?.Invoke(orthographicSize);

      // 사이즈 수정
      // var repSize = Mathf.Clamp(orthographicSize, CameraMinSize, CameraMaxSize);
      // m_fZoomDistance = Math.Abs(((repSize - CameraMinSize) / ((CameraMaxSize - CameraMinSize) * 1.1765f)) - 1);
      //SoundManager.ZoomEffectAmvienceVolume();
      // UIManager.Instance.ResizeHUD();

      yield return null;
    }

    callback?.Invoke();
  }

  /// <summary>
  /// 사이즈를 통해 카메라 사이즈 비율을 구하기 위한 함수 정의
  /// </summary>
  /// <returns></returns>
  public float GetCameraSizeRatio(float fSize)
  {
    var fRatio = 0f;
    if (fSize > CameraMaxSize)
    {
      fRatio = 1f;
    }
    else if (fSize < CameraMinSize)
    {
      fRatio = 0f;
    }
    else
    {
      //사이즈 / (카메라 사이즈 최소값 + 카메라 사이즈 최대값)
      fRatio = (fSize - CameraMinSize) / (CameraMaxSize - CameraMinSize);
    }

    return fRatio;
  }

  public void MoveAndZoom(Vector3 pos, float startZoom, float endZoom, float duration = 0.5f, float delay = 0f, bool blockTouch = false, Action callback = null)
  {
    if (IsLock || IsDragLock)
      return;

    if (null != coMoveOrZoom)
    {
      StopCoroutine(coMoveOrZoom);
      coMoveOrZoom = null;
    }

    if (blockTouch)
    {
      IsLock = true;
      IsDragLock = true;
      TouchManager.DisableTouchEvent();
      callback += () =>
      {
        IsLock = false;
        IsDragLock = false;
        TouchManager.EnableTouchEvent();
      };
    }

    startZoom = Mathf.Clamp(startZoom, CameraMinSize, CameraMaxSize);
    endZoom = Mathf.Clamp(endZoom, CameraMinSize, CameraMaxSize);
    coMoveOrZoom = StartCoroutine(_MoveZoom(pos, startZoom, endZoom, duration, delay, callback));
    startMoveOrZoom = true;
  }

  public void Move(Vector3 pos, float duration = 0.5f, float delay = 0f,
    bool blockTouch = false, Action callback = null)
  {
    if (IsLock || IsDragLock)
      return;

    if (null != coMoveOrZoom)
    {
      StopCoroutine(coMoveOrZoom);
      coMoveOrZoom = null;
    }

    if (blockTouch)
    {
      IsLock = true;
      IsDragLock = true;
      TouchManager.DisableTouchEvent();
      callback += () =>
      {
        IsLock = false;
        IsDragLock = false;
        TouchManager.EnableTouchEvent();
      };
    }

    coMoveOrZoom = StartCoroutine(_Move(pos, duration, delay, callback));
    startMoveOrZoom = true;
  }

  public void Zoom(float startZoom, float endZoom, float duration = 0.5f, float delay = 0f, bool blockTouch = false,
    Action callback = null)
  {
    if (IsLock || IsDragLock)
      return;

    if (null != coMoveOrZoom)
    {
      StopCoroutine(coMoveOrZoom);
      coMoveOrZoom = null;
    }

    if (blockTouch)
    {
      IsLock = true;
      IsDragLock = true;
      TouchManager.DisableTouchEvent();
      callback += () =>
      {
        IsLock = false;
        IsDragLock = false;
        TouchManager.EnableTouchEvent();
      };
    }

    startZoom = Mathf.Clamp(startZoom, CameraMinSize, CameraMaxSize);
    endZoom = Mathf.Clamp(endZoom, CameraMinSize, CameraMaxSize);
    coMoveOrZoom = StartCoroutine(_Zoom(startZoom, endZoom, duration, delay, callback));

    startMoveOrZoom = true;
  }

  public void ForceMove(BaseObject obj)
  {
    if (null == obj)
      return;

    transform.position = RivisionPos(obj.transform.position);
  }

  public void Follow(Transform transform, float? zoomSize = null, float duration = 1)
  {

  }

  public void AddMoveUpdateAction(Action<Vector3> action)
  {
    moveUpdateAction += action;
  }

  public void RemoveMoveUpdateAction(Action<Vector3> action)
  {
    moveUpdateAction -= action;
  }

  public void ClearMoveUpdateAction()
  {
    moveUpdateAction = null;
  }

  public void AddZoomUpdateAction(Action<float> action)
  {
    zoomUpdateAction += action;
    action?.Invoke(orthographicSize);
  }

  public void RemoveZoomUpdateAction(Action<float> action)
  {
    zoomUpdateAction -= action;
  }

  public void ClearZoomUpdateAction()
  {
    zoomUpdateAction = null;
  }

  /// <summary>
  /// 카메라 영역을 체크하기 위한 함수(카메라 영역이면 true, 아니면 false)
  /// </summary>
  public bool IsInCameraArea(Vector2 pos)
  {
    return CameraArea.Contains(pos);
    // return CameraArea.x < pos.x && pos.x < CameraArea.x + CameraArea.width && CameraArea.y < pos.y && pos.y < CameraArea.y + CameraArea.height;
  }
  public bool IsInCameraArea(Rect rect)
  {
    return CameraArea.Overlaps(rect);
  }

  public void SetRender(bool value)
  {
    foreach (var kv in cameras)
    {
      kv.Value.cullingMask = value ? cameraCullingMasks[kv.Key] : ~-1;
    }

    IsRendering = value;
  }

  #region Touch Event

  public void OnTouchBegan(Vector3 pos, bool isFirstTouchedUI)
  {
    TouchedUIIsHUD = false;
    if (IsLock || IsDragLock)
      return;

    // TODO kwrain tutorial
    //if (TutorialManager.IsPlaying)
    //  return;

    if (!startMoveOrZoom)
    {
      if (null != coMoveOrZoom)
      {
        StopCoroutine(coMoveOrZoom);
        coMoveOrZoom = null;
      }
    }

    if (isFirstTouchedUI)
    {
      var raycastResult = KRaycast.GetRaycastResult();
      if (raycastResult.gameObject != null)
      {
        var hud = raycastResult.gameObject.GetComponentInParent<HUDBehaviour>();
        if (hud == null)
          return;

        TouchedUIIsHUD = true;
      }
    }

    startMoveOrZoom = false;
    autoDragPos = Vector3.zero;
  }
  public void OnTouchStationary(Vector3 pos, float time, bool isFirstTouchedUI)
  {
    if (isFirstTouchedUI /*|| TutorialManager.IsPlaying*/)
      return;

  }
  public void OnTouchMoved(Vector3 lastPos, Vector3 newPos, bool isFirstTouchedUI)
  {
    //if (TutorialManager.IsPlaying)
    //  return;

    if (isFirstTouchedUI && !TouchedUIIsHUD)
      return;

    if (!IsLock)
    {
      UpdateMove(lastPos, newPos, true);
    }
  }
  public void OnTouchEnded(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {
    //if (TutorialManager.IsPlaying)
    //  return;

    if (null != coAutoDrag)
    {
      StopCoroutine(coAutoDrag);
      coAutoDrag = null;
    }

    if (isFirstTouchedUI)
      return;

    if (!IsLock)
    {
      if (IsValidAutoMoved() && coAutoDrag == null)
      {
        coAutoDrag = StartCoroutine(UpdateAutoMove());
      }
    }
  }
  public void OnTouchCanceled(Vector3 pos, bool isFirstTouchedUI, bool isMoved) { }
  public void OnLongTouched(Vector3 pos, bool isFirstTouchedUI) { }
  public void OnClicked(Vector3 pos, bool isFirstTouchedUI) { }
  public void OnPinchUpdated(float offset, float zoomSpeed, bool isFirstTouchedUI)
  {
    //if (TutorialManager.IsPlaying)
    //  return;

    // Debug.Log($"W : {ScreenWidth}, H : {ScreenHeight}");

    if (isFirstTouchedUI)
      return;

    if (UIManager.Instance.OpenPopup!= null)
    {
      var uiAttribute = UIManager.Instance.OpenPopup.Attribute;
      if (!uiAttribute.IsBlockMoveClose && uiAttribute.UseDim || uiAttribute.UseDim)
      {
        return;
      }
    }

    if (!IsLock)
    {
      UpdateZoom(offset, zoomSpeed);
    }
  }
  public void OnPinchEnded()
  {
  }

  public void OnChangeTouchEventState(bool state)
  {

  }

  #endregion

#if UNITY_EDITOR
  void OnDrawGizmos()
  {
    if(!cameras.ContainsKey(CameraTypes.Main))
         return;

    //카메라 라인
    Gizmos.color = Color.yellow;
    var x = transform.position.x;
    var y = transform.position.y;
    var halfSize = CameraSize * 0.5f;
    Gizmos.DrawLine(new Vector3(x + halfSize.x, y - halfSize.y, 0), new Vector3(x + halfSize.x, y + halfSize.y, 0));
    Gizmos.DrawLine(new Vector3(x - halfSize.x, y - halfSize.y, 0), new Vector3(x - halfSize.x, y + halfSize.y, 0));
    Gizmos.DrawLine(new Vector3(x - halfSize.x, y + halfSize.y, 0), new Vector3(x + halfSize.x, y + halfSize.y, 0));
    Gizmos.DrawLine(new Vector3(x - halfSize.x, y - halfSize.y, 0), new Vector3(x + halfSize.x, y - halfSize.y, 0));

    // 맵 최대 사이즈
    Gizmos.color = Color.red;
    Gizmos.DrawLine(new Vector3(mapSizeWidth.y, mapSizeHeight.x, 0), new Vector3(mapSizeWidth.y, mapSizeHeight.y, 0));
    Gizmos.DrawLine(new Vector3(mapSizeWidth.x, mapSizeHeight.x, 0), new Vector3(mapSizeWidth.x, mapSizeHeight.y, 0));
    Gizmos.DrawLine(new Vector3(mapSizeWidth.x, mapSizeHeight.y, 0), new Vector3(mapSizeWidth.y, mapSizeHeight.y, 0));
    Gizmos.DrawLine(new Vector3(mapSizeWidth.x, mapSizeHeight.x, 0), new Vector3(mapSizeWidth.y, mapSizeHeight.x, 0));
  }
#endif
}
