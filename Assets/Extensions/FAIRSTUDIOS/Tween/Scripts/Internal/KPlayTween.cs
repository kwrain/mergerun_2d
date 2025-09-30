using UnityEngine;
using UnityEngine.EventSystems;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Internal/Play Tween")]
	public class uPlayTween : MonoBehaviour, KIPointHandler {
		public KTweener tweenTarget;
		public PlayDirection playDirection = PlayDirection.Forward;
		public Trigger trigger = Trigger.OnPointerClick;

		// Use this for initialization
		void Start () {
			if (tweenTarget == null) {
				tweenTarget = GetComponent<KTweener>();
			}		
		}

		public void OnPointerEnter (PointerEventData eventData) {
			TriggerPlay (Trigger.OnPointerEnter);
		}

		public void OnPointerDown (PointerEventData eventData) {
			TriggerPlay (Trigger.OnPointerDown);
		}

		public void OnPointerClick (PointerEventData eventData) {
			TriggerPlay (Trigger.OnPointerClick);
		}

		public void OnPointerUp (PointerEventData eventData) {
			TriggerPlay (Trigger.OnPointerUp);
		}

		public void OnPointerExit (PointerEventData eventData) {
			TriggerPlay (Trigger.OnPointerExit);
		}

		private void TriggerPlay(Trigger _trigger) {
			if (_trigger == trigger) {
				Play();
			}
		}

		/// <summary>
		/// Play this instance.
		/// </summary>
		private void Play() {
			if (playDirection == PlayDirection.Toggle) {
				tweenTarget.Toggle();
			}
			else {
				tweenTarget.Play(playDirection);
			}
		}

	}
}