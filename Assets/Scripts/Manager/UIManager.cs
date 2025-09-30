using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using FAIRSTUDIOS.Tools;
using FAIRSTUDIOS.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UIBehaviour = FAIRSTUDIOS.UI.UIBehaviour;

namespace FAIRSTUDIOS.UI
{
  public enum EUIDimType
  {
    NONE,

    BLACK,
    WHITE,
    CLEAR
  }

  public enum UICanvasTypes
  {
    /// <summary>
    /// TOUCH EFFECT 노출용 캔버스
    /// </summary>
    TouchEffect,

    /// <summary>
    /// TOAST / INDICATOR / IN_APP_MESSAGE 노출용 캔버스
    /// </summary>
    Focus,

    /// <summary>
    /// 시스템 Popup 노출용 캔버스
    /// </summary>
    System,

    /// <summary>
    /// 일반 Popup 노출용 캔버스
    /// </summary>
    Front,

    /// <summary>
    /// 일반 UI 노출용 캔버스
    /// </summary>
    Middle,

    /// <summary>
    /// 배경 관련 UI 노출용 캔버스
    /// </summary>
    Back,

    /// <summary>
    /// 게임 내 HUD 노출용 캔버스
    /// </summary>
    HUD,

    /// <summary>
    /// 비노출용 UI Pool 용 캔버스
    /// </summary>
    Pool,

    Max
  }

  public enum UIOverlapTypes
  {
    UNIQUE,
    UNIQUE_UPDATE,
    OVERLAP
  }

  #region Attribute

  public class UIElementAttribute : Attribute
  {
    /// <summary>
    /// UI Type
    /// </summary>
    public virtual Type Type { get; set; }

    /// <summary>
    /// UI/ 아래 경로를 입력한다.
    /// <para>- 각 타입에 따라 경로가 다름.</para>
    /// </summary>
    public string PrefabPath { get; set; }
  }

  /// <summary>
  /// UI, Popup, HUD 의 기본 어트리뷰트
  /// </summary>
  public class UIBaseAttribute : UIElementAttribute
  {
    /// <summary>
    /// UI Type
    /// </summary>
    public override Type Type
    {
      get => base.Type;
      set
      {
        base.Type = value;
        UniqueID = value.ToString();
      }
    }

    public string UniqueID { get; protected set; }
  }

  public class UIAttribute : UIBaseAttribute
  {
    public bool DontCloseOnLoad { get; set; } = false;
    public bool ShowPrevUI { get; set; } = true;

    public bool UseGameCamera { get; set; } = false;

    public bool UseHUD { get; set; } = false;
  }

  /// <summary>
  /// UI 생성 시 필요한 정보를 가지고 있는 Atrribute
  /// <para>- Managed UI 는 [Type], [UniqueID], [PrefabPath] 를 반드시 입력해야 한다.</para>
  /// <see cref="UseDim"/> qwdonqwoidnioqwndionqwiod
  /// <para>- Default Option : UseDim = true, IsExternalTouch = false, IsMovable = false, IsOverlap = false</para>
  /// - Parts UI 는 [PrefabPath] 만 입력한다. (옵션을 입력하여도 적용되지 않음)
  /// </summary>
  public class PopupUIAttribute : UIAttribute, ICloneable
  {
    private string overlapID;
    /// <summary>
    /// 자동으로 지정되므로 세팅하지 않아도된다.
    /// </summary>
    public string OverlapID
    {
      get
      {
        return overlapID;
      }
      set
      {
        overlapID = value;
        UniqueID = string.Format("{0}_{1}", UniqueID, overlapID);
      }
    }

    /// <summary>
    /// UI가 노출 될 Canvas 지정
    /// </summary>
    public UICanvasTypes CanvasType = UICanvasTypes.Middle;

    #region Option

    /// <summary>
    /// UI Dim 영역을 사용 여부
    /// </summary>
    public bool UseDim => DimType != EUIDimType.NONE;

    /// <summary>
    /// DimType 설정
    /// <para>CLEAR는 팝업 영역 외 터치를 막기 위함</para>
    /// </summary>
    public EUIDimType DimType = EUIDimType.BLACK;

    /// <summary>
    /// UI 외부영역을 터치해도 종료되지않음
    /// </summary>
    public bool DontCloseOnInput = true;

    /// <summary>
    /// UI 외부영역을 무브해도 종료되지않음
    /// </summary>
    public bool IsBlockMoveClose = true;

    /// <summary>
    /// 중복 허용 여부
    /// - 중복 허용 시 난수 값을 발생시켜 UniqueID 끝에 붙힌다.
    /// </summary>
    public UIOverlapTypes OverlapType = UIOverlapTypes.UNIQUE_UPDATE;
    public float OverlapTime = 0;

    #endregion

    public PopupUIAttribute Clone()
    {
      var clone = MemberwiseClone();
      return (PopupUIAttribute)Convert.ChangeType(clone, typeof(PopupUIAttribute));
    }

    object ICloneable.Clone()
    {
      return MemberwiseClone();
    }
  }

  public class HUDAttribute : UIBaseAttribute
  {
    /// <summary>
    /// 하나만 존재, false 인 경우 중복 생성 가능.
    /// </summary>
    public bool isUnique { get; set; }

    /// <summary>
    /// HUD Target 이 유저 스크린 밖으로 벗어났을 경우 내비게이션 아이콘 사용여부
    /// </summary>
    public bool ShowNavigation { get; set; }

    /// <summary>
    /// <code>
    /// Canvas Sort Order 값
    /// - 0 보다 클 경우에만 오브젝트 생성
    /// </code>
    /// </summary>
    public int SortingLayer { get; set; }

    public int OrderInLayer { get; set; }
  }
#endregion
}

public class UIManager : Singleton<UIManager>, ITouchEvent
{
  private const int TOUCH_PRIORITY = 1000;

  // TODO
  // Scritable Object 로 전환
  public const float TWEEN_UI_SHOW = 0.2f;
  public const float TWEEN_UI_HIDE = 0.2f;

  public const float TWEEN_BUTTON_DOWN = 0.1f;
  public const float TWEEN_BUTTON_UP = 0.2f;

  public const float TWEEN_UI_DIM = 0.7f;

  public const float QUEST_GOAL_HEIGHT_MAXCOUNT = 3;
  public const float QUEST_HEIGHT_MAXCOUNT = 3;

  public const float EVENT_HEIGHT_MAXCOUNT = 3;

  public const float SERVER_SELECTION_BUTTON_HEIGHT_MAXCOUNT = 5; //서버 선택 버튼 스크롤 카운트

  public const float TWEEN_UI_TOAST_STAY = 0.7f;

  private class Dim
  {
    private Image image;
    private KTweenAlpha tween;
    private RectTransform rectTransform;
    private List<PopupUIBehaviour> behaviours = new();

    public PopupUIBehaviour this[int index] => behaviours[index];

    public int Count => behaviours.Count;

    public Dim(EUIDimType dimType, Canvas canvas)
    {
      var go = new GameObject($"DIM_{dimType}");
      go.SetActive(false);

      image = go.AddComponent<Image>();
      var rtCanvas = canvas.transform as RectTransform;
      image.sprite = ResourceManager.Instance.Load<Sprite>("Sprites/UI/ui_dim");
      image.rectTransform.sizeDelta = rtCanvas.sizeDelta;
      var color = dimType switch
      {
        EUIDimType.BLACK => Color.black,
        EUIDimType.WHITE => Color.white,
        EUIDimType.CLEAR => Color.clear,
        _ => Color.clear
      };
      color.a = 0;
      image.color = color;
      image.gameObject.SetParent(canvas);
      image.transform.SetSiblingIndex(0);

      image.transform.localPosition = Vector3.zero;
      image.transform.localScale = Vector3.one;
      image.rectTransform.anchorMin = Vector2.zero;
      image.rectTransform.anchorMax = Vector2.one;
      image.rectTransform.anchoredPosition = Vector2.zero;
      image.rectTransform.sizeDelta = Vector2.zero;

      rectTransform = image.rectTransform;

      tween = go.AddComponent<KTweenAlpha>();
      tween.duration = TWEEN_UI_SHOW;
      tween.enabled = false;
    }

    public void Add(PopupUIBehaviour behaviour) => behaviours.Add(behaviour);
    public bool Remove(PopupUIBehaviour behaviour) => behaviours.Remove(behaviour);
    public bool Contains(PopupUIBehaviour behaviour) => behaviours.Contains(behaviour);

    public void SetActive(bool value) => rectTransform.SetActive(value);
    public void SetParent(Component component) => rectTransform.gameObject.SetParent(component);

    public void SetAsFirstSibling() => rectTransform.SetAsFirstSibling();

    public void SetSiblingIndex(int index) => rectTransform.SetSiblingIndex(index);

    public void SetTweenEnabled(bool value)
    {
      tween.enabled = value;
      if (value == false)
      {
        tween.alpha = 0f;
        tween.ClearFinishedEvent();
      }
    }

    public void Show()
    {
      // lds - 22.10.31, 이벤트 클리어 추가 ( Hide 호출 후 즉시 Show가 호출 될 때 Hide의 이벤트가 남아 dim이 꺼지는 상황 발생 )
      tween.ClearFinishedEvent();
      tween.from = tween.enabled ? image.color.a : 0f;
      tween.to = 0.7f;
      tween.RePlay();
    }

    public void Hide()
    {
      // lds - 22.10.31, 이벤트 클리어 추가 ( 리셋 용도. )
      tween.ClearFinishedEvent();
      tween.from = tween.enabled ? image.color.a : 0.7f;
      tween.to = 0f;
      tween.RePlay();
      tween.AddFinishedEvent(() =>
      {
        SetActive(false);
        tween.ClearFinishedEvent();
      });
    }

  }

  // OLD
  //public const int Width = 960;
  //public const int Height = 640;

  // RENEWAL
  //public const int Width = 2960;
  //public const int Height = 1440;

  //// RENEWAL
  public const int Width = 1280;
  public const int Height = 720;

  // UI 종합적으로 관리하는 매니저
  // UI 종류는 Toast UI, PopUp UI, UI 로 크게 3가지로 나뉜다.
  // 추후 변경 가능성 있음

  // 1) Toast UI
  //  - System, Front Canvas 에 노출된다.

  // 2) Popup UI
  //  - 팝업 형태의 UI며, 중요도에 따라 3개의 캔버스에 배치된다.
  //  - 캔버스에 배치된 UI 는 SbliingIndex 에 따라 오더링된다.
  //  - 주로 Front, Middle Canvas 에 노출된다.

  // 3) UI
  //  - 스크린에 항상 노출되고 있는 UI를 말한다.
  //  - Show/Hide 직접 호출로 제어하고, 씬 전환이 유지 여부를 Attribute 에 저장한다.
  //  - Back Canvas 에 노출된다.
  //  - 동시에 여러 UI가 노출될 수 없다.

  // UI Canvas 의 종류
  // System, Front, Middle, Back

  #region Default

  private Dictionary<UICanvasTypes, Canvas> dtCanvas = new();
  private Dictionary<UICanvasTypes, CanvasGroup> dtCanvasGroups = new();
  private Dictionary<UICanvasTypes, GraphicRaycaster> dtCanvasGraphicRaycaster = new();
  private List<PopupUIBehaviour> hidingPopups = new();

  Transform trUIPool;

  /// <summary>
  /// 이번 터치에 UI 가 Show 됬는지 확인하는 플래그
  /// </summary>
  public bool isShow;
  /// <summary>
  /// 이번 터치에 UI 가 Hide 됬는지 확인하는 플래그
  /// </summary>
  bool isHide;

  /// <summary>
  /// OnTouchEnded 에서 OnTouchMoved 가 호출됬었는지 확인하는 플래그
  /// </summary>
  bool isMovoed;

  public Camera UICamera { get; private set; }
  public Camera HUDCamera { get; private set; }

  public bool IsOpenUI { get { return null != CurrUI; } }

  /// <summary>
  /// 열려있는 Popup UI 가 있는지 확인하는 프로퍼티
  /// </summary>
  public bool IsOpenPopup { get { return null != OpenPopup; } }

  public float CanvasScale { get { return GetCanvas().transform.localScale.x; } }
  public static float CanvasWidth
  {
    get
    {
      var rectTransform = Instance.GetCanvas().GetComponent<RectTransform>();
      return rectTransform.sizeDelta.x;
    }
  }
  public static float CanvasHeight
  {
    get
    {
      var rectTransform = Instance.GetCanvas().GetComponent<RectTransform>();
      return rectTransform.sizeDelta.y;
    }
  }

  public Action<UIManager> onEscapeAction;

  protected override void Awake()
  {
    base.Awake();

    transform.position = new Vector3(0, 100, 0);

    InitCanvas();

    trUIPool = UIHelper.CreateGameObject("UIPool", gameObject).transform;
    trUIPool.SetParent(GetCanvas(UICanvasTypes.Pool).transform);
    var rectTransform = trUIPool.GetComponent<RectTransform>();
    rectTransform.anchoredPosition3D = Vector3.zero;
    rectTransform.anchorMin = new Vector2(0, 0);
    rectTransform.anchorMax = new Vector2(1, 1);
    rectTransform.offsetMin = new Vector2(0, 0);
    rectTransform.offsetMax = new Vector2(0, 0);
    trUIPool.localScale = Vector3.one;
  }

  private void Update()
  {
    if (!TouchManager.IsEnableTouch)
      return;

    if (Input.GetKeyDown(KeyCode.Escape))
    {
      onEscapeAction?.Invoke(this);
    }

    HUDCreatingLoop();
  }

#if DEV
  bool showSafeArea = false;
  private void OnGUI()
  {
    GUIStyle style = new GUIStyle();
    style.alignment = TextAnchor.MiddleCenter;
    style.fontSize = 30;

    if (GUI.Button(new Rect(0, Screen.height * 0.3f, Screen.width * 0.15f, Screen.height * 0.05f), "Safe Area"))
    {
      showSafeArea = !showSafeArea;

    }

    if (showSafeArea)
    {
      Rect rectSafeArea = Screen.safeArea;
      string safeArea = string.Format("xMin : {0} / yMin : {1} / xMax : {2} / yMax : {3}", rectSafeArea.x, rectSafeArea.y, rectSafeArea.width, rectSafeArea.height);
      GUI.Box(rectSafeArea, string.Empty);
      GUI.TextArea(rectSafeArea, safeArea, style);
    }
  }
#endif

  protected override void ScenePreloadEvent(Scene currScene)
  {
    base.ScenePreloadEvent(currScene);

    // Prepare UI 관련 초기화
    if(PreparingUI != null)
    {
      PreparingUI.CancelPrepareTimer(); // 진행하던 타이머 테스크 정지
      PreparingUI = null;
    }

    // Prepare Popup 관련 초기화
    foreach(var kv in dtPreparePopup)
    {
      var key = kv.Key; // attribute
      var value = kv.Value; // popup behaviour list
      foreach(var behaviour in value)
      {
        behaviour.CancelPrepareTimer(); // 진행하던 타이머 테스크 정지
      }
      value.Clear(); // popup behaviour list 클리어.
    }

    foreach (var kv in dtCanvasPopup)
    {
      if(kv.Value == null || kv.Value.Count == 0)
        continue;

      var index = 0;
      var behaviours = kv.Value.ToList();
      var count = behaviours.Count;
      while (index >= count)
      {
        var behaviour = behaviours[index++];
        if (behaviour.Attribute.DontCloseOnLoad)
          continue;

        kv.Value.Remove(behaviour);
        if (behaviour.Attribute.UseDim && dtDim.ContainsKey(behaviour.Attribute.DimType))
        {
          dtDim[behaviour.Attribute.DimType].Remove(behaviour);
        }

        PushPopupPool(behaviour);
      }
    }

    PrepareUICancel();

    // 생성된 전체 Popup 을 체크한다.
    // UI를 ObjectPool 로 옮긴다.
    // DontCloseOnLoad 인 경우 유지한다.
    foreach (var kv in dtPopupPool.Where(kv => kv.Value == null))
    {
      dtPopupPool.Remove(kv.Key);
    }
    UpdateDim();

    // 생성된 전체 ToastUI를 체크한다.
    // UI를 ObjectPool 로 옮긴다.
    //int count = ltToast.Count;
    //for (int i = 0; i < count; i++)
    //{
    //  ltToast[i].transform.SetParent(trUIPool);
    //}

    isShow = false;
    isHide = false;
    isMovoed = false;
  }

  protected override void SceneLoadedEvent(Scene scene, LoadSceneMode SceneMode)
  {
    base.SceneLoadedEvent(scene, SceneMode);

    TouchManager.AddListenerTouchEvent(this, TOUCH_PRIORITY);
  }

  public override async Task Initialize()
  {
    await base.Initialize();

    if (CurrUI != null)
    {
      CurrUI.Initialize();
    }

    foreach (var kv in dtUIPool)
    {
      kv.Value.Initialize();
    }

    foreach (var kv in dtPopupPool)
    {
      kv.Value.Initialize();
    }

    InitializeHUD();
  }

  #region Canvas

  private void InitCanvas()
  {
    dtCanvas.Clear();
    dtCanvasGraphicRaycaster.Clear();

    var go = UIHelper.CreateGameObject("UI.Camera", gameObject);
    UICamera = go.AddComponent<Camera>();
    UICamera.clearFlags = CameraClearFlags.Depth;
    UICamera.cullingMask = LayerMask.GetMask("UI");
    UICamera.orthographic = true;
    UICamera.orthographicSize = 5;
    UICamera.nearClipPlane = 0.3f;
    UICamera.farClipPlane = 300;
    UICamera.depth = 25;

    go = UIHelper.CreateGameObject("HUD.Camera", gameObject);
    go.transform.localPosition = new Vector3(0f, 75f, 0f);
    HUDCamera = go.AddComponent<Camera>();
    HUDCamera.clearFlags = CameraClearFlags.Depth;
    HUDCamera.cullingMask = LayerMask.GetMask("UI");
    HUDCamera.orthographic = true;
    HUDCamera.orthographicSize = 5;
    HUDCamera.nearClipPlane = 0.3f;
    HUDCamera.farClipPlane = 300;
    HUDCamera.depth = 15;

    const string canvasNameFormat = "UI.Canvas.{0}";
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      var resolution = new Vector2Int(Width, Height);

      var canvasType = (UICanvasTypes)i;
      var canvas = UIHelper.CreateCanvas(string.Format(name, canvasType), gameObject, resolution);
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.gameObject.layer = 5;
      canvas.name = string.Format(canvasNameFormat, canvasType);
      canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
      var scaler = canvas.gameObject.GetComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
      scaler.referencePixelsPerUnit = 50;
      scaler.matchWidthOrHeight = 1f;

      CanvasGroup canvasGroup = null;
      switch (canvasType)
      {
        case UICanvasTypes.TouchEffect:
          canvas.planeDistance = 25;
          canvas.sortingOrder = 60;
          break;

        case UICanvasTypes.Focus:
          canvas.planeDistance = 50;
          canvas.sortingOrder = 50;
          break;

        case UICanvasTypes.System:
          canvas.planeDistance = 100;
          canvas.sortingOrder = 40;
          break;

        case UICanvasTypes.Front:
          canvas.planeDistance = 150;
          canvas.sortingOrder = 30;
          break;

        case UICanvasTypes.Middle:
          canvas.planeDistance = 200;
          canvas.sortingOrder = 25;
          break;

        case UICanvasTypes.Back:
          canvas.planeDistance = 250;
          canvas.sortingOrder = 15;
          break;

        case UICanvasTypes.HUD:
          canvas.worldCamera = HUDCamera;
          canvas.planeDistance = 300;
          canvas.sortingOrder = 0;
          canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
          break;

        case UICanvasTypes.Pool:
          canvas.planeDistance = 1000;
          canvas.sortingOrder = -10;
          break;
      }

      if (null != canvas)
      {
        dtCanvas.Add(canvasType, canvas);
        dtCanvasGroups.Add(canvasType, canvasGroup);
        dtCanvasGraphicRaycaster.Add(canvasType, canvas.GetComponent<GraphicRaycaster>());
      }

      if (!dtCanvasPopup.ContainsKey(canvasType))
      {
        dtCanvasPopup.Add(canvasType, new List<PopupUIBehaviour>());
      }

      if (!dtPreparePopup.ContainsKey(canvasType))
      {
        dtPreparePopup.Add(canvasType, new List<PopupUIBehaviour>());
      }
    }
  }

  /// <summary>
  /// 캔버스 불러오기
  /// </summary>
  /// <param name="canvasType"></param>
  /// <returns></returns>
  public Canvas GetCanvas(UICanvasTypes canvasType = UICanvasTypes.Middle)
  {
    if (dtCanvas.Count == 0)
      InitCanvas();

    return dtCanvas[canvasType];
  }

  /// <summary>
  /// 캔버스 오브젝트 불러오기
  /// </summary>
  /// <param name="canvasType"></param>
  /// <returns></returns>
  public GameObject GetCanvasObject(UICanvasTypes canvasType = UICanvasTypes.Middle)
  {
    var canvas = GetCanvas(canvasType);
    if (null != canvas)
    {
      return canvas.gameObject;
    }
    else
    {
      return null;
    }
  }

  public CanvasGroup GetCanvasGroup(UICanvasTypes canvasType = UICanvasTypes.Middle)
  {
    if (dtCanvas.Count == 0)
      InitCanvas();

    return dtCanvasGroups[canvasType];
  }

  public GraphicRaycaster GetCanvasGraphicRaycaster(UICanvasTypes canvasType = UICanvasTypes.Middle)
  {
    if (dtCanvas.Count == 0)
      InitCanvas();

    return dtCanvasGraphicRaycaster[canvasType];
  }

  public bool IsTouched(Vector3 pos, params UICanvasTypes[] ignoreCanvas)
  {
    var pointerEventData = new PointerEventData(null) {position = pos};
    var result = new List<RaycastResult>();

    if (ignoreCanvas == null || ignoreCanvas.Length == 0)
    {
      foreach (var kv in dtCanvasGraphicRaycaster)
      {
        kv.Value.Raycast(pointerEventData, result);

        if (result.Count > 0)
          return true;
      }
    }
    else
    {
      foreach (var kv in dtCanvasGraphicRaycaster)
      {
        if (IgnoreContains(kv.Key))
          continue;

        kv.Value.Raycast(pointerEventData, result);
        if (result.Count > 0)
          return true;
      }
    }

    return false;

    bool IgnoreContains(UICanvasTypes canvasTypes)
    {
      foreach (var ignore in ignoreCanvas)
      {
        if (ignore == canvasTypes)
          return true;
      }

      return false;
    }
  }

  #endregion Canvas

  public bool IsBlockEvent { get; private set; }
  public bool IsLoading { get; private set; }
  public bool BlockEventFromUser { get; private set; }
  public bool LoadingFromUser { get; private set; }

  /// <summary>
  /// lds - 23.3.16, 추가 <br/>
  /// <seealso cref="GraphicRaycasterRegistration"/> 를 통해 등록된 이벤트 <br/>
  /// <seealso cref="GraphicRaycaster"/> 컴포넌트를 가진 객체를 대상으로 하며 <br/>
  /// <seealso cref="IsBlockEvent"/> 상태를 파라미터로 넘겨준다. <br/>
  /// 등록된 그래픽 레이케스터 enabled를 일괄 변경
  /// </summary>
  public Action<bool> onChangeBlockEventState;

  public void BlockEvent(params GraphicRaycaster[] raycasters)
  {
    BlockEvent(true, raycasters);
  }

  public void BlockEvent(bool blockEventFromUser = false, params GraphicRaycaster[] raycasters)
  {
    // Debug.Log("BlockEvent");

    if (IsBlockEvent) // 이벤트 블락 상태일 때
    {
      // 유저가 잠금하지 않은 상태에서 도중에 유저가 잠금 했을 때
      if (!BlockEventFromUser && blockEventFromUser == true)
      {
        BlockEventFromUser = true; // 유저가 잠금 했음 상태로 변경
      }
      return;
    }

    IsBlockEvent = true; // 이벤트 블락 처리함.
    BlockEventFromUser = blockEventFromUser; // 유저가 잠금을 했는지 유무

    // 이벤트 잠금
    TouchManager.DisableTouchEvent();
    foreach (var kv in dtCanvasGraphicRaycaster)
    {
      kv.Value.enabled = false;
    }

    if (raycasters != null)
    {
      foreach (var raycaster in raycasters)
      {
        raycaster.enabled = false;
      }
    }

    onChangeBlockEventState?.Invoke(false); // lds - 23.3.16, 등록된 그래픽 레이케스터 enabled를 false로 일괄 변경
  }

  public void UnblockEvent(params GraphicRaycaster[] raycasters)
  {
    UnblockEvent(true, raycasters);
  }
  public void UnblockEvent(bool unBlockEventFromUser = false, params GraphicRaycaster[] raycasters)
  {
    // Debug.Log("UnblockEvent");

    if (!IsBlockEvent) // 이벤트 블락 상태가 아닐 땐 잠금 해제 하지 않음
      return;

    // 유저가 잠금 시킨 상태에서 도중에 시스템이 잠금 해제를 한 경우 잠금 해제 하지 않음
    if(BlockEventFromUser && !unBlockEventFromUser)
      return;

    IsBlockEvent = false;
    BlockEventFromUser = false;

    // 이벤트 잠금 해제
    TouchManager.EnableTouchEvent();
    foreach (var kv in dtCanvasGraphicRaycaster)
    {
      kv.Value.enabled = true;
    }

    if (raycasters != null)
    {
      foreach (var raycaster in raycasters)
      {
        raycaster.enabled = true;
      }
    }  

    onChangeBlockEventState?.Invoke(true); // lds - 23.3.16, 등록된 그래픽 레이케스터 enabled를 true로 일괄 변경
  }

  public void ShowLoading(params GraphicRaycaster[] raycasters)
  {
    ShowLoading(true, loopIndicator: true, raycasters: raycasters); // 기본 인디케이터는 루프 true
  }

  /// <summary>
  /// loopIndicator가 true일 때만 반드시 HideLoading()을 호출한다.
  /// </summary>
  /// <param name="loadingFromUser">직접 호출할 경우 true 지정</param>
  /// <param name="type">인디케이터 아이콘 종류</param>
  /// <param name="textProgress">인디케이터 프로그레스 내용</param>
  /// <param name="onCompleted">인디케이터 프로그레스 완료 시 콜벡</param>
  /// <param name="loopIndicator">루프 유무, true로 지정할 경우 반드시 HideLoading() 호출 해야함.</param>
  /// <param name="raycasters">UI 캔버스외에 직접 블락 처리해야될 레이케스터 리스트</param>
  /// <param name="dimType">PopupIndicator의 딤드 종류</param>
  public void ShowLoading(bool loadingFromUser = false
  , PopupIndicator.EIndicatorType type = PopupIndicator.EIndicatorType.Common
  , string textProgress = "50140" // 50140: 로딩중
  , Action onCompleted = null // 로딩 닫힐 때 콜벡
  , bool loopIndicator = false // 인디케이터 루프 여부
  , EUIDimType dimType = EUIDimType.CLEAR // 딤드 종류
  , params GraphicRaycaster[] raycasters)
  {
    if(!LoadingFromUser && loadingFromUser) // 강제 로딩 상태가 아니고, 강제 로딩 상태로 파라미터가 들어오면
      LoadingFromUser = true; // 강제 로딩 상태로 변경.

    if (!IsLoading) // 로딩 상태가 아니라면
    {
      IsLoading = true; // 로딩 상태로 변경

      // 로딩 팝업 호출
      var attribute = GetPopupUIAttribute<PopupIndicator>().Clone();
      attribute.DimType = dimType;
      var indicator = ShowPopup(attribute) as PopupIndicator;
      if (indicator != null) indicator.SetIndicator(type, textProgress, onCompleted, loopIndicator);
    }

    BlockEvent(loadingFromUser, raycasters); // BlockEvent 갱신
  }

  public void HideLoading(params GraphicRaycaster[] raycasters)
  {
    HideLoading(true, raycasters);
  }
  public void HideLoading(bool hideLoadingFromUser = false, params GraphicRaycaster[] raycasters)
  {
    if (IsLoading) // 로딩 상태라면
    {
      if(LoadingFromUser == hideLoadingFromUser) // 강제 로딩 상태와, 강제 로딩 숨기기 상태가 같을 때만 로딩 인디케이터 팝업 닫음.
      {
        IsLoading = false; // 로딩 상태 해제
        LoadingFromUser = false; // 강제 로딩 상태 초기화.
        HidePopup<PopupIndicator>(); // 로딩 팝업 숨김
      }
    }
    UnblockEvent(hideLoadingFromUser, raycasters); // UnblockEvent 갱신
  }

  public void UpdateAll(object param = null)
  {
    CurrUI.UpdateUI(param);

    foreach (var kv in dtCanvasPopup)
    {
      if(kv.Value == null || kv.Value.Count == 0)
        continue;

      foreach (var popup in kv.Value)
      {
        popup.UpdateUI(param);
      }
    }

    foreach (var kv in dtUIPool)
    {
      kv.Value.UpdateUI(param);
    }

    foreach (var kv in dtPopupPool)
    {
      kv.Value.UpdateUI(param);
    }

    foreach(var kv in activeHUDListByLayers)
    {
      foreach(var kv2 in kv.Value)
      {
        kv2.UpdateHUD(param);
      }
    }
  }

  public void RequireUpdateUI<T>() where T: UIBehaviour
  {
    var type = GetUIAttribute<T>().Type;
    if (CurrUI != null && CurrUI.Attribute.Type == type)
    {
      CurrUI.RequireUpdate = true;
      return;
    }

    foreach (var kv in dtUIPool)
    {
      if(kv.Value.Attribute.Type != type)
        continue;

      kv.Value.RequireUpdate = true;
      return;
    }
  }

  public void RequireUpdatePopup<T>() where T: PopupUIBehaviour
  {
    var attribute = GetPopupUIAttribute<T>();
    var type = attribute.Type;
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      var behaviour = dtCanvasPopup[(UICanvasTypes)i].Find(item => item.Attribute.Type == type);
      if (behaviour == null)
        continue;

      behaviour.RequireUpdate = true;
      return;
    }

    foreach (var kv in dtPopupPool)
    {
      if(kv.Value.Attribute.Type != type)
        continue;

      kv.Value.RequireUpdate = true;
      return;
    }
  }
  public void RequireUpdateAll()
  {
    CurrUI.RequireUpdate = true;

    foreach (var kv in dtCanvasPopup)
    {
      if(kv.Value == null || kv.Value.Count == 0)
        continue;

      foreach (var popup in kv.Value)
      {
        popup.RequireUpdate = true;
      }
    }

    foreach (var kv in dtUIPool)
    {
      kv.Value.RequireUpdate = true;
    }

    foreach (var kv in dtPopupPool)
    {
      kv.Value.RequireUpdate = true;
    }

    foreach(var kv in activeHUDListByLayers)
    {
      foreach(var kv2 in kv.Value)
      {
        kv2.RequireUpdate = true;
      }
    }
  }

  #region TouchEffect

  private GameObject touchEffect;
  private IObjectPool<DOTweenSequence> touchEffectPool;

  private DOTweenSequence CreateTouchEffect()
  {
    var go = Instantiate(touchEffect);
    var effect = go.GetComponent<DOTweenSequence>();
    go.SetParent(GetCanvasObject(UICanvasTypes.TouchEffect));
    return effect;
  }

  private void OnGetTouchEffect(DOTweenSequence particle)
  {
    particle.SetActive(true);
    particle.OnComplete(() => { touchEffectPool.Release(particle); });
    particle.Play();
  }

  private void OnReleaseTouchEffect(DOTweenSequence particle)
  {
    particle.Stop();
    particle.SetActive(false);
  }

  private void OnDestroyTouchEffect(DOTweenSequence particle)
  {
    GameManager.Instance.ScheduleForDestruction(particle.gameObject);
  }

  /// <summary>
  /// lds - 로컬 에셋 로케이터맵이 만들어진 후 리소스를 로드하도록 변경
  /// </summary>
  public void LoadTouchEffect()
  {
    touchEffect = ResourceManager.Instance.Load<GameObject>("UI/Effect/TouchEffect");
    touchEffectPool = new ObjectPool<DOTweenSequence>(CreateTouchEffect, OnGetTouchEffect, OnReleaseTouchEffect, OnDestroyTouchEffect, maxSize: 3);
  }

  #endregion

  #endregion


  #region ToastMessage
  private ToastMessage toastMessage;

  private void CreateToastMessage()
  {
    var prefabPath = "UI/Toast/ToastMessage";
    var go = ResourceManager.Instance.Load<GameObject>(prefabPath);
    if (null == go)
      return;

    go = Instantiate(go);
    if (null == go)
      return;

    toastMessage = go.GetComponent<ToastMessage>();
    go.SetParent(GetCanvasObject(UICanvasTypes.Focus));
  }

  public void ShowToastMessage(int localizeID, SoundFxTypes eSound = SoundFxTypes.NONE)
  {
    ShowToastMessage(localizeID.ToString(), eSound);
  }

  /// <summary>
  /// 토스트 호출 함수
  /// </summary>
  /// <param name="text"></param>
  /// <param name="bMapHold"></param>
  /// <param name="strSound">토스트 발생 시 사운드 발생(null 일 경우 메시지 없음)</param>
  public void ShowToastMessage(string text, SoundFxTypes eSound = SoundFxTypes.NONE)
  {
    if(toastMessage == null)
    {
      CreateToastMessage();
    }

    if (toastMessage != null) toastMessage.Show(text);
  }
  #endregion

  #region Popup UI
  const float OVERLAP_INTERVAL = 1.5f;

  private Dictionary<Type, PopupUIAttribute> popupUIAttributes = new();
  private Dictionary<UICanvasTypes, List<PopupUIBehaviour>> dtPreparePopup = new();
  private Dictionary<UICanvasTypes, List<PopupUIBehaviour>> dtCanvasPopup = new();

  /// <summary>
  /// UIObject Pool
  /// </summary>
  Dictionary<string, PopupUIBehaviour> dtPopupPool = new();
  Dictionary<string, bool> dtOverlapUIBlock = new();

  private Dictionary<EUIDimType, Dim> dtDim = new();

  public PopupUIBehaviour OpenPopup
  {
    get
    {
      if (null == dtCanvasPopup || dtCanvasPopup.Count == 0)
        return null;

      PopupUIBehaviour uiBehaviour = null;
      List<PopupUIBehaviour> ltParentUI = null;
      for (var i = 0; i < (int)UICanvasTypes.Max; i++)
      {
        ltParentUI = dtCanvasPopup[(UICanvasTypes)i];
        if (ltParentUI.Count == 0)
          continue;

        uiBehaviour = ltParentUI[ltParentUI.Count - 1];
        if (null != uiBehaviour)
          break;
      }

      return uiBehaviour;
    }
  }

  #region Dim

  private void UpdateDim()
  {
    // 갱신
    // 캔버스별로 딤 팝업 체크
    List<EUIDimType> checkedDimTypes = new();
    for (var i = 0; i < (int) UICanvasTypes.Max; i++)
    {
      // 앞에서부터 체크한다.
      var behaviours = dtCanvasPopup[(UICanvasTypes) i];
      var useDimBehaviours = behaviours.FindAll(behaviour => behaviour.Attribute.UseDim);
      if(useDimBehaviours == null || useDimBehaviours.Count == 0)
        continue;

      foreach (var behaviour in useDimBehaviours)
      {
        // 타입별로 모든 딤 팝업이 사용되면 확인을 종료한다.
        if(checkedDimTypes.Contains(behaviour.Attribute.DimType))
          continue;

        ShowDim(behaviour);
        checkedDimTypes.Add(behaviour.Attribute.DimType);
      }
    }

    if (checkedDimTypes.Count == 0)
    {
      HideAllDim();
    }
    else
    {
      for (var i = 0; i < Enum<EUIDimType>.Count; i++)
      {
        var dimType = (EUIDimType) i;
        if(checkedDimTypes.Contains(dimType))
           continue;

        HideDim(dimType);
      }
    }
  }
  private void ShowDim(PopupUIBehaviour behaviour)
  {
    var dimType = behaviour.Attribute.DimType;
    if(dimType == EUIDimType.NONE) return;
    var canvas = GetCanvas(behaviour.Attribute.CanvasType);
    if (!dtDim.ContainsKey(dimType))
    {
      dtDim[dimType] = new Dim(dimType, canvas);
    }

    var dim = dtDim[dimType];
    if (dim.Count == 0)
    {
      // 페이드 연출 여부를 결정
      // 없던걸 생성하는거라 연출이 필요함.

      if (dimType != EUIDimType.CLEAR)
      {
        dim.Show();
      }
    }

    dim.SetParent(canvas);
    dim.SetAsFirstSibling();

    if (dim.Contains(behaviour))
      return;

    dim.Add(behaviour);
    var index = behaviour.rectTransform.GetSiblingIndex();
    dim.SetSiblingIndex(--index);
    dim.SetActive(true);
  }

  private void HideDim(EUIDimType dimType)
  {
    if (!dtDim.ContainsKey(dimType))
      return;

    dtDim[dimType].SetAsFirstSibling();
    dtDim[dimType].SetActive(false);
  }
  private void HideDim(PopupUIBehaviour behaviour)
  {
    var dimType = behaviour.Attribute.DimType;
    if (!dtDim.ContainsKey(dimType) || !dtDim[dimType].Contains(behaviour))
      return;

    var dim = dtDim[dimType];
    dim.Remove(behaviour);
    if (dim.Count == 0)
    {
      // 페이드 연출하고 오브젝트 꺼야됨.
      if (dimType != EUIDimType.CLEAR)
      {
        dim.Hide();
      }
      else
      {
        dim.SetActive(false);
      }
    }
    else
    {
      var rtPopup = dim[^1].rectTransform;
      if (rtPopup == null)
        return;

      dim.SetParent(rtPopup.parent);
      dim.SetAsFirstSibling();
      var index = rtPopup.GetSiblingIndex();
      dim.SetSiblingIndex(--index);
    }
  }
  private void HideAllDim()
  {
    // 노출중인 모든 Dim을 제거한다.
    foreach (var kv in dtDim)
    {
      kv.Value.SetAsFirstSibling();
      kv.Value.SetActive(false);
      kv.Value.SetTweenEnabled(false);
    }
  }

  #endregion

  public PopupUIAttribute GetPopupUIAttribute<T>() where T : PopupUIBehaviour
  {
    var type = typeof(T);

    return GetPopupUIAttribute(type);
  }
  public PopupUIAttribute GetPopupUIAttribute(Type type)
  {
    PopupUIAttribute uiAtrribute = null;
    if (popupUIAttributes.ContainsKey(type))
    {
      uiAtrribute = popupUIAttributes[type];
    }
    else
    {
      var attributes = (PopupUIAttribute[])type.GetCustomAttributes(typeof(PopupUIAttribute), false);
      if (null == attributes || 0 == attributes.Length)
        return null;

      uiAtrribute = attributes[0];
      popupUIAttributes.Add(type, uiAtrribute);
    }

    return uiAtrribute;
  }

  private IEnumerator OverlapUIInterval(PopupUIAttribute attribute)
  {
    dtOverlapUIBlock[attribute.UniqueID] = true;

    yield return new WaitForSeconds(OVERLAP_INTERVAL);

    dtOverlapUIBlock[attribute.UniqueID] = false;
  }

  private PopupUIBehaviour CreatePopup(PopupUIAttribute attribute)
  {
    if (string.IsNullOrEmpty(attribute.PrefabPath))
    {
      Debug.LogError($"Popup prefab path error! (Type : {attribute.Type})");
      return null;
    }

    var prefab = ResourceManager.Instance.Load<GameObject>($"UI/Popup/{attribute.PrefabPath}");
    if (null == prefab)
      return null;

    var go = Instantiate(prefab);
    if (null == go)
      return null;

    go.name = prefab.name;
    var uiBehaviour = go.GetComponent<PopupUIBehaviour>();

    return uiBehaviour;
  }

  private int CreatedPopupCount(Type type)
  {
    var count = 0;
    foreach (var kv in dtPopupPool)
    {
      if (kv.Value.GetType() == type)
        count++;
    }

    return count;
  }

  private bool ContainsPool(PopupUIBehaviour uiBehaviour)
  {
    if (dtPopupPool.Count == 0 || !dtPopupPool.ContainsKey(uiBehaviour.Attribute.UniqueID))
      return false;

    return true;
  }

  public PopupUIBehaviour PopPopupPool(PopupUIAttribute attribute)
  {
    PopupUIBehaviour uiBehaviour = null;

    // UniqueID 세팅
    var uniqueID = attribute.UniqueID;
    if (attribute.OverlapType == UIOverlapTypes.OVERLAP)
    {
      foreach (var kv in dtPopupPool)
      {
        if (kv.Value.GetType() == attribute.Type && !kv.Value.IsActiveSelf())
        {
          uiBehaviour = kv.Value;
          uniqueID = uiBehaviour.Attribute.UniqueID;
          break;
        }
      }
    }
    else if (dtPopupPool.Count > 0 && dtPopupPool.ContainsKey(uniqueID))
    {
      uiBehaviour = dtPopupPool[uniqueID];
      if (uiBehaviour.IsActiveSelf()) // 현재 노출중인 UI
        uiBehaviour = null;
    }

    if (uiBehaviour == null)
    {
      // 닫고 있는 팝업 중 사용 가능한 팝업이 있는지 체크한다.
      // 사용 가능한 팝업이 있는 경우 닫는 연출과 콜백을 제거 하고, 재활용하도록 한다.
      uiBehaviour = hidingPopups.Find(popup => popup.Attribute.Type == attribute.Type);
      if (uiBehaviour == null)
      {
        uiBehaviour = CreatePopup(attribute);
        if (uiBehaviour == null)
          return null;
      }
      else
      {
        hidingPopups.Remove(uiBehaviour);
      }

      if (attribute.OverlapType == UIOverlapTypes.OVERLAP && string.IsNullOrEmpty(attribute.OverlapID))
      {
        attribute.OverlapID = CreatedPopupCount(attribute.Type).ToString(); // 생성된 UI 수로 확인해야함
      }

      if (!dtPopupPool.ContainsKey(attribute.UniqueID))
      {
        dtPopupPool.Add(attribute.UniqueID, uiBehaviour);
      }

      dtCanvasPopup ??= new Dictionary<UICanvasTypes, List<PopupUIBehaviour>>();

      if (!dtCanvasPopup.ContainsKey(attribute.CanvasType))
        dtCanvasPopup.Add(attribute.CanvasType, new List<PopupUIBehaviour>());
    }

    uiBehaviour.Attribute = attribute;
    uiBehaviour.SetParentUICanvas(attribute.CanvasType);
    uiBehaviour.InitAnchor();
    uiBehaviour.SetActive(true);

    return uiBehaviour;
  }
  private void PushPopupPool(PopupUIBehaviour uiBehaviour)
  {
    if (trUIPool == null || uiBehaviour.transform.parent == trUIPool)
    {
      return;
    }

    if (uiBehaviour == null)
    {
      var key = string.Empty;
      foreach (var kv in dtPopupPool)
      {
        if (kv.Value == uiBehaviour)
        {
          key = kv.Key;
          break;
        }
      }

      dtPopupPool.Remove(key);
    }

    var attribute = uiBehaviour.Attribute;
    if (attribute == null)
      return;

    hidingPopups.Remove(uiBehaviour); // 연출이 완료된 팝업은 제거한다.

    uiBehaviour.transform.SetParent(trUIPool);

    if (uiBehaviour.enabled)
      uiBehaviour.SetActive(false);

    var uniqueID = attribute.UniqueID;
    if (attribute.OverlapType == UIOverlapTypes.OVERLAP && string.IsNullOrEmpty(attribute.OverlapID))
      uniqueID = $"{attribute.UniqueID}_{attribute.OverlapID}";

    if (string.IsNullOrEmpty(uniqueID) || dtPopupPool == null)
    {
      return;
    }

    if (!dtPopupPool.ContainsKey(uniqueID))
    {
      dtPopupPool.Add(uniqueID, uiBehaviour);
    }

    uiBehaviour.OnAfterPushToPool();
  }

  private void RegisterPopup(PopupUIBehaviour uiBehaviour)
  {
    if (uiBehaviour == null)
      return;

    var attribute = uiBehaviour.Attribute; // UI에 있는 Attribute 로 교체
    if (trUIPool == null)
      return;

    dtCanvasPopup[attribute.CanvasType].Add(uiBehaviour); // 딕셔너리에 팝업을 등록함.
    uiBehaviour.rectTransform.SetAsLastSibling();

    ShowDim(uiBehaviour);
    isShow = true;
  }
  private void UnregisterPopup(PopupUIBehaviour uiBehaviour, Action onHide)
  {
    var attribute = uiBehaviour.Attribute;
    var ltUI = dtCanvasPopup[attribute.CanvasType];
    var index = ltUI.FindIndex(ui => ui.Attribute.UniqueID == attribute.UniqueID);
    if (index == -1)
      return;

    ltUI.RemoveAt(index);

    // Dim 을 사용하는 UI 체크
    HideDim(uiBehaviour);
    //HDebug.Log("PopUI : " + attribute.UniqueID);

    if (onHide == null)
    {
      onHide = () => { PushPopupPool(uiBehaviour); };
    }
    else
    {
      onHide += () => { PushPopupPool(uiBehaviour); };
    }

    if(!hidingPopups.Contains(uiBehaviour))
    {
      hidingPopups.Add(uiBehaviour); // 팝업 종료 연출이 진행중인 팝업을 애드한다.
    }

    uiBehaviour.Hide(onHide);
    isHide = true;
  }

  public PopupUIBehaviour GetOpenPopup(Type type)
  {
    if (!IsOpenPopup)
      return null;

    PopupUIBehaviour uiBehaviour = null;
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      uiBehaviour = dtCanvasPopup[(UICanvasTypes)i].Find(item => item.Attribute.Type == type);
      if (null != uiBehaviour)
        break;
    }

    return uiBehaviour;
  }
  public T GetOpenPopup<T>() where T : PopupUIBehaviour
  {
    return GetOpenPopup(typeof(T)) as T;
  }

  /// <summary>
  /// TODO : 개수가 올바르게 안되면 다시 확인 필요.
  /// </summary>
  /// <returns></returns>
  public int GetOpenedPopupCount()
  {
    if (!IsOpenPopup)
      return 0;

    int result = 0;
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      result += dtCanvasPopup[(UICanvasTypes)i].Count;
    }

    return result;
  }

  public T GetUniquePopup<T>() where T : PopupUIBehaviour
  {
    if (!IsOpenPopup)
      return null;

    var attribute = GetPopupUIAttribute<T>();
    return (T)GetUniquePopup(attribute.UniqueID);
  }
  public PopupUIBehaviour GetUniquePopup(string uniqueID)
  {
    if (!IsOpenPopup)
      return null;

    PopupUIBehaviour uiBehaviour = null;
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      uiBehaviour = dtCanvasPopup[(UICanvasTypes)i].Find(item => item.Attribute.UniqueID == uniqueID);
      if (null != uiBehaviour)
        break;
    }

    return uiBehaviour;
  }

  public T GetPreparePopup<T>() where T : PopupUIBehaviour
  {
    if (!IsOpenPopup)
      return null;

    var attribute = GetPopupUIAttribute<T>();
    return (T)GetPreparePopup(attribute.UniqueID);
  }
  public PopupUIBehaviour GetPreparePopup(string uniqueID)
  {
    if (!IsOpenPopup)
      return null;

    PopupUIBehaviour uiBehaviour = null;
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      uiBehaviour = dtPreparePopup[(UICanvasTypes)i].Find(item => item.Attribute.UniqueID == uniqueID);
      if (null != uiBehaviour)
        break;
    }

    return uiBehaviour;
  }

  public bool CheckOpenPopup(Type type)
  {
    if (!IsOpenPopup)
      return false;

    return null != GetOpenPopup(type);
  }
  /// <summary>
  /// 특정 UI가 열려있는지 확인하는 함수
  ///  - UIBehaviour 클래스를 상속받은 UI만 확인가능.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public bool CheckOpenPopup<T>() where T : PopupUIBehaviour => CheckOpenPopup(typeof(T));

  public bool CheckOpenPopup(PopupUIBehaviour uiBehaviour)
  {
    if (!IsOpenPopup)
      return false;

    var bContain = false;
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      bContain = dtCanvasPopup[(UICanvasTypes)i].Contains(uiBehaviour);
      if (bContain)
        break;
    }

    return bContain;
  }
  public bool CheckPreparePopup<T>() where T : PopupUIBehaviour
  {
    return null != GetPreparePopup<T>();
  }
  public bool CheckPreparePopup(PopupUIBehaviour uiBehaviour)
  {
    var bContain = false;
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      bContain = dtPreparePopup[(UICanvasTypes)i].Contains(uiBehaviour);
      if (bContain)
        break;
    }

    return bContain;
  }

  public bool CheckHidingPopup<T>() where T : PopupUIBehaviour
  {
    var type = typeof(T);
    return hidingPopups.FindIndex(popup => popup.Attribute.Type == type) >= 0;
  }

  public PopupUIBehaviour PreparePopup(PopupUIAttribute attribute, object obj = null, UIBehaviour.OnPrepared onPrepared = null)
  {
    if (attribute == null)
      return null;

    // 커스텀 어트리뷰트는 일회성으로 사용 후 제거되어한다.
    if (string.IsNullOrEmpty(attribute.UniqueID))
      return null;

    if (dtPreparePopup[attribute.CanvasType].Find(item => item.Attribute.UniqueID == attribute.UniqueID) != null)
      return null;

    PopupUIBehaviour uiBehaviour = null;
    if (attribute.OverlapType == UIOverlapTypes.OVERLAP && attribute.OverlapTime > 0)
    {
      if (dtOverlapUIBlock.ContainsKey(attribute.UniqueID))
      {
        if (dtOverlapUIBlock[attribute.UniqueID])
        {
          return null;
        }
      }

      uiBehaviour = PopPopupPool(attribute);
      dtPreparePopup[attribute.CanvasType].Add(uiBehaviour);
      uiBehaviour.Prepare(obj);
      uiBehaviour.preparedCallback = onPrepared; // lds - 23.2.15, prepared 마지막에 호출시킬 콜벡
      Instance.StartCoroutine(OverlapUIInterval(attribute));
    }
    else if (CheckOpenPopup(attribute.Type))
    {
      switch (attribute.OverlapType)
      {
        case UIOverlapTypes.UNIQUE: // 하나만 존재하기 떄문에 리턴처리.
          return default;

        case UIOverlapTypes.UNIQUE_UPDATE: // 현재 팝업이 열린 채로 업데이트만 해준다.
          uiBehaviour = GetUniquePopup(attribute.UniqueID);
          if (null != uiBehaviour)
          {
            // 이미 팝업이 노출되어 있는 경우는 ?
            uiBehaviour.Show(obj, true);
            isShow = true;
          }
          else
          {
            return default;
          }
          break;
      }
    }
    else
    {
      uiBehaviour = PopPopupPool(attribute);
      dtPreparePopup[attribute.CanvasType].Add(uiBehaviour);
      uiBehaviour.Prepare(obj);
      uiBehaviour.preparedCallback = onPrepared; // lds - 23.2.15, prepared 마지막에 호출시킬 콜벡
    }

    BlockEvent();
    PreparePopupCancelTimer();

    return uiBehaviour;

    async void PreparePopupCancelTimer()
    {
      try
      {
        // 각 Popup에서 설정한 값 만큼 대기, 기본 3초
        // 대기 테스크는 uiBehaviour의 Prepared 가 호출 되면서 강제 테스크 취소.
        if (uiBehaviour == null)
          return;

        await Task.Delay(uiBehaviour.PreparingCancelTime, uiBehaviour.PrepareCancelTokenSource.Token);
        // uiBehaviour의 Prepare 단계에서 3초를 넘어가면 강제 취소 처리.
        if (dtPreparePopup[uiBehaviour.Attribute.CanvasType].Contains(uiBehaviour))
          PreparePopupCancel(uiBehaviour.Attribute.Type);
      }
      catch (Exception ex)
      {
        // try문에서 지연 테스크를 강제 취소 시 catch문으로 들어옴.
        // 의도적인 익셉션이며, 버그 및 에러가 아님.
        return;
      }
    }
  }
  public PopupUIBehaviour PreparePopup(Type type, object obj = null, UIBehaviour.OnPrepared onPrepared = null)
  {
    // 기본 어트리뷰트 정보로 세팅한다.
    var attribute = GetPopupUIAttribute(type);
    return PreparePopup(attribute, obj, onPrepared);
  }

  public T PreparePopup<T>(object obj = null, UIBehaviour.OnPrepared onPrepared = null) where T : PopupUIBehaviour
  {
    return PreparePopup(typeof(T), obj, onPrepared) as T;
  }

  public PopupUIBehaviour PreparedPopup<T>(object obj = null, bool bUpdateUI = false, Action onComplete = null) where T : PopupUIBehaviour
  {
    return PreparedPopup(typeof(T), obj, bUpdateUI, onComplete);
  }
  public PopupUIBehaviour PreparedPopup(Type type, object obj = null, bool bUpdateUI = false, Action onComplete = null)
  {
    var attribute = GetPopupUIAttribute(type);
    return PreparedPopup(attribute, obj, bUpdateUI, onComplete);
  }
  public PopupUIBehaviour PreparedPopup(PopupUIAttribute attribute, object obj = null, bool bUpdateUI = false, Action onComplete = null)
  {
    if (attribute == null)
    {
      UnblockEvent();
      return null;
    }

    if (!dtPreparePopup.ContainsKey(attribute.CanvasType) || dtPreparePopup[attribute.CanvasType].Count == 0)
    {
      UnblockEvent();
      return null;
    }

    var uiBehaviour = dtPreparePopup[attribute.CanvasType].Find(item => item.Attribute.UniqueID == attribute.UniqueID);
    if (uiBehaviour == null)
    {
      UnblockEvent();
      return null;
    }

    dtPreparePopup[attribute.CanvasType].Remove(uiBehaviour);
    RegisterPopup(uiBehaviour);
    uiBehaviour.Prepared();

    if (onComplete == null)
    {
      onComplete = () => { UnblockEvent(); };
    }
    else
    {
      onComplete += () => { UnblockEvent(); };
    }

    uiBehaviour.Show(obj, bUpdateUI, onComplete);

    return uiBehaviour;
  }

  public void PreparePopupCancel(Type type)
  {
    UnblockEvent();

    var attribute = GetPopupUIAttribute(type);
    if(!dtPreparePopup.ContainsKey(attribute.CanvasType) || dtPreparePopup[attribute.CanvasType].Count == 0)
      return;

    var uiBehaviour = dtPreparePopup[attribute.CanvasType].Find(item => item.Attribute.UniqueID == attribute.UniqueID);
    if (uiBehaviour == null)
      return;

    uiBehaviour.preparedCallback?.Invoke(false); // lds - 23.2.15, 준비가 실패되면 콜벡 호출
    uiBehaviour.preparedCallback = null;
    dtPreparePopup[attribute.CanvasType].Remove(uiBehaviour);
    PushPopupPool(uiBehaviour);
  }

  public void PreparePopupCancel<T>() where T : PopupUIBehaviour
  {
    PreparePopupCancel(typeof(T));
  }

  public PopupUIBehaviour ShowPopup(PopupUIAttribute attribute, object obj = null, bool bUpdateUI = false, Action onComplete = null)
  {
    if (attribute == null)
      return null;

    var popup = PreparePopup(attribute, obj);
    PreparedPopup(attribute, obj, bUpdateUI, onComplete);
    return popup;
  }
  public PopupUIBehaviour ShowPopup(Type type, object obj = null, bool bUpdateUI = false, Action onComplete = null)
  {
    var attribute = GetPopupUIAttribute(type);
    if (null == attribute)
      return null;

    var popup = PreparePopup(type, obj);
    PreparedPopup(type, obj, bUpdateUI, onComplete);
    return popup;
  }
  public T ShowPopup<T>(object obj = null, bool bUpdateUI = false, Action onComplete = null) where T : PopupUIBehaviour
  {
    // 기본 어트리뷰트 정보로 세팅한다.
    var attribute = GetPopupUIAttribute<T>();
    if (null == attribute)
      return null;

    var popup = PreparePopup<T>(obj);
    PreparedPopup<T>(obj, bUpdateUI, onComplete);
    return popup;
  }

  public void HidePopup<T>(Action complete = null) where T : PopupUIBehaviour
  {
    var popup = GetOpenPopup<T>();
    if (popup == null)
      return;

    UnregisterPopup(popup, complete);
  }
  public void HidePopup(PopupUIBehaviour uiBehaviour, Action complete = null)
  {
    if (null == uiBehaviour || !CheckOpenPopup(uiBehaviour))
      return;

    UnregisterPopup(uiBehaviour, complete);
  }

  public void HideAllPopup()
  {
    if (!IsOpenPopup)
      return;

    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      var ltUI = dtCanvasPopup[(UICanvasTypes)i];
      for (var j = ltUI.Count - 1; j >= 0; j--)
      {
        ltUI[j].Hide();
      }

      ltUI.Clear();
    }

    HideAllDim();
  }
  public void HideAllPopup(Action complete)
  {
    if (!IsOpenPopup)
      return;

    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      var ltUI = dtCanvasPopup[(UICanvasTypes)i];
      for (var j = ltUI.Count - 1; j >= 0; j--)
      {
        //마지막 액션이 종료 될때 콜백이 있으면 처리한다.
        if (i == (int)UICanvasTypes.Max - 1 && j == ltUI.Count - 1)
          ltUI[j].Hide(complete);
        else
          ltUI[j].Hide();
      }

      ltUI.Clear();
    }

    HideAllDim();
  }
  #endregion

  #region UI
  Dictionary<Type, UIAttribute> dtUIAttribute = new();
  Dictionary<string, UIBehaviour> dtUIPool = new();

  public Action<Type, Type> OnUIChangedEvent;

  public UIBehaviour PreparingUI { get; private set; }

  public UIBehaviour PrevUI
  {
    get
    {
      if (CurrUI == null)
        return null;

      return CurrUI.PrevUI;
    }
  }
  public UIBehaviour CurrUI { get; private set; }

  public bool CheckCurrUI<T>() where T : UIBehaviour
  {
    if(CurrUI == null)
    {
      return false;
    }
    else
    {
      return CurrUI.Attribute.Type == typeof(T);
    }
  }

  public UIAttribute GetUIAttribute<T>() where T : UIBehaviour
  {
    var type = typeof(T);

    return GetUIAttribute(type);
  }
  public UIAttribute GetUIAttribute(Type type)
  {
    UIAttribute uiAtrribute = null;
    if (dtUIAttribute.ContainsKey(type))
    {
      uiAtrribute = dtUIAttribute[type];
    }
    else
    {
      var attributes = (UIAttribute[])type.GetCustomAttributes(typeof(UIAttribute), false);
      if (null == attributes || 0 == attributes.Length)
        return null;

      uiAtrribute = attributes[0];
      dtUIAttribute.Add(type, uiAtrribute);
    }

    return uiAtrribute;
  }

  private UIBehaviour CreateUI(UIAttribute attribute)
  {
    var prefabPath = string.Format("UI/UI/{0}", attribute.PrefabPath);
    var prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
    if (null == prefab)
      return null;

    var go = Instantiate(prefab);
    if (null == go)
      return null;

    go.name = prefab.name;
    var uiBehaviour = go.GetComponent<UIBehaviour>();
    uiBehaviour.SetActive(true);

    return uiBehaviour;
  }
  private void CreateUI(GameObject go)
  {
    if (go == null)
      return;

    var goUI = Instantiate(go);
    var uiBehaviour = goUI.GetComponent<UIBehaviour>();
    UIAttribute attribute = null;
    try
    {
      attribute = uiBehaviour.Attribute;
    }
    catch
    {
      Debug.Log(goUI.name);
    }

    goUI.name = attribute.UniqueID;
    goUI.SetPosition(go.transform.localPosition);

    PushUIPool(uiBehaviour);
  }

  public UIBehaviour PopUIPool(UIAttribute attribute)
  {
    //Debug.LogFormat("Push : {0}", attribute.Type);
    UIBehaviour uiBehaviour = null;

    // UniqueID 세팅
    var uniqueID = attribute.UniqueID;
    if (dtUIPool.Count == 0 || !dtUIPool.ContainsKey(uniqueID))
      return null;

    uiBehaviour = dtUIPool[uniqueID];
    uiBehaviour.SetActive(true);

    return uiBehaviour;
  }
  public void PushUIPool(UIBehaviour uiBehaviour)
  {
    //Debug.LogFormat("Push : {0}", uiBehaviour.name);
    if (trUIPool == null || uiBehaviour == null || uiBehaviour.transform.parent == trUIPool)
    {
      return;
    }
    uiBehaviour.transform.SetParent(trUIPool);

    var attribute = uiBehaviour.Attribute;
    if (attribute == null)
    {
      return;
    }

    if (uiBehaviour.enabled)
      uiBehaviour.SetActive(false);

    var uniqueID = attribute.UniqueID;
    if (string.IsNullOrEmpty(uniqueID) || dtUIPool == null)
    {
      return;
    }

    if (!dtUIPool.ContainsKey(uniqueID))
    {
      dtUIPool.Add(uniqueID, uiBehaviour);
    }
  }
  /// <summary>
  /// 이전에 노출하던 UI를 노출한다.
  /// </summary>
  /// <param name="bUpdatePrevUI"></param>
  private void ShowPrevUI(bool bUpdatePrevUI)
  {
    if (PrevUI == null || CurrUI == null)
      return;

    // 현재 노출 중인 UI 를 Hide 시킨다.
    // 이전 UI를 노출시키는 경우 PrevUI를 저장하지 않는다.
    #if LOG
    Debug.Log($"PrevUI {CurrUI.PrevUI.name}");
    #endif

    HideAllPopup(); // lds 23.2.3, Show보다 늦게 호출되는 상황이 있으므로 위치 옮김.

    var prevUI = CurrUI;
    var currUI = prevUI.PrevUI;

    CurrUI = currUI;
    currUI.SetParentUICanvas(UICanvasTypes.Back);
    currUI.rectTransform.SetAsFirstSibling();
    // currUI.Prepared();

    prevUI.Hide(CompletePrevHide, false);

    isShow = true;

    // 닫힐땐 즉시 닫히도록 처리힌다.
    if(!currUI.Attribute.UseHUD)
    {
      SetActiveAllHUD(false);
    }

    void CompletePrevHide()
    {
      // 켜질떈 이전 유아이 닫히고 노출되도록 처리힌다.
      if (currUI.Attribute.UseHUD)
      {
        SetActiveAllHUD(true);
      }

      // Kiwoo 2022-06-30
      // ViewModelSafeAreaApply 에서 Camera.main 을 찾을 수 없는 현상이 생김
      // 카메라 세팅 처리를 미리함.
      SetActiveCameras(currUI.Attribute, out var lateSetting);
      OnUIChangedEvent?.Invoke(prevUI.Attribute.Type, currUI.Attribute.Type);
      currUI.SetActive(true);
      currUI.Show(bUpdate: bUpdatePrevUI, complete: lateSetting);
    }
  }

  public UIBehaviour PrepareUI(UIAttribute attribute, object obj = null, UIBehaviour.OnPrepared onPrepared = null)
  {
    if(PreparingUI != null)
      return null;

    // 기본 어트리뷰트 정보로 세팅한다.
    // 노출 중인 경우 무시한다.
    if (attribute == null || (CurrUI != null ? CurrUI.GetType() : null) == attribute.Type)
      return null;

    var uiBehaviour = PopUIPool(attribute);
    if (null == uiBehaviour)
    {
      //Debug.LogFormat("Create : {0}", attribute.Type);
      uiBehaviour = CreateUI(attribute);
    }

    uiBehaviour.Attribute = attribute;
    if (!dtUIPool.ContainsKey(attribute.UniqueID))
    {
      dtUIPool.Add(attribute.UniqueID, uiBehaviour);
    }

    PreparingUI = uiBehaviour;
    PreparingUI.SetParentUICanvas(UICanvasTypes.Back);
    PreparingUI.InitAnchor();
    PreparingUI.Prepare(obj); // lds - 누락된 사항으로 보여서 추가.
    PreparingUI.preparedCallback = onPrepared; // lds - 23.2.15, prepared 마지막에 호출시킬 콜벡

    BlockEvent();
    CancelTask();

    return PreparingUI;

    async void CancelTask()
    {
      try
      {
        // 각 UI에서 설정한 값 만큼 대기, 기본 3초
        // 대기 테스크는 uiBehaviour의 Prepared 가 호출 되면서 강제 테스크 취소.
        await Task.Delay(uiBehaviour.PreparingCancelTime, uiBehaviour.PrepareCancelTokenSource.Token);
        if (PreparingUI == uiBehaviour)
          PrepareUICancel();
      }
      catch (Exception ex)
      {
        // try문에서 지연 테스크를 강제 취소 시 catch문으로 들어옴.
        // 의도적인 익셉션, 버그, 에러 아님.
        return;
      }
    }
  }
  public UIBehaviour PrepareUI(Type type, object obj = null, UIBehaviour.OnPrepared onPrepared = null)
  {
    // 기본 어트리뷰트 정보로 세팅한다.
    var attribute = GetUIAttribute(type);
    if (attribute == null || attribute.Type != type)
      return null;

    return PrepareUI(attribute, obj, onPrepared);
  }

  public T PrepareUI<T>(object obj = null, UIBehaviour.OnPrepared preparedCallback = null) where T : UIBehaviour
  {
    return PrepareUI(typeof(T), obj, preparedCallback) as T;
  }

  public void PreparedUI<T>(object obj = null, bool bUpdateUI = false, Action onShow = null, Action onPrevHide = null) where T : UIBehaviour
  {
    if (PreparingUI != null && PreparingUI.Attribute.Type == typeof(T))
    {
      PreparedUI(obj, bUpdateUI, onShow, onPrevHide);
    }
  }
  private void PreparedUI(object obj = null, bool bUpdateUI = false, Action onShow = null, Action onPrevHide = null)
  {
    if (PreparingUI == null)
      return;

    HideAllPopup(); // lds 23.2.3, Show보다 늦게 호출되는 상황이 있으므로 위치 옮김.

    if (onShow == null)
    {
      onShow = () => { UnblockEvent(); };
    }
    else
    {
      onShow += () => { UnblockEvent(); };
    }

    var uiBehaviour = PreparingUI;
    Type prevType = null;
    if (CurrUI != null)
    {
      uiBehaviour.PrevUI = CurrUI;
      prevType = uiBehaviour.PrevUI.Attribute.Type;

      CurrUI = uiBehaviour;
      CurrUI.SetParentUICanvas(UICanvasTypes.Back);
      CurrUI.rectTransform.SetAsFirstSibling();
      CurrUI.Prepared();

      // 이전 UI는 false, 현재 UI는 true
      if (HasCameraSettingChanged(uiBehaviour.PrevUI.Attribute, uiBehaviour.Attribute))
      {
        // 위 경우 이전 UI 종료 애니메이션을 동작하는 동안 게임화면이 노출되기 떄문에 미리 카메라를 작동시켜줘야한다.
        if (uiBehaviour.PrevUI.UseHideAnimation)
        {
          SetActiveCameras(uiBehaviour.Attribute, out _);
        }
        // 애니메이션이 없는 경우 현재 UI가 노출될때 세팅되는 결과에 따라 처리되도록 한다.
      }

      uiBehaviour.PrevUI.Hide(CompletePrevHide);
    }
    else
    {
      CurrUI = uiBehaviour;
      CurrUI.SetParentUICanvas(UICanvasTypes.Back);
      CurrUI.rectTransform.SetAsFirstSibling();
      CurrUI.Prepared();

      CompletePrevHide();
    }

    PreparingUI = null;
    isShow = true;

    // 닫힐땐 즉시 닫히도록 처리힌다.
    if (!uiBehaviour.Attribute.UseHUD)
    {
      SetActiveAllHUD(false);
    }
    void CompletePrevHide()
    {
      // 켜질떈 이전 유아이 닫히고 노출되도록 처리힌다.
      if (uiBehaviour.Attribute.UseHUD)
      {
        SetActiveAllHUD(true);
      }

      // Kiwoo 2022-06-30
      // ViewModelSafeAreaApply 에서 Camera.main 을 찾을 수 없는 현상이 생김
      // 카메라 세팅 처리를 미리함.
      if (SetActiveCameras(uiBehaviour.Attribute, out var lateSetting))
      {
        onShow += lateSetting;
      }
      onShow += () => { UnblockEvent(); };
      onPrevHide?.Invoke();
      OnUIChangedEvent?.Invoke(prevType, CurrUI.Attribute.Type);
      CurrUI.Show(obj, bUpdateUI, onShow);
    }


    bool HasCameraSettingChanged(UIAttribute prevAttribute, UIAttribute currentAttribute)
    {
      return !prevAttribute.UseGameCamera && currentAttribute.UseGameCamera;
    }
}

  public void PrepareUICancel()
  {
    UnblockEvent();

    if (PreparingUI == null)
      return;

    PreparingUI.preparedCallback?.Invoke(false); // lds - 23.2.15, 준비가 실패되면 콜벡 호출
    PreparingUI.preparedCallback = null;
    PushUIPool(PreparingUI);
    PreparingUI = null;
  }
  public void PrepareUICancel<T>() where T : UIBehaviour
  {
    if (PreparingUI == null || PreparingUI.Attribute.Type != typeof(T))
    {
      UnblockEvent();
      return;
    }

    PrepareUICancel();
  }

  public UIBehaviour ShowUI(UIAttribute attribute, object obj = null, bool bUpdateUI = false, Action onShow = null, Action onPrevHide = null)
  {
    if (attribute == null)
      return null;

    var ui = PrepareUI(attribute, obj);
    PreparedUI(obj, bUpdateUI, onShow, onPrevHide);
    return ui;
  }

  public UIBehaviour ShowUI(Type type, object obj = null, bool bUpdateUI = false, Action onShow = null, Action onPrevHide = null)
  {
    var attribute = GetUIAttribute(type);
    if (null == attribute || attribute.Type != type)
      return null;

    return ShowUI(attribute, obj, bUpdateUI, onShow, onPrevHide);;
  }
  public T ShowUI<T>(object obj = null, bool bUpdateUI = false, Action onShow = null, Action onPrevHide = null) where T : UIBehaviour
  {
    // 기본 어트리뷰트 정보로 세팅한다.
    var attribute = GetUIAttribute<T>();
    if (null == attribute || attribute.Type != typeof(T))
      return null;

    return ShowUI(attribute, obj, bUpdateUI, onShow, onPrevHide) as T;
  }

  public void HideUI(bool showPrevUI = false, bool bUpdatePrevUI = false, Action complete = null)
  {
    // 노출 중인 UI 가 존재하지 않으면 무시한다.
    if (null == CurrUI)
      return;

    if (showPrevUI)
    {
      ShowPrevUI(bUpdatePrevUI);
    }
    else if (CurrUI != null && CurrUI.PrevUI != null)
    {
      var currUI = CurrUI;
      complete += () => { CurrUI = null; };
      currUI.Hide(complete, false);
    }
  }
  /// <summary>
  /// 현재 열려있는 UI 가 Hide 된다.
  /// <para> - Generic 타입으로 전달된 UI 가 열린다.</para>
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="callback"></param>
  public void HideUI<T>(bool showPrevUI = true, bool bUpdatePrevUI = false, Action complete = null) where T : UIBehaviour
  {
    // 노출 중인 UI 가 존재하지 않으면 무시한다.
    if (null == CurrUI || CurrUI.GetType() != typeof(T))
      return;

    if (showPrevUI)
    {
      ShowPrevUI(bUpdatePrevUI);
    }
    else
    {
      var currUI = CurrUI;
      complete += () => { CurrUI = null; };
      currUI.Hide(complete, false);
    }
  }


  #endregion

  #region HUD

  // int -> target InstanceID
  // private Dictionary<int, HUDGroup> hudGroups = new();

  private const string HUD_ROOT_PATH = "UI/HUD/";
  private const int HUD_POOLING_MAX_SIZE = 50;

  private Dictionary<Type, HUDAttribute> hudAttributes = new();
  private Dictionary<int, List<HUDBehaviour>> activeHUDListByLayers = new();

  private Dictionary<string, Queue<HUDBehaviour>> hudPools = new();

  /// <summary>
  /// 노출중이지만, 꺼놓은 HUD
  /// </summary>
  private Dictionary<Type, List<BaseObject>> pauseHUDTargets = new();

  private Dictionary<int, RectTransform> hudSortingLayers = new();

  public bool IsActiveHUD { get; private set; } = true;

  // 특정 유아이만 노출하도록 설정
  private List<Type> hudExposeTypes = new();

  private Dictionary<int, HUDAttribute> waitCreateHUD = new();
  private Dictionary<int, HUDBehaviour> createdHUD = new();

  private bool waitHUDCreate = true;

  private async Task HUDCreatingLoop()
  {
    // HUD Create Loop
    if (waitCreateHUD.Count == 0 || waitHUDCreate)
      return;

    waitHUDCreate = true;
    var keysSnapshot = new List<int>(waitCreateHUD.Keys);
    keysSnapshot.Reverse();

    int processedCount = 0;
    foreach (var key in keysSnapshot)
    {
      if (!waitCreateHUD.TryGetValue(key, out var attribute))
        continue;

      var hud = CreateHUD(attribute);
      if (hud != null)
      {
        createdHUD.Add(key, hud);
      }

      waitCreateHUD.Remove(key);
      processedCount++;

      if (processedCount % 3 == 0)
      {
        await Task.Yield();
      }
    }

    waitHUDCreate = false;
  }

  public HUDBehaviour CreateHUD(HUDAttribute attribute)
  {
    var prefabPath = $"{HUD_ROOT_PATH}{attribute.PrefabPath}";
    var goPrefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
    if (null == goPrefab)
      return null;

    var goUI = Instantiate(goPrefab);
    if (null == goUI)
      return null;

    goUI.SetActive(false);
    var uiBehaviour = goUI.GetComponent<HUDBehaviour>();

#if UNITY_EDITOR
    goUI.name = goPrefab.name;
#endif

    return uiBehaviour;
  }

  private void AddActiveHUD(HUDBehaviour hudBehaviour)
  {
    var sortingLayer = hudBehaviour.Attribute.SortingLayer;
    activeHUDListByLayers[sortingLayer].Add(hudBehaviour);
  }

  private void RemoveActiveHUD(HUDBehaviour hudBehaviour)
  {
    var sortingLayer = hudBehaviour.Attribute.SortingLayer;
    if (activeHUDListByLayers.ContainsKey(sortingLayer))
    {
      activeHUDListByLayers[sortingLayer].Remove(hudBehaviour);
    }
  }

  private bool ContainsActiveHUD(HUDBehaviour hudBehaviour)
  {
    var sortingLayer = hudBehaviour.Attribute.SortingLayer;
    return activeHUDListByLayers.ContainsKey(sortingLayer) && activeHUDListByLayers[sortingLayer].Contains(hudBehaviour);
  }

  private bool AddPauseHUD(HUDBehaviour hud)
  {
    if (!pauseHUDTargets.ContainsKey(hud.Attribute.Type))
    {
      pauseHUDTargets.Add(hud.Attribute.Type, new List<BaseObject>());
    }

    if (pauseHUDTargets[hud.Attribute.Type].Contains(hud.Target)) return false;

    pauseHUDTargets[hud.Attribute.Type].Add(hud.Target);
    // 비노출 상태로 전환할때, hud 를 Hide 하고 해당 hud 의 target 을 캐싱하고 있다가, 다시 노출해야하는 시점에 target 의 UpdateHUD 를 호출?
    if (ContainsActiveHUD(hud))
    {
      hud.Hide();
    }
    RemoveActiveHUD(hud);

    return true;
  }

  public bool AddPauseHUD(BaseObject target)
  {
    if (target.HUDBehaviour == null)
      return false;

    var type = target.HUDBehaviour.Attribute.Type;
    if (!pauseHUDTargets.ContainsKey(type))
    {
      pauseHUDTargets.Add(type, new List<BaseObject>());
    }

    if (pauseHUDTargets[type].Contains(target)) return false;

    pauseHUDTargets[type].Add(target);
    // 비노출 상태로 전환할때, hud 를 Hide 하고 해당 hud 의 target 을 캐싱하고 있다가, 다시 노출해야하는 시점에 target 의 UpdateHUD 를 호출?
    if (ContainsActiveHUD(target.HUDBehaviour))
    {
      target.HUDBehaviour.Hide();
    }
    RemoveActiveHUD(target.HUDBehaviour);

    return true;
  }

  public bool AddPauseHUD<T>(BaseObject target) where T : HUDBehaviour
  {
    var type = typeof(T);
    if (!pauseHUDTargets.ContainsKey(type))
    {
      pauseHUDTargets.Add(type, new List<BaseObject>());
    }

    if (pauseHUDTargets[type].Contains(target))
      return false;

    pauseHUDTargets[type].Add(target);
    return true;
  }

  public bool RemovePauseHUD<T>(BaseObject target) where T : HUDBehaviour
  {
    return RemovePauseHUD(typeof(T), target);
  }

  public bool RemovePauseHUD(Type type, BaseObject target)
  {
    if (!pauseHUDTargets.ContainsKey(type) || !pauseHUDTargets[type].Contains(target))
      return false;

    pauseHUDTargets[type].Remove(target);
    return true;
  }

  public bool ContainsPauseHUD(BaseObject target, out Type type)
  {
    type = null;
    foreach (var kv in pauseHUDTargets)
    {
      if(kv.Value.Contains(target))
      {
        type = kv.Key;
        return true;
      }
    }

    return false;
  }

  private bool ContainsPauseHUD(HUDBehaviour hud)
  {
    if (!pauseHUDTargets.ContainsKey(hud.Attribute.Type))
      return false;

    return pauseHUDTargets[hud.Attribute.Type].Contains(hud.Target);
  }

  public void InitializeHUD()
  {
    // kw 24.12.18
    // - 현재 HUDBehaviour 의 Initialize 함수가 비어있는 관계로 주석처리함.
    // foreach (var kv in activeHUDListByLayers)
    // {
    //   foreach(var kv2 in kv.Value)
    //   {
    //     kv2.Initialize();
    //   }
    // }

    // foreach(var kv in pauseHUDTargets)
    // {
    //   foreach (var kv2 in kv.Value)
    //   {
    //     kv2.HUDBehaviour?.Initialize();
    //   }
    // }

    if(waitHUDCreate)
    {
      // 
    }

    pauseHUDTargets.Clear();
  }

  public HUDAttribute GetHUDAttribute<T>() where T : HUDBehaviour
  {
    var type = typeof(T);

    return GetHUDAttribute(type);
  }
  private HUDAttribute GetHUDAttribute(Type type)
  {
    HUDAttribute uiAtrribute = null;
    if (hudAttributes.ContainsKey(type))
    {
      uiAtrribute = hudAttributes[type];
    }
    else
    {
      var attributes = type.GetCustomAttributes(typeof(HUDAttribute), false) as HUDAttribute[];
      if (null == attributes || 0 == attributes.Length)
        return null;

      uiAtrribute = attributes[0];
      hudAttributes.Add(type, uiAtrribute);
    }

    return uiAtrribute;
  }

  public HUDBehaviour PopHUDPool(HUDAttribute attribute)
  {
    if (!hudPools.ContainsKey(attribute.UniqueID))
      return null;

    var queue = hudPools[attribute.UniqueID];
    if(queue == null || queue.Count == 0)
    {
      return null;
    }

    var hud = queue.Dequeue();
    return hud;
  }

  public void PushHUDPool(HUDBehaviour hud)
  {
    var key = hud.Attribute.UniqueID;
    if (!hudPools.ContainsKey(key))
    {
      hudPools[key] = new Queue<HUDBehaviour>();
    }

    RemoveActiveHUD(hud);
    var queue = hudPools[key];
    if(queue.Count < HUD_POOLING_MAX_SIZE)
    {
      queue.Enqueue(hud);
    }
    else
    {
      GameManager.Instance.ScheduleForDestruction(hud.gameObject);
    }
  }

  private async Task<T> _CreateHUD<T>(BaseObject target, HUDAttribute attribute) where T : HUDBehaviour
  {
    //  대기 상태.
    var key = target.GetInstanceID();
    waitCreateHUD.Add(key, attribute);

    while(!createdHUD.ContainsKey(key))
    {
      await Task.Yield();
    }

    var hud = createdHUD[key];
    createdHUD.Remove(key);
    return hud == null ? null : (T)hud;
  }

  public async Task<T> ShowHUD<T>(BaseObject target, Action complete = null, int sortingLayer = -1) where T : HUDBehaviour
  {
    // 기본 어트리뷰트 정보로 세팅한다.
    var attribute = GetHUDAttribute<T>();
    if (null == attribute)
      return null;

    if (string.IsNullOrEmpty(attribute.UniqueID))
      return null;

    // 사용 가능한 UI 가 있을 경우 재활용한다.
    // 없을 경우 새로 생성한다.

    // 노출 중인 HUD 가 있을 경우.
    if(target.HUDBehaviour != null)
    {
      await target.HUDBehaviour.Hide();
    }

    // 노출하려는 대상이 존재하는 경우.
    var active = IsActiveHUD;
    if (hudExposeTypes.Count > 0)
    {
      active = hudExposeTypes.Contains(attribute.Type);
    }

    if(active)
    {
      var hudBehaviour = PopHUDPool(attribute);
      // var hudBehaviour = PopHUDPool(attribute.Type, attribute.isUnique);
      if (hudBehaviour == null)
      {
        if (attribute.isUnique)
        {
          foreach (var kv in activeHUDListByLayers)
          {
            foreach (var hud in kv.Value)
            {
              if (hud.Attribute.Type == attribute.Type)
              {
                hudBehaviour = hud;
                break;
              }
            }
            if (hudBehaviour != null)
              break;
          }
        }
        if (hudBehaviour == null)
        {
          hudBehaviour = CreateHUD(attribute);
          // hudBehaviour = await _CreateHUD<T>(target, attribute);
        }
      }

      if (hudBehaviour == null)
        return null;

      if (sortingLayer == -1)
      {
        sortingLayer = attribute.SortingLayer;
      }

      if (!hudSortingLayers.ContainsKey(sortingLayer))
      {
        var goLayer = new GameObject($"Layer_{sortingLayer}")
        {
          layer = LayerMask.NameToLayer("UI")
        };
        goLayer.SetParent(GetCanvasObject(UICanvasTypes.HUD));
        var rt = goLayer.AddComponent<RectTransform>();
        rt.anchoredPosition3D = Vector3.zero;
        rt.anchorMax = Vector2.one;
        rt.anchorMin = rt.offsetMin = rt.offsetMax = Vector2.zero;
        hudSortingLayers.Add(sortingLayer, rt);

        // Key 목록을 List로 변환
        var values = hudSortingLayers.Keys.ToList();
        values.Sort();

        // 정렬된 Dictionary 객체 구현
        var temp = new Dictionary<int, RectTransform>();
        foreach (var layer in values)
        {
          rt = hudSortingLayers[layer];
          rt.SetAsLastSibling();
          temp.Add(layer, hudSortingLayers[layer]);
        }
        hudSortingLayers = temp;

        if (!activeHUDListByLayers.ContainsKey(sortingLayer))
        {
          activeHUDListByLayers[sortingLayer] = new List<HUDBehaviour>();
        }
      }

      hudBehaviour.gameObject.SetParent(hudSortingLayers[sortingLayer].gameObject);
      hudBehaviour.Attribute.SortingLayer = sortingLayer;

      // 노출대상이 아닌 경우 오브젝트가 켜진채로 풀로 이동해야함.
      hudBehaviour.SetActive(active);
      hudBehaviour.SetData(target);
      hudBehaviour.Show(complete);

      AddActiveHUD(hudBehaviour);
      RemovePauseHUD(hudBehaviour.Attribute.Type, target);

      UpdateHUDOrderInLayer();

      return (T)hudBehaviour;
    }
    else
    {
      AddPauseHUD<T>(target);
    }

    return null;
  }

  public async void HideHUD(BaseObject target)
  {
    if (target.HUDBehaviour == null)
      return;

    await target.HUDBehaviour.Hide();

    UpdateHUDOrderInLayer();
  }

  // 지정된 타입외의 HUD 는 꺼주고, 새로 노출되지 않게해야한다.
  // 즉, ltActivateHUD 에 등록은 되지만, 숨김처리되어야한다.
  // 반대로 켜주는것도 만들어야함
  public void ClearHUDExposeType()
  {
    hudExposeTypes.Clear();
  }
  /// <summary>
  /// 노출 유형 등록
  /// <code>
  /// - 노출 유형이 등록되면 해당 타입을 제외한 나머지를 모두 종료해야한다.
  /// - 새로 노출되는 경우(Show) 에는 ltActivateHUD 에 등록을 하지만 노출하지 않는다.
  /// </code>
  /// </summary>
  /// <param name="types"></param>
  public void AddHUDExposeType(params Type[] types)
  {
    foreach (var type in types)
    {
      if(hudExposeTypes.Contains(type)) continue;
      hudExposeTypes.Add(type);
    }

    var i = 0;
    foreach (var kv in activeHUDListByLayers)
    {
      while (i < kv.Value.Count)
      {
        var hud = kv.Value[i];
        if (pauseHUDTargets.ContainsKey(hud.Attribute.Type))
        {
          List<BaseObject> pauseTargets = pauseHUDTargets[hud.Attribute.Type];
          if (pauseTargets != null && pauseTargets.Contains(hud.Target))
          {
            i++;
            continue;
          }
        }

        if (hudExposeTypes.Contains(hud.Attribute.Type))
        {
          i++;
          continue;
        }

        // 이미 Hide 중인 경우?
        if (hud.IsPlayingHideAnimation)
        {
          i++;
          continue;
        }

        AddPauseHUD(hud);
      }

      i = 0;
    }


    UpdateHUDOrderInLayer();
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="clearUpdate">hudExposeTypes 의 Count 가 0 이 되는 순간 Pause 항목 UpdateHUD 여부</param>
  /// <param name="types">노출 중지 대상.</param>
  public void RemoveHUDExposeType(bool clearUpdate, params Type[] types)
  {
    foreach (var type in types)
    {
      hudExposeTypes.Remove(type);
    }

    if(hudExposeTypes.Count == 0) // Pause 항목을 전부 노출처리함.
    {
      if (clearUpdate)
      {
        var i = 0;
        foreach (var kv in pauseHUDTargets)
        {
          while (i < kv.Value.Count)
          {
            // Debug.LogError($"Index : {i} / Count : {kv.Value.Count}");
            if (hudExposeTypes.Count > 0 && !hudExposeTypes.Contains(kv.Key))
            {
              i++;
              continue;
            }
            var target = kv.Value[i];
            if (target == null)
            {
              i++;
              continue;
            }

            if(RemovePauseHUD(kv.Key, target))
            {
              if (target.gameObject.activeSelf)
              {
                target.UpdateHUD();
              }
            }
            else
            {
              i++;
            }
          }

          i = 0;
        }
      }
    }
    else // 노출중인 오브젝트중 hudExposeTypes 에 있는 type 제외한 hud를 pause한다.
    {
      foreach (var kv in activeHUDListByLayers)
      {
        var i = 0;
        while (i < kv.Value.Count)
        {
          var hud = kv.Value[i];
          if (hudExposeTypes.Contains(hud.Attribute.Type))
          {
            i++;
            continue;
          }

          if (AddPauseHUD(hud))
            continue;

          i++;
        }
      }
    }

    UpdateHUDOrderInLayer();
  }

  public void UpdateHUDOrderInLayer()
  {
    foreach (var kv in activeHUDListByLayers)
    {
      kv.Value.Sort((hud1, hud2) =>
      {
        if (hud1.Attribute.OrderInLayer != hud2.Attribute.OrderInLayer)
          return hud2.Attribute.OrderInLayer.CompareTo(hud1.Attribute.OrderInLayer);
        if (hud1.Target == null || hud2.Target == null)
          return 0;

        return hud2.Target.RenderOrder.CompareTo(hud1.Target.RenderOrder);

      });

      foreach (var hud in kv.Value)
      {
        hud.rectTransform.SetAsFirstSibling();
      }
    }
  }

  public void ChangeHUDSortingLayer(HUDBehaviour hudBehaviour, int sortingLayer)
  {
    if (hudBehaviour == null || hudBehaviour.Attribute.SortingLayer == sortingLayer)
      return;

    if (!hudSortingLayers.ContainsKey(sortingLayer))
    {
      var goLayer = new GameObject($"Layer_{sortingLayer}")
      {
        layer = LayerMask.NameToLayer("UI")
      };
      goLayer.SetParent(GetCanvasObject(UICanvasTypes.HUD));
      var rt = goLayer.AddComponent<RectTransform>();
      rt.anchoredPosition3D = Vector3.zero;
      rt.anchorMax = Vector2.one;
      rt.anchorMin = rt.offsetMin = rt.offsetMax = Vector2.zero;
      hudSortingLayers.Add(sortingLayer, rt);

      // Key 목록을 List로 변환
      var values = hudSortingLayers.Keys.ToList();
      values.Sort();

      // 정렬된 Dictionary 객체 구현
      var temp = new Dictionary<int, RectTransform>();
      foreach (var layer in values)
      {
        rt = hudSortingLayers[layer];
        rt.SetAsLastSibling();
        temp.Add(layer, hudSortingLayers[layer]);
      }
      hudSortingLayers = temp;

      if (!activeHUDListByLayers.ContainsKey(sortingLayer))
      {
        activeHUDListByLayers.Add(sortingLayer, new List<HUDBehaviour>());
      }
    }

    RemoveActiveHUD(hudBehaviour);
    hudBehaviour.gameObject.SetParent(hudSortingLayers[sortingLayer].gameObject);
    hudBehaviour.Attribute.SortingLayer = sortingLayer;
    AddActiveHUD(hudBehaviour);
  }

  public void SetActiveAllHUD(bool value)
  {
    var i = 0;
    IsActiveHUD = value;
    if (value)
    {
      foreach (var kv in pauseHUDTargets)
      {
        while (i < kv.Value.Count)
        {
          if (hudExposeTypes.Count > 0 && !hudExposeTypes.Contains(kv.Key))
          {
            i++;
            continue;
          }
          var target = kv.Value[i];
          if(target == null)
          {
            i++;
            continue;
          }

          if (RemovePauseHUD(kv.Key, target))
          {
            if (target.gameObject.activeSelf)
            {
              target.UpdateHUD();
            }
          }
          else
          {
            i++;
          }
        }

        i = 0;
      }

      UpdateHUDOrderInLayer();
    }
    else
    {
      foreach (var kv in activeHUDListByLayers)
      {
        while (i < kv.Value.Count)
        {
          // Debug.LogError($"Index : {i} / Count : {kv.Value.Count}");
          var hud = kv.Value[i];
          if(pauseHUDTargets.ContainsKey(hud.Attribute.Type))
          {
            List<BaseObject> pauseTargets = pauseHUDTargets[hud.Attribute.Type];
            if (pauseTargets != null && pauseTargets.Contains(hud.Target))
            {
              i++;
              continue;
            }
          }

          if (hudExposeTypes.Count > 0 && hudExposeTypes.Contains(hud.Attribute.Type))
          {
            i++;
            continue;
          }

          // 이미 Hide 중인 경우?
          if(hud.IsPlayingHideAnimation)
          {
            i++;
            continue;
          }

          AddPauseHUD(hud);
        }

        i = 0;
      }
    }
  }

  /// <summary>
  /// lds - UIAttribute의 카메라 사용값에 따라서 해당 카메라들의 활성화 유무를 결정.
  /// </summary>
  /// <param name="attribute"></param>
  private bool SetActiveCameras(UIAttribute attribute, out Action lateSetting)
  {
    lateSetting = null;
    // GameManager.Instance.GameCamera.IsRendering
    if (attribute.UseGameCamera)
    {
      GameManager.Instance.GameCamera.SetRender(attribute.UseGameCamera);
    }
    else
    {
      lateSetting = () =>
      {
        GameManager.Instance.GameCamera.SetRender(false);
      };
    }

    return lateSetting != null;
  }

  #endregion

  #region TouchEvent

  public void OnTouchBegan(Vector3 pos, bool isFirstTouchedUI)
  {
    isShow = isHide = false;
    // lds - 23.11.3, 방어코드 추가
    if(touchEffectPool == null)
    {
      return;
    }

    var effect = touchEffectPool.Get();
    var newPos = UICamera.ScreenToWorldPoint(pos);
    newPos.z = 0;
    var rt = effect.transform as RectTransform;
    rt.position = newPos;
    rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);

    var p = RectTransformUtility.WorldToScreenPoint(UICamera, newPos);
  }
  public void OnTouchStationary(Vector3 pos, float time, bool isFirstTouchedUI) { }
  public void OnTouchMoved(Vector3 lastPos, Vector3 newPos, bool isFirstTouchedUI)
  {
    if (!IsOpenPopup)
      return;

    // UI 가 드래그를 허용하는 경우 UI를 종료하지 않는다.
    if (OpenPopup.Attribute.IsBlockMoveClose)
    {
      isMovoed = true;
    }
    else // UI 가 드래그를 허용하지 않는 경우 포지션이 노출된 UI 외부영역을 가리키는 순간 종료된다.
    {
      var lt = KRaycast.GetGraphicRayCastHitComponentInParent<UIBehaviour>(newPos);
      if (lt == null)
      {
        if (null != OpenPopup)
        {
          OpenPopup.Hide();
        }
      }
    }
  }
  public void OnTouchEnded(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {
    if (!IsOpenPopup)
      return;

    // 현재 터치이벤트로 인해 노출되는 UI가 있으면 리턴한다.
    if (isShow)
    {
      //HDebug.Log("UI OnTouchEnded / isShow Return");

      isShow = false;
      isMovoed = false;

      return;
    }

    // 현재 터치이벤트로 인해 종료되는 UI가 있으면 리턴한다.
    if (isHide)
    {
      //HDebug.Log("UI OnTouchEnded / isHide Return");

      isHide = false;
      isMovoed = false;

      return;
    }

    // 연출 중인 UI 의 경우 무시한다.
    if (null != OpenPopup && OpenPopup.IsPlayingAnimation)
    {
      return;
    }

    if (OpenPopup.Attribute.DontCloseOnInput && !isMovoed)
      return;

    if (OpenPopup.Attribute.IsBlockMoveClose && isMovoed)
    {
      isMovoed = false;
      return;
    }

    // 열려있는 자식 UI가 있다면 자식 UI를 터치했는가 ?
    // 터치했다면 유지, 그렇지 않을 경우 자신의 부모 UI를 터치했는가?
    // 터치했다면 자식 UI를 닫고, 그렇지 않다면 부모 UI를 닫는다.

    // 열려있는 자식 UI가 없을 때 부모 UI를 터치하였는가 ?
    // 터치했다면 유지, 그렇지 않을 경우 부모UI를 닫는다.
    var lt = KRaycast.GetGraphicRayCastHitComponentsInParent<PopupUIBehaviour>(pos);
    if (null == lt || lt.Count == 0)
    {
      //팝업이 열린상태에서 외부를 찍었을 때 창이 닫히는지 판단한.
      if (null != OpenPopup)
      {
        OpenPopup.Hide();
        //Hide(OpenParentUI);
      }
    }
    else
    {
      // UIAttribute 없거나, UniqueID가 없으면 관리 대상이 아니라고 판단.
      lt.RemoveAll(item => item.Attribute == null || string.IsNullOrEmpty(item.Attribute.UniqueID));

      var uiBehaviour = lt.Find(item => item.Attribute.UniqueID == OpenPopup.Attribute.UniqueID);
      if (null == uiBehaviour)
      {
        OpenPopup.Hide();
        //Hide(OpenParentUI);
      }
    }
  }
  public void OnTouchCanceled(Vector3 pos, bool isFirstTouchedUI, bool isMoved) { }
  public void OnLongTouched(Vector3 pos, bool isFirstTouchedUI) { }
  public void OnClicked(Vector3 pos, bool isFirstTouchedUI) { }
  public void OnPinchUpdated(float offset, float zoomSpeed, bool isFirstTouchedUI)
  {
    if (!IsOpenPopup)
      return;

    // 줌인/아웃 시 꺼져야 할 UI
    // Dim 을 사용하지 않는 UI
    // 그 중 IsMoveable 이 true 인 경우는 제외한다.
    // 외부 영역 터치로 종료되지 않는 팝업도 제외한다.
    if (OpenPopup.Attribute.IsBlockMoveClose || OpenPopup.Attribute.DontCloseOnInput)
    {
      return;
    }
    else
    {
      OpenPopup.Hide();
    }
  }
  public void OnPinchEnded()
  {
  }

  public void OnChangeTouchEventState(bool state)
  {

  }

  #endregion
}
