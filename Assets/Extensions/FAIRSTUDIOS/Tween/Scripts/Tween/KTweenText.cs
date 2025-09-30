using UnityEngine;
using UnityEngine.UI;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Tween/Tween Text")]	
	
	public class KTweenText : KTweenValue {

		private Text mText;
		public Text cacheText {
			get {
				mText = GetComponent<Text>();
				if (mText == null) {
					Debug.LogError("'uTweenText' can't find 'Text'");
				}
				return mText;
			}
		}

		/// <summary>
		/// number after the digit point
		/// </summary>
		public int digits;

		protected override void ValueUpdate (float value, bool isFinished)
		{
			cacheText.text = (System.Math.Round(value, digits)).ToString();
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

    public static KTweenText Begin(Text label, float from, float to, float duration = 0f, float delay = 0f)
    {
			KTweenText comp = InitializeTween<KTweenText>(label.gameObject);
      comp.Begin(from, to, duration, delay);

      return comp;
		}
	}
}
