using UnityEngine.EventSystems;

namespace FAIRSTUDIOS.Tools
{
  public interface KIPointHandler : IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
  {
    new void OnPointerDown(PointerEventData eventData);
    new void OnPointerClick(PointerEventData eventData);
    new void OnPointerUp(PointerEventData eventData);
  }
}