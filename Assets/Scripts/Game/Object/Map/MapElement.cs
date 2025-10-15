using UnityEngine;

public class MapElement : MonoBehaviour
{
  public enum MapElementTypes
  {
    None = -1,

    Ground = 0,
    Bridge,

    Count
  }

  [field: SerializeField] public MapElementTypes ElementType { get; private set; }

  public void SetElement(MapElementTypes elementType)
  {

  }
}
