using UnityEngine;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Tween/Tween Scale")]
	public class KTweenScale : KTweener
  {
		public Vector3 from = Vector3.zero;
		public Vector3 to = Vector3.one;

    RectTransform mRectTransform;
    Transform mTransform;

    public RectTransform rectTransform { get { if (mRectTransform == null) mRectTransform = GetComponent<RectTransform>(); return mRectTransform; } }
    public Transform cachedTransform { get { if (mTransform == null) mTransform = GetComponent<Transform>(); return mTransform; } }
		public Vector3 value {
			get 
      {
        if (null == rectTransform)
          return cachedTransform.localScale;

        return rectTransform.localScale;
      }
			set 
      {
        if (null == rectTransform)
        {
          cachedTransform.localScale = value;
        }
        else
        {
          rectTransform.localScale = value;
        }
      }
		}

		protected override void OnUpdate (float factor, bool isFinished)
		{
      value = from + factor * (to - from);
		}

    public void Begin(Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
    {
      this.from = from;
      this.to = to;
      this.duration = duration;
      this.delay = delay;

      if (duration <= 0)
      {
        Sample(1, true);
        enabled = false;
      }
      else
      {
        if (!this.IsActiveInHierarchy())
        {
          ForceStart = false;
          enabled = false;
          return;
        }
        ForceStart = true;
        enabled = true;
      }
    }

		public static KTweenScale Begin(GameObject go, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
    {
			KTweenScale comp = KTweener.InitializeTween<KTweenScale>(go);
      comp.Begin(from, to, duration, delay);

			return comp;
		}
	}
}
