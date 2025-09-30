using System;
using System.Threading.Tasks;
using UnityEngine;

namespace FAIRSTUDIOS.UI
{
  public class HUDBehaviour : HUDTracking
  {
    private HUDAttribute attribute;

    [SerializeField, Space] protected bool useShowAnimation;
    [SerializeField] protected bool useHideAnimation;

    [SerializeField, Space] protected BaseObject target;

    public bool RequireUpdate { get; set; }

    public bool IsPlayingShowAnimation { get; protected set; }
    public bool IsPlayingHideAnimation { get; protected set; }

    public HUDAttribute Attribute
    {
      get
      {
        if (attribute != null)
          return attribute;

        var attributes = (HUDAttribute[])GetType().GetCustomAttributes(typeof(HUDAttribute), false);
        if (attributes.Length == 0)
          return null;

        attribute = attributes[0];

        return attribute;
      }
      set => attribute = value;
    }

    protected override Vector3 TrackingPosition => Target.transform.position + trackingOffset + (Vector3) Target.circleCollider.offset;
    protected override Vector3 TrackingSize => trackingSize * Target.circleCollider.radius;

    public virtual BaseObject Target
    {
      get => target;
      protected set
      {
        target = value;
        if (value == null)
        {
          trackingTarget = null;
          return;
        }

        trackingTarget = value.gameObject;
      }
    }

    public virtual void Initialize() { }

    public void UpdateHUD(params object[] args)
    {
      if (target == null)
        return;

      // target.UpdateHUD(args);
    }

    public virtual void SetData(BaseObject target)
    {
      if (target == null)
        return;

      // if(Target != null)
      //   Target.HUDBehaviour = null;

      // LateUpdate Hide 함수 중복 호출 우려가 있어 미리 세팅한다.
      Target = target;
      // target.HUDBehaviour = this;

      Tracking();
    }

    public virtual async Task Show(Action complete = null)
    {
      if(IsPlayingShowAnimation)
      {
        return;
      }
      
      gameObject.SetActive(IsPlayingShowAnimation = RaycastTarget = true);

      if (useShowAnimation)
      {
        await ShowAnimation(complete);
      }
      else
      {
        CompleteShow();
      }
    }
    public virtual async Task Hide(Action complete = null)
    {
      if (IsPlayingHideAnimation)
      {
        return;
      }
      if (Target != null)
      {
        // Target.HUDBehaviour = null;
        Target = null;
      }

      RaycastTarget = false;
      IsPlayingHideAnimation = true;
      // GameManager.Instance.SelectedObject = null;

      if (useHideAnimation)
      {
        await HideAnimation(complete);
      }
      else
      {
        CompleteHide();
      }
    }

    protected virtual async Task ShowAnimation(Action callback)
    {
      // CompleteShow();
    }

    protected virtual async Task HideAnimation(Action callback)
    {
      // CompleteHide();
    }

    protected virtual void CompleteShow()
    {
      IsPlayingShowAnimation = false;
    }

    protected virtual void CompleteHide()
    {
      // UI 종료 시 UIPool 로 이동이 안됬으면 이동시켜준다.
      if (null != Attribute)
      {
        UIManager.Instance.PushHUDPool(this);
      }

      gameObject.SetActive(IsPlayingHideAnimation = false);
    }

    public virtual void OnButtonHUD() { }
  }
}
