using UnityEngine;
using UnityEngine.EventSystems;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Internal/Drag Object")]
	public class KDragObject : MonoBehaviour, IDragHandler {

		public RectTransform target;

		RectTransform cacheTarget {
			get {
				if (target == null) {
					target = GetComponent<RectTransform>();
				}
				return target;
			}
		}

		// Use this for initialization
		void Start () {		

		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void OnDrag (PointerEventData eventData)
    {
			Vector3 from = cacheTarget.localPosition;
			Vector3 to = from + new Vector3 (eventData.delta.x, eventData.delta.y, 0);
			KTweenPosition.Begin (gameObject, from, to, .02f);//.easeType = EaseType.easeInBack;
			//cacheTarget.localPosition += new Vector3 (eventData.delta.x, eventData.delta.y, 0);
		}
	}
}