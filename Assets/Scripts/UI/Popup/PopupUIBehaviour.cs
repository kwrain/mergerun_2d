using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace FAIRSTUDIOS.UI
{
  /// <summary>
  /// Popup UI 기본 클래스
  /// </summary>
  public class PopupUIBehaviour : UIBehaviour
  {
    protected UnityEvent onButtonCancel;
    protected UnityEvent onButtonClose;
    protected UnityEvent onButtonOK;

    [SerializeField, HideInInspector] protected bool useRewind;
    protected Func<bool> onRewind;

    protected override SoundFxTypes ShowSound => SoundFxTypes.SHOW_POPUP;
    protected override SoundFxTypes HideSound => SoundFxTypes.HIDE_POPUP;

    public PopupUIAttribute Attribute { get; set; }

    protected bool IsRewind { get; set; }

    public new virtual void Prepared()
    {
      if (canvasGroup != null)
      {
        canvasGroup.interactable = true;
      }

      CancelPrepareTimer();
      preparedCallback?.Invoke(true); // lds - 23.2.15, 준비가 완료되면 콜벡 호출
      preparedCallback = null;
    }

    public static Sequence CreateDefaultShowAnimation(RectTransform rectTransform, CanvasGroup canvasGroup, Action complete = null)
    {
      var sequence = DOTween.Sequence();
      sequence = DOTween.Sequence();
      sequence.Append(rectTransform.DOAnchorPosY(0, UIManager.TWEEN_UI_SHOW).SetEase(Ease.OutQuad).ChangeStartValue(new Vector2(rectTransform.anchoredPosition.x, -50f)));
      sequence.Join(canvasGroup.DOFade(1, UIManager.TWEEN_UI_SHOW).SetEase(Ease.OutQuad).ChangeStartValue(0));
      sequence.OnRewind(() =>
      {
        canvasGroup.alpha = 1;
      });
      sequence.OnComplete(() =>
      {
        complete?.Invoke();
        CompleteShow();
      });
      sequence.SetAutoKill(false);
      sequence.Pause();

      return sequence;

      void CompleteShow()
      {
        if (canvasGroup != null)
        {
          canvasGroup.alpha = 1;
          canvasGroup.blocksRaycasts = true;
        }
      }
    }
    protected override void DefaultShowAnimation()
    {
      if (showDefaultSequence == null)
      {
        showDefaultSequence = DOTween.Sequence();
        showDefaultSequence.Append(rectTransform.DOAnchorPosY(0, UIManager.TWEEN_UI_SHOW).SetEase(Ease.OutQuad).ChangeStartValue(new Vector2(rectTransform.anchoredPosition.x, -50f)));
        showDefaultSequence.Join(canvasGroup.DOFade(1, UIManager.TWEEN_UI_SHOW).SetEase(Ease.OutQuad).ChangeStartValue(0));
        showDefaultSequence.OnComplete(CompleteShow);
        showDefaultSequence.SetAutoKill(false);
        showDefaultSequence.Pause();
      }

      showDefaultSequence.OnComplete(CompleteShow);
      showDefaultSequence.Restart();
    }

    public static Sequence CreateDefaultHideAnimation(RectTransform rectTransform, CanvasGroup canvasGroup, Action complete = null)
    {
      var sequence = DOTween.Sequence();
      sequence = DOTween.Sequence();
      sequence.Append(rectTransform.DOAnchorPosY( -50, UIManager.TWEEN_UI_HIDE).SetEase(Ease.OutQuad));
      sequence.Join(canvasGroup.DOFade(0, UIManager.TWEEN_UI_HIDE).SetEase(Ease.OutQuad));
      sequence.OnComplete(() =>
      {
        complete?.Invoke();
        complete = null;

        CompleteHide();
      });
      sequence.SetAutoKill(false);
      sequence.Pause();

      return sequence;

      void CompleteHide()
      {
      }
    }
    protected override void DefaultHideAnimation()
    {
      if (hideDefaultSequence == null)
      {
        hideDefaultSequence = DOTween.Sequence();
        hideDefaultSequence.Append(rectTransform.DOAnchorPosY( -50, UIManager.TWEEN_UI_HIDE).SetEase(Ease.OutQuad));
        hideDefaultSequence.Join(canvasGroup.DOFade(0, UIManager.TWEEN_UI_HIDE).SetEase(Ease.OutQuad));

        hideDefaultSequence.SetAutoKill(false);
        hideDefaultSequence.Pause();
      }

      hideDefaultSequence.OnComplete(CompleteHide);
      hideDefaultSequence.Restart();
    }

    protected override void CompleteHide()
    {
      if(useRewind && IsRewind)
      {
        IsRewind = OnRewind(); // 내부에서 Rewind 여부 결정
        if (UseShowAnimation)
        {
          if (UseShowDefaultAnimation)
          {
            DefaultShowAnimation();
          }
          else
          {
            ShowAnimation();
          }
        }
        else
        {
          CompleteShow();
        }
      }
      else
      {
        onButtonClose?.Invoke();

        base.CompleteHide();
      }
    }

    /// <summary>
    /// Popup 을 닫는다.
    /// </summary>
    /// <param name="callback"></param>
    public override void Hide(Action complete = null, bool check = true)
    {
      // 되감기 코드
      if(useRewind)
      {
        if(IsRewind)
        {
          if (UseHideAnimation && gameObject.activeSelf)
          {
            if (UseHideDefaultAnimation)
            {
              DefaultHideAnimation();
            }
            else
            {
              HideAnimation();
            }
          }
          else
          {
            CompleteHide();
          }

          return;
        }
      }

      if (check && UIManager.Instance.CheckOpenPopup(this))
      {
        UIManager.Instance.HidePopup(this, complete);
      }
      else
      {
        if (canvasGroup != null)
        {
          canvasGroup.blocksRaycasts = false;
        }

        if (complete != null)
        {
          onHideCompleteAction += complete;
        }

        if (UseHideAnimation && gameObject.activeSelf)
        {
          if (UseHideDefaultAnimation)
          {
            DefaultHideAnimation();
          }
          else
          {
            HideAnimation();
          }
        }
        else
        {
          CompleteHide();
        }
      }
    }

    protected override void PlayShowSound()
    {
      if (!PlayShowAndHideSound || isPlayedShowSound)
        return;

      SoundManager.Instance.PlayFX(ShowSound);
      isPlayedShowSound = true;
    }
    protected override void PlayHideSound()
    {
      if (!PlayShowAndHideSound || isPlayedHideSound)
        return; 

      SoundManager.Instance.PlayFX(HideSound, UIManager.TWEEN_UI_HIDE * 0.5f);
      isPlayedHideSound = true;
    }

    protected virtual bool OnRewind()
    {
      return false;
    }

    #region Button Event

    public void ClearButtonEvent()
    {
      onButtonCancel = null;
      onButtonClose = null;
      onButtonOK = null;
    }

    public virtual void OnButtonCancel()
    {
      if (null != onButtonCancel)
      {
        onButtonCancel.Invoke();
        onButtonCancel = null;
      }
    }
    public virtual void OnButtonClose()
    {
      if (null != onButtonClose)
      {
        onButtonClose.Invoke();
        onButtonClose = null;
      }

      Hide();
    }
    public void OnButtonShowPopup(string name)
    {
      var type = System.Type.GetType(name);
      if (type == null)
        return;

      UIManager.Instance.ShowPopup(type);
    }
    public virtual void OnButtonOK()
    {
      if (null != onButtonOK)
      {
        onButtonOK.Invoke();
        onButtonOK = null;
      }
    }

    public override void OnButtonEscape()
    {
      OnButtonClose();
    }

    #endregion
  }
}