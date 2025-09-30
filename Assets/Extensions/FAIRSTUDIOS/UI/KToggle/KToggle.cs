using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FAIRSTUDIOS.UI
{
  /// <summary>
  /// Simple toggle -- something that has an 'on' and 'off' states: checkbox, toggle button, radio button, etc.
  /// </summary>
  [AddComponentMenu("K.UI/Toggle")]
  [RequireComponent(typeof(RectTransform))]
  public class KToggle : Selectable, IPointerClickHandler, ISubmitHandler, ICanvasElement
  {
    public enum ToggleTransition
    {
      None,
      Fade,
    }

    [Serializable]
    public class ToggleEvent : UnityEvent<bool>
    { }

    [Serializable]
    public class ToggleEventObject : UnityEvent<KToggle>
    { }

    /// <summary>
    /// Variable to identify this script, change the datatype if needed to fit your use case 
    /// </summary>
    [SerializeField]
    private string m_UniqueID;

    // Whether the toggle is on
    [Tooltip("Is the toggle currently on or off?")]
    [SerializeField]
    private bool m_IsOn = true;
    [SerializeField]
    private bool m_CheckBeforeChange;
    [SerializeField]
    private float m_CheckBeforeChangeWaitTime = 3f;
    private bool m_AllowChange;
    private CancellationTokenSource cancelTokenSource; // 추가
    
    [SerializeField]
    private Color m_ColorOn = Color.white;
    [SerializeField]
    private Color m_ColorOff = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField]
    private List<MaskableGraphic> m_Graphics;

    [SerializeField]
    private Color m_ShadowColorOn = Color.white;
    [SerializeField]
    private Color m_ShadowColorOff = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField]
    private List<Shadow> m_Shadows;

    /// <summary>
    /// Transition type.
    /// </summary>
    public ToggleTransition toggleTransition = ToggleTransition.Fade;
    [SerializeField]
    private float m_fadeDuration = 0.1f;

    /// <summary>
    /// Graphic the toggle should be working with.
    /// </summary>
    [SerializeField]
    private Graphic graphic;
    [SerializeField]
    private List<Graphic> addedGraphics;
    
    [SerializeField]
    private bool m_ShowBackground = true;
    [SerializeField]
    private Graphic backgroundGraphic;
    [SerializeField]
    private List<Graphic> addedBacgkroundGraphics;
    [SerializeField]
    private Graphic unInteractableGraphic;
    [SerializeField]
    private List<Graphic> addedUnInteractionableGraphics;

    // group that this toggle can belong to
    [SerializeField]
    private KToggleGroup m_Group;

    [Tooltip("This event is called before the toggle is changed.")]
    public ToggleEventObject onBeforeToggleChange = new ToggleEventObject();
    
    /// <summary>
    /// Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
    /// </summary>
    [Tooltip("Use this event if you only need the bool state of the toggle that was changed")]
    public ToggleEvent onValueChanged = new ToggleEvent();

    /// <summary>
    /// Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
    /// </summary>
    [Tooltip("Use this event if you need access to the toggle that was changed")]
    public ToggleEventObject onToggleChanged = new ToggleEventObject();

    public ToggleEvent onNotifyValueChanged = new ToggleEvent();
    public ToggleEventObject onIsOnChanged = new ToggleEventObject();

    /// <summary>
    /// 상호작용불가능 할 때 클릭하면 발생하는 이벤트
    /// <seealso cref="onClickDisabled"/>
    /// </summary>
    [Tooltip("this event is called when uninteractable toogle.")]
    public ToggleEventObject onClickDisabled = new ToggleEventObject();

    public string UniqueID { get { return m_UniqueID; } set { m_UniqueID = value; } }

    public KToggleGroup Group
    {
      get { return m_Group; }
      set
      {
        m_Group = value;

        SetToggleGroup(m_Group, true);
        PlayEffect(true);
      }
    }

    public bool isOn
    {
      get { return m_IsOn; }
      set
      {
        Set(value);
      }
    }
    public bool checkBeforeChange { get => m_CheckBeforeChange; set => m_CheckBeforeChange = value; }
    
    public new virtual bool interactable
    {
      get { return base.interactable; }
      set
      {
        base.interactable = value;
        if (base.interactable == false)
          DisableEffect();
        else
          Set(isOn, false);
      }
    }

    public bool showBackground { get => m_ShowBackground; set => m_ShowBackground = value; }
    protected KToggle()
    { }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
      base.OnValidate();

      //Set(isOn, false);
      //PlayEffect(toggleTransition == ToggleTransition.None);

      if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

#endif // if UNITY_EDITOR

    public virtual void Rebuild(CanvasUpdate executing)
    {
#if UNITY_EDITOR
      if (executing == CanvasUpdate.Prelayout)
      {
        onValueChanged.Invoke(isOn);
        onToggleChanged.Invoke(this);
      }
#endif
    }

    public virtual void LayoutComplete()
    { }

    public virtual void GraphicUpdateComplete()
    { }
    
    /// <summary>
    /// Assume the correct visual state.
    /// </summary>
    protected override void Start()
    {
      PlayEffect(true);
    }

    protected override void OnEnable()
    {
      base.OnEnable();
      SetToggleGroup(m_Group, false);
      PlayEffect(true);
      if (interactable == false)
        DisableEffect();
      else
        Set(m_IsOn, false);
    }

    protected override void OnDisable()
    {
      SetToggleGroup(null, false);
      base.OnDisable();
    }

    protected override void OnDidApplyAnimationProperties()
    {
      // Check if isOn has been changed by the animation.
      // Unfortunately there is no way to check if we don't have a graphic.
      if (graphic != null)
      {
        bool oldValue = !Mathf.Approximately(graphic.canvasRenderer.GetColor().a, 0);
        if (isOn != oldValue)
        {
          isOn = oldValue;
          Set(!oldValue);
        }
      }

      base.OnDidApplyAnimationProperties();
    }

    // lds - 추가
    public void SetInteractable(bool value, bool sendCallback, bool bTransition = true)
    {
      base.interactable = value;
      if (base.interactable == false)
        DisableEffect();
      else
        Set(isOn, sendCallback, bTransition);
    }

    private void SetToggleGroup(KToggleGroup newGroup, bool setMemberValue)
    {
      // Sometimes IsActive returns false in OnDisable so don't check for it.
      // Rather remove the toggle too often than too little.
      if (m_Group != null)
        m_Group.UnregisterToggle(this);

      // At runtime the group variable should be set but not when calling this method from OnEnable or OnDisable.
      // That's why we use the setMemberValue parameter.
      if (setMemberValue)
        m_Group = newGroup;

      // Only register to the new group if this Toggle is active.
      if (newGroup != null && IsActive())
        newGroup.RegisterToggle(this);

      // If we are in a new group, and this toggle is on, notify group.
      // Note: Don't refer to m_Group here as it's not guaranteed to have been set.
      if (newGroup != null && isOn && IsActive())
        newGroup.NotifyToggleOn(this);
    }

    private void Set(bool value)
    {
      Set(value, true);
    }

    public async void Set(bool value, bool sendCallback, bool bTransition = true, bool fromToggleGroup = false)
    {
      if (Application.isPlaying && m_CheckBeforeChange && sendCallback)
      {
        if (!fromToggleGroup || m_Group != null && m_Group.SelectedToggle == this)
        {
          m_AllowChange = false;
          cancelTokenSource = new CancellationTokenSource();
          onBeforeToggleChange.Invoke(this);
          await WaitToggleChange();
          if (!m_AllowChange)
            return;
        }
      }
      
      // if we are in a group and set to true, do group logic
      m_IsOn = value;

      if (m_Group != null && IsActive())
      {
        if (isOn || (!m_Group.AnyTogglesOn() && !m_Group.AllowSwitchOff))
        {
          m_IsOn = true;
          m_Group.NotifyToggleOn(this, bTransition);
        }
      }

      // Always send event when toggle is clicked, even if value didn't change
      // due to already active toggle in a toggle group being clicked.
      // Controls like Dropdown rely on this.
      // It's up to the user to ignore a selection being set to the same value it already was, if desired.

      var lastTransition = toggleTransition;
      toggleTransition = bTransition ? toggleTransition : ToggleTransition.None;
      PlayEffect(toggleTransition == ToggleTransition.None);
      toggleTransition = lastTransition;
      onNotifyValueChanged?.Invoke(isOn);
      onIsOnChanged?.Invoke(this);  // lds - sendCallback 없이 
      if (sendCallback)
      {
        onValueChanged.Invoke(isOn);
        onToggleChanged.Invoke(this);
      }
    }

    private void DisableEffect()
    {
      if (graphic == null)
        return;

      graphic.canvasRenderer.SetAlpha(0f);
      for (int i = 0; i < addedGraphics.Count; i++)
      {
        addedGraphics[i].canvasRenderer.SetAlpha(0f);
      }

      if (backgroundGraphic != null)
      {
        backgroundGraphic.canvasRenderer.SetAlpha(0f);
        for (int i = 0; i < addedBacgkroundGraphics.Count; i++)
        {
          addedBacgkroundGraphics[i].canvasRenderer.SetAlpha(0f);
        }
      }

      if (unInteractableGraphic != null)
      {
        unInteractableGraphic.canvasRenderer.SetAlpha(1f);
        for (int i = 0; i < addedUnInteractionableGraphics.Count; i++)
        {
          addedUnInteractionableGraphics[i].canvasRenderer.SetAlpha(1f);
        }  
      }
    }

    /// <summary>
    /// Play the appropriate effect.
    /// </summary>
    private void PlayEffect(bool instant)
    {
      if (interactable == false) return;
      if (graphic == null)
        return;

#if UNITY_EDITOR
      if (!Application.isPlaying)
      {
        graphic.canvasRenderer.SetAlpha(isOn ? 1f : 0f);
        for (int i = 0; i < addedGraphics.Count; i++)
        {
          addedGraphics[i].canvasRenderer.SetAlpha(isOn ? 1f : 0f);
        }
      }
      else
#endif
      {
        graphic.CrossFadeAlpha(isOn ? 1f : 0f, instant ? 0f : m_fadeDuration, true);
        for (int i = 0; i < addedGraphics.Count; i++)
        {
          if (addedGraphics[i] == null)
            continue;

          addedGraphics[i].CrossFadeAlpha(isOn ? 1f : 0f, instant ? 0f : m_fadeDuration, true);
        }
      }

      if (showBackground)
      {
        if(backgroundGraphic != null && backgroundGraphic.canvasRenderer.GetAlpha() != 1)
        {
#if UNITY_EDITOR
          if (!Application.isPlaying)
          {
            backgroundGraphic.canvasRenderer.SetAlpha(1);

            for (int i = 0; i < addedBacgkroundGraphics.Count; i++)
            {
              addedBacgkroundGraphics[i].canvasRenderer.SetAlpha(1);
            }
          }
          else
#endif
          {
            backgroundGraphic.CrossFadeAlpha(1f, 0f, true);

            for (int i = 0; i < addedBacgkroundGraphics.Count; i++)
            {
              addedBacgkroundGraphics[i].CrossFadeAlpha(1f, 0f, true);
            }

          }
        }
      }
      else
      {
        if (backgroundGraphic != null)
        {
#if UNITY_EDITOR
          if (!Application.isPlaying)
          {
            backgroundGraphic.canvasRenderer.SetAlpha(!isOn ? 1f : 0f);
            for (int i = 0; i < addedBacgkroundGraphics.Count; i++)
            {
              addedBacgkroundGraphics[i].canvasRenderer.SetAlpha(!isOn ? 1f : 0f);
            }
          }
          else
#endif
          {
            backgroundGraphic.CrossFadeAlpha(!isOn ? 1f : 0f, instant ? 0f : m_fadeDuration, true);

            for (int i = 0; i < addedBacgkroundGraphics.Count; i++)
            {
              addedBacgkroundGraphics[i].CrossFadeAlpha(!isOn ? 1f : 0f, instant ? 0f : m_fadeDuration, true);
            }

          }
        }
      }

      if (m_Graphics != null)
      {
        for (int i = 0; i < m_Graphics.Count; i++)
        {
          if (m_Graphics[i] == null)
            continue;

#if UNITY_EDITOR
          if (!Application.isPlaying)
            m_Graphics[i].canvasRenderer.SetColor(isOn ? m_ColorOn : m_ColorOff);
          else
#endif
            m_Graphics[i].CrossFadeColor(isOn ? m_ColorOn : m_ColorOff, instant ? 0f : 0.1f, true, true);
        }
      }

      if (m_Shadows != null)
      {
        for (int i = 0; i < m_Shadows.Count; i++)
        {
          if (m_Shadows[i] == null)
            continue;

          m_Shadows[i].effectColor = isOn ? m_ShadowColorOn : m_ShadowColorOff;
        }
      }

      if (unInteractableGraphic == null) return;
      unInteractableGraphic.canvasRenderer.SetAlpha(0f);
      for (int i = 0; i < addedUnInteractionableGraphics.Count; i++)
      {
        addedUnInteractionableGraphics[i].canvasRenderer.SetAlpha(0f);
      }
    }

    private void InternalToggle()
    {
      if (!IsActive())
        return;
      if(!IsInteractable())
      {
        onClickDisabled?.Invoke(this);
        return;
      }

      isOn = !isOn;
    }

    private async Task WaitToggleChange()
    {
      if (m_AllowChange)
        return;
      
      var time = 0f;
      while (!m_AllowChange)
      {
        time += Time.deltaTime;
        if (time > m_CheckBeforeChangeWaitTime || cancelTokenSource.Token.IsCancellationRequested)
          break;
        
        await Task.Yield();
      }
    }

    public void AllowChange()
    {
      if (!m_CheckBeforeChange)
        return;
      
      m_AllowChange = true;
    }
    public void DisallowChange()
    {
      cancelTokenSource?.Cancel();
    }

    /// <summary>
    /// React to clicks.
    /// </summary>
    public virtual void OnPointerClick(PointerEventData eventData)
    {
      if (eventData.button != PointerEventData.InputButton.Left)
        return;

      SoundManager.Instance.PlayFX(SoundFxTypes.BTN);

      InternalToggle();
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
      InternalToggle();
    }
  }
}
