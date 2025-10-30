using UnityEngine;
using static StageDataTable;


public class MapElement : MonoBehaviour
{
  public enum MapElementTypes
  {
    None = -1,

    Ground = 0,
    Bridge,

    Count
  }


  [SerializeField] private MapElementTypes elementType;

  public BoxCollider2D boxCollider;
  public SpriteRenderer spriteRenderer;

  protected MapData Data { get; private set; }

  public MapElementTypes ElementType => elementType;

  private void Awake()
  {

  }

  public virtual void SetData(MapData mapData)
  {
    Data = mapData;

    transform.position = mapData.position;
    transform.localScale = mapData.scale;

    boxCollider.size = spriteRenderer.size = mapData.size;
  }
}
