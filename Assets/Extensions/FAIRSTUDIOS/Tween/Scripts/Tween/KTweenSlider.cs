using UnityEngine;
using UnityEngine.UI;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Tween/Tween Slider")]	
	public class KTweenSlider : KTweenValue {

		private Slider mSlider;
		public Slider cacheSlider {
			get {
				mSlider = GetComponent<Slider>();
				if (mSlider == null) {
					Debug.LogError("'uTweenSlider' can't find 'Slider'");
				}
				return mSlider;
			}
		}

		/// <summary>
		/// The need carry.
		/// when is true, value==1 equal value=0
		/// </summary>
		public bool NeedCarry = false;

		public float sliderValue {
			set {
				if (NeedCarry) {
					cacheSlider.value = (value>=1)?value - Mathf.Floor(value) : value;
				}
				else {
					cacheSlider.value = (value>1)?value - Mathf.Floor(value) : value;
				}
			}
		}

		protected override void ValueUpdate (float value, bool isFinished)
		{
			this.sliderValue = value;
		}

    public void Begin(float from, float to, float duration = 1f, float delay = 0f)
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
        ForceStart = true;
        enabled = true;
      }
    }

    public static KTweenSlider Begin(Slider slider, float from, float to, float duration = 1f, float delay = 0f)
    {
			KTweenSlider comp = InitializeTween<KTweenSlider>(slider.gameObject);
      comp.Begin(from, to, duration, delay);

      return comp;
		}
	}
}
