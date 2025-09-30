using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DG.Tweening
{
  [AddComponentMenu("DOTween/DOTween Animation Extended")]
  public class DOTweenAnimationExtended : DOTweenAnimation
  {
    #region Custom

    public enum WhenStopped
    {
      StartPosition,
      BeforeStartPosition,
      EndPosition,
      MaintainPosition,
    }

    public string comment;

    public WhenStopped whenStopped = WhenStopped.StartPosition;

    public float fromToValueFloat;
    public Vector3 fromToValueV3;
    public Vector2 fromToValueV2;
    public Color fromToValueColor = new(1, 1, 1, 1);
    public string fromToValueString = "";
    public Rect fromToValueRect = new(0, 0, 0, 0);
    public bool useTargetAsFromV3;
    public Transform fromToValueTransform;

    private float cachedValueFloat;
    private Quaternion cachedQuaternion;
    private Vector3 cachedValueV3;
    private Vector2 cachedValueV2;
    private Color cachedValueColor = new(1, 1, 1, 1);
    private string cachedValueString = "";
    private Rect cachedValueRect = new(0, 0, 0, 0);

    public bool startValueApplyOnCreate;
    public Action startValueApplyAction;

    protected virtual bool Independent => true;

    #endregion

    private bool CallStoppedAction =>
      Application.isEditor && !Application.isPlaying || whenStopped != WhenStopped.MaintainPosition;

    public override void CreateTween(bool regenerateIfExists = false, bool andPlay = true)
    {
      // 에디터에서 플레이할 때만 체크
      if (Application.isPlaying)
      {
        // 시퀀스용이면 Awake에서 생성하지 않음.
        if (!Independent)
          return;
      }

      tween = CreateTweenInstance(regenerateIfExists, andPlay);
    }

    public virtual Tween CreateTweenInstance(bool regenerateIfExists = false, bool andPlay = true)
    {
      Tween tween = null;
      if (!isValid)
      {
        if (regenerateIfExists)
        {
          // Called manually: warn users
          Debug.LogWarning(
            string.Format("{0} :: This DOTweenAnimation isn't valid and its tween won't be created",
              this.gameObject.name), this.gameObject);
        }

        return tween;
      }

      if (tween != null)
      {
        if (tween.active)
        {
          if (regenerateIfExists) tween.Kill();
          else return tween;
        }

        tween = null;
      }

      //            if (target == null) {
      //                Debug.LogWarning(string.Format("{0} :: This DOTweenAnimation's target is NULL, because the animation was created with a DOTween Pro version older than 0.9.255. To fix this, exit Play mode then simply select this object, and it will update automatically", this.gameObject.name), this.gameObject);
      //                return;
      //            }

      GameObject tweenGO = GetTweenGO();
      if (target == null || tweenGO == null)
      {
        if (targetIsSelf && target == null)
        {
          // Old error caused during upgrade from DOTween Pro 0.9.255
          Debug.LogWarning(
            string.Format(
              "{0} :: This DOTweenAnimation's target is NULL, because the animation was created with a DOTween Pro version older than 0.9.255. To fix this, exit Play mode then simply select this object, and it will update automatically",
              this.gameObject.name), this.gameObject);
        }
        else
        {
          // Missing non-self target
          Debug.LogWarning(
            string.Format("{0} :: This DOTweenAnimation's target/GameObject is unset: the tween will not be created.",
              this.gameObject.name), this.gameObject);
        }

        return tween;
      }

      if (forcedTargetType != TargetType.Unset) targetType = forcedTargetType;
      if (targetType == TargetType.Unset)
      {
        // Legacy DOTweenAnimation (made with a version older than 0.9.450) without stored targetType > assign it now
        targetType = TypeToDOTargetType(target.GetType());
      }

      switch (animationType)
      {
        case AnimationType.None:
          break;
        case AnimationType.Move:
          CheckTransformPosition();

          // DOTweenSequence -> 자기자신을 등록하고 뭔가 매니저 같은 놈이 있고, 거기에서 트리거를 빠꿔야함?
          // DoTweenSequenceManager.SetTrigger("Show");
          switch (targetType)
          {
            case TargetType.Transform:
            {
              var transform = ((Transform) target);
              cachedValueV3 = transform.position;
              tween = transform.DOMove(endValueV3, duration, optionalBool0);
              var tweenerCore = (TweenerCore<Vector3, Vector3, VectorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueV3);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueV3,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue,
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueV3 : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { transform.position = stoppedValue; });
              };
            }
              break;
            case TargetType.RectTransform:
            {
#if true // UI_MARKER
              var rectTransform = (RectTransform) target.transform;
              cachedValueV3 = rectTransform.anchoredPosition3D;
              tween = rectTransform.DOAnchorPos3D(endValueV3, duration, optionalBool0);
              var tweenerCore = (TweenerCore<Vector3, Vector3, VectorOptions>) tween;
              startValueApplyAction = () =>
              { 
                tweenerCore.ChangeStartValue(fromToValueV3);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueV3,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue,
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueV3 : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { rectTransform.anchoredPosition3D = stoppedValue; });
              }; 
#else
              tween = ((Transform)target).DOMove(endValueV3, duration, optionalBool0);
#endif
            }
              break;
            case TargetType.Rigidbody:
            {
#if true // PHYSICS_MARKER
              var rigidbody = (Rigidbody) target;
              cachedValueV3 = rigidbody.position;
              tween = rigidbody.DOMove(endValueV3, duration, optionalBool0);
              var tweenerCore = (TweenerCore<Vector3, Vector3, VectorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueV3);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueV3,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue,
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueV3 : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { rigidbody.position = stoppedValue; });
              };
#else
              tween = ((Transform)target).DOMove(endValueV3, duration, optionalBool0);
#endif
            }
              break;
            case TargetType.Rigidbody2D:
            {
#if true // PHYSICS2D_MARKER
              var rigidbody2D = (Rigidbody2D) target;
              cachedValueV2 = rigidbody2D.position;
              tween = rigidbody2D.DOMove(endValueV2, duration, optionalBool0);
              var tweenerCore = (TweenerCore<Vector2, Vector2, VectorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueV2);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueV2,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue,
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueV2 : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { rigidbody2D.position = stoppedValue; });
              };
#else
              tween = ((Transform)target).DOMove(endValueV3, duration, optionalBool0);
#endif
            }
              break;
          }

          break;
        case AnimationType.LocalMove:
        {
          cachedValueV3 = tweenGO.transform.localPosition;
          tween = tweenGO.transform.DOLocalMove(endValueV3, duration, optionalBool0);
          var tweenerCore = (TweenerCore<Vector3, Vector3, VectorOptions>) tween;
          startValueApplyAction = () =>
          {
            tweenerCore.ChangeStartValue(fromToValueV3);
            var stoppedValue = whenStopped switch
            {
              WhenStopped.BeforeStartPosition => cachedValueV3,
              WhenStopped.StartPosition => tweenerCore.startValue,
              WhenStopped.EndPosition => tweenerCore.endValue,
            };
            stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueV3 : stoppedValue;
            if (CallStoppedAction) tween.OnRewind(() => { tweenGO.transform.localPosition = stoppedValue; });
          }; 
        }
          break;
        case AnimationType.Rotate:
          switch (targetType)
          {
            case TargetType.Transform:
            {
              var transform = (Transform) target;
              cachedQuaternion = transform.rotation;
              tween = transform.DORotate(endValueV3, duration, optionalRotationMode);
              var tweenerCore = (TweenerCore<Quaternion, Vector3, QuaternionOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueV3);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedQuaternion,
                  WhenStopped.StartPosition => Quaternion.Euler(fromToValueV3),
                  WhenStopped.EndPosition => Quaternion.Euler(endValueV3.x, endValueV3.y, endValueV3.z),
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedQuaternion : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { transform.rotation = stoppedValue; });
              };
            }
              break;
            case TargetType.Rigidbody:
            {
#if true // PHYSICS_MARKER
              var rigidbody = (Rigidbody) target;
              cachedQuaternion = rigidbody.rotation;
              tween = rigidbody.DORotate(endValueV3, duration, optionalRotationMode);
              var tweenerCore = (TweenerCore<Quaternion, Vector3, QuaternionOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueV3);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedQuaternion,
                  WhenStopped.StartPosition => Quaternion.Euler(fromToValueV3),
                  WhenStopped.EndPosition => Quaternion.Euler(endValueV3.x, endValueV3.y, endValueV3.z),
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedQuaternion : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { rigidbody.rotation = stoppedValue; });
              };
#else
              tween = ((Transform)target).DORotate(endValueV3, duration, optionalRotationMode);
#endif
            }
              break;
            case TargetType.Rigidbody2D:
            {
#if true // PHYSICS2D_MARKER
              var rigidbody2D = (Rigidbody2D) target;
              cachedValueFloat = rigidbody2D.rotation;
              tween = rigidbody2D.DORotate(endValueFloat, duration);
              var tweenerCore = (TweenerCore<float, float, FloatOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueFloat);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueFloat,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue,
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueFloat : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { rigidbody2D.rotation = stoppedValue; });
              };

#else
              tween = ((Transform)target).DORotate(endValueV3, duration, optionalRotationMode);
#endif
            }
              break;
          }

          break;
        case AnimationType.LocalRotate:
        {
          cachedQuaternion = tweenGO.transform.localRotation;
          tween = tweenGO.transform.DOLocalRotate(endValueV3, duration, optionalRotationMode);
          var tweenerCore = (TweenerCore<Quaternion, Vector3, QuaternionOptions>) tween;
          startValueApplyAction = () =>
          { 
            tweenerCore.ChangeStartValue(fromToValueV3);
            var stoppedValue = whenStopped switch
            {
              WhenStopped.BeforeStartPosition => cachedQuaternion,
              WhenStopped.StartPosition => Quaternion.Euler(fromToValueV3),
              WhenStopped.EndPosition => Quaternion.Euler(endValueV3.x, endValueV3.y, endValueV3.z),
            };
            stoppedValue = Application.isEditor && !Application.isPlaying ? cachedQuaternion : stoppedValue;
            if (CallStoppedAction) tween.OnRewind(() => { tweenGO.transform.localRotation = stoppedValue; });
          };
        }
          break;
        case AnimationType.Scale:
          switch (targetType)
          {
#if false // TK2D_MARKER
                case TargetType.tk2dTextMesh:
                    tween =
 ((tk2dTextMesh)target).DOScale(optionalBool0 ? new Vector3(endValueFloat, endValueFloat, endValueFloat) : endValueV3, duration);
                    break;
                case TargetType.tk2dBaseSprite:
                    tween =
 ((tk2dBaseSprite)target).DOScale(optionalBool0 ? new Vector3(endValueFloat, endValueFloat, endValueFloat) : endValueV3, duration);
                    break;
#endif
            default:
            {
              cachedValueV3 = tweenGO.transform.localScale;
              tweenGO.transform.localScale = optionalBool0
                ? new Vector3(fromToValueFloat, fromToValueFloat, fromToValueFloat)
                : fromToValueV3;
              tween = tweenGO.transform.DOScale(
                optionalBool0 ? new Vector3(endValueFloat, endValueFloat, endValueFloat) : endValueV3, duration);
              var tweenerCore = (TweenerCore<Vector3, Vector3, VectorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(optionalBool0
                  ? new Vector3(fromToValueFloat, fromToValueFloat, fromToValueFloat)
                  : fromToValueV3);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueV3,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue,
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueV3 : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { tweenGO.transform.localScale = stoppedValue; });
              };

            }
              break;
          }

          break;
#if true // UI_MARKER
        case AnimationType.UIWidthHeight:
        {
          var rectTransform = (RectTransform) target.transform;
          cachedValueV2 = rectTransform.sizeDelta;
          rectTransform.sizeDelta = optionalBool0 ? new Vector2(fromToValueFloat, fromToValueFloat) : fromToValueV2;
          tween = rectTransform.DOSizeDelta(optionalBool0 ? new Vector2(endValueFloat, endValueFloat) : endValueV2,
            duration);
          var tweenerCore = (TweenerCore<Vector2, Vector2, VectorOptions>) tween;
          startValueApplyAction = () =>
          {
            tweenerCore.ChangeStartValue(
              optionalBool0 ? new Vector2(fromToValueFloat, fromToValueFloat) : fromToValueV2);
            var stoppedValue = whenStopped switch
            {
              WhenStopped.BeforeStartPosition => cachedValueV2,
              WhenStopped.StartPosition => tweenerCore.startValue,
              WhenStopped.EndPosition => tweenerCore.endValue,
            };
            stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueV2 : stoppedValue;
            if (CallStoppedAction) tween.OnRewind(() => { rectTransform.sizeDelta = stoppedValue; });
          };
        }
          break;
#endif
        case AnimationType.Color:
          isRelative = false;
          switch (targetType)
          {
            case TargetType.Renderer:
            {
              var renderer = (Renderer) target;
              cachedValueColor = renderer.material.color;
              tween = renderer.material.DOColor(endValueColor, duration);
              var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueColor);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueColor,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { renderer.material.color = stoppedValue; });
              };
            }
              break;
            case TargetType.Light:
            {
              var light = (Light) target;
              cachedValueColor = light.color;
              tween = light.DOColor(endValueColor, duration);
              var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueColor);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueColor,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { light.color = stoppedValue; });
              }; 
            }
              break;
#if true // SPRITE_MARKER
            case TargetType.SpriteRenderer:
            {
              var spriteRenderer = (SpriteRenderer) target;
              cachedValueColor = spriteRenderer.color;
              tween = spriteRenderer.DOColor(endValueColor, duration);
              var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueColor);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueColor,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { spriteRenderer.color = stoppedValue; });
              };
            }
              break;
#endif
#if true // UI_MARKER
            case TargetType.Image:
            {
              var graphic = (Graphic) target;
              cachedValueColor = graphic.color;
              tween = graphic.DOColor(endValueColor, duration);
              var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueColor);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueColor,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { graphic.color = stoppedValue; });
              };
            }
              break;
            case TargetType.Text:
            {
              var text = (Text) target;
              cachedValueColor = text.color;
              tween = text.DOColor(endValueColor, duration);
              var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueColor);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueColor,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { text.color = stoppedValue; });
              };
            }
              break;
#endif
#if false // TK2D_MARKER
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOColor(endValueColor, duration);
                    break;
                case TargetType.tk2dBaseSprite:
                    tween = ((tk2dBaseSprite)target).DOColor(endValueColor, duration);
                    break;
#endif
#if false // TEXTMESHPRO_MARKER
                case TargetType.TextMeshProUGUI:
                    tween = ((TextMeshProUGUI)target).DOColor(endValueColor, duration);
                    break;
                case TargetType.TextMeshPro:
                    tween = ((TextMeshPro)target).DOColor(endValueColor, duration);
                    break;
#endif
          }

          break;
        case AnimationType.Fade:
          isRelative = false;
          switch (targetType)
          {
            case TargetType.Renderer:
            {
              var renderer = (Renderer) target;
              cachedValueColor = renderer.material.color;
              tween = renderer.material.DOFade(endValueFloat, duration);
              var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(new Color(renderer.material.color.r,
                renderer.material.color.g,
                renderer.material.color.b, fromToValueFloat));
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueColor,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => new Color(renderer.material.color.r, renderer.material.color.g,
                    renderer.material.color.b, endValueFloat)
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { renderer.material.color = stoppedValue; });
              };
            }
              break;
            case TargetType.Light:
            {
              var light = (Light) target;
              cachedValueFloat = light.intensity;
              tween = light.DOIntensity(endValueFloat, duration);
              var tweenerCore = (TweenerCore<float, float, FloatOptions>) tween;
              startValueApplyAction = () =>
              { 
                tweenerCore.ChangeStartValue(fromToValueFloat);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueFloat,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueFloat : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { light.intensity = stoppedValue; });
              };
            }
              break;
#if true // SPRITE_MARKER
            case TargetType.SpriteRenderer:
            {
              var spriteRenderer = (SpriteRenderer) target;
              cachedValueColor = spriteRenderer.color;
              tween = spriteRenderer.DOFade(endValueFloat, duration);
              var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
              startValueApplyAction = () => 
              {
                tweenerCore.ChangeStartValue(new Color(spriteRenderer.color.r,
                  spriteRenderer.color.g, spriteRenderer.color.b, fromToValueFloat));
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueColor,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => new Color(spriteRenderer.color.r, spriteRenderer.color.g,
                    spriteRenderer.color.b, endValueFloat)
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { spriteRenderer.color = stoppedValue; });
              };
            }
              break;
#endif
#if true // UI_MARKER
            case TargetType.Image:
            {
              var graphic = (Graphic) target;
              cachedValueColor = graphic.color;
              tween = graphic.DOFade(endValueFloat, duration);
              var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(new Color(graphic.color.r, graphic.color.g, graphic.color.b,
                  fromToValueFloat));
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueColor,
                  WhenStopped.StartPosition => new Color(graphic.color.r, graphic.color.g, graphic.color.b,
                    fromToValueFloat),
                  WhenStopped.EndPosition => new Color(graphic.color.r, graphic.color.g, graphic.color.b, endValueFloat)
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { graphic.color = stoppedValue; });
              };
            }
              break;
            case TargetType.Text:
            {
              var text = (Text) target;
              cachedValueColor = text.color;
              tween = text.DOFade(endValueFloat, duration);
              var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(new Color(text.color.r, text.color.g, text.color.b, fromToValueFloat));
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueColor,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => new Color(text.color.r, text.color.g, text.color.b, endValueFloat)
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { text.color = stoppedValue; });
              };
            }
              break;
            case TargetType.CanvasGroup:
            {
              var canvasGroup = (CanvasGroup) target;
              cachedValueFloat = canvasGroup.alpha;
              tween = canvasGroup.DOFade(endValueFloat, duration);
              var tweenerCore = (TweenerCore<float, float, FloatOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueFloat);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueFloat,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueFloat : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { canvasGroup.alpha = stoppedValue; });
              };
            }
              break;
#endif
#if false // TK2D_MARKER
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOFade(endValueFloat, duration);
                    break;
                case TargetType.tk2dBaseSprite:
                    tween = ((tk2dBaseSprite)target).DOFade(endValueFloat, duration);
                    break;
#endif
#if false // TEXTMESHPRO_MARKER
                case TargetType.TextMeshProUGUI:
                    tween = ((TextMeshProUGUI)target).DOFade(endValueFloat, duration);
                    break;
                case TargetType.TextMeshPro:
                    tween = ((TextMeshPro)target).DOFade(endValueFloat, duration);
                    break;
#endif
          }

          break;
        case AnimationType.Text:
#if true // UI_MARKER
          switch (targetType)
          {
            case TargetType.Text:
            {
              var text = (Text) target;
              cachedValueString = text.text;
              tween = text.DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
              var tweenerCore = (TweenerCore<string, string, StringOptions>) tween;
              startValueApplyAction = () =>
              {
                tweenerCore.ChangeStartValue(fromToValueString);
                var stoppedValue = whenStopped switch
                {
                  WhenStopped.BeforeStartPosition => cachedValueString,
                  WhenStopped.StartPosition => tweenerCore.startValue,
                  WhenStopped.EndPosition => tweenerCore.endValue
                };
                stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueString : stoppedValue;
                if (CallStoppedAction) tween.OnRewind(() => { text.text = stoppedValue; });
              };
              }
              break;
          }
#endif
#if false // TK2D_MARKER
                switch (targetType) {
                case TargetType.tk2dTextMesh:
                    tween =
 ((tk2dTextMesh)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
                }
#endif
#if false // TEXTMESHPRO_MARKER
                switch (targetType) {
                case TargetType.TextMeshProUGUI:
                    tween =
 ((TextMeshProUGUI)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
                case TargetType.TextMeshPro:
                    tween =
 ((TextMeshPro)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
                }
#endif
          break;
        case AnimationType.PunchPosition:
          switch (targetType)
          {
            case TargetType.Transform:
            {
              tween = ((Transform) target).DOPunchPosition(endValueV3, duration, optionalInt0, optionalFloat0,
                optionalBool0);
            }
              break;
#if true // UI_MARKER
            case TargetType.RectTransform:
            {
              tween = ((RectTransform) target).DOPunchAnchorPos(endValueV3, duration, optionalInt0, optionalFloat0,
                optionalBool0);
            }
              break;
#endif
          }

          break;
        case AnimationType.PunchScale:
        {
          tween = tweenGO.transform.DOPunchScale(endValueV3, duration, optionalInt0, optionalFloat0);
        }
          break;
        case AnimationType.PunchRotation:
        {
          tween = tweenGO.transform.DOPunchRotation(endValueV3, duration, optionalInt0, optionalFloat0);
        }
          break;
        case AnimationType.ShakePosition:
          switch (targetType)
          {
            case TargetType.Transform:
            {
              tween = ((Transform) target).DOShakePosition(duration, endValueV3, optionalInt0, optionalFloat0,
                optionalBool0, optionalBool1, optionalShakeRandomnessMode);
            }
              break;
#if true // UI_MARKER
            case TargetType.RectTransform:
            {
              tween = ((RectTransform) target).DOShakeAnchorPos(duration, endValueV3, optionalInt0, optionalFloat0,
                optionalBool0, optionalBool1, optionalShakeRandomnessMode);
            }
              break;
#endif
          }

          break;
        case AnimationType.ShakeScale:
        {
          tween = tweenGO.transform.DOShakeScale(duration, endValueV3, optionalInt0, optionalFloat0, optionalBool1,
            optionalShakeRandomnessMode);
        }
          break;
        case AnimationType.ShakeRotation:
        {
          tween = tweenGO.transform.DOShakeRotation(duration, endValueV3, optionalInt0, optionalFloat0, optionalBool1,
            optionalShakeRandomnessMode);
        }

          break;
        case AnimationType.CameraAspect:
        {
          var camera = (Camera) target;
          cachedValueFloat = camera.aspect;
          tween = camera.DOAspect(endValueFloat, duration);
          var tweenerCore = (TweenerCore<float, float, FloatOptions>) tween;
          startValueApplyAction = () =>
          {
            tweenerCore.ChangeStartValue(fromToValueFloat);
            var stoppedValue = whenStopped switch
            {
              WhenStopped.BeforeStartPosition => cachedValueFloat,
              WhenStopped.StartPosition => tweenerCore.startValue,
              WhenStopped.EndPosition => tweenerCore.endValue,
            };
            stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueFloat : stoppedValue;
            if (CallStoppedAction) tween.OnRewind(() => { camera.aspect = stoppedValue; });
          };
        }
          break;
        case AnimationType.CameraBackgroundColor:
        {
          var camera = (Camera) target;
          cachedValueColor = camera.backgroundColor;
          tween = camera.DOColor(endValueColor, duration);
          var tweenerCore = (TweenerCore<Color, Color, ColorOptions>) tween;
          startValueApplyAction = () =>
          {
            tweenerCore.ChangeStartValue(fromToValueColor);
            var stoppedValue = whenStopped switch
            {
              WhenStopped.BeforeStartPosition => cachedValueColor,
              WhenStopped.StartPosition => tweenerCore.startValue,
              WhenStopped.EndPosition => tweenerCore.endValue,
            };
            stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueColor : stoppedValue;
            if (CallStoppedAction) tween.OnRewind(() => { camera.backgroundColor = stoppedValue; });
          };
        }
          break;
        case AnimationType.CameraFieldOfView:
        {
          var camera = (Camera) target;
          cachedValueFloat = camera.aspect;
          tween = camera.DOFieldOfView(endValueFloat, duration);
          var tweenerCore = (TweenerCore<float, float, FloatOptions>) tween;
          startValueApplyAction = () =>
          {
            tweenerCore.ChangeStartValue(fromToValueFloat);
            var stoppedValue = whenStopped switch
            {
              WhenStopped.BeforeStartPosition => cachedValueFloat,
              WhenStopped.StartPosition => tweenerCore.startValue,
              WhenStopped.EndPosition => tweenerCore.endValue,
            };
            stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueFloat : stoppedValue;
            if (CallStoppedAction) tween.OnRewind(() => { camera.aspect = stoppedValue; });
          };
        }
          break;
        case AnimationType.CameraOrthoSize:
        {
          var camera = (Camera) target;
          cachedValueFloat = camera.orthographicSize;
          tween = camera.DOOrthoSize(endValueFloat, duration);
          var tweenerCore = (TweenerCore<float, float, FloatOptions>) tween;
          startValueApplyAction = () =>
          {
            tweenerCore.ChangeStartValue(fromToValueFloat);
            var stoppedValue = whenStopped switch
            {
              WhenStopped.BeforeStartPosition => cachedValueFloat,
              WhenStopped.StartPosition => tweenerCore.startValue,
              WhenStopped.EndPosition => tweenerCore.endValue,
            };
            stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueFloat : stoppedValue;
            if (CallStoppedAction) tween.OnRewind(() => { camera.orthographicSize = stoppedValue; });
          };
        }
          break;
        case AnimationType.CameraPixelRect:
        {
          var camera = (Camera) target;
          cachedValueRect = camera.pixelRect;
          tween = camera.DOPixelRect(endValueRect, duration);
          var tweenerCore = (TweenerCore<Rect, Rect, RectOptions>) tween;
          startValueApplyAction = () =>
          {
            tweenerCore.ChangeStartValue(fromToValueRect);
            var stoppedValue = whenStopped switch
            {
              WhenStopped.BeforeStartPosition => cachedValueRect,
              WhenStopped.StartPosition => tweenerCore.startValue,
              WhenStopped.EndPosition => tweenerCore.endValue,
            };
            stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueRect : stoppedValue;
            if (CallStoppedAction) tween.OnRewind(() => { camera.pixelRect = stoppedValue; });
          };
        }
          break;
        case AnimationType.CameraRect:
        {
          var camera = (Camera) target;
          cachedValueRect = camera.rect;
          tween = camera.DORect(endValueRect, duration);
          var tweenerCore = (TweenerCore<Rect, Rect, RectOptions>) tween;
          startValueApplyAction = () =>
          {
            tweenerCore.ChangeStartValue(fromToValueRect);
            var stoppedValue = whenStopped switch
            {
              WhenStopped.BeforeStartPosition => cachedValueRect,
              WhenStopped.StartPosition => tweenerCore.startValue,
              WhenStopped.EndPosition => tweenerCore.endValue,
            };
            stoppedValue = Application.isEditor && !Application.isPlaying ? cachedValueRect : stoppedValue;
            if (CallStoppedAction) tween.OnRewind(() => { camera.rect = stoppedValue; });
          };
        }
          break;
      }

      if (tween == null) return tween;

      if (!Application.isPlaying)
      {
        startValueApplyAction?.Invoke();
      }
      else if (startValueApplyOnCreate)
      {
        startValueApplyAction?.Invoke();
      }

      // Created

      if (isFrom)
      {
        ((Tweener) tween).From(isRelative);
      }
      else
      {
        tween.SetRelative(isRelative);
      }

      GameObject setTarget = GetTweenTarget();
      tween.SetTarget(setTarget).SetDelay(delay).SetLoops(loops, loopType).SetAutoKill(autoKill)
        .OnKill(() => tween = null);
      if (isSpeedBased) tween.SetSpeedBased();
      if (easeType == Ease.INTERNAL_Custom) tween.SetEase(easeCurve);
      else tween.SetEase(easeType);
      if (!string.IsNullOrEmpty(id)) tween.SetId(id);
      tween.SetUpdate(isIndependentUpdate);

      if (hasOnStart)
      {
        if (onStart != null) tween.OnStart(onStart.Invoke);
      }
      else onStart = null;

      if (hasOnPlay)
      {
        if (onPlay != null) tween.OnPlay(onPlay.Invoke);
      }
      else onPlay = null;

      if (hasOnUpdate)
      {
        if (onUpdate != null) tween.OnUpdate(onUpdate.Invoke);
      }
      else onUpdate = null;

      if (hasOnStepComplete)
      {
        if (onStepComplete != null) tween.OnStepComplete(onStepComplete.Invoke);
      }
      else onStepComplete = null;

      if (hasOnComplete)
      {
        if (onComplete != null) tween.OnComplete(onComplete.Invoke);
      }
      else onComplete = null;

      if (hasOnRewind)
      {
        if (onRewind != null) tween.OnRewind(onRewind.Invoke);
      }
      else onRewind = null;

      if (andPlay) tween.Play();
      else tween.Pause();

      if (hasOnTweenCreated && onTweenCreated != null) onTweenCreated.Invoke();

      return tween;

      void CheckTransformPosition()
      {
        if (!useTargetAsV3 && !useTargetAsFromV3)
          return;

        isRelative = false;

        SetTransformPosition(fromToValueTransform, ref fromToValueV3);
        SetTransformPosition(endValueTransform, ref endValueV3);
      }

      void SetTransformPosition(Transform transform, ref Vector3 vector3)
      {
        if (transform == null)
        {
          Debug.LogWarning(
            string.Format("{0} :: This tween's TO target is NULL, a Vector3 of (0,0,0) will be used instead",
              this.gameObject.name), this.gameObject);
          vector3 = Vector3.zero;
        }
        else
        {
#if true // UI_MARKER
          if (targetType == TargetType.RectTransform)
          {
            RectTransform endValueT = transform as RectTransform;
            if (endValueT == null)
            {
              Debug.LogWarning(
                string.Format(
                  "{0} :: This tween's TO target should be a RectTransform, a Vector3 of (0,0,0) will be used instead",
                  this.gameObject.name), this.gameObject);
              vector3 = Vector3.zero;
            }
            else
            {
              RectTransform rTarget = target as RectTransform;
              if (rTarget == null)
              {
                Debug.LogWarning(
                  string.Format(
                    "{0} :: This tween's target and TO target are not of the same type. Please reassign the values",
                    this.gameObject.name), this.gameObject);
              }
              else
              {
                // Problem: doesn't work inside Awake (ararargh!)
                vector3 = DOTweenModuleUI.Utils.SwitchToRectTransform(endValueT, rTarget);
              }
            }
          }
          else
#endif
            vector3 = transform.position;
        }
      }
    }
  }
}