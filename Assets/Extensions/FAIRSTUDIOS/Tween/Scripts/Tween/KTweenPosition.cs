using UnityEngine;
using UnityEngine.Events;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Tween/Tween Position")]
	public class KTweenPosition : KTweener {
		
		public Vector3 from;
		public Vector3 to;

    public bool isWorldPosition = false;
    public bool useRectTransform = false;

		RectTransform _rectTransform;

    public RectTransform rectTransform
    {
      get
      {
        if (_rectTransform == null)
          _rectTransform = GetComponent<RectTransform>();

        return _rectTransform;
      }
    }
		public virtual Vector3 value
    {
			get
      {
        if(isWorldPosition)
        {
          return transform.position;
        }
        else
        {
          if (useRectTransform && null != rectTransform)
          {
            return rectTransform.anchoredPosition;
          }
          else
          {
            return transform.localPosition;
          }
        }
      }
			set
      {
        if (isWorldPosition)
        {
          transform.position = value;
        }
        else
        {
          if (useRectTransform && null != rectTransform)
          {
            rectTransform.anchoredPosition = value;
          }
          else
          {
            transform.localPosition = value;
          }
        }
      }
		}
		
		protected override void OnUpdate (float factor, bool isFinished)
		{
      value = from + factor * (to - from);
    }

    public void Begin(Vector3 from, Vector3 to, float duration = 1f, float delay = 0f, EaseType easeType = EaseType.linear, LoopStyle loopStyle = LoopStyle.Once, UnityAction onFinished = null)
    {
      this.from = from;
      this.to = to;
      this.duration = duration;
      this.delay = delay;
      this.easeType = easeType;
      this.loopStyle = loopStyle;

      if (onFinished != null)
        this.onFinished.AddListener(onFinished);

      if (duration <= 0)
      {
        Sample(1, true);
        enabled = false;
      }
      else
      {
        ForceStart = true;
        enabled = true;
      }
    }

    public static KTweenPosition Begin(GameObject go,Vector3 from, Vector3 to, float duration = 1f, float delay = 0f, UnityAction onFinished = null)
    {
      KTweenPosition comp = InitializeTween<KTweenPosition>(go);
      comp.Begin(from, to, duration, delay, comp.easeType, comp.loopStyle, onFinished);

      return comp;
    }

    public static KTweenPosition Begin(GameObject go, bool isWorldPosition, bool useRectTransform, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f, UnityAction onFinished = null)
    {
      KTweenPosition comp = InitializeTween<KTweenPosition>(go);
      comp.isWorldPosition = isWorldPosition;
      comp.useRectTransform = useRectTransform;

      comp.Begin(from, to, duration, delay, comp.easeType, comp.loopStyle, onFinished);

      return comp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go">포지션 이동을 할 대상</param>
    /// <param name="isWorldPosition">월드 포지션 기준으로 이동 할 것인지</param>
    /// <param name="useRectTransform">RectTransform 이 존재하면 anchoredPosition 으로 이동 할 것인지</param>
    /// <param name="from">시작 벡터</param>
    /// <param name="to">끝 벡터</param>
    /// <param name="duration">이동 시간</param>
    /// <param name="delay">딜레이</param>
    /// <param name="easeType"></param>
    /// <param name="loopStyle"></param>
    /// <param name="onFinished">종료 시 발생하는 이벤트</param>
    /// <returns></returns>
    public static KTweenPosition Begin(GameObject go, bool isWorldPosition, bool useRectTransform, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f, EaseType easeType = EaseType.linear, LoopStyle loopStyle = LoopStyle.Once, UnityAction onFinished = null)
    {
			KTweenPosition comp = InitializeTween<KTweenPosition>(go);
      comp.isWorldPosition = isWorldPosition;
      comp.useRectTransform = useRectTransform;

      comp.Begin(from, to, duration, delay, easeType, loopStyle, onFinished);

      return comp;
    }

		[ContextMenu("Set 'From' to current value")]
		public override void SetStartToCurrentValue () { from = value; }
		
		[ContextMenu("Set 'To' to current value")]
		public override void SetEndToCurrentValue () { to = value; }
		
		[ContextMenu("Assume value of 'From'")]
		public override void SetCurrentValueToStart () { value = from; }
		
		[ContextMenu("Assume value of 'To'")]
		public override void SetCurrentValueToEnd () { value = to; }
	}
}
