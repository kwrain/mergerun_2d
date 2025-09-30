using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FAIRSTUDIOS.UI
{
  [AddComponentMenu("K.UI/Button")]
  public class KButton : Selectable, IPointerClickHandler, ISubmitHandler
  {
    [Serializable]
    public class ButtonEvent : UnityEvent
    {
    }

    [Serializable]
    public class ButtonEventObject : UnityEvent<KButton>
    {
    }

    public enum EPositionType
    {
      Fixed,
      Floating
    }

    private RectTransform _rectTransform;

    private Camera cam;

    private Vector2 fixedPosition = Vector2.zero;

    private bool isBlockEvent;

    private Coroutine coPress;

    [SerializeField] private string m_UniqueID;

    [Tooltip("Touch Additional Area")] [SerializeField]
    bool isAdditionalArea = false; // 터치 영역 확대 여부

    [SerializeField] Image m_AdditionalArea;

    [SerializeField] EPositionType positionType = EPositionType.Fixed;

    [SerializeField] Color m_ColorOn = Color.white;
    [SerializeField] Color m_ColorOff = new(1f, 1f, 1f, 0.5f);
    [SerializeField] List<MaskableGraphic> m_Graphics;

    [SerializeField] Color m_ShadowColorOn = Color.white;
    [SerializeField] Color m_ShadowColorOff = new(1f, 1f, 1f, 0.5f);
    [SerializeField] List<Shadow> m_Shadows;

    [SerializeField] RectTransform rtFloating;

    [Range(0f, 5f), SerializeField] float blockEventTime = 0.2f; // 클릭 방어 시간
    [SerializeField] private bool blockAutoInteractable; // 방어 시간동안 자동으로 인터렉터블 변경

    [Range(0f, 2f), SerializeField] float pressedWaitTime;
    [Range(0f, 1f), SerializeField] float repeatedSpeed = 0.4f;
    [Range(0f, 1f), SerializeField] float maxAcceleration;
    [SerializeField, Tooltip("Pressed 동작이후에 OnClick, OnUp 이벤트를 동작 못하도록 하는 옵션")]
    private bool blockPointerUpWhenPressed = false;
    private bool isInvokedPressed = false;

    // click
    [SerializeField] private bool customClickSound = false;
    [SerializeField] private SoundFxTypes clickSound = SoundFxTypes.BTN;
    private SoundFxTypes defaultClickSound = SoundFxTypes.BTN;

    // disabled
    [SerializeField] private bool customDisabledSound = false;
    [SerializeField] private SoundFxTypes disabledSound = SoundFxTypes.BTN;
    private SoundFxTypes defaultDisabledSound = SoundFxTypes.BTN;

    // down
    [SerializeField] private bool customDownSound = false;
    [SerializeField] private SoundFxTypes downSound = SoundFxTypes.NONE;
    private SoundFxTypes defaultDownSound = SoundFxTypes.NONE;

    // up
    [SerializeField] private bool customUpSound = false;
    [SerializeField] private SoundFxTypes upSound = SoundFxTypes.NONE;
    private SoundFxTypes defaultUpSound = SoundFxTypes.NONE;

    /// <summary>
    /// 상호작용불가능 할 때 클릭하면 발생하는 이벤트
    /// <seealso cref="onClickDisabled"/>
    /// </summary>
    [SerializeField] private ButtonEvent m_OnClickDisabled = new();
    [SerializeField] private ButtonEvent m_OnClick = new();
    [SerializeField] private ButtonEvent m_OnDown = new();
    [SerializeField] private ButtonEvent m_OnUp = new();
    [SerializeField] private ButtonEvent m_OnBlocked = new();
    [SerializeField] private ButtonEvent m_OnPress = new();
    [SerializeField] private ButtonEvent m_OnPressed = new();

    public Graphic graphic;

    public List<Graphic> addedGraphics;

    public Graphic unInteractableGraphic;

    public List<Graphic> addedUnInteractionableGraphics;

    public string UniqueID
    {
      get => m_UniqueID;
      set => m_UniqueID = value;
    }

    public bool IsAdditionalArea
    {
      get => isAdditionalArea;
      set => isAdditionalArea = value;
    }

    public EPositionType PositionType
    {
      get => positionType;
      set
      {
        positionType = value;
        CheckPositionType();
      }
    }

    public RectTransform rectTransform
    {
      get
      {
        if (null == _rectTransform)
          _rectTransform = GetComponent<RectTransform>();

        return _rectTransform;
      }
    }

    public new virtual bool interactable
    {
      set
      {
        base.interactable = value;

        if (m_Graphics != null)
        {
          for (var i = 0; i < m_Graphics.Count; ++i)
          {
            m_Graphics[i].color = base.interactable == true ? m_ColorOn : m_ColorOff;
          }
        }

        if (m_Shadows != null)
        {
          for (var i = 0; i < m_Shadows.Count; ++i)
          {
            m_Shadows[i].effectColor = base.interactable == true ? m_ShadowColorOn : m_ShadowColorOff;
          }
        }

        if (base.interactable == false)
          DisableEffect();
        else
          EnableEffect();
      }
      get => base.interactable;
    }

    public bool IsPress { get; private set; }

    public SoundFxTypes ClickSound
    {
      get => clickSound;
      set => clickSound = value;
    }

    public SoundFxTypes DownSound
    {
      get => downSound;
      set => downSound = value;
    }

    public SoundFxTypes UpSound
    {
      get => upSound;
      set => upSound = value;
    }

    public SoundFxTypes DisabledSound
    {
      get => disabledSound;
      set => disabledSound = value;
    }

    public ButtonEvent onClickDisabled
    {
      get => m_OnClickDisabled;
      set => m_OnClickDisabled = value;
    }

    public ButtonEvent onClick
    {
      get => m_OnClick;
      set => m_OnClick = value;
    }

    public ButtonEvent onDown
    {
      get => m_OnDown;
      set => m_OnDown = value;
    }

    public ButtonEvent onUp
    {
      get => m_OnUp;
      set => m_OnUp = value;
    }

    public ButtonEvent onBlocked
    {
      get => m_OnBlocked;
      set => m_OnBlocked = value;
    }

    public ButtonEvent onPress
    {
      get => m_OnPress;
      set => m_OnPress = value;
    }

    public ButtonEvent onPressed
    {
      get => m_OnPressed;
      set => m_OnPressed = value;
    }



    protected override void Awake()
    {
      base.Awake();

      var images = GetComponentsInChildren<Image>(true);
      foreach (var image in images)
      {
        if (image.name == "ImgAdditionalArea")
        {
          m_AdditionalArea = image;
          break;
        }
      }

      SetAdditionalTouchArea(isAdditionalArea);

      CheckPositionType();
    }

    protected override void OnEnable()
    {
      base.OnEnable();

      IsPress = isBlockEvent = false;
      isInvokedPressed = false;
      if (interactable == true)
        EnableEffect();
      else
        DisableEffect();
    }

    protected override void OnDisable()
    {
      base.OnDisable();

      StopAllCoroutines();
      coPress = null;

      if (isBlockEvent && blockAutoInteractable)
        interactable = true;
    }

    protected override void OnRectTransformDimensionsChange()
    {
      CheckPositionType();
    }

    public void CheckPositionType()
    {
      switch (positionType)
      {
        case EPositionType.Fixed:
          if (rtFloating != null)
          {
            rtFloating.SetActive(false);
          }

          break;

        case EPositionType.Floating:
          if (cam == null)
          {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
              cam = canvas.worldCamera;
            }
          }

          // Floating 체크
          if (rtFloating == null)
          {
            rtFloating = transform.Find("Floating") as RectTransform;
            if (rtFloating == null)
            {
              var go = new GameObject("Floating", typeof(AtlasImage));
              go.SetParent(transform);

              rtFloating = go.transform as RectTransform;
            }
          }

          rtFloating.SetActive(true);
          rectTransform.anchorMin = rtFloating.anchorMin;
          rectTransform.anchorMax = rtFloating.anchorMax;
          fixedPosition = rtFloating.anchoredPosition;
          break;
      }
    }

    /// <summary>
    /// 추가 터치 영역 설정 함수
    /// </summary>
    /// <param name="bAdditionalArea"></param>
    public void SetAdditionalTouchArea(bool bAdditionalArea)
    {
      if (bAdditionalArea)
      {
        var graphics = GetComponentsInChildren<MaskableGraphic>();

        if (m_AdditionalArea == null)
        {
          var images = GetComponentsInChildren<Image>(true);
          foreach (var image in images)
          {
            if (image.name == "AdditionalArea")
            {
              m_AdditionalArea = image;
              break;
            }
          }

          if (m_AdditionalArea == null)
          {
            var go = new GameObject("AdditionalArea");
            go.SetParent(gameObject);

            m_AdditionalArea = go.AddComponent<Image>();
            m_AdditionalArea.color = Color.clear;
            m_AdditionalArea.rectTransform.sizeDelta = graphics[0].rectTransform.sizeDelta * 1.5f;
            m_AdditionalArea.enabled = true;
          }
        }

        if (m_AdditionalArea != null && graphics.Length > 0)
        {
          var image = graphics[0].GetComponent<Image>();
          if (null != image)
          {
            m_AdditionalArea.sprite = image.sprite;
          }
        }
        else
        {
          m_AdditionalArea.enabled = false;
        }
      }
      else if (m_AdditionalArea != null)
      {
        m_AdditionalArea.enabled = false;
      }
    }

    private IEnumerator _BlockButtonEvent()
    {
      isBlockEvent = true;

      if (blockAutoInteractable)
      {
        interactable = false;
      }

      yield return new WaitForSeconds(blockEventTime);

      if (blockAutoInteractable)
      {
        interactable = true;
      }

      isBlockEvent = false;
    }

    private IEnumerator _Press()
    {
      float waitTime = 0;
      float repeatedTime = 0;
      var fAcceleration = 0f; //누적 반복 가속도

      var callPressed = false;

      IsPress = true;
      while (IsPress)
      {
        if (waitTime >= pressedWaitTime)
        {
          if (!callPressed)
          {
            onPressed.Invoke();
            callPressed = true;
            isInvokedPressed = true;
          }

          // 반복 처리
          if (repeatedSpeed > 0)
          {
            if (repeatedTime >= repeatedSpeed)
            {
              if (maxAcceleration > 0)
              {
                fAcceleration += Time.deltaTime * repeatedSpeed; //가속도 시키기 위해
                fAcceleration = Mathf.Clamp(fAcceleration, 0, maxAcceleration); //가속도 제한
              }

              repeatedTime = 0;

              onPress.Invoke();
            }
            else
            {
              repeatedTime += Time.deltaTime + fAcceleration;
            }
          }
          else
          {
            onPress.Invoke();
          }
        }
        else if (pressedWaitTime > 0)
        {
          waitTime += Time.deltaTime;
        }

        yield return null;
      }
    }

    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
      if (cam == null)
      {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
          cam = canvas.worldCamera;
        }
      }

      var localPoint = Vector2.zero;
      if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, cam, out localPoint))
      {
        var pivotOffset = rectTransform.pivot * rectTransform.sizeDelta;
        return localPoint - (rtFloating.anchorMax * rectTransform.sizeDelta) + pivotOffset;
      }

      return Vector2.zero;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      // lds - 23.4.13, 마우스 좌클릭 외 입력 막음
      if (eventData.button != PointerEventData.InputButton.Left)
        return;
      if (isBlockEvent)
      {
        onBlocked.Invoke();
        return;
      }

      if (positionType == EPositionType.Floating)
        return;

      // 이벤트 발생이 연속적으로 들어오면 일정시간동안 블럭처리한다.
      // 연출은 진행하되, 이벤트를 실행하지 않는다.

      Press();

      if (blockEventTime > 0)
      {
        if (gameObject.activeSelf && gameObject.activeInHierarchy)
        {
          StartCoroutine(_BlockButtonEvent());
        }
      }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
      // lds - 23.4.13, 마우스 좌클릭 외 입력 막음
      if (eventData.button != PointerEventData.InputButton.Left)
        return;
      if (interactable)
      {
        if (isBlockEvent)
        {
          onBlocked.Invoke();
          return;
        }

        if(pressedWaitTime > 0f)
        {
          // lds - 23.1.21, pressedWaitTime이 0초보다 큰 경우에만 동작하도록함.
          coPress = StartCoroutine(_Press());
        }

        base.OnPointerDown(eventData);

        if (positionType == EPositionType.Floating)
        {
          rtFloating.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        }

        onDown.Invoke();
        PlayEffectSound(downSound, defaultDownSound, customDownSound);
      }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
      // lds - 23.4.13, 마우스 좌클릭 외 입력 막음
      if (eventData.button != PointerEventData.InputButton.Left)
        return;
      if (coPress != null)
      {
        StopCoroutine(coPress);
        coPress = null;
        IsPress = false;
      }

      if (interactable)
      {
        base.OnPointerUp(eventData);

        if (positionType == EPositionType.Floating)
        {
          if (rtFloating == null)
          {
            CheckPositionType();
          }

          if (RectTransformUtility.RectangleContainsScreenPoint(rtFloating, eventData.position, cam))
          {
            rtFloating.anchoredPosition = fixedPosition;

            if (!blockPointerUpWhenPressed || !isInvokedPressed)
            {
              onClick.Invoke();
              PlayEffectSound(clickSound, defaultClickSound, customClickSound);
            }
          }
          else
          {
            rtFloating.anchoredPosition = fixedPosition;
          }
        }

        if (!blockPointerUpWhenPressed || !isInvokedPressed)
        {
          onUp.Invoke();
          PlayEffectSound(upSound, defaultUpSound, customUpSound);
        }
      }
    }

    public void SetRepeatedSpeed(float repeatedSpeed)
    {
      this.repeatedSpeed = repeatedSpeed;
    }

    private void Press()
    {
      if (!IsActive())
        return;

      if (!IsInteractable())
      {
        UISystemProfilerApi.AddMarker("Button.onUninteractClick", this);
        m_OnClickDisabled.Invoke();
        PlayEffectSound(disabledSound, defaultDisabledSound, customDisabledSound);
        return;
      }

      UISystemProfilerApi.AddMarker("Button.onClick", this);
      if(!blockPointerUpWhenPressed || !isInvokedPressed)
      {
        m_OnClick.Invoke();
        PlayEffectSound(ClickSound, defaultClickSound, customClickSound);
      }
      isInvokedPressed = false;
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
      Press();

      // if we get set disabled during the press
      // don't run the coroutine.
      if (!IsActive() || !IsInteractable())
        return;

      DoStateTransition(SelectionState.Pressed, false);
      StartCoroutine(OnFinishSubmit());
    }

    private IEnumerator OnFinishSubmit()
    {
      var fadeTime = colors.fadeDuration;
      var elapsedTime = 0f;

      while (elapsedTime < fadeTime)
      {
        elapsedTime += Time.unscaledDeltaTime;
        yield return null;
      }

      DoStateTransition(currentSelectionState, false);
    }

    private void DisableEffect()
    {
      if (graphic == null)
        return;

      graphic.canvasRenderer.SetAlpha(0f);
      for (var i = 0; i < addedGraphics.Count; i++)
      {
        addedGraphics[i].canvasRenderer.SetAlpha(0f);
      }

      if (unInteractableGraphic == null) return;
      unInteractableGraphic.canvasRenderer.SetAlpha(1f);
      for (var i = 0; i < addedUnInteractionableGraphics.Count; i++)
      {
        addedUnInteractionableGraphics[i].canvasRenderer.SetAlpha(1f);
      }
    }

    private void EnableEffect()
    {
      if (graphic == null)
        return;

      graphic.canvasRenderer.SetAlpha(1f);
      for (var i = 0; i < addedGraphics.Count; i++)
      {
        addedGraphics[i].canvasRenderer.SetAlpha(1f);
      }

      if (unInteractableGraphic == null) return;
      unInteractableGraphic.canvasRenderer.SetAlpha(0f);
      for (var i = 0; i < addedUnInteractionableGraphics.Count; i++)
      {
        addedUnInteractionableGraphics[i].canvasRenderer.SetAlpha(0f);
      }
    }

    private void PlayEffectSound(SoundFxTypes soundType, SoundFxTypes defaultSoundType, bool useCustom)
    {
      if(useCustom)
      {
        SoundManager.Instance.PlayFX(soundType);
      }
      else
      {
        SoundManager.Instance.PlayFX(defaultSoundType);
      }
    }
  }
}