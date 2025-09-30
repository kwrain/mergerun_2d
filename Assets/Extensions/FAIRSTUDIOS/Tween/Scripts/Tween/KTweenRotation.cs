using UnityEngine;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Tween/Tween Rotation")]
	public class KTweenRotation : KTweener {
		public Vector3 from;
		public Vector3 to;

		Transform mTransfrom;
    public Transform target;

    public Transform cacheRectTransfrom
    {
			get { 
				if (target == null) {
          mTransfrom = GetComponent<Transform>();
				}
				else {
					mTransfrom = target;
				}
				return mTransfrom;			
			}
		}

		public Quaternion value {
			get { 
				return cacheRectTransfrom.localRotation;
			}
			set {
				cacheRectTransfrom.localRotation = value;
			}
		}

		protected override void OnUpdate (float _factor, bool _isFinished)
		{
			value = Quaternion.Euler(Vector3.Lerp(from, to, _factor));
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
        ForceStart = true;
        enabled = true;
      }
    }

    public static KTweenRotation Begin(GameObject go, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
    {
			KTweenRotation comp = InitializeTween<KTweenRotation>(go);
      comp.Begin(from, to, duration, delay);

      return comp;
		}
	}
}